namespace Sandbox.Catalog;

#nullable enable

/// <summary>
/// unit of publications
/// </summary>
#nullable enable
public sealed class Catalog<TKey, TValue>
	where TKey : notnull
{
	private IReadOnlyDictionary<TKey, TValue>? _byKey;
	private IReadOnlyList<TValue>? _values;
	
	public bool IsReady => _byKey is not null;
	public int Count => _byKey?.Count ?? 0;

	public void Set( IReadOnlyDictionary<TKey, TValue> byKey )
	{
		_byKey = byKey ?? throw new ArgumentNullException( nameof(byKey) );
		_values = byKey.Values as IReadOnlyList<TValue> ?? byKey.Values.ToArray();
	}

	public void Clear()
	{
		_byKey = null;
		_values = null;
	}

	public bool TryGet( TKey key, out TValue value )
	{
		if ( _byKey is null )
		{
			value = default!;
			return false;
		}

		return _byKey.TryGetValue( key, out value! );
	}

	public TValue GetOrThrow( TKey key )
	{
		if ( _byKey is null ) throw new InvalidOperationException( "Catalog not ready" );
		return _byKey[key];
	}

	public TValue GetRandomOrThrow()
	{
		if ( _values is null || _values.Count == 0 )
			throw new InvalidOperationException( "Catalog not ready or empty" );

		var i = Random.Shared.Next( _values.Count );
		return _values[i];
	}
}


