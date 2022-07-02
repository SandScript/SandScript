using SandScript.AbstractSyntaxTrees;

namespace SandScript;

public sealed class CharacterTypeProvider : ILiteralTypeProvider
{
	public override string ToString() => TypeName;
	
	public string TypeName => "Character";
	public string TypeIdentifier => "char";
	
	public Type BackingType => typeof(char);

	public Dictionary<TokenType, Func<object?, object?, object?>> BinaryOperations { get; } = new()
	{
		{TokenType.EqualsEquals, BinEquals},
		{TokenType.BangEquals, BinNotEquals}
	};
	
	public Dictionary<TokenType, Func<object?, object?>> UnaryOperations { get; } = new();

	public bool Compare( object? left, object? right ) => (char)left! == (char)right!;
	
	public object CreateDefault() => default(char);

	public object? GetLiteral( Lexer lexer )
	{
		if ( lexer.CurrentChar != '\'' )
			return null;
		
		var location = new TokenLocation( lexer.Row, lexer.Column );
	    
		lexer.Advance();
		var character = lexer.CurrentChar;
		lexer.Advance();
	    
		if ( lexer.CurrentChar != '\'' )
			lexer.Diagnostics.UnclosedCharacter( location );
		else
			lexer.Advance();

		return character;
	}
	
	public LiteralAst? GetLiteralAst( Token token ) => token.Value is char ? new LiteralAst( token, this ) : null;
	
	private static object? BinEquals( object? left, object? right ) => (char)left! == (char)right!;

	private static object? BinNotEquals( object? left, object? right ) => !(bool)BinEquals( left, right )!;
}
