namespace SandScript.AbstractSyntaxTrees;

public sealed class VariableAst : Ast
{
	public readonly Token Token;

	public readonly string VariableName;

	public VariableAst( Token token ) : base( token.Location )
	{
		Token = token;
		
		VariableName = (string)Token.Value;
	}
}
