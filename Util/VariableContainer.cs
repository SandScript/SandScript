using System.Collections;
using System.Text;

namespace SandScript;

public class VariableContainer<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
{
	public VariableContainer<TKey, TValue>? Parent { get; }
	public string Name { get; }

	public TValue this[ TKey key ]
	{
		get => _variables[key];
		set => _variables[key] = value;
	}

	public ICollection<TKey> Keys => _variables.Keys;
	public ICollection<TValue> Values => _variables.Values;
	
	public int Count => _variables.Count;
	public bool IsReadOnly => false;
	
	private readonly Dictionary<TKey, TValue> _variables;

	public VariableContainer( string name, VariableContainer<TKey, TValue>? parent,
		IEnumerable<KeyValuePair<TKey, TValue>>? startVariables, IEqualityComparer<TKey>? comparer )
	{
		Parent = parent;
		Name = name;

		_variables = startVariables is not null
			? new Dictionary<TKey, TValue>( startVariables, comparer )
			: new Dictionary<TKey, TValue>( comparer );
	}
	
	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _variables.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	
	public void Add( TKey key, TValue value ) => _variables.Add( key, value );

	public void Add( KeyValuePair<TKey, TValue> item ) => Add( item.Key, item.Value );

	public bool Remove( TKey key ) => _variables.Remove( key );
	
	public bool Remove( KeyValuePair<TKey, TValue> item ) => _variables.Remove( item.Key );
	
	public void Clear() => _variables.Clear();

	public void CopyTo( KeyValuePair<TKey, TValue>[] array, int arrayIndex ) => throw new NotImplementedException();

	public bool Contains( KeyValuePair<TKey, TValue> item ) =>
		TryGetValue( item.Key, out var value ) && value!.Equals( item.Value );

	public bool ContainsKey( TKey key ) => ContainsKey( key, true, out _ );

	public bool ContainsKey( TKey key, bool recursive ) => ContainsKey( key, recursive, out _ );

	public bool ContainsKey( TKey key, bool recursive, out VariableContainer<TKey, TValue> container )
	{
		if ( _variables.ContainsKey( key ) )
		{
			container = this;
			return true;
		}

		if ( recursive && Parent is not null )
			return Parent.ContainsKey( key, recursive, out container );

		container = default;
		return false;
	}

	public void AddOrUpdate( TKey key, TValue value )
	{
		if ( ContainsKey( key, true, out var container ) )
			container[key] = value;

		Add( key, value );
	}

	public bool TryGetValue( TKey key, out TValue value ) => TryGetValue( key, out value, out _ );

	public bool TryGetValue( TKey key, out TValue value, out VariableContainer<TKey, TValue> container )
	{
		if ( !ContainsKey( key, true, out container ) )
		{
			value = default;
			return false;
		}

		value = container[key];
		return true;
	}

	public override string ToString()
	{
		var sb = new StringBuilder();
		
		sb.Append( "VariableContainer \"" + Name + "\"" );
		foreach ( var (key, value) in _variables )
			sb.Append( "\n" + key + ": " + value );

		return sb.ToString();
	}
}
