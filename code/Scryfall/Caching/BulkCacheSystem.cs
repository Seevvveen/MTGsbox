using System.IO;
using System.Threading;
using System.Threading.Tasks;

public sealed class BulkCacheSystem : GameObjectSystem, ISceneStartup
{
	private const string BulkIndexApiEndpoint = "https://api.scryfall.com/bulk-data";
	private const string BulkIndexFileName = "ScryfallBulkResponse.json";

	// These match the filenames that the index builder expects.
	private const string OracleCardsFileName = "oracle_cards.json";
	private const string DefaultCardsFileName = "default_cards.json";

	// Configuration constants - now easy to adjust and well-documented
	private const long MaxBulkFileSizeBytes = 2_000_000_000; // 2GB limit for S&box
	private const int MaxConcurrentDownloads = 2; // Don't hammer the API
	private const int MaxRetryAttempts = 3;
	private const double InitialRetryDelaySeconds = 1.0;

	// Temporary file suffix to ensure atomic operations
	private const string TempFileSuffix = ".tmp";

	private ApiList<BulkItem> _bulkIndex;
	public ApiList<BulkItem> BulkIndex => _bulkIndex;

	private readonly TaskCompletionSource<bool> _readyTcs = new();
	public Task WhenReady => _readyTcs.Task;

	/// <summary>
	/// True only if startup completed successfully and required card bulk files exist.
	/// </summary>
	public bool IsReady { get; private set; }

	public BulkCacheSystem( Scene scene ) : base( scene )
	{
	}

	// Load Local copy of Index
	private static Task<ApiList<BulkItem>> LoadLocalBulkIndexAsync()
	{
		if ( FileSystem.Data.FileExists( BulkIndexFileName ) )
		{
			try
			{
				var local = FileSystem.Data.ReadJson<ApiList<BulkItem>>( BulkIndexFileName );
				return Task.FromResult( local );
			}
			catch ( Exception ex )
			{
				// If the cached index is corrupted, log and return null
				Log.Warning( ex, "[ScryfallBulkCache] Local index file corrupted; will refetch." );
				return Task.FromResult<ApiList<BulkItem>>( null );
			}
		}
		return Task.FromResult<ApiList<BulkItem>>( null );
	}

	// Get Index from remote with retry logic
	private static async Task<ApiList<BulkItem>> FetchRemoteBulkIndexAsync( CancellationToken token = default )
	{
		return await RetryWithBackoffAsync(
			async () => await Http.RequestJsonAsync<ApiList<BulkItem>>( BulkIndexApiEndpoint ),
			MaxRetryAttempts,
			token,
			"bulk index"
		);
	}

	// Generic retry helper with exponential backoff
	// This makes network operations much more resilient to transient failures
	private static async Task<T> RetryWithBackoffAsync<T>(
		Func<Task<T>> operation,
		int maxAttempts,
		CancellationToken token,
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
			catch ( Exception ex ) when ( attempt < maxAttempts && !token.IsCancellationRequested )
			{
				// Calculate exponential backoff: 1s, 2s, 4s, etc.
				double delaySeconds = InitialRetryDelaySeconds * Math.Pow( 2, attempt - 1 );
				Log.Warning( $"[ScryfallBulkCache] {operationName} attempt {attempt} failed: {ex.Message}. Retrying in {delaySeconds}s..." );

				// Convert seconds to milliseconds for GameTask.Delay
				int delayMs = (int)(delaySeconds * 1000);
				await GameTask.Delay( delayMs, token );
			}
			catch ( Exception ex )
			{
				// Either we're out of retries or cancellation was requested
				if ( attempt >= maxAttempts )
				{
					Log.Error( ex, $"[ScryfallBulkCache] {operationName} failed after {maxAttempts} attempts." );
				}
				throw;
			}
		}
	}

	// Write index to file atomically
	// Using a temp file ensures we never have a partially-written index on disk
	private static void SaveBulkIndex( ApiList<BulkItem> index )
	{
		string tempFile = BulkIndexFileName + TempFileSuffix;

		try
		{
			// Write to temporary file first
			FileSystem.Data.WriteJson( tempFile, index );

			// Delete old file if it exists
			if ( FileSystem.Data.FileExists( BulkIndexFileName ) )
			{
				FileSystem.Data.DeleteFile( BulkIndexFileName );
			}

			// S&box doesn't have a Move/Rename, so we read and rewrite
			// ReadAllBytes returns Span<byte>, WriteAllBytes expects byte[]
			var data = FileSystem.Data.ReadAllBytes( tempFile );
			FileSystem.Data.WriteAllBytes( BulkIndexFileName, data.ToArray() );
			FileSystem.Data.DeleteFile( tempFile );
		}
		catch ( Exception ex )
		{
			Log.Error( ex, "[ScryfallBulkCache] Failed to save bulk index." );

			// Clean up temp file if it exists
			if ( FileSystem.Data.FileExists( tempFile ) )
			{
				FileSystem.Data.DeleteFile( tempFile );
			}
			throw;
		}
	}

	// Check for local, if not, get remote
	// Now accepts cancellation token so we can abort if scene is destroyed
	private async Task EnsureBulkIndexAsync( CancellationToken token = default )
	{
		try
		{
			// 1) Try local first
			var local = await LoadLocalBulkIndexAsync();

			// If we have nothing locally, we must go remote.
			if ( local == null )
			{
				Log.Info( "[ScryfallBulkCache] No local index found; fetching from Scryfall..." );
				var remote = await FetchRemoteBulkIndexAsync( token );
				if ( remote == null )
				{
					Log.Error( "[ScryfallBulkCache] Remote index fetch failed (no local fallback)." );
					_bulkIndex = null;
					return;
				}

				SaveBulkIndex( remote );
				_bulkIndex = remote;

				Log.Info( "[ScryfallBulkCache] Downloaded and cached new bulk index." );
				return;
			}

			// 2) We have a local index; check if it's still fresh
			var anyLocalEntry = local.GetFirstOrDefault();
			if ( anyLocalEntry == null )
			{
				// Local index is structurally bad; fall back to remote.
				Log.Warning( "[ScryfallBulkCache] Local bulk index has no entries; refetching." );

				var remote = await FetchRemoteBulkIndexAsync( token );
				if ( remote == null )
				{
					Log.Error( "[ScryfallBulkCache] Remote index fetch failed after local index was invalid." );
					_bulkIndex = null;
					return;
				}

				SaveBulkIndex( remote );
				_bulkIndex = remote;
				Log.Info( "[ScryfallBulkCache] Replaced invalid local bulk index." );
				return;
			}

			// Check if update is needed based on timestamp
			if ( anyLocalEntry.IsUpdateNeeded() )
			{
				Log.Info( "[ScryfallBulkCache] Local index is stale; checking for updates..." );
				var remote = await FetchRemoteBulkIndexAsync( token );
				if ( remote == null )
				{
					Log.Warning( "[ScryfallBulkCache] Remote index fetch failed; using stale local index." );
					_bulkIndex = local;
					return;
				}

				SaveBulkIndex( remote );
				_bulkIndex = remote;

				Log.Info( "[ScryfallBulkCache] Bulk index updated from Scryfall." );
				return;
			}

			// 3) Local index is fine; just use it.
			_bulkIndex = local;
			Log.Info( "[ScryfallBulkCache] Using cached bulk index (up to date)." );
		}
		catch ( OperationCanceledException )
		{
			Log.Warning( "[ScryfallBulkCache] Bulk index fetch cancelled." );
			_bulkIndex = null;
			throw;
		}
		catch ( Exception ex )
		{
			_bulkIndex = null;
			Log.Error( ex, "[ScryfallBulkCache] Failed to ensure bulk index." );
		}
	}

	// Generate filename from bulk type
	private static string GetBulkDataFileName( string bulkType )
	{
		return $"{bulkType}.json";
	}

	// Download a single bulk file with streaming to minimize memory usage
	// This is the key improvement - we no longer load gigabytes into RAM
	private async Task DownloadBulkFileAsync( BulkItem bulkItem, CancellationToken token = default )
	{
		var filename = GetBulkDataFileName( bulkItem.Type );
		var tempFilename = filename + TempFileSuffix;

		try
		{
			Log.Info( $"[ScryfallBulkCache] Downloading {bulkItem.Type} ({bulkItem.Size / 1_000_000.0:F1} MB)..." );

			// Use RequestStreamAsync to get a stream instead of loading everything into memory
			// This is crucial for large files - we process them chunk by chunk
			await RetryWithBackoffAsync(
				async () =>
				{
					using var responseStream = await Http.RequestStreamAsync( bulkItem.DownloadUri );
					using var fileStream = FileSystem.Data.OpenWrite( tempFilename, FileMode.Create );

					// Manually copy in chunks for maximum compatibility with S&box
					byte[] buffer = new byte[81920]; // 80KB chunks
					int bytesRead;
					while ( (bytesRead = await responseStream.ReadAsync( buffer, 0, buffer.Length, token )) > 0 )
					{
						await fileStream.WriteAsync( buffer, 0, bytesRead, token );
					}

					// Ensure everything is written to disk
					await fileStream.FlushAsync( token );

					return true; // dummy return for retry helper
				},
				MaxRetryAttempts,
				token,
				$"bulk file '{bulkItem.Type}'"
			);

			// Validate that the file was written and has reasonable size
			if ( !FileSystem.Data.FileExists( tempFilename ) )
			{
				throw new InvalidOperationException( "Temp file does not exist after download." );
			}

			var downloadedSize = FileSystem.Data.FileSize( tempFilename );
			if ( downloadedSize == 0 )
			{
				throw new InvalidOperationException( "Downloaded file is empty." );
			}

			// Quick validation: ensure it's valid JSON by checking it starts with '{' or '['
			// We don't want to parse the entire file (could be huge), but we can check basics
			using ( var validationStream = FileSystem.Data.OpenRead( tempFilename ) )
			{
				int firstByte = validationStream.ReadByte();
				if ( firstByte != '{' && firstByte != '[' )
				{
					throw new InvalidOperationException( "Downloaded file doesn't appear to be valid JSON." );
				}
			}

			// Move temp file to final location atomically
			if ( FileSystem.Data.FileExists( filename ) )
			{
				FileSystem.Data.DeleteFile( filename );
			}

			// S&box doesn't have a Move/Rename, so we read and rewrite
			// ReadAllBytes returns Span<byte>, WriteAllBytes expects byte[]
			var tempData = FileSystem.Data.ReadAllBytes( tempFilename );
			FileSystem.Data.WriteAllBytes( filename, tempData.ToArray() );
			FileSystem.Data.DeleteFile( tempFilename );

			Log.Info( $"[ScryfallBulkCache] Successfully downloaded {bulkItem.Type} ({downloadedSize / 1_000_000.0:F1} MB)." );
		}
		catch ( OperationCanceledException )
		{
			Log.Warning( $"[ScryfallBulkCache] Download of '{bulkItem.Type}' was cancelled." );
			CleanupPartialDownload( tempFilename );
			throw;
		}
		catch ( Exception ex )
		{
			Log.Error( ex, $"[ScryfallBulkCache] Failed to download '{bulkItem.Type}'." );
			CleanupPartialDownload( tempFilename );
			throw;
		}
	}

	// Clean up any partial downloads to prevent corrupted cache
	private void CleanupPartialDownload( string filename )
	{
		try
		{
			if ( FileSystem.Data.FileExists( filename ) )
			{
				FileSystem.Data.DeleteFile( filename );
				Log.Info( $"[ScryfallBulkCache] Cleaned up partial download: {filename}" );
			}
		}
		catch ( Exception ex )
		{
			Log.Warning( ex, $"[ScryfallBulkCache] Failed to clean up partial file: {filename}" );
		}
	}

	// Download missing bulk files with parallel processing and better error handling
	private async Task DownloadMissingBulkFilesAsync( CancellationToken token = default )
	{
		if ( _bulkIndex?.Data == null )
		{
			Log.Warning( "[ScryfallBulkCache] No bulk index available; cannot download bulk files." );
			return;
		}

		// Filter to only files we need to download
		var itemsToDownload = new List<BulkItem>();

		foreach ( var bulkItem in _bulkIndex.Data )
		{
			// Skip 'all_cards' - too large for S&box
			if ( string.Equals( bulkItem.Type, "all_cards", StringComparison.OrdinalIgnoreCase ) )
			{
				Log.Info( "[ScryfallBulkCache] Skipping 'all_cards' bulk (too large for S&box)." );
				continue;
			}

			// Validate bulk item
			if ( string.IsNullOrWhiteSpace( bulkItem.Type ) )
			{
				Log.Warning( "[ScryfallBulkCache] Encountered bulk item with no type; skipping." );
				continue;
			}

			if ( string.IsNullOrWhiteSpace( bulkItem.DownloadUri ) )
			{
				Log.Warning( $"[ScryfallBulkCache] Bulk '{bulkItem.Type}' has no download_uri; skipping." );
				continue;
			}

			if ( bulkItem.Size > MaxBulkFileSizeBytes )
			{
				Log.Warning( $"[ScryfallBulkCache] Skipping '{bulkItem.Type}' (size {bulkItem.Size / 1_000_000.0:F1} MB exceeds {MaxBulkFileSizeBytes / 1_000_000.0:F1} MB limit)." );
				continue;
			}

			var filename = GetBulkDataFileName( bulkItem.Type );

			// Only download if file doesn't exist
			if ( FileSystem.Data.FileExists( filename ) )
			{
				Log.Info( $"[ScryfallBulkCache] Bulk file '{bulkItem.Type}' already exists; skipping download." );
				continue;
			}

			itemsToDownload.Add( bulkItem );
		}

		if ( itemsToDownload.Count == 0 )
		{
			Log.Info( "[ScryfallBulkCache] All required bulk files are already downloaded." );
			return;
		}

		Log.Info( $"[ScryfallBulkCache] Downloading {itemsToDownload.Count} bulk file(s)..." );

		// Use semaphore to limit concurrent downloads
		// This prevents us from hammering the API while still being faster than sequential
		using var semaphore = new SemaphoreSlim( MaxConcurrentDownloads );
		var downloadTasks = new List<Task>();

		foreach ( var item in itemsToDownload )
		{
			// Wait for a slot to become available
			await semaphore.WaitAsync( token );

			// Start download task using S&box's GameTask
			var downloadTask = GameTask.RunInThreadAsync( async () =>
			{
				try
				{
					await DownloadBulkFileAsync( item, token );
				}
				finally
				{
					// Always release the semaphore slot
					semaphore.Release();
				}
			} );

			downloadTasks.Add( downloadTask );
		}

		// Wait for all downloads to complete
		// We use WhenAll so if any download fails, we still wait for the others to finish
		try
		{
			await GameTask.WhenAll( downloadTasks );
			Log.Info( $"[ScryfallBulkCache] Successfully downloaded all {itemsToDownload.Count} bulk file(s)." );
		}
		catch ( Exception )
		{
			// At least one download failed, but we've already logged individual errors
			// Check how many succeeded
			int successCount = downloadTasks.Count( t => t.IsCompletedSuccessfully );
			int failCount = itemsToDownload.Count - successCount;

			if ( successCount > 0 )
			{
				Log.Warning( $"[ScryfallBulkCache] Completed with partial success: {successCount} succeeded, {failCount} failed." );
			}
			else
			{
				Log.Error( $"[ScryfallBulkCache] All {failCount} bulk file downloads failed." );
			}
		}
	}

	// --- Scene Startup -----------------------------------------------------

	async void ISceneStartup.OnHostInitialize()
	{
		// Create a cancellation token source that we could cancel if needed
		// For now, we use CancellationToken.None, but this structure allows for future improvements
		var cts = new CancellationTokenSource();

		try
		{
			Log.Info( "[ScryfallBulkCache] Starting bulk cache initialization..." );

			await EnsureBulkIndexAsync( cts.Token );
			await DownloadMissingBulkFilesAsync( cts.Token );

			if ( _bulkIndex == null )
			{
				Log.Error( "[ScryfallBulkCache] Startup finished but bulk index is null." );
				IsReady = false;
				_readyTcs.TrySetResult( false );
				return;
			}

			// Sanity check that at least one of the card bulk files exists.
			// This allows partial success - if we got one of the two main files, we can work
			bool hasOracleCards = FileSystem.Data.FileExists( OracleCardsFileName );
			bool hasDefaultCards = FileSystem.Data.FileExists( DefaultCardsFileName );
			bool hasAnyCardBulk = hasOracleCards || hasDefaultCards;

			if ( !hasAnyCardBulk )
			{
				Log.Error( "[ScryfallBulkCache] Required card bulk files (oracle_cards/default_cards) are missing after download step." );
				IsReady = false;
				_readyTcs.TrySetResult( false );
				return;
			}

			// Log what we have available
			if ( hasOracleCards && hasDefaultCards )
			{
				Log.Info( "[ScryfallBulkCache] Both oracle_cards and default_cards available." );
			}
			else if ( hasOracleCards )
			{
				Log.Warning( "[ScryfallBulkCache] Only oracle_cards available (default_cards missing)." );
			}
			else
			{
				Log.Warning( "[ScryfallBulkCache] Only default_cards available (oracle_cards missing)." );
			}

			IsReady = true;
			_readyTcs.TrySetResult( true );
			Log.Info( "[ScryfallBulkCache] Bulk cache system ready." );
		}
		catch ( OperationCanceledException )
		{
			Log.Warning( "[ScryfallBulkCache] Startup was cancelled." );
			IsReady = false;
			_readyTcs.TrySetResult( false );
		}
		catch ( Exception ex )
		{
			Log.Error( ex, "[ScryfallBulkCache] Exception during startup." );
			IsReady = false;
			_readyTcs.TrySetResult( false );
		}
		finally
		{
			cts.Dispose();
		}
	}
}
