using System.Text;

namespace SandScript.AbstractSyntaxTrees;

public class WhitespaceAst : Ast
{
	public readonly int NumWhitespace;
	
	public WhitespaceAst( TokenLocation startLocation, int numWhitespace ) : base(startLocation)
	{
		NumWhitespace = numWhitespace;
	}

	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "Whitespace @ " );
		sb.Append( StartLocation );

		return sb.ToString();
	}
}
