using System;

namespace SandScript;

public sealed class TokenLocation
{
	public static readonly TokenLocation Zero = new(0, 0);
	
	public int Row { get; }
	public int Column { get; }

	public TokenLocation( int row, int column )
	{
		Row = row;
		Column = column;
	}

	public override string ToString() => Row + ":" + Column;
	
	private bool Equals(TokenLocation other) => Row == other.Row && Column == other.Column;
	public override bool Equals(object? obj) => obj is TokenLocation other && Equals(other);
	public override int GetHashCode() => HashCode.Combine(Row, Column);

	public static bool operator ==( TokenLocation left, TokenLocation right ) =>
		left.Row == right.Row && left.Column == right.Column;
	public static bool operator !=( TokenLocation left, TokenLocation right ) => !(left == right);
}
