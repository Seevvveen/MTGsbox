using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// Takes a group of Card Objects and compiles them an index for fast lookup
/// </summary>
public sealed class CardIndexBuilder
{
	public async Task<IReadOnlyDictionary<Guid, Card>> FromLargeFile( string file )
	{
		await GameTask.MainThread();

		if ( !FileSystem.Data.FileExists( file ) )
		{
			Log.Warning( $"[CardIndexBuilder] File not found: {file}" );
			return new Dictionary<Guid, Card>();
		}

		var json = await FileSystem.Data.ReadAllTextAsync( file );
		if ( string.IsNullOrWhiteSpace( json ) )
		{
			Log.Warning( $"[CardIndexBuilder] File is empty: {file}" );
			return new Dictionary<Guid, Card>();
		}

		await GameTask.WorkerThread();

		var cards = JsonSerializer.Deserialize<List<Card>>( json );
		if ( cards == null || cards.Count == 0 )
		{
			Log.Warning( $"[CardIndexBuilder] No cards in: {file}" );
			return new Dictionary<Guid, Card>();
		}

		var dict = new Dictionary<Guid, Card>( capacity: cards.Count );

		foreach ( var card in cards )
		{
			if ( card?.Id == null || card.Id == Guid.Empty )
				continue;
			dict[card.Id] = card;
		}

		Log.Info( $"[CardIndexBuilder] Loaded {dict.Count} cards from {file}" );
		return dict;
	}

}
