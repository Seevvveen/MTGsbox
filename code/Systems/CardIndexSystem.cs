using System.Threading.Tasks;

/// <summary>
/// Assemble a dictonary from a set of cards
/// </summary>
public sealed class CardIndexSystem : GameObjectSystem<CardIndexSystem>, ISceneStartup
{
	// Composition
	private BulkCacheSystem _BulkCacheSystem { get; set; }
	private CardIndexBuilder _CardIndexBuilder { get; set; } = new CardIndexBuilder();

	//Ready Signal
	private readonly TaskCompletionSource<bool> _SignalReady = new();
	public Task WhenReady => _SignalReady.Task;
	public bool IsReady { get; private set; }
	private TaskSource _SignalLife;

	//Card Dictonaries
	public IReadOnlyDictionary<Guid, Card> OracleDictionary { get; set; } =
		new Dictionary<Guid, Card>();
	public IReadOnlyDictionary<Guid, Card> DefaultCardsDictionary { get; set; } =
		new Dictionary<Guid, Card>();

	public CardIndexSystem( Scene scene ) : base( scene )
	{
	}

	/// <summary>
	/// Orchestrate Building Dictonarys
	/// </summary>
	async void ISceneStartup.OnHostInitialize()
	{
		try
		{
			_BulkCacheSystem = BulkCacheSystem.Current;

			Log.Info( "[CardIndexSystem] Waiting for Bulk System" );
			await _BulkCacheSystem.WhenReady;

			if ( !_BulkCacheSystem.IsReady )
			{
				await GameTask.MainThread();
				Log.Error( "[CardIndexSystem] BulkCacheSystem not ready after WhenReady" );
				IsReady = false;
				_SignalReady.TrySetResult( false );
				return;
			}

			// Load card indexes
			if ( FileSystem.Data.FileExists( "oracle_cards.json" ) )
			{
				OracleDictionary = await _CardIndexBuilder.FromLargeFile( "oracle_cards.json" );
				//Log.Info( $"[CardIndexSystem] Loaded {OracleDictionary.Count} oracle cards" );
			}

			if ( FileSystem.Data.FileExists( "default_cards.json" ) )
			{
				DefaultCardsDictionary = await _CardIndexBuilder.FromLargeFile( "default_cards.json" );
				//Log.Info( $"[CardIndexSystem] Loaded {DefaultCardsDictionary.Count} default cards" );
			}

			// Signal ready
			Log.Info( "[CardIndexSystem] Ready" );
			IsReady = true;
			var success = _SignalReady.TrySetResult( true );
		}
		catch ( Exception ex )
		{
			await GameTask.MainThread();
			Log.Error( $"[CardIndexSystem] Failed to initialize: {ex}" );
			IsReady = false;
			_SignalReady.TrySetResult( false );
		}
	}





	public Card? GetCard( Guid id )
	{
		return DefaultCardsDictionary.TryGetValue( id, out var card ) ? card : null;
	}

	public Card? GetCard( string id )
	{
		if ( !Guid.TryParse( id, out var guid ) )
		{
			Log.Warning( $"[CardIndexSystem] Invalid GUID format: {id}" );
			return null;
		}
		return GetCard( guid ); // Reuse the Guid version
	}


	// Safe lookup - returns null if not found
	public Card? GetCardOrNull( Guid id )
	{
		return OracleDictionary.TryGetValue( id, out var card ) ? card : null;
	}

	// Safe lookup with default
	public Card GetCardOrDefault( Guid id, Card defaultCard )
	{
		return OracleDictionary.TryGetValue( id, out var card ) ? card : defaultCard;
	}

	// Check if card exists
	public bool HasCard( Guid id )
	{
		return OracleDictionary.ContainsKey( id );
	}

	// Get multiple cards at once
	public IEnumerable<Card> GetCards( IEnumerable<Guid> ids )
	{
		foreach ( var id in ids )
		{
			if ( OracleDictionary.TryGetValue( id, out var card ) )
				yield return card;
		}
	}

	// Get multiple cards - returns only found cards
	public List<Card> GetCardsAsList( IEnumerable<Guid> ids )
	{
		var result = new List<Card>();
		foreach ( var id in ids )
		{
			if ( OracleDictionary.TryGetValue( id, out var card ) )
				result.Add( card );
		}
		return result;
	}

	// Query cards by predicate
	public IEnumerable<Card> FindCards( Func<Card, bool> predicate )
	{
		return OracleDictionary.Values.Where( predicate );
	}

	// Get all cards
	public IEnumerable<Card> GetAllCards()
	{
		return OracleDictionary.Values;
	}

}
