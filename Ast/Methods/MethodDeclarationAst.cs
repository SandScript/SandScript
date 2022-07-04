using System.Collections.Immutable;

namespace SandScript.AbstractSyntaxTrees;

public sealed class MethodDeclarationAst : Ast
{
	public readonly VariableTypeAst ReturnType;
	public readonly VariableAst MethodNameVariable;
	public readonly ImmutableArray<ParameterAst> Parameters;
	public readonly BlockAst Scope;

	public readonly string MethodName;

	public MethodDeclarationAst( VariableTypeAst returnType, VariableAst methodNameVariable,
		ImmutableArray<ParameterAst> parameters, BlockAst scope ) : base( returnType.Token.Location )
	{
		ReturnType = returnType;
		MethodNameVariable = methodNameVariable;
		Parameters = parameters;
		Scope = scope;

		MethodName = (string)MethodNameVariable.Token.Value;
	}
}
