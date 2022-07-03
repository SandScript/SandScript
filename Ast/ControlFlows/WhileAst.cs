namespace SandScript.AbstractSyntaxTrees;

public sealed class WhileAst : Ast
{
	public readonly Ast BooleanExpression;
	public readonly BlockAst Block;

	public WhileAst( TokenLocation location, Ast booleanExpression, BlockAst block ) : base( location )
	{
		BooleanExpression = booleanExpression;
		Block = block;
	}
}
