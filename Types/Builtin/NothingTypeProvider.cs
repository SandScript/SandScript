using System;
using System.Collections.Generic;

namespace SandScript;

public sealed class NothingTypeProvider : ITypeProvider
{
	public override string ToString() => TypeName;
	
	public string TypeName => "Nothing";
	public string TypeIdentifier => "void";
	
	public Type BackingType => typeof(void);
	
	public Dictionary<TokenType, Func<object?, object?, object?>> BinaryOperations { get; } = new();
	public Dictionary<TokenType, Func<object?, object?>> UnaryOperations { get; } = new();

	public bool Compare( object? left, object? right ) => left is null && right is null;

	public object? CreateDefault() => null;
}
