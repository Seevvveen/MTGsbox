using System.Threading;
using System.Threading.Tasks;
using Sandbox.Diagnostics;
using Sandbox.Engine;
using Sandbox.Scryfall.Types.Responses;

namespace Sandbox.Scryfall;


/// <summary>
/// Runs once at start-up to ensure the users local copy of Scryfalls bulk data is up-to-date
///
/// Constructs -> Runs Async -> Discards
/// </summary>
public sealed class ScryfallBulkSyncJob( CacheService cache )
{
	private const string MetaFilename = "BulkMeta.json";
	private const long MaxFileSizeBytes = 1_000_000_000; // 1 GB
	private const string BulkIndexUri = "https://api.scryfall.com/bulk-data";
	private ScryfallList<ScryfallBulkData> _meta = new();

	private readonly Logger _logger = new Logger("ScryfallBulkSyncJob");


	// Main Operation
	public async Task RunAsync( CancellationToken token )
	{
		_meta = await LoadOrDownloadMetaAsync( token );
		var plan = BuildDownloadPlan( _meta );
		await DownloadPlanAsync(plan, token );
	}

	private async Task<ScryfallList<ScryfallBulkData>> LoadOrDownloadMetaAsync( CancellationToken token )
	{
		if ( cache.Exists( MetaFilename ) )
		{
			_logger.Info($"Found: {MetaFilename}");
			return cache.ReadJson<ScryfallList<ScryfallBulkData>>( MetaFilename );
		}
		
		_logger.Warning( $"Missing {MetaFilename}, downloading..." );
		await DownloadToCacheAsync( BulkIndexUri, MetaFilename, token );
		
		return cache.ReadJson<ScryfallList<ScryfallBulkData>>( MetaFilename );
	}

	/// <summary>
	/// Builds a List of indexes that need downloading
	/// </summary>
	private static IReadOnlyList<(string DownloadUri, string LocalFileName, string Type)> BuildDownloadPlan( ScryfallList<ScryfallBulkData> meta )
	{
		if ( meta?.Data is null || meta.Data.Count == 0 )
			return Array.Empty<(string, string, string)>();

		return meta.Data.Where( item =>
			!string.IsNullOrWhiteSpace( item.Type )        &&
			!string.IsNullOrWhiteSpace( item.DownloadUri ) &&
			item.Size <= MaxFileSizeBytes
		).Select( item =>
		{
			var type = item.Type;
			var uri = item.DownloadUri;
			var file = $"{type}.json";
			return (uri, file, type);
		} ).ToList();
	}
	
	private async Task DownloadPlanAsync( IReadOnlyList<(string DownloadUri, string LocalFileName, string Type)> plan, CancellationToken token )
	{
		foreach ( var (uri, file, type) in plan )
		{
			if (cache.Exists( file ) )
				continue;
			
			_logger.Info( $"Downloading: {type} -> {file}" );
			await DownloadToCacheAsync(uri, file, token );
		}
	}

	private async Task DownloadToCacheAsync( string uri, string filename, CancellationToken token )
	{
		try
		{
			await cache.DownloadToFileAsync( uri, filename, MaxFileSizeBytes,token );
		}
		catch ( Exception e )
		{
			_logger.Error( $"Error downloading {filename} from {uri}, {e}" );
		}
	}
	
	


}
