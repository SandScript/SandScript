using System.Collections.Immutable;
using System.Text;

namespace SandScript.AbstractSyntaxTrees;

public sealed class NestedScopeAst : Ast
{
	public readonly ImmutableArray<Ast> Statements;

	public NestedScopeAst( TokenLocation location, ImmutableArray<Ast> statements ) : base( location )
	{
		Statements = statements;
	}

	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "Compound Statement @ " );
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
