using System.Threading.Tasks;

public sealed class LocalCardIndexSystem : GameObjectSystem, ISceneStartup
{
	private const string OracleCardsFileName = "oracle_cards.json";
	private const string DefaultCardsFileName = "default_cards.json";

	public LocalCardIndex OracleCards { get; private set; }
	public LocalCardIndex DefaultCards { get; private set; }

	// Global cached build tasks – survive hotloads
	private static Task<LocalCardIndex> _oracleIndexTask;
	private static Task<LocalCardIndex> _defaultIndexTask;

	private readonly TaskCompletionSource<bool> _readyTcs = new();
	public Task WhenReady => _readyTcs.Task;

	public bool IsReady =>
		WhenReady.IsCompletedSuccessfully &&
		(OracleCards != null || DefaultCards != null);

	public LocalCardIndexSystem( Scene scene ) : base( scene )
	{
	}

	private async Task<LocalCardIndex> BuildIndexAsync( string filename )
	{
		if ( !FileSystem.Data.FileExists( filename ) )
		{
			Log.Warning( $"[LocalCardIndexSystem] Bulk file '{filename}' not found in FileSystem.Data." );
			return null;
		}

		// Step 1: read + deserialize JSON on a worker thread
		var cards = await GameTask.RunInThreadAsync( () =>
		{
			try
			{
				return FileSystem.Data.ReadJson<List<Card>>( filename );
			}
			catch ( Exception ex )
			{
				Log.Error( ex, $"[LocalCardIndexSystem] Failed to read '{filename}'." );
				return null;
			}
		} );

		if ( cards == null || cards.Count == 0 )
		{
			Log.Warning( $"[LocalCardIndexSystem] Bulk file '{filename}' contained no cards." );
			return null;
		}

		// Step 2: build the index in chunks, yielding between chunks
		return await GameTask.RunInThreadAsync( async () =>
		{
			try
			{
				var builder = new LocalCardIndexBuilder();

				const int chunkSize = 1000;
				for ( int i = 0; i < cards.Count; i += chunkSize )
				{
					int count = Math.Min( chunkSize, cards.Count - i );

					for ( int j = 0; j < count; j++ )
					{
						var card = cards[i + j];
						builder.AddCard( card );
					}

					// Let hotload and other work proceed
					await GameTask.Yield();
				}

				return builder.Build();
			}
			catch ( Exception ex )
			{
				Log.Error( ex, $"[LocalCardIndexSystem] Failed to build index from '{filename}'." );
				return null;
			}
		} );
	}

	async void ISceneStartup.OnHostInitialize()
	{
		try
		{
			// Only kick off builds once per process
			_oracleIndexTask ??= BuildIndexAsync( OracleCardsFileName );
			_defaultIndexTask ??= BuildIndexAsync( DefaultCardsFileName );

			OracleCards = await _oracleIndexTask;
			DefaultCards = await _defaultIndexTask;

			if ( OracleCards == null && DefaultCards == null )
			{
				Log.Warning( "[LocalCardIndexSystem] Failed to build any card indexes." );
			}

			_readyTcs.TrySetResult( true );
		}
		catch ( Exception ex )
		{
			Log.Error( ex, "[LocalCardIndexSystem] Exception during index build." );
			_readyTcs.TrySetResult( false );
		}
	}
}
