using System;
using System.Collections.Generic;
using System.Linq;
using SandScript.AbstractSyntaxTrees;

namespace SandScript;

public class MethodSignature : IEquatable<MethodSignature>
{
	private readonly string _name;
	private readonly IReadOnlyList<ITypeProvider> _types;

	private MethodSignature( string name, IReadOnlyList<ITypeProvider> types )
	{
		_name = name;
		_types = types;
	}
	
	private string StringifyTypes()
	{
		switch ( _types.Count )
		{
			case 0:
				return string.Empty;
			case 1:
				return _types[0].ToString()!;
		}

		var typeSignature = _types[0].ToString();
		for ( var i = 1; i < _types.Count; i++ )
		{
			typeSignature += ',';
			typeSignature += _types[i];
		}

		return typeSignature!;
	}

	public override string ToString() => _name + '(' + StringifyTypes() + ')';

	public bool Equals( MethodSignature? other )
	{
		if ( other is null )
			return false;
		
		if ( _name != other._name )
			return false;

		if ( _types.Count != other._types.Count )
			return false;

		for ( var i = 0; i < _types.Count; i++ )
		{
			if ( _types[i] == other._types[i] )
				continue;

			if ( _types[i] == TypeProviders.Builtin.Variable ||
			     other._types[i] == TypeProviders.Builtin.Variable )
				continue;
			
			return false;
		}

		return true;
	}

	public override bool Equals( object? obj )
	{
		if ( ReferenceEquals( null, obj ) )
			return false;

		if ( ReferenceEquals( this, obj ) )
			return true;

		return obj.GetType() == GetType() && Equals( (MethodSignature)obj );
	}

	public override int GetHashCode() => HashCode.Combine( _name, _types );

	public static MethodSignature From( string methodName, ScriptMethod method ) =>
		new(methodName, method.Parameters.Select( parameter => parameter.Item2 ).ToList());

	public static MethodSignature From( ScriptMethod method ) => From( method.Name, method );

	public static MethodSignature From( MethodDeclarationAst methodDeclaration ) =>
		new(methodDeclaration.MethodName, methodDeclaration.Parameters.Select( parameter => parameter.VariableType.TypeProvider ).ToList());

	public static MethodSignature From( MethodCallAst methodCall ) =>
		new(methodCall.MethodName, methodCall.ArgumentTypes);
}
