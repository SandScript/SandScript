using System.Collections.Immutable;
using System.Text;

namespace SandScript.AbstractSyntaxTrees;

public sealed class MethodCallAst : Ast
{
	public readonly Token NameToken;
	public readonly ImmutableArray<Ast> Arguments;
	internal ImmutableArray<ITypeProvider> ArgumentTypes;

	public readonly string MethodName;

	public MethodCallAst( Token nameToken, ImmutableArray<Ast> arguments ) : base( nameToken.Location )
	{
		NameToken = nameToken;
		Arguments = arguments;
		ArgumentTypes = ImmutableArray<ITypeProvider>.Empty;

		MethodName = (string)nameToken.Value;
	}

	internal MethodCallAst( Token nameToken, ImmutableArray<Ast> arguments, ImmutableArray<ITypeProvider> argumentTypes )
		: base( nameToken.Location )
	{
		NameToken = nameToken;
		Arguments = arguments;
		ArgumentTypes = argumentTypes;

		MethodName = (string)nameToken.Value;
	}

	public override string Dump( string indent = "" )
	{
		var sb = new StringBuilder();
		sb.Append( indent );
		sb.Append( "Method Call @ " );
		sb.Append( StartLocation );
		sb.Append( " (" );
		sb.Append( Arguments.Length );
		sb.Append( " arguments)" );
		sb.Append( '\n' );

		var newIndent = indent + '\t';
		sb.Append( newIndent );
		sb.Append( MethodName );
		sb.Append( '\n' );

		foreach ( var argument in Arguments )
		{
			sb.Append( argument.Dump( newIndent ) );
			sb.Append( '\n' );
		}

		return sb.ToString();
	}
}
