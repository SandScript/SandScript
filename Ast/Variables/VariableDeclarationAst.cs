using System.Collections.Immutable;
using System.Text;

namespace SandScript.AbstractSyntaxTrees;

public sealed class VariableDeclarationAst : Ast
{
	public readonly VariableTypeAst VariableType;
	public readonly ImmutableArray<VariableAst> VariableNames;
	public readonly Ast DefaultExpression;

	public VariableDeclarationAst( VariableTypeAst variableType, ImmutableArray<VariableAst> variableNames, Ast defaultExpression )
		: base( variableType.Token.Location )
	{
		VariableType = variableType;
		VariableNames = variableNames;
		DefaultExpression = defaultExpression;
	}

	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "Variable Delcaration @ " );
		sb.Append( StartLocation );
		sb.Append( '\n' );

		var newIndent = indent + '\t';
		sb.Append( VariableType.Dump( newIndent ) );
		sb.Append( '\n' );

		foreach ( var variableName in VariableNames )
		{
			sb.Append( variableName.Dump( newIndent ) );
			sb.Append( '\n' );
		}

		sb.Append( DefaultExpression.Dump( newIndent ) );
		
		return sb.ToString();
	}
}
