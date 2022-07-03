namespace SandScript.AbstractSyntaxTrees;

public sealed class AssignmentAst : Ast
{
	public readonly VariableAst Variable;
	public readonly Token Operator;
	public readonly Ast Expression;

	public AssignmentAst( VariableAst variable, Token op, Ast expression ) : base( variable.Token.Location )
	{
		Variable = variable;
		Operator = op;
		Expression = expression;
	}
}
