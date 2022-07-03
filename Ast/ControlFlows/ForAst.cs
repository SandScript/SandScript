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
}
