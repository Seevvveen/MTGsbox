using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Static helper to turn List of Cards into CardIndex
/// </summary>
public static class GlobalCardIndexBuilder
{
	private const string OracleCardsFileName = "oracle_cards.json";
	private const string DefaultCardsFileName = "default_cards.json";

	// Configuration constants for reliability
	private const int MaxRetryAttempts = 3;
	private const double InitialRetryDelaySeconds = 0.5;
	private const int YieldInterval = 15_000; // Yield every n cards processed
	private const int MinimumExpectedCards = 100; // Sanity check for valid data

	private static readonly object _lock = new();

	// Shared build task for the whole process.
	private static Task<IndexBuildResult> _buildTask;

	public sealed class IndexBuildResult
	{
		public LocalCardIndex OracleCards { get; }
		public LocalCardIndex DefaultCards { get; }
		public bool Success { get; }
		public Exception Error { get; }

		public IndexBuildResult( LocalCardIndex oracle, LocalCardIndex @default, bool success, Exception error )
		{
			OracleCards = oracle;
			DefaultCards = @default;
			Success = success;
			Error = error;
		}
	}

	/// <summary>
	/// Ensure that the global indexes are built once per process.
	/// - If a successful build already exists, returns it.
	/// - If a build is in progress, returns that task.
	/// - If the previous build failed, starts a new build.
	/// </summary>
	public static Task<IndexBuildResult> EnsureBuiltAsync()
	{
		lock ( _lock )
		{
			if ( _buildTask != null )
			{
				// If build still running, share the same task.
				if ( !_buildTask.IsCompleted )
				{
					Log.Info( "[GlobalCardIndexBuilder] Build already in progress; sharing existing task." );
					return _buildTask;
				}

				// Completed successfully and marked success => reuse forever.
				if ( _buildTask.IsCompletedSuccessfully && _buildTask.Result.Success )
				{
					Log.Info( "[GlobalCardIndexBuilder] Using cached successful build." );
					return _buildTask;
				}

				// Otherwise (faulted, canceled, or success == false): allow a retry.
				Log.Warning( "[GlobalCardIndexBuilder] Previous build failed; attempting rebuild." );
			}

			_buildTask = BuildInternalAsync();
			return _buildTask;
		}
	}

	/// <summary>
	/// Generic retry helper with exponential backoff for file operations.
	/// </summary>
	private static async Task<T> RetryWithBackoffAsync<T>(
		Func<Task<T>> operation,
		int maxAttempts,
		string operationName
	)
	{
		int attempt = 0;
		while ( true )
		{
			attempt++;
			try
			{
				return await operation();
			}
			catch ( Exception ex ) when ( attempt < maxAttempts )
			{
				// Calculate exponential backoff: 0.5s, 1s, 2s, etc.
				double delaySeconds = InitialRetryDelaySeconds * Math.Pow( 2, attempt - 1 );
				Log.Warning( $"[GlobalCardIndexBuilder] {operationName} attempt {attempt} failed: {ex.Message}. Retrying in {delaySeconds}s..." );

				int delayMs = (int)(delaySeconds * 1000);
				await GameTask.Delay( delayMs );
			}
			catch ( Exception ex )
			{
				// Out of retries
				if ( attempt >= maxAttempts )
				{
					Log.Error( ex, $"[GlobalCardIndexBuilder] {operationName} failed after {maxAttempts} attempts." );
				}
				throw;
			}
		}
	}

	/// <summary>
	/// Load and validate a card list from file with retry logic.
	/// </summary>
	private static async Task<List<Card>> LoadCardListWithRetryAsync( string filename )
	{
		return await RetryWithBackoffAsync(
			async () =>
			{
				// ReadJson is synchronous, but we wrap it in a Task for retry logic
				await GameTask.Yield(); // Yield to keep async scheduler happy

				if ( !FileSystem.Data.FileExists( filename ) )
				{
					throw new FileNotFoundException( $"Card file '{filename}' not found." );
				}

				var fileSize = FileSystem.Data.FileSize( filename );
				Log.Info( $"[GlobalCardIndexBuilder] Loading '{filename}' ({fileSize / 1_000_000.0:F1} MB)..." );

				List<Card> cards = FileSystem.Data.ReadJson<List<Card>>( filename );

				// Validate loaded data
				if ( cards == null )
				{
					throw new InvalidOperationException( $"Deserialized card list from '{filename}' is null." );
				}

				if ( cards.Count < MinimumExpectedCards )
				{
					throw new InvalidOperationException(
						$"Card list from '{filename}' contains only {cards.Count} cards (expected at least {MinimumExpectedCards}). " +
						"File may be corrupted or incomplete."
					);
				}

				Log.Info( $"[GlobalCardIndexBuilder] Loaded {cards.Count:N0} cards from '{filename}'." );
				return cards;
			},
			MaxRetryAttempts,
			$"loading '{filename}'"
		);
	}

	/// <summary>
	/// Builds the indexes on a worker thread, yielding periodically so the
	/// async watchdog doesn't complain about long-running tasks.
	/// </summary>
	private static async Task<IndexBuildResult> BuildInternalAsync()
	{
		// Move this async method onto a worker thread managed by S&box.
		await GameTask.WorkerThread();

		try
		{
			Log.Info( "[GlobalCardIndexBuilder] Starting card index build..." );

			List<Card> oracleList = null;
			List<Card> defaultList = null;

			// Load files with retry logic and validation
			if ( FileSystem.Data.FileExists( OracleCardsFileName ) )
			{
				try
				{
					oracleList = await LoadCardListWithRetryAsync( OracleCardsFileName );
				}
				catch ( Exception ex )
				{
					Log.Error( ex, $"[GlobalCardIndexBuilder] Failed to load '{OracleCardsFileName}' after retries." );
					// Continue - we might still have default_cards
				}
			}
			else
			{
				Log.Warning( $"[GlobalCardIndexBuilder] '{OracleCardsFileName}' not found." );
			}

			if ( FileSystem.Data.FileExists( DefaultCardsFileName ) )
			{
				try
				{
					defaultList = await LoadCardListWithRetryAsync( DefaultCardsFileName );
				}
				catch ( Exception ex )
				{
					Log.Error( ex, $"[GlobalCardIndexBuilder] Failed to load '{DefaultCardsFileName}' after retries." );
					// Continue - we might still have oracle_cards
				}
			}
			else
			{
				Log.Warning( $"[GlobalCardIndexBuilder] '{DefaultCardsFileName}' not found." );
			}

			// Check if we have at least one valid card list
			if ( oracleList == null && defaultList == null )
			{
				var errorMsg = "Both oracle_cards and default_cards are missing or failed to load.";
				Log.Error( $"[GlobalCardIndexBuilder] {errorMsg}" );
				return new IndexBuildResult(
					oracle: null,
					@default: null,
					success: false,
					error: new Exception( errorMsg )
				);
			}

			// Build indexes with cooperative yielding.
			LocalCardIndex oracleIndex = null;
			LocalCardIndex defaultIndex = null;

			if ( oracleList != null )
			{
				Log.Info( $"[GlobalCardIndexBuilder] Building oracle_cards index ({oracleList.Count:N0} cards)..." );
				oracleIndex = await BuildIndexAsync( oracleList, "oracle_cards" );
			}

			if ( defaultList != null )
			{
				Log.Info( $"[GlobalCardIndexBuilder] Building default_cards index ({defaultList.Count:N0} cards)..." );
				defaultIndex = await BuildIndexAsync( defaultList, "default_cards" );
			}

			// Validate that we got at least one index
			if ( oracleIndex == null && defaultIndex == null )
			{
				var errorMsg = "Index build produced null indexes for both oracle and default cards.";
				Log.Error( $"[GlobalCardIndexBuilder] {errorMsg}" );
				return new IndexBuildResult(
					oracle: null,
					@default: null,
					success: false,
					error: new Exception( errorMsg )
				);
			}

			// Log summary
			if ( oracleIndex != null && defaultIndex != null )
			{
				Log.Info( $"[GlobalCardIndexBuilder] Successfully built both indexes (oracle: {oracleIndex.Count:N0}, default: {defaultIndex.Count:N0})." );
			}
			else if ( oracleIndex != null )
			{
				Log.Warning( $"[GlobalCardIndexBuilder] Built oracle_cards index only ({oracleIndex.Count:N0} cards)." );
			}
			else
			{
				Log.Warning( $"[GlobalCardIndexBuilder] Built default_cards index only ({defaultIndex.Count:N0} cards)." );
			}

			return new IndexBuildResult(
				oracle: oracleIndex,
				@default: defaultIndex,
				success: true,
				error: null
			);
		}
		catch ( Exception ex )
		{
			Log.Error( ex, "[GlobalCardIndexBuilder] Unexpected exception during index build." );
			return new IndexBuildResult(
				oracle: null,
				@default: null,
				success: false,
				error: ex
			);
		}
	}

	/// <summary>
	/// Builds a LocalCardIndex from a large list of cards, yielding regularly to
	/// keep the async scheduler happy and avoid the "running without yielding" warning.
	/// </summary>
	private static async Task<LocalCardIndex> BuildIndexAsync( List<Card> cards, string indexName )
	{
		var builder = new LocalCardIndexBuilder();

		int processed = 0;
		int skippedNullCards = 0;
		int lastProgressLog = 0;
		const int ProgressLogInterval = 50_000; // Log every 50k cards

		foreach ( var card in cards )
		{
			if ( card == null )
			{
				skippedNullCards++;
				continue;
			}

			builder.AddCard( card );
			processed++;

			// Yield regularly for async scheduler
			if ( processed % YieldInterval == 0 )
			{
				await GameTask.Yield();
			}

			// Log progress periodically
			if ( processed - lastProgressLog >= ProgressLogInterval )
			{
				Log.Info( $"[GlobalCardIndexBuilder] {indexName}: Processed {processed:N0}/{cards.Count:N0} cards..." );
				lastProgressLog = processed;
			}
		}

		if ( skippedNullCards > 0 )
		{
			Log.Warning( $"[GlobalCardIndexBuilder] {indexName}: Skipped {skippedNullCards:N0} null card entries while building index." );
		}

		var index = builder.Build();

		// Validate the built index
		if ( index == null )
		{
			throw new InvalidOperationException( $"Builder produced null index for '{indexName}'." );
		}

		if ( index.Count == 0 )
		{
			throw new InvalidOperationException( $"Built index for '{indexName}' is empty (no cards indexed)." );
		}

		Log.Info( $"[GlobalCardIndexBuilder] {indexName}: Index built successfully with {index.Count:N0} cards indexed." );
		return index;
	}
}
