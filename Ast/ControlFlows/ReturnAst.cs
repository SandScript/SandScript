using System.Text;

namespace SandScript.AbstractSyntaxTrees;

public sealed class ReturnAst : Ast
{
	public readonly Ast Expression;

	public ReturnAst( TokenLocation location, Ast expression ) : base( location )
	{
		Expression = expression;
	}

	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "Return @ " );
		sb.Append( StartLocation );
		sb.Append( '\n' );

		sb.Append( Expression.Dump( indent + '\t' ) );
		
		return sb.ToString();
	}
}
