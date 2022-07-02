using System.Text;

namespace SandScript;

public class VariableManager<TKey, TValue> where TKey : notnull
{
	public readonly VariableContainer<TKey, TValue> Global;
	public VariableContainer<TKey, TValue> Current;

	public VariableManager()
	{
		Global = new VariableContainer<TKey, TValue>( null, "Global", null );
		Current = Global;
	}

	public void Enter( string name, IEnumerable<KeyValuePair<TKey, TValue>>? startVariables ) =>
		Current = new VariableContainer<TKey, TValue>( Current, name, startVariables );

	public void Leave()
	{
		if ( Current != Global )
			Current = Current.Parent!;
	}

	public override string ToString()
	{
		var sb = new StringBuilder();
		
		sb.Append( "VariableManager\nGlobals:\n" );
		sb.Append( Global );
		sb.Append( "\nCurrent:\n" );
		sb.Append( Current );

		return sb.ToString();
	}
}
