using System;
using System.Collections.Generic;
using System.Reflection;
using SandScript.AbstractSyntaxTrees;

namespace SandScript;

/// <summary>
/// Wrapper for a CSharp method or SandScript method.
/// </summary>
public class ScriptMethod : IEquatable<ScriptMethod>
{
	/// <summary>
	/// The name of the method.
	/// </summary>
	public readonly string Name;
	/// <summary>
	/// The return type of the method.
	/// </summary>
	public readonly ITypeProvider ReturnTypeProvider;
	/// <summary>
	/// The information for each parameter.
	/// </summary>
	public readonly IReadOnlyList<(string, ITypeProvider)> Parameters;
	/// <summary>
	/// The signature of this method.
	/// </summary>
	public readonly MethodSignature Signature;
	
	private readonly bool _isCsMethod;
	
	private MethodInfo? MethodInfo { get; }
	private ParameterInfo[]? MethodParameters { get; }
	private MethodDeclarationAst? MethodDeclarationAst { get; }
	
	public ScriptMethod( MethodDeclarationAst methodDeclarationAst )
	{
		Name = methodDeclarationAst.MethodName;
		ReturnTypeProvider = methodDeclarationAst.ReturnTypeAst.TypeProvider;

		_isCsMethod = false;
		MethodDeclarationAst = methodDeclarationAst;

		var parameters = new List<(string, ITypeProvider)>();
		foreach ( var parameter in methodDeclarationAst.ParameterAsts )
			parameters.Add( (parameter.ParameterNameAst.VariableName, parameter.ParameterTypeAst.TypeProvider) );
		Parameters = parameters;
		
		Signature = MethodSignature.From( this );
	}

	internal ScriptMethod( MethodInfo methodInfo, ScriptMethodAttribute attribute )
	{
		Name = attribute.MethodName;
		ReturnTypeProvider = TypeProviders.GetByBackingType( methodInfo.ReturnType )!;

		_isCsMethod = true;
		MethodInfo = methodInfo;
		MethodParameters = methodInfo.GetParameters();

		var parameters = new List<(string, ITypeProvider)>();
		var methodParameters = methodInfo.GetParameters();
		for ( var i = 1; i < methodParameters.Length; i++ )
		{
			var parameter = methodParameters[i];
			parameters.Add( (parameter.Name!, TypeProviders.GetByBackingType( parameter.ParameterType )!) );
		}
		Parameters = parameters;
		
		Signature = MethodSignature.From( this );
	}

	/// <summary>
	/// Invokes the method in the context of the interpreter passed.
	/// </summary>
	/// <param name="interpreter">The interpreter to pass through</param>
	/// <param name="values">The arguments of the method.</param>
	/// <returns>The returned value of the method.</returns>
	public object? Invoke( Interpreter interpreter, object?[] values )
	{
		if ( _isCsMethod )
		{
			var parameters = new object?[values.Length + 1];
			Array.Copy( values, 0, parameters, 1, values.Length );
			
			parameters[0] = interpreter.Owner;
			for ( var i = 0; i < parameters.Length-1; i++ )
			{
				if ( MethodParameters![i + 1].ParameterType == typeof(ScriptValue) )
					parameters[i + 1] = ScriptValue.From( values[i] );
			}

			var result = MethodInfo!.Invoke( null, parameters );
			if ( result is ScriptValue sv )
				return sv.Value;
			
			return result;
		}
		else
		{
			var parameters = new Dictionary<string, object?>();
			for ( var i = 0; i < values.Length; i++ )
				parameters.Add( Parameters[i].Item1, values[i] );
			
			using var scope = interpreter.Variables.Enter( Guid.Empty, parameters );
			var result = interpreter.Visit( MethodDeclarationAst!.BodyAst );

			interpreter.Returning = false;
			return result;
		}
	}

	public bool Equals( ScriptMethod? other )
	{
		if ( ReferenceEquals( null, other ) )
			return false;

		if ( ReferenceEquals( this, other ) )
			return true;

		return ReturnTypeProvider == other.ReturnTypeProvider && Signature.Equals( other.Signature );
	}

	public override bool Equals( object? obj )
	{
		if ( ReferenceEquals( null, obj ) )
			return false;

		if ( ReferenceEquals( this, obj ) )
			return true;

		return obj.GetType() == GetType() && Equals( (ScriptMethod)obj );
	}

	public override int GetHashCode()
	{
		return Signature.GetHashCode();
	}
}
