using System.Threading.Tasks;

/// <summary>
/// Scene system that manages card indexes for fast card lookups.
/// Depends on BulkCacheSystem for card data files.
/// Uses GlobalCardIndexBuilder for process-wide index caching.
/// </summary>
public sealed class LocalCardIndexSystem : GameObjectSystem, ISceneStartup
{
	public LocalCardIndex OracleCards { get; private set; }
	public LocalCardIndex DefaultCards { get; private set; }

	private readonly TaskCompletionSource<bool> _readyTcs = new();
	public Task WhenReady => _readyTcs.Task;

	/// <summary>
	/// True only if global indexes were successfully built and assigned.
	/// At least one index (OracleCards or DefaultCards) must be available.
	/// </summary>
	public bool IsReady { get; private set; }

	public LocalCardIndexSystem( Scene scene ) : base( scene )
	{
	}

	async void ISceneStartup.OnHostInitialize()
	{
		try
		{
			Log.Info( "[LocalCardIndexSystem] Starting card index initialization..." );

			// Hard dependency on BulkCacheSystem
			var bulk = Scene.GetSystem<BulkCacheSystem>();
			if ( bulk == null )
			{
				Log.Error( "[LocalCardIndexSystem] BulkCacheSystem is missing from the Scene. Cannot build card indexes." );
				IsReady = false;
				_readyTcs.TrySetResult( false );
				return;
			}

			Log.Info( "[LocalCardIndexSystem] Waiting for BulkCacheSystem to be ready..." );
			await bulk.WhenReady;

			if ( !bulk.IsReady )
			{
				Log.Error( "[LocalCardIndexSystem] BulkCacheSystem failed to initialize; cannot build card indexes." );
				IsReady = false;
				_readyTcs.TrySetResult( false );
				return;
			}

			Log.Info( "[LocalCardIndexSystem] BulkCacheSystem ready. Building card indexes..." );

			// Global one-time index build (per process).
			var result = await GlobalCardIndexBuilder.EnsureBuiltAsync();

			if ( !result.Success )
			{
				var errorMessage = result.Error?.Message ?? "Unknown error";
				Log.Error( $"[LocalCardIndexSystem] GlobalCardIndexBuilder failed: {errorMessage}" );

				if ( result.Error != null )
				{
					Log.Error( result.Error, "[LocalCardIndexSystem] Index build exception details:" );
				}

				IsReady = false;
				_readyTcs.TrySetResult( false );
				return;
			}

			// Assign indexes
			OracleCards = result.OracleCards;
			DefaultCards = result.DefaultCards;

			// Validate we have at least one index
			if ( OracleCards == null && DefaultCards == null )
			{
				Log.Error( "[LocalCardIndexSystem] Global builder returned null indexes for both oracle and default cards." );
				IsReady = false;
				_readyTcs.TrySetResult( false );
				return;
			}

			// Log detailed statistics about what we have
			if ( OracleCards != null && DefaultCards != null )
			{
				Log.Info( "[LocalCardIndexSystem] Both card indexes loaded successfully:" );
				Log.Info( $"  - Oracle Cards: {OracleCards.GetStatistics()}" );
				Log.Info( $"  - Default Cards: {DefaultCards.GetStatistics()}" );
			}
			else if ( OracleCards != null )
			{
				Log.Warning( "[LocalCardIndexSystem] Only Oracle Cards index loaded (Default Cards unavailable)." );
				Log.Info( $"  - Oracle Cards: {OracleCards.GetStatistics()}" );
			}
			else // DefaultCards != null
			{
				Log.Warning( "[LocalCardIndexSystem] Only Default Cards index loaded (Oracle Cards unavailable)." );
				Log.Info( $"  - Default Cards: {DefaultCards.GetStatistics()}" );
			}

			IsReady = true;
			_readyTcs.TrySetResult( true );
			Log.Info( "[LocalCardIndexSystem] Card index system ready." );
		}
		catch ( Exception ex )
		{
			Log.Error( ex, "[LocalCardIndexSystem] Unexpected exception during index initialization." );
			IsReady = false;
			_readyTcs.TrySetResult( false );
		}
	}

	/// <summary>
	/// Get the preferred index for card lookups. 
	/// Returns OracleCards if available, otherwise DefaultCards.
	/// Returns null if neither is available.
	/// </summary>
	public LocalCardIndex GetPreferredIndex()
	{
		return OracleCards ?? DefaultCards;
	}

	/// <summary>
	/// Try to find a card by ID in any available index.
	/// Checks OracleCards first, then DefaultCards.
	/// </summary>
	public bool TryFindCard( Guid id, out Card card )
	{
		card = null;

		if ( !IsReady )
			return false;

		// Try oracle cards first (usually more comprehensive)
		if ( OracleCards != null && OracleCards.TryGetById( id, out card ) )
			return true;

		// Fall back to default cards
		if ( DefaultCards != null && DefaultCards.TryGetById( id, out card ) )
			return true;

		return false;
	}

	/// <summary>
	/// Try to find a card by ID string in any available index.
	/// </summary>
	public bool TryFindCard( string id, out Card card )
	{
		card = null;

		if ( !IsReady )
			return false;

		// Try oracle cards first
		if ( OracleCards != null && OracleCards.TryGetById( id, out card ) )
			return true;

		// Fall back to default cards
		if ( DefaultCards != null && DefaultCards.TryGetById( id, out card ) )
			return true;

		return false;
	}

	/// <summary>
	/// Try to find cards by Oracle ID in any available index.
	/// </summary>
	public bool TryFindByOracleId( Guid oracleId, out IReadOnlyList<Card> cards )
	{
		cards = null;

		if ( !IsReady )
			return false;

		// Try oracle cards first
		if ( OracleCards != null && OracleCards.TryGetByOracleId( oracleId, out cards ) )
			return true;

		// Fall back to default cards
		if ( DefaultCards != null && DefaultCards.TryGetByOracleId( oracleId, out cards ) )
			return true;

		return false;
	}

	/// <summary>
	/// Try to find cards by name in any available index.
	/// </summary>
	public bool TryFindByName( string name, out IReadOnlyList<Card> cards )
	{
		cards = null;

		if ( !IsReady || string.IsNullOrWhiteSpace( name ) )
			return false;

		// Try oracle cards first
		if ( OracleCards != null && OracleCards.TryGetByName( name, out cards ) )
			return true;

		// Fall back to default cards
		if ( DefaultCards != null && DefaultCards.TryGetByName( name, out cards ) )
			return true;

		return false;
	}
}
