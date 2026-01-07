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

	public bool IsReady => _byKey is not null;
	public int Count => _byKey?.Count ?? 0;

	public void Set( IReadOnlyDictionary<TKey, TValue> byKey )
	{
		_byKey = byKey ?? throw new ArgumentNullException( nameof(byKey) );
	}

	public void Clear()
	{
		_byKey = null;
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
}


