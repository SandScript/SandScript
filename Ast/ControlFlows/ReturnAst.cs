namespace SandScript.AbstractSyntaxTrees;

public sealed class ReturnAst : Ast
{
	public readonly Ast Expression;

	public ReturnAst( TokenLocation location, Ast expression ) : base( location )
	{
		Expression = expression;
	}
}
