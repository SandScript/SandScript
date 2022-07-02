using System.Text;

namespace SandScript.AbstractSyntaxTrees;

public sealed class ProgramAst : Ast
{
	public CompoundStatementAst Compound { get; }

	public ProgramAst( CompoundStatementAst compound ) : base( new TokenLocation( 0, 0 ) )
	{
		Compound = compound;
	}

	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "Program @ " );
		sb.Append( StartLocation );
		sb.Append( '\n' );
		sb.Append( Compound.Dump( indent + "\t" ) );

		return sb.ToString();
	}
}
