using System.Text;

namespace SandScript.AbstractSyntaxTrees;

public sealed class IfAst : Ast
{
	public readonly Ast BooleanExpression;
	public readonly NestedScopeAst TrueBranch;
	public readonly Ast FalseBranch;

	public IfAst( TokenLocation location, Ast booleanExpression, NestedScopeAst trueBranch, Ast falseBranch ) : base( location )
	{
		BooleanExpression = booleanExpression;
		TrueBranch = trueBranch;
		FalseBranch = falseBranch;
	}

	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "If @ " );
		sb.Append( StartLocation );
		sb.Append( '\n' );

		var newIndent = indent + '\t';
		sb.Append( BooleanExpression.Dump( newIndent ) );
		sb.Append( '\n' );

		sb.Append( TrueBranch.Dump( newIndent ) );
		sb.Append( '\n' );

		sb.Append( FalseBranch.Dump( newIndent ) );
		
		return sb.ToString();
	}
}
