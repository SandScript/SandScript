using System.Collections.Immutable;
using System.Text;

namespace SandScript.AbstractSyntaxTrees;

public sealed class ProgramAst : Ast
{
	public ImmutableArray<Ast> Statements { get; }

	public ProgramAst( ImmutableArray<Ast> statements ) : base( new TokenLocation( 0, 0 ) )
	{
		Statements = statements;
	}

	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "Program @ " );
		sb.Append( StartLocation );
		sb.Append( " (" );
		sb.Append( Statements.Length );
		sb.Append( " statements)" );
		sb.Append( '\n' );
		
		var newIndent = indent + '\t';
		for ( var i = 0; i < Statements.Length; i++ )
		{
			sb.Append( indent );
			sb.Append( i + 1 );
			sb.Append( ":\n" );
			sb.Append( Statements[i].Dump( newIndent ) );
			sb.Append( '\n' );
		}

		return sb.ToString();
	}
}
