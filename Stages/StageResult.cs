namespace SandScript;

public sealed class StageResult
{
	public StageResultType Type { get; }
	public object?[] Results { get; }
	
	private StageResult( StageResultType type, object?[] results )
	{
		Type = type;
		Results = results;
	}

	public static StageResult Success( params object?[] results )
	{
		return new StageResult(StageResultType.Success, results);
	}

	public static StageResult NeedsRepeating( params object?[] results )
	{
		return new StageResult(StageResultType.NeedsRepeating, results);
	}

	public static StageResult Fail( params object?[] results )
	{
		return new StageResult(StageResultType.Failed, results);
	}
}
