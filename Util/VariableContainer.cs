using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SandScript;

public class VariableContainer<TKey, TValue> where TKey : notnull
{
	public VariableContainer<TKey, TValue>? Parent { get; }
	public string Name { get; }

	public IReadOnlyDictionary<TKey, TValue> Variables => _variables;
	private readonly Dictionary<TKey, TValue> _variables;

	public VariableContainer( VariableContainer<TKey, TValue>? parent, string name,
		IEnumerable<KeyValuePair<TKey, TValue>>? startVariables )
	{
		Parent = parent;
		Name = name;

		_variables = startVariables is not null
			? new Dictionary<TKey, TValue>( startVariables )
			: new Dictionary<TKey, TValue>();
	}
	
	public void Set( TKey key, TValue value )
	{
		if ( ContainsKey( key ) )
			_variables[key] = value;
		else
			_variables.Add( key, value );
	}

	public TValue? Get( TKey key, out VariableContainer<TKey, TValue>? container )
	{
		if ( TryGetValue( key, out var value ) )
		{
			container = this;
			return value;
		}

		container = null;
		return Parent is not null ? Parent.Get( key, out container ) : default;
	}

	public bool TryGet( TKey key, [NotNullWhen(true)] out TValue? value,
		[NotNullWhen(true)] out VariableContainer<TKey, TValue>? container )
	{
		if ( TryGetValue( key, out var val ) )
		{
			container = this;
			value = val;
			return true;
		}

		container = null;
		if ( Parent is not null )
			return Parent.TryGet( key, out value, out container );

		value = default;
		return false;
	}

	private bool TryGetValue( TKey key, [MaybeNullWhen(true)] out TValue value )
	{
		if ( _variables.TryGetValue( key, out value ) )
			return true;

		foreach ( var pair in _variables )
		{
			if ( !key.Equals( pair.Key ) )
				continue;

			value = pair.Value;
			return true;
		}

		value = default;
		return false;
	}

	private bool ContainsKey( TKey key )
	{
		if ( _variables.ContainsKey( key ) )
			return true;

		foreach ( var pair in _variables )
		{
			if ( key.Equals( pair.Key ) )
				return true;
		}

		return false;
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
