using System.Text;

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

	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "Binary Operator @ " );
		sb.Append( StartLocation );
		sb.Append( " (" );
		sb.Append( Operator.Value );
		sb.Append( ")\n" );

		var newIndent = indent + '\t';
		sb.Append( Left.Dump( newIndent ) );
		sb.Append( '\n' );
		sb.Append( Right.Dump( newIndent ) );

		return sb.ToString();
	}
}
