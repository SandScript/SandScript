using System.Text;

namespace SandScript.AbstractSyntaxTrees;

public class CommentAst : Ast
{
	public readonly string Contents;
	public readonly bool MultiLine;
	
	public CommentAst( TokenLocation location, string contents, bool multiline ) : base(location)
	{
		Contents = contents;
		MultiLine = multiline;
	}

	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "Comment @ " );
		sb.Append( StartLocation );

		return sb.ToString();
	}
}
