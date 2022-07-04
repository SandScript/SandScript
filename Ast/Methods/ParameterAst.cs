namespace SandScript.AbstractSyntaxTrees;

public sealed class ParameterAst : Ast
{
	public readonly VariableTypeAst ParameterType;
	public readonly VariableAst ParameterName;
	
	public ParameterAst( VariableTypeAst parameterType, VariableAst variableName )
		: base( parameterType.Token.Location )
	{
		ParameterType = parameterType;
		ParameterName = variableName;
	}
}
