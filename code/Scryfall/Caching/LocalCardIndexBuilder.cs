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
		return new LocalCardIndex( _byId, _byOracleId, _byName );
	}

	/// <summary>
	/// Helper for callers that just have an IEnumerable&lt;Card&gt; and want the dictionaries.
	/// This keeps all indexing logic in one place.
	/// </summary>
	public static (Dictionary<Guid, Card> byId,
				   Dictionary<Guid, List<Card>> byOracleId,
				   Dictionary<string, List<Card>> byName)
		BuildDictionaries( IEnumerable<Card> cards )
	{
		var builder = new LocalCardIndexBuilder();

		foreach ( var card in cards )
		{
			builder.AddCard( card );
		}

		return (builder._byId, builder._byOracleId, builder._byName);
	}
}
