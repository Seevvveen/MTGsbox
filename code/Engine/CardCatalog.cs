#nullable enable
using Sandbox.Diagnostics;
using Sandbox.Engine;
using Sandbox.Scryfall.Types.DTOs;

namespace Sandbox.Engine;

/// <summary>
/// Holds completed card indexes for the duration of the game.
/// The catalog does not build; it is populated atomically via Set(result).
/// </summary>
public sealed class CardCatalog
{
	private readonly Logger _logger = new( "CardCatalog" );

	private IReadOnlyDictionary<Guid, ScryfallCard>? _byId;
	private IReadOnlyDictionary<Guid, ScryfallCard>? _byOracleId;
	private IReadOnlyDictionary<string, IReadOnlyList<Guid>>? _byExactName;

	public bool IsReady => _byId is not null;
	public int Count => _byId?.Count ?? 0;

	/// <summary>
	/// Publish a completed build result.
	/// Caller is responsible for sequencing (startup must finish before gameplay uses this).
	/// </summary>
	public void Set( CardIndexBuildJob.BuildResult result, bool allowOverwrite = false )
	{
		if ( result is null ) throw new ArgumentNullException( nameof(result) );

		if ( IsReady && !allowOverwrite )
			throw new InvalidOperationException( "CardCatalog has already been set." );

		_byId = result.ById;
		_byOracleId = result.ByOracleId;
		_byExactName = result.ByExactName;

		_logger.Info(
			$"Catalog ready. Cards={result.ById.Count:n0}, OracleIds={result.ByOracleId.Count:n0}, Names={result.ByExactName.Count:n0}"
		);
	}

	private void EnsureReady()
	{
		if ( !IsReady )
			throw new InvalidOperationException( "CardCatalog is not ready. Startup must call Set(result) first." );
	}

	// -------------------------
	// Lookups: Card Id
	// -------------------------

	public bool TryGetById( Guid id, out ScryfallCard? card )
	{
		if ( _byId is not null && _byId.TryGetValue( id, out card ) )
			return true;

		card = null!;
		return false;
	}

	public ScryfallCard GetById( Guid id )
	{
		EnsureReady();

		if ( !_byId!.TryGetValue( id, out var card ) )
			throw new KeyNotFoundException( $"Card not found by Id: {id}" );

		return card;
	}

	// -------------------------
	// Lookups: Oracle Id
	// -------------------------

	public bool TryGetByOracleId( Guid oracleId, out ScryfallCard? card )
	{
		if ( _byOracleId is not null && _byOracleId.TryGetValue( oracleId, out card ) )
			return true;

		card = null!;
		return false;
	}

	public ScryfallCard GetByOracleId( Guid oracleId )
	{
		EnsureReady();

		if ( !_byOracleId!.TryGetValue( oracleId, out var card ) )
			throw new KeyNotFoundException( $"Card not found by OracleId: {oracleId}" );

		return card;
	}

	// -------------------------
	// Lookups: Exact Name
	// -------------------------

	public bool TryGetIdsByExactName( string name, out IReadOnlyList<Guid>? ids )
	{
		if ( string.IsNullOrWhiteSpace( name ) )
		{
			ids = Array.Empty<Guid>();
			return false;
		}

		if ( _byExactName is not null && _byExactName.TryGetValue( name, out ids ) )
			return true;

		ids = Array.Empty<Guid>();
		return false;
	}

	public IReadOnlyList<Guid> GetIdsByExactName( string name )
	{
		EnsureReady();

		if ( string.IsNullOrWhiteSpace( name ) )
			throw new ArgumentException( "Name is null/empty.", nameof(name) );

		return _byExactName!.TryGetValue( name, out var ids )
			? ids
			: Array.Empty<Guid>();
	}

	/// <summary>Common UI convenience: get the first card with an exact name.</summary>
	public bool TryGetFirstByExactName( string name, out ScryfallCard card )
	{
		card = null!;

		if ( !TryGetIdsByExactName( name, out var ids ) || ids.Count == 0 )
			return false;

		return TryGetById( ids[0], out card );
	}

	/// <summary>Convenience: map exact-name IDs to cards (O(k)).</summary>
	public IEnumerable<ScryfallCard> GetCardsByExactName( string name )
	{
		EnsureReady();

		if ( !_byExactName!.TryGetValue( name, out var ids ) )
			yield break;

		foreach ( var id in ids )
		{
			if ( _byId!.TryGetValue( id, out var card ) )
				yield return card;
		}
	}

	// -------------------------
	// Enumerations (optional)
	// -------------------------

	public IEnumerable<ScryfallCard> AllCards()
	{
		EnsureReady();
		return _byId!.Values;
	}
}
