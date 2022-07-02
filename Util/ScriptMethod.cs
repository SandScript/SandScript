using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using SandScript.AbstractSyntaxTrees;

namespace SandScript;

public class ScriptMethod
{
	public readonly string Name;
	public readonly ITypeProvider ReturnTypeProvider;
	public readonly IReadOnlyList<(string, ITypeProvider)> Parameters;
	public readonly MethodSignature Signature;
	
	private readonly bool _isCsMethod;
	
	[MemberNotNullWhen(true, nameof(_isCsMethod))]
	private MethodInfo? MethodInfo { get; }
	[MemberNotNullWhen(true, nameof(_isCsMethod))]
	private ParameterInfo[]? MethodParameters { get; }
	[MemberNotNullWhen(false, nameof(_isCsMethod))]
	private MethodDeclarationAst? MethodDeclarationAst { get; }

	internal ScriptMethod( MethodInfo methodInfo, ScriptMethodAttribute attribute )
	{
		Name = attribute.MethodName;
		ReturnTypeProvider = TypeProviders.GetByType( methodInfo.ReturnType )!;

		_isCsMethod = true;
		MethodInfo = methodInfo;
		MethodParameters = methodInfo.GetParameters();

		var parameters = new List<(string, ITypeProvider)>();
		var methodParameters = methodInfo.GetParameters();
		for ( var i = 1; i < methodParameters.Length; i++ )
		{
			var parameter = methodParameters[i];
			parameters.Add( (parameter.Name!, TypeProviders.GetByType( parameter.ParameterType )!) );
		}
		Parameters = parameters;
		
		Signature = MethodSignature.From( this );
	}

	internal ScriptMethod( MethodDeclarationAst methodDeclarationAst )
	{
		Name = methodDeclarationAst.MethodName;
		ReturnTypeProvider = methodDeclarationAst.ReturnType.TypeProvider;

		_isCsMethod = false;
		MethodDeclarationAst = methodDeclarationAst;

		var parameters = new List<(string, ITypeProvider)>();
		foreach ( var parameter in methodDeclarationAst.Parameters )
			parameters.Add( (parameter.VariableNames.First().VariableName, parameter.VariableType.TypeProvider) );
		Parameters = parameters;
		
		Signature = MethodSignature.From( this );
	}

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
			
			interpreter.Variables.Enter( "Method", parameters );
			var result = interpreter.Visit( MethodDeclarationAst!.Scope );
			interpreter.Variables.Leave();
			return result;
		}
	}
}
