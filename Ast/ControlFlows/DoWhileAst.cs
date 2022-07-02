using System.Text;

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

	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "Do While @ " );
		sb.Append( StartLocation );
		sb.Append( '\n' );

		var newIndent = indent + '\t';
		sb.Append( BooleanExpression.Dump( newIndent ) );
		sb.Append( '\n' );

		sb.Append( Block.Dump( newIndent ) );
		
		return sb.ToString();
	}
}
