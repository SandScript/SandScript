using System.Text;
using SandScript.Exceptions;

namespace SandScript.AbstractSyntaxTrees;

public sealed class VariableTypeAst : Ast
{
	public readonly Token Token;

	public readonly ITypeProvider TypeProvider;
	
	public VariableTypeAst( Token token ) : base( token.Location )
	{
		Token = token;
		TypeProvider = TypeProviders.GetByIdentifier( (string)token.Value ) ??
		               throw new TypeUnsupportedException( (string)token.Value );
	}

	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "Variable Type @ " );
		sb.Append( StartLocation );
		sb.Append( '\n' );
		
		sb.Append( indent + '\t' );
		sb.Append( TypeProvider );

		return sb.ToString();
	}
}
