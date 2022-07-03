using System.Text;

namespace SandScript;

public class VariableManager<TKey, TValue> where TKey : notnull
{
	public VariableContainer<TKey, TValue> Root
	{
		get
		{
			var root = Current;
			while ( root!.Parent is not null )
				root = Current.Parent;
			
			return root;
		}
	}

	public VariableContainer<TKey, TValue> Current;

	public VariableManager()
	{
	}

	public void Enter( string name, IEnumerable<KeyValuePair<TKey, TValue>>? startVariables ) =>
		Current = new VariableContainer<TKey, TValue>( Current, name, startVariables );

	public void Leave()
	{
		if ( Current.Parent is not null )
			Current = Current.Parent!;
	}

	public override string ToString()
	{
		var sb = new StringBuilder();
		
		sb.Append( "VariableManager\nGlobals:\n" );
		sb.Append( Root );
		sb.Append( "\nCurrent:\n" );
		sb.Append( Current );

		return sb.ToString();
	}
}
