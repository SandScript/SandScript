using System.Text;

namespace SandScript.AbstractSyntaxTrees;

public sealed class ForAst : Ast
{
	public readonly VariableDeclarationAst VariableDeclaration;
	public readonly Ast BooleanExpression;
	public readonly AssignmentAst Iterator;
	public readonly BlockAst Block;

	public ForAst( TokenLocation location, VariableDeclarationAst variableDeclaration, Ast booleanExpression,
		AssignmentAst iterator, BlockAst block ) : base( location )
	{
		VariableDeclaration = variableDeclaration;
		BooleanExpression = booleanExpression;
		Iterator = iterator;
		Block = block;
	}

	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "For @ " );
		sb.Append( StartLocation );
		sb.Append( '\n' );

		var newIndent = indent + '\t';
		sb.Append( VariableDeclaration.Dump( newIndent ) );
		sb.Append( '\n' );

		sb.Append( BooleanExpression.Dump( newIndent ) );
		sb.Append( '\n' );

		sb.Append( Iterator.Dump( newIndent ) );
		sb.Append( '\n' );

		sb.Append( Block.Dump( newIndent ) );
		
		return sb.ToString();
	}
}
