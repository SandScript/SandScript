namespace SandScript;

public sealed class Token
{
	public TokenType Type { get; }
	public object Value { get; }

	public TokenLocation Location { get; }

	public Token( TokenType type, object value, int row, int column )
		: this( type, value, new TokenLocation( row, column ) )
	{
	}

	public Token( TokenType type, object value, TokenLocation location )
	{
		Type = type;
		Value = value;
		Location = location;
	}

	private bool Equals( Token other ) =>
		Type == other.Type && Value.Equals( other.Value ) && Location.Equals( other.Location );

	public override bool Equals( object? obj ) => ReferenceEquals( this, obj ) || obj is Token other && Equals( other );

	public override int GetHashCode() => HashCode.Combine( (int)Type, Value, Location );

	public override string ToString() => Type.ToString() + ':' + Value + " (" + Location + ')';

	public static bool operator ==( Token left, Token right ) => left.Equals( right );

	public static bool operator !=( Token left, Token right ) => !(left == right);
}
