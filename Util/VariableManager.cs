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

	private readonly IEqualityComparer<TKey>? _comparer;

	public VariableManager( IEqualityComparer<TKey>? comparer )
	{
		_comparer = comparer;
		Current = new VariableContainer<TKey, TValue>( "Root", null, null, _comparer );
	}

	public void Enter( string name, IEnumerable<KeyValuePair<TKey, TValue>>? startVariables = null ) =>
		Current = new VariableContainer<TKey, TValue>( name, Current, startVariables, _comparer );

	public void Leave()
	{
		if ( Current.Parent is not null )
			Current = Current.Parent!;
	}
}
