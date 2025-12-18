/// <summary>
/// A Group of Cards that allows fast loookup
/// </summary>
public sealed class CardIndex
{
	//The Three Dictonarys that are build
	public IReadOnlyDictionary<Guid, Card> ById { get; }
	public IReadOnlyDictionary<Guid, IReadOnlyList<Card>> ByOracleId { get; }
	public IReadOnlyDictionary<string, IReadOnlyList<Card>> ByName { get; }




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

}
