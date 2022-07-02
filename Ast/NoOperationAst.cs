using System.Text;

namespace SandScript.AbstractSyntaxTrees;

public sealed class NoOperationAst : Ast
{
	public NoOperationAst( TokenLocation location ) : base( location ) { }
	
	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "NoOp @ " );
		sb.Append( StartLocation );
		
		return sb.ToString();
	}
}
