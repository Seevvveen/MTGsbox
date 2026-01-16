#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Diagnostics;

namespace Sandbox._Startup;

/// <summary>
/// Runs once at start-up to ensure the user's local copy of Scryfall symbology is up-to-date.
/// Construct -> RunAsync -> Discard
///
/// Fixed to match your CacheService:
/// - DownloadToFileAsync(uri, filename, maxBytes, token)
/// - Exists(filename)
/// - Size(filename)
/// </summary>
public sealed class ScryfallSymbologySyncJob
{
	private const string SymbologyFilename = "ScryfallSymbology.json";
	private const long MaxFileSizeBytes = 50_000_000; // generous ceiling
	private const string SymbologyUri = "https://api.scryfall.com/symbology";

	private readonly CacheService _cache;
	private readonly Logger _logger = new( "ScryfallSymbologySyncJob" );

	public ScryfallSymbologySyncJob( CacheService cache )
	{
		_cache = cache ?? throw new ArgumentNullException( nameof( cache ) );
	}

	public async Task RunAsync( CancellationToken token = default )
	{
		await EnsureSymbologyAsync( token );
	}

	private async Task EnsureSymbologyAsync( CancellationToken token )
	{
		if ( IsValidExistingFile() )
		{
			_logger.Info( $"Found: {SymbologyFilename}" );
			return;
		}

		_logger.Warning( $"Missing/invalid {SymbologyFilename}, downloading..." );

		try
		{
			await _cache.DownloadToFileAsync(
				uri: SymbologyUri,
				filename: SymbologyFilename,
				maxBytes: MaxFileSizeBytes,
				token: token
			);

			if ( !IsValidExistingFile() )
				throw new InvalidOperationException( $"Downloaded symbology failed validation: {SymbologyFilename}" );

			_logger.Info( $"Downloaded: {SymbologyFilename}" );
		}
		catch ( Exception ex )
		{
			_logger.Error( $"Error downloading {SymbologyFilename} from {SymbologyUri}: {ex}" );
			throw;
		}
	}

	private bool IsValidExistingFile()
	{
		if ( !_cache.Exists( SymbologyFilename ) )
			return false;

		var size = _cache.Size( SymbologyFilename );
		return size > 0 && size <= MaxFileSizeBytes;
	}
}
