using System.Text;

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

	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "Assignment @ " );
		sb.Append( StartLocation );
		sb.Append( '\n' );
		
		var newIndent = indent + '\t';
		sb.Append( Variable.Dump( newIndent ) );
		sb.Append( '\n' );
		
		sb.Append( newIndent );
		sb.Append( Operator.Value );
		sb.Append( '\n' );

		sb.Append( Expression.Dump( newIndent ) );

		return sb.ToString();
	}
}
