using System.Text;

namespace SandScript.AbstractSyntaxTrees;

public sealed class VariableAst : Ast
{
	public readonly Token Token;

	public readonly string VariableName;

	public VariableAst( Token token ) : base( token.Location )
	{
		Token = token;
		
		VariableName = (string)Token.Value;
	}

	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "Variable @ " );
		sb.Append( StartLocation );
		sb.Append( '\n' );

		var newIndent = indent + '\t';
		sb.Append( newIndent );
		sb.Append( VariableName );

		return sb.ToString();
	}
}
