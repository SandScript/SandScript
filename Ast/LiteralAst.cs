using System.Text;

namespace SandScript.AbstractSyntaxTrees;

public class LiteralAst : Ast
{
	public readonly Token Token;
	public readonly ITypeProvider TypeProvider;
	
	public object Value => Token.Value;

	public LiteralAst( Token token, ITypeProvider typeProvider ) : base ( token.Location )
	{
		Token = token;
		TypeProvider = typeProvider;
	}

	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "Literal @ " );
		sb.Append( StartLocation );
		sb.Append( '\n' );

		sb.Append( indent + '\t' );
		sb.Append( Value );

		return sb.ToString();
	}
}
