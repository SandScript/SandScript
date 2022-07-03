using System.Collections.Immutable;

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
}
