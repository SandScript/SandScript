using System.Text;

namespace SandScript.AbstractSyntaxTrees;

public sealed class DoWhileAst : Ast
{
	public readonly Ast BooleanExpression;
	public readonly NestedScopeAst Compound;

	public DoWhileAst( TokenLocation location, Ast booleanExpression, NestedScopeAst compound ) : base( location )
	{
		BooleanExpression = booleanExpression;
		Compound = compound;
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

		sb.Append( Compound.Dump( newIndent ) );
		
		return sb.ToString();
	}
}
