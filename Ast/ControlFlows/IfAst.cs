namespace SandScript.AbstractSyntaxTrees;

public sealed class IfAst : Ast
{
	public readonly Ast BooleanExpression;
	public readonly BlockAst TrueBranch;
	public readonly Ast FalseBranch;

	public IfAst( TokenLocation location, Ast booleanExpression, BlockAst trueBranch, Ast falseBranch ) : base( location )
	{
		BooleanExpression = booleanExpression;
		TrueBranch = trueBranch;
		FalseBranch = falseBranch;
	}
}
