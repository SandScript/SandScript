namespace SandScript.AbstractSyntaxTrees;

public sealed class DoWhileAst : Ast
{
	public readonly Ast BooleanExpression;
	public readonly BlockAst Block;

	public DoWhileAst( TokenLocation location, Ast booleanExpression, BlockAst block ) : base( location )
	{
		BooleanExpression = booleanExpression;
		Block = block;
	}
}
