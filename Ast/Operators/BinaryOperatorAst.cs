namespace SandScript.AbstractSyntaxTrees;

public sealed class BinaryOperatorAst : Ast
{
	public readonly Ast Left;
	public readonly Token Operator;
	public readonly Ast Right;

	public BinaryOperatorAst( Ast left, Token op, Ast right ) : base( left.StartLocation )
	{
		Left = left;
		Operator = op;
		Right = right;
	}
}
