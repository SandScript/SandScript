﻿namespace SandScript;

public sealed class VariableTypeProvider : ITypeProvider
{
	public override string ToString() => "Any";
	
	public string TypeName => "Variable";
	public string TypeIdentifier => "var";
	
	public Type BackingType => typeof(object);
	
	public Dictionary<TokenType, Func<object?, object?, object?>> BinaryOperations { get; } = new();
	public Dictionary<TokenType, Func<object?, object?>> UnaryOperations { get; } = new();

	public bool Compare( object? left, object? right )
	{
		if ( left is null )
			return right is null;

		return left.Equals( right );
	}

	public object? CreateDefault() => default;
}
