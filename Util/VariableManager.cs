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

	public VariableManager( IEqualityComparer<TKey>? comparer )
	{
		Current = new VariableContainer<TKey, TValue>( Guid.Empty, null, null, comparer );
	}

	public void Enter( Guid guid, IEnumerable<KeyValuePair<TKey, TValue>>? startVariables = null )
	{
		if ( !Current.Children.ContainsKey( guid ) )
		{
			Current = Current.AddChild( guid, startVariables );
			return;
		}
		
		Current = Current.Children[guid];
		Current.Clear();
		if ( startVariables is null )
			return;
			
		foreach ( var pair in startVariables )
			Current.Add( pair );
	}

	public void Leave()
	{
		if ( Current.Parent is not null )
			Current = Current.Parent!;
	}
}
