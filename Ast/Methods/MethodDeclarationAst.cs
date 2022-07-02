using System.Text;

namespace SandScript.AbstractSyntaxTrees;

public sealed class MethodDeclarationAst : Ast
{
	public readonly VariableTypeAst ReturnType;
	public readonly VariableAst MethodNameVariable;
	public readonly List<VariableDeclarationAst> Parameters;
	public readonly CompoundStatementAst Compound;

	public readonly string MethodName;

	public MethodDeclarationAst( VariableTypeAst returnType, VariableAst methodNameVariable, List<VariableDeclarationAst> parameters,
		CompoundStatementAst compound ) : base( returnType.Token.Location )
	{
		ReturnType = returnType;
		MethodNameVariable = methodNameVariable;
		Parameters = parameters;
		Compound = compound;

		MethodName = (string)MethodNameVariable.Token.Value;
	}

	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "Method Declaration @ " );
		sb.Append( StartLocation );
		sb.Append( " (" );
		sb.Append( Parameters.Count );
		sb.Append( " parameters)" );
		sb.Append( '\n' );

		var newIndent = indent + '\t';
		sb.Append( ReturnType.Dump( newIndent ) );
		sb.Append( '\n' );

		sb.Append( MethodNameVariable.Dump( newIndent ) );
		sb.Append( '\n' );

		foreach ( var parameter in Parameters )
		{
			sb.Append( parameter.Dump( newIndent ) );
			sb.Append( '\n' );
		}

		sb.Append( Compound.Dump( newIndent ) );
		
		return sb.ToString();
	}
}
