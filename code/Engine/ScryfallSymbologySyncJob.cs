#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Diagnostics;

namespace Sandbox.Engine.StartUp;

/// <summary>
/// Runs once at start-up to ensure the users local copy of Scryfalls symbology is up-to-date
///
/// Construct -> Run Async -> Discard
/// </summary>
public sealed class ScryfallSymbologySyncJob( CacheService cache )
{
	private const string SymbologyFilename = "ScryfallSymbology.json";
	private const long MaxFileSizeBytes = 50_000_000; // symbology is small; generous ceiling
	private const string SymbologyUri = "https://api.scryfall.com/symbology";

	private readonly Logger _logger = new Logger( "ScryfallSymbologySyncJob" );

	/// <summary>
	/// Main operation: ensure the symbology file exists locally (download if missing).
	/// </summary>
	public async Task RunAsync( CancellationToken token )
	{
		await LoadOrDownloadSymbologyAsync( token );
	}

	/// <summary>
	/// Returns true if the file is available locally after this call.
	/// </summary>
	private async Task<bool> LoadOrDownloadSymbologyAsync( CancellationToken token )
	{
		if ( cache.Exists( SymbologyFilename ) )
		{
			_logger.Info( $"Found: {SymbologyFilename}" );
			return true;
		}

		_logger.Warning( $"Missing {SymbologyFilename}, downloading..." );
		return await DownloadToCacheAsync( SymbologyUri, SymbologyFilename, token );
	}

	private async Task<bool> DownloadToCacheAsync( string uri, string filename, CancellationToken token )
	{
		try
		{
			await cache.DownloadToFileAsync( uri, filename, MaxFileSizeBytes, token );
			_logger.Info( $"Downloaded: {filename}" );
			return true;
		}
		catch ( Exception e )
		{
			_logger.Error( $"Error downloading {filename} from {uri}, {e}" );
			return false;
		}
	}
}
