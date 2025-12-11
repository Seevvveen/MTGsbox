using System.IO;
using System.Threading;
using System.Threading.Tasks;

public sealed class BulkCacheSystem : GameObjectSystem, ISceneStartup
{
	private const string BulkIndexApiEndpoint = "https://api.scryfall.com/bulk-data";
	private const string BulkIndexFileName = "ScryfallBulkResponse.json"; //explicit

	private ApiList<BulkItem> _bulkIndex;
	public ApiList<BulkItem> BulkIndex => _bulkIndex; //Public

	public BulkCacheSystem( Scene scene ) : base( scene )
	{
	}

	// Load Local copy of Index
	static private Task<ApiList<BulkItem>> LoadLocalBulkIndexAsync()
	{
		if ( FileSystem.Data.FileExists( BulkIndexFileName ) )
		{
			var local = FileSystem.Data.ReadJson<ApiList<BulkItem>>( BulkIndexFileName );
			return Task.FromResult( local );
		}
		return Task.FromResult<ApiList<BulkItem>>( null );
	}

	// Get Index from remote
	static private Task<ApiList<BulkItem>> FetchRemoteBulkIndexAsync()
	{
		return Http.RequestJsonAsync<ApiList<BulkItem>>( BulkIndexApiEndpoint );
	}

	// write index to file
	static private void SaveBulkIndex( ApiList<BulkItem> index )
	{
		FileSystem.Data.WriteJson( BulkIndexFileName, index );
	}

	// Check for local, if not, get remote
	private async Task EnsureBulkIndexAsync()
	{
		try
		{
			// 1) Try local first
			var local = await LoadLocalBulkIndexAsync();

			// If we have nothing locally, we must go remote.
			if ( local == null )
			{
				var remote = await FetchRemoteBulkIndexAsync();
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

				var remote = await FetchRemoteBulkIndexAsync();
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

			// Your BulkItem.IsUpdateNeeded() presumably compares timestamps.
			if ( anyLocalEntry.IsUpdateNeeded() )
			{
				var remote = await FetchRemoteBulkIndexAsync();
				if ( remote == null )
				{
					Log.Error( "[ScryfallBulkCache] Remote index fetch failed; using stale local index." );
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
			Log.Info( "[ScryfallBulkCache] Using cached bulk index." );
		}
		catch ( Exception ex )
		{
			_bulkIndex = null;
			Log.Error( ex, "[ScryfallBulkCache] Failed to ensure bulk index." );
		}
	}

	// generate filename
	private static string GetBulkDataFileName( string bulkType )
	{
		return $"{bulkType}.json";
	}

	// Write Nonexisting 
	private async Task DownloadMissingBulkFilesAsync( CancellationToken token = default )
	{
		if ( _bulkIndex?.Data == null )
		{
			Log.Warning( "[ScryfallBulkCache] No bulk index available; cannot download bulk files." );
			return;
		}

		foreach ( var bulkItem in _bulkIndex.Data )
		{
			try
			{
				// Hard Skip... Fix?
				if ( string.Equals( bulkItem.Type, "all_cards", StringComparison.OrdinalIgnoreCase ) )
				{
					Log.Warning( "[ScryfallBulkCache] Skipping 'all_cards' bulk (too large for S&box)." );
					continue;
				}

				// Null Check
				if ( string.IsNullOrWhiteSpace( bulkItem.Type ) )
				{
					Log.Warning( "[ScryfallBulkCache] Encountered bulk item with no type; skipping." );
					continue;
				}

				var filename = GetBulkDataFileName( bulkItem.Type );

				// Only missing bulk
				if ( FileSystem.Data.FileExists( filename ) )
				{
					continue;
				}

				// Download link exists?
				if ( string.IsNullOrWhiteSpace( bulkItem.DownloadUri ) )
				{
					Log.Warning( $"[ScryfallBulkCache] Bulk '{bulkItem.Type}' has no download_uri; skipping." );
					continue;
				}

				// 2GB Size Guard
				if ( bulkItem.Size > 2_000_000_000 )
				{
					Log.Warning( $"[ScryfallBulkCache] Skipping '{bulkItem.Type}' (size {bulkItem.Size} > 2GB limit)." );
					continue;
				}

				// Steam download with bytes
				byte[] data = await Http.RequestBytesAsync(
					bulkItem.DownloadUri,
					method: "GET",
					content: null,
					headers: null,
					cancellationToken: token
				);

				// Write byte stream 
				using var fileStream = FileSystem.Data.OpenWrite( filename, FileMode.Create );
				await fileStream.WriteAsync( data, 0, data.Length, token );

				Log.Info( $"[ScryfallBulkCache] Downloaded bulk package: {bulkItem.Type} -> {filename}" );
			}
			catch ( OperationCanceledException )
			{
				Log.Warning( "[ScryfallBulkCache] Bulk download cancelled." );
				throw; // rethrow so callers know we bailed
			}
			catch ( Exception ex )
			{
				Log.Error( ex, $"[ScryfallBulkCache] Failed downloading bulk '{bulkItem?.Type}'." );
			}
		}
	}

	// --- Scene Startup -----------------------------------------------------

	async void ISceneStartup.OnHostInitialize()
	{
		// Local Check
		await EnsureBulkIndexAsync();
		// Get Missing
		await DownloadMissingBulkFilesAsync( CancellationToken.None );
	}
}
