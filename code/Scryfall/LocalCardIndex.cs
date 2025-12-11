public sealed class LocalCardIndex
{
	public IReadOnlyDictionary<Guid, Card> ById { get; }
	public IReadOnlyDictionary<Guid, List<Card>> ByOracleId { get; }
	public IReadOnlyDictionary<string, List<Card>> ByName { get; }

	public int Count => ById.Count;

	public LocalCardIndex( IEnumerable<Card> cards )
	{
		var byId = new Dictionary<Guid, Card>();
		var byOracleId = new Dictionary<Guid, List<Card>>();
		var byName = new Dictionary<string, List<Card>>( StringComparer.OrdinalIgnoreCase );

		foreach ( var card in cards )
		{
			if ( card.Id != Guid.Empty )
				byId[card.Id] = card;

			if ( card.OracleId is Guid oracleId && oracleId != Guid.Empty )
			{
				if ( !byOracleId.TryGetValue( oracleId, out var list ) )
				{
					list = new List<Card>();
					byOracleId[oracleId] = list;
				}
				list.Add( card );
			}

			if ( !string.IsNullOrEmpty( card.Name ) )
			{
				if ( !byName.TryGetValue( card.Name, out var list ) )
				{
					list = new List<Card>();
					byName[card.Name] = list;
				}
				list.Add( card );
			}
		}

		ById = byId;
		ByOracleId = byOracleId;
		ByName = byName;
	}

	//for when the system has already built these dictionaries
	public LocalCardIndex(
		Dictionary<Guid, Card> byId,
		Dictionary<Guid, List<Card>> byOracleId,
		Dictionary<string, List<Card>> byName )
	{
		ById = byId;
		ByOracleId = byOracleId;
		ByName = byName;
	}

	public bool TryGetById( Guid id, out Card card )
		=> ById.TryGetValue( id, out card );

	public bool TryGetByOracleId( Guid oracleId, out List<Card> cards )
		=> ByOracleId.TryGetValue( oracleId, out cards );

	public bool TryGetByName( string name, out List<Card> cards )
		=> ByName.TryGetValue( name, out cards );

	// Optional string overloads, if you want them
	public bool TryGetById( string id, out Card card )
	{
		card = null;
		if ( !Guid.TryParse( id, out var guid ) ) return false;
		return ById.TryGetValue( guid, out card );
	}

	public bool TryGetByOracleId( string oracleId, out List<Card> cards )
	{
		cards = null;
		if ( !Guid.TryParse( oracleId, out var guid ) ) return false;
		return ByOracleId.TryGetValue( guid, out cards );
	}
}

public sealed class LocalCardIndexBuilder
{
	private readonly Dictionary<Guid, Card> _byId = new();
	private readonly Dictionary<Guid, List<Card>> _byOracleId = new();
	private readonly Dictionary<string, List<Card>> _byName =
		new( StringComparer.OrdinalIgnoreCase );

	public int Count => _byId.Count;

	public void AddCard( Card card )
	{
		if ( card == null )
			return;

		if ( card.Id != Guid.Empty )
			_byId[card.Id] = card;

		if ( card.OracleId is Guid oracleId && oracleId != Guid.Empty )
		{
			if ( !_byOracleId.TryGetValue( oracleId, out var list ) )
			{
				list = new List<Card>();
				_byOracleId[oracleId] = list;
			}
			list.Add( card );
		}

		if ( !string.IsNullOrEmpty( card.Name ) )
		{
			if ( !_byName.TryGetValue( card.Name, out var list ) )
			{
				list = new List<Card>();
				_byName[card.Name] = list;
			}
			list.Add( card );
		}
	}

	public LocalCardIndex Build()
	{
		// You can wrap these in ReadOnlyDictionary if you want stronger immutability
		return new LocalCardIndex(
			_byId,
			_byOracleId,
			_byName
		);
	}
}
