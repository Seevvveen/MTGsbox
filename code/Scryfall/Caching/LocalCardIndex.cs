using System.Collections.ObjectModel;

/// <summary>
/// Immutable index providing fast lookups for Magic: The Gathering cards by ID, Oracle ID, and name.
/// Thread-safe for concurrent reads.
/// </summary>
public sealed class LocalCardIndex
{
	public IReadOnlyDictionary<Guid, Card> ById { get; }
	public IReadOnlyDictionary<Guid, IReadOnlyList<Card>> ByOracleId { get; }
	public IReadOnlyDictionary<string, IReadOnlyList<Card>> ByName { get; }

	/// <summary>
	/// Total number of unique cards indexed by ID.
	/// </summary>
	public int Count => ById.Count;

	/// <summary>
	/// Main constructor: takes already-built dictionaries.
	/// Lists are wrapped in ReadOnlyCollection for immutability.
	/// </summary>
	public LocalCardIndex(
		IDictionary<Guid, Card> byId,
		IDictionary<Guid, List<Card>> byOracleId,
		IDictionary<string, List<Card>> byName )
	{
		// Validate inputs
		if ( byId == null ) throw new ArgumentNullException( nameof( byId ) );
		if ( byOracleId == null ) throw new ArgumentNullException( nameof( byOracleId ) );
		if ( byName == null ) throw new ArgumentNullException( nameof( byName ) );

		// Wrap dictionaries for immutability
		ById = new ReadOnlyDictionary<Guid, Card>( byId );

		// Wrap List<Card> values in ReadOnlyCollection for true immutability
		// This prevents callers from modifying the lists
		var readOnlyByOracleId = new Dictionary<Guid, IReadOnlyList<Card>>();
		foreach ( var kvp in byOracleId )
		{
			readOnlyByOracleId[kvp.Key] = new ReadOnlyCollection<Card>( kvp.Value );
		}
		ByOracleId = new ReadOnlyDictionary<Guid, IReadOnlyList<Card>>( readOnlyByOracleId );

		var readOnlyByName = new Dictionary<string, IReadOnlyList<Card>>();
		foreach ( var kvp in byName )
		{
			readOnlyByName[kvp.Key] = new ReadOnlyCollection<Card>( kvp.Value );
		}
		ByName = new ReadOnlyDictionary<string, IReadOnlyList<Card>>( readOnlyByName );
	}

	/// <summary>
	/// Convenience constructor: build from cards using the builder.
	/// </summary>
	public LocalCardIndex( IEnumerable<Card> cards )
		: this( LocalCardIndexBuilder.BuildDictionaries( cards ) )
	{
	}

	/// <summary>
	/// Helper to adapt the tuple from LocalCardIndexBuilder.
	/// </summary>
	private LocalCardIndex(
		(IDictionary<Guid, Card> byId,
		 IDictionary<Guid, List<Card>> byOracleId,
		 IDictionary<string, List<Card>> byName) dicts )
		: this( dicts.byId, dicts.byOracleId, dicts.byName )
	{
	}

	/// <summary>
	/// Look up a card by its unique ID.
	/// </summary>
	public bool TryGetById( Guid id, out Card card )
		=> ById.TryGetValue( id, out card );

	/// <summary>
	/// Look up all printings of a card by its Oracle ID.
	/// Returns a read-only list to prevent modification.
	/// </summary>
	public bool TryGetByOracleId( Guid oracleId, out IReadOnlyList<Card> cards )
		=> ByOracleId.TryGetValue( oracleId, out cards );

	/// <summary>
	/// Look up all cards with a given name (handles multi-faced cards, reprints, etc.).
	/// Returns a read-only list to prevent modification.
	/// </summary>
	public bool TryGetByName( string name, out IReadOnlyList<Card> cards )
		=> ByName.TryGetValue( name, out cards );

	/// <summary>
	/// Look up a card by its ID as a string. Returns false if the string is not a valid GUID.
	/// </summary>
	public bool TryGetById( string id, out Card card )
	{
		card = null;

		// Validate input
		if ( string.IsNullOrWhiteSpace( id ) )
			return false;

		if ( !Guid.TryParse( id, out var guid ) )
			return false;

		return ById.TryGetValue( guid, out card );
	}

	/// <summary>
	/// Look up all printings by Oracle ID as a string. Returns false if the string is not a valid GUID.
	/// </summary>
	public bool TryGetByOracleId( string oracleId, out IReadOnlyList<Card> cards )
	{
		cards = null;

		// Validate input
		if ( string.IsNullOrWhiteSpace( oracleId ) )
			return false;

		if ( !Guid.TryParse( oracleId, out var guid ) )
			return false;

		return ByOracleId.TryGetValue( guid, out cards );
	}

	/// <summary>
	/// Get statistics about the index for debugging/logging.
	/// </summary>
	public string GetStatistics()
	{
		int totalPrintings = ByOracleId.Values.Sum( list => list.Count );
		int uniqueNames = ByName.Count;
		int uniqueOracleIds = ByOracleId.Count;

		return $"Index Stats: {Count:N0} unique cards, {uniqueOracleIds:N0} oracle IDs, {uniqueNames:N0} unique names, {totalPrintings:N0} total printings";
	}
}
