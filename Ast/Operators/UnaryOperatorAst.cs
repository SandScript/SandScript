using System.Text;

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

	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "Unary Operator @ " );
		sb.Append( StartLocation );
		sb.Append( " (" );
		sb.Append( Operator.Value );
		sb.Append( ")\n" );

		sb.Append( Operand.Dump( indent + '\t' ) );

		return sb.ToString();
	}
}
