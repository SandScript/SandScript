using System.Collections.Immutable;

namespace SandScript.AbstractSyntaxTrees;

public sealed class MethodCallAst : Ast
{
	public readonly Token NameToken;
	public readonly ImmutableArray<Ast> Arguments;
	internal ImmutableArray<ITypeProvider> ArgumentTypes;

	public readonly string MethodName;

	public MethodCallAst( Token nameToken, ImmutableArray<Ast> arguments ) : base( nameToken.Location )
	{
		NameToken = nameToken;
		Arguments = arguments;
		ArgumentTypes = ImmutableArray<ITypeProvider>.Empty;

		MethodName = (string)nameToken.Value;
	}

	internal MethodCallAst( Token nameToken, ImmutableArray<Ast> arguments, ImmutableArray<ITypeProvider> argumentTypes )
		: base( nameToken.Location )
	{
		NameToken = nameToken;
		Arguments = arguments;
		ArgumentTypes = argumentTypes;

		MethodName = (string)nameToken.Value;
	}
}
