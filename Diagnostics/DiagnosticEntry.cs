namespace SandScript;

public class DiagnosticEntry
{
	public readonly DiagnosticLevel Level;
	public readonly string Message;
	public readonly TokenLocation Location;

	private readonly string _text;

	public DiagnosticEntry( DiagnosticLevel level, string message, TokenLocation location )
	{
		Level = level;
		Message = message;
		Location = location;

		_text = Message;
		if ( location != TokenLocation.Zero )
			_text += " at " + location;
	}

	public override string ToString() => _text;
}
