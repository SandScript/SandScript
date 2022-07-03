namespace SandScript.AbstractSyntaxTrees;

public sealed class UnaryOperatorAst : Ast
{
	public readonly Token Operator;
	public readonly Ast Operand;

	public UnaryOperatorAst( Token op, Ast operand ) : base( op.Location )
	{
		Operator = op;
		Operand = operand;
	}
}
