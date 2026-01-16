namespace Sandbox._Startup;

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class Catalog<TKey, TValue> where TKey : notnull
{
	// Single published reference. Do not mutate the dictionary after Set().
	private IReadOnlyDictionary<TKey, TValue>? _byKey;

	public bool IsReady => _byKey is not null;
	public int Count => _byKey?.Count ?? 0;

	public void Set( IReadOnlyDictionary<TKey, TValue> byKey )
	{
		ArgumentNullException.ThrowIfNull( byKey );
		_byKey = byKey;
	}

	public void Clear()
	{
		_byKey = null;
	}

	public bool TryGet( TKey key, out TValue value )
	{
		var dict = _byKey;
		if ( dict is null )
		{
			value = default!;
			return false;
		}

		return dict.TryGetValue( key, out value! );
	}

	public TValue GetOrThrow( TKey key )
	{
		var dict = _byKey ?? throw new InvalidOperationException( "Catalog not ready" );

		if ( !dict.TryGetValue( key, out var value ) )
			throw new KeyNotFoundException( $"Key '{key}' not found in catalog" );

		return value!;
	}

	public TValue GetRandomOrThrow()
	{
		var dict = _byKey;
		if ( dict is null || dict.Count == 0 )
			throw new InvalidOperationException( "Catalog not ready or empty" );

		// Avoid caching Values list; just pick by enumerating once.
		// If you call this a lot, see Option B for caching.
		var idx = Random.Shared.Next( dict.Count );
		return dict.Values.ElementAt( idx );
	}

	public IReadOnlyList<TValue> GetAll()
	{
		var dict = _byKey;
		if ( dict is null || dict.Count == 0 )
			return Array.Empty<TValue>();

		// Materialize once per call; if you want caching, use Option B.
		return dict.Values as IReadOnlyList<TValue> ?? dict.Values.ToArray();
	}

	public IEnumerable<TKey> GetKeys()
	{
		return _byKey?.Keys ?? Array.Empty<TKey>();
	}

	public bool ContainsKey( TKey key )
	{
		return _byKey?.ContainsKey( key ) ?? false;
	}

	public TValue? GetOrDefault( TKey key, TValue? defaultValue = default )
	{
		return TryGet( key, out var value ) ? value : defaultValue;
	}

	public IEnumerable<TValue> Where( Func<TValue, bool> predicate )
	{
		var dict = _byKey;
		return dict is null ? Enumerable.Empty<TValue>() : dict.Values.Where( predicate );
	}
}
