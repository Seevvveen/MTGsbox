using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sandbox.__Rewrite;

public class DiskDataSystem : GameObjectSystem, ISceneStartup
{
    // -------------------------
    // Constants
    // -------------------------
    private const string ScryfallBulkDataUrl = "https://api.scryfall.com/bulk-data";
    private const string OracleCardsType     = "oracle_cards";
    private const string BulkIndexPath       = "scryfall/bulk-data.json";
    private const string OracleJsonPath      = "scryfall/oracle_cards.json";
    private const string OracleMetaPath      = "scryfall/oracle_cards.meta.json";

    private static readonly TimeSpan MinRedownloadInterval = TimeSpan.FromHours( 24 );
    private static readonly System.Threading.SemaphoreSlim MetaLock = new( 1, 1 );

    // -------------------------
    // Startup
    // -------------------------
    public DiskDataSystem( Scene scene ) : base( scene ) {}

    void ISceneStartup.OnClientInitialize()
    {
        _ = RunStartupSafe();
    }

    private async Task RunStartupSafe()
    {
        try
        {
            await OnClientInitializeAsync();
        }
        catch ( Exception e )
        {
            Log.Error( $"DiskDataSystem startup failed: {e}" );
        }
    }

    private async Task OnClientInitializeAsync()
    {
        // 1) Always refresh the bulk-data index from Scryfall
        if ( !await DownloadIndexAsync() )
            return;

        // 2) Parse index and find the oracle_cards entry
        var oracle = TryReadBulkItemFromIndex( BulkIndexPath, OracleCardsType );
        if ( oracle == null )
        {
            Log.Error( "bulk-data.json is missing the oracle_cards entry." );
            return;
        }

        // 3) Download oracle if needed
        if ( ShouldDownloadOracle( oracle ) )
        {
            Log.Info( $"Downloading oracle cards… updated_at={oracle.UpdatedAt}, size={oracle.Size} bytes" );

            bool isGzip = string.Equals( oracle.ContentEncoding, "gzip", StringComparison.OrdinalIgnoreCase );
            if ( !await DownloadBulkToFileAsync( oracle.DownloadUri, OracleJsonPath, decompressGzip: isGzip ) )
                return;

            await WriteOracleMetaAsync( oracle.UpdatedAt );

            Log.Info( $"Oracle cards saved: {FileSystem.Data.GetFullPath( FileSystem.NormalizeFilename( OracleJsonPath ) )}" );
            Log.Info( $"Size: {FileSystem.Data.FileSize( FileSystem.NormalizeFilename( OracleJsonPath ) )} bytes" );
        }
        else
        {
            Log.Info( $"Oracle cards are up-to-date (index updated_at={oracle.UpdatedAt})." );
        }

        // 4) Build/refresh gameplay blob (idempotent)
        await CardDataProcessor.ProcessAsync( oracle.UpdatedAt );

        // 5) Load blob into runtime lookup database
        if ( !CardDatabase.TryLoadFromDisk() )
            Log.Error( "CardDatabase failed to load gameplay blob." );
    }


    // -------------------------
    // Index download
    // -------------------------
    private async Task<bool> DownloadIndexAsync()
    {
        Log.Info( "Fetching Scryfall bulk data index…" );

        if ( !await DownloadBulkToFileAsync( ScryfallBulkDataUrl, BulkIndexPath, decompressGzip: false ) )
            return false;

        Log.Info( $"Index saved: {FileSystem.Data.GetFullPath( FileSystem.NormalizeFilename( BulkIndexPath ) )}" );
        Log.Info( $"Index size: {FileSystem.Data.FileSize( FileSystem.NormalizeFilename( BulkIndexPath ) )} bytes" );
        return true;
    }

    // -------------------------
    // Download decision logic
    // -------------------------
    private bool ShouldDownloadOracle( ScryfallBulkEntry oracle )
    {
        if ( !OracleDataExistsLocally() )           return true;
        if ( !TryGetLocalMeta( out var meta ) )      return true;
        if ( !IsRemoteNewer( oracle, meta ) )        return false;
        if ( IsWithinRateLimitWindow( meta ) )       return false;
        return true;
    }

    private static bool OracleDataExistsLocally()
        => FileSystem.Data.FileExists( FileSystem.NormalizeFilename( OracleJsonPath ) );

    private static bool TryGetLocalMeta( out OracleMeta meta )
    {
        meta = ReadOracleMeta();
        return meta != null && !string.IsNullOrWhiteSpace( meta.LastUpdatedAt );
    }

    private static bool IsRemoteNewer( ScryfallBulkEntry oracle, OracleMeta meta )
    {
        if ( !TryParseIso8601Utc( oracle.UpdatedAt, out var remoteUpdated ) )
            return true; // can't compare safely, assume we need to re-download

        if ( !TryParseIso8601Utc( meta.LastUpdatedAt, out var localUpdated ) )
            return true;

        return remoteUpdated > localUpdated;
    }

    private static bool IsWithinRateLimitWindow( OracleMeta meta )
    {
        if ( !TryParseIso8601Utc( meta.LastDownloadedAt, out var lastDownload ) )
            return false;

        var timeSinceLastDownload = DateTimeOffset.UtcNow - lastDownload;
        if ( timeSinceLastDownload < MinRedownloadInterval )
        {
            Log.Info( $"Oracle is newer remotely but last download was {timeSinceLastDownload.TotalMinutes:0} min ago; skipping due to MinRedownloadInterval." );
            return true;
        }

        return false;
    }

    // -------------------------
    // Bulk download: fetch -> tmp -> final swap
    // -------------------------
    private async Task<bool> DownloadBulkToFileAsync( string url, string destPath, bool decompressGzip )
    {
        string normalizedDest = FileSystem.NormalizeFilename( destPath );
        string tmpPath        = normalizedDest + ".tmp";

        if ( !await FetchToTempFileAsync( url, tmpPath, decompressGzip ) )
            return false;

        return await PromoteTempFileAsync( tmpPath, normalizedDest );
    }

    private static async Task<bool> FetchToTempFileAsync( string url, string tmpPath, bool decompressGzip )
    {
        var headers = new Dictionary<string, string> { { "Accept", "application/json" } };

        HttpResponseMessage response = null;
        Stream netStream             = null;
        Stream decodeStream          = null;
        Stream outStream             = null;

        try
        {
            response = await Sandbox.Http.RequestAsync( url, "GET", null, headers );

            if ( response == null || !response.IsSuccessStatusCode )
                throw new HttpRequestException( $"HTTP {(int?)response?.StatusCode} {response?.ReasonPhrase}" );

            netStream    = await response.Content.ReadAsStreamAsync();
            decodeStream = ( decompressGzip && ResponseIsGzipped( response, url ) )
                ? new GZipStream( netStream, CompressionMode.Decompress, leaveOpen: true )
                : netStream;

            outStream = FileSystem.Data.OpenWrite( tmpPath, FileMode.Create );
            await decodeStream.CopyToAsync( outStream, bufferSize: 1024 * 1024 );
            await outStream.FlushAsync();
            return true;
        }
        catch ( Exception e )
        {
            Log.Error( $"Download failed [{url}]: {e}" );
            TryDeleteFile( tmpPath );
            return false;
        }
        finally
        {
            TryDispose( outStream );
            // Only dispose decodeStream separately when it's a wrapper around netStream
            if ( decodeStream != null && !ReferenceEquals( decodeStream, netStream ) )
                TryDispose( decodeStream );
            TryDispose( netStream );
            TryDispose( response );
        }
    }

    private static async Task<bool> PromoteTempFileAsync( string tmpPath, string destPath )
    {
        try
        {
            if ( FileSystem.Data.FileExists( destPath ) )
                FileSystem.Data.DeleteFile( destPath );

            await StreamCopyFileAsync( FileSystem.Data, tmpPath, destPath );
            FileSystem.Data.DeleteFile( tmpPath );
            return true;
        }
        catch ( Exception e )
        {
            Log.Error( $"Failed to promote temp file [{tmpPath} -> {destPath}]: {e}" );
            TryDeleteFile( tmpPath );
            return false;
        }
    }

    private static async Task StreamCopyFileAsync( BaseFileSystem fs, string src, string dst )
    {
        Stream input  = null;
        Stream output = null;

        try
        {
            input  = fs.OpenRead( src );
            output = fs.OpenWrite( dst, FileMode.Create );
            await input.CopyToAsync( output, bufferSize: 1024 * 1024 );
            await output.FlushAsync();
        }
        finally
        {
            TryDispose( output );
            TryDispose( input );
        }
    }

    private static bool ResponseIsGzipped( HttpResponseMessage response, string urlFallback )
    {
        var encodings = response?.Content?.Headers?.ContentEncoding;
        if ( encodings != null )
        {
            foreach ( var encoding in encodings )
                if ( string.Equals( encoding, "gzip", StringComparison.OrdinalIgnoreCase ) )
                    return true;
        }

        // Fallback: infer from URL extension
        return !string.IsNullOrEmpty( urlFallback )
            && urlFallback.EndsWith( ".gz", StringComparison.OrdinalIgnoreCase );
    }

    // -------------------------
    // Index parsing
    // -------------------------
    private static ScryfallBulkEntry TryReadBulkItemFromIndex( string indexPath, string type )
    {
        indexPath = FileSystem.NormalizeFilename( indexPath );

        if ( !FileSystem.Data.FileExists( indexPath ) )
            return null;

        try
        {
            using var stream = FileSystem.Data.OpenRead( indexPath );
            var index = JsonSerializer.Deserialize<ScryfallBulkIndex>( stream );

            return index?.Data?.Find( entry =>
                string.Equals( entry.Type, type, StringComparison.OrdinalIgnoreCase ) );
        }
        catch ( Exception e )
        {
            Log.Error( $"Failed to parse bulk-data index: {e}" );
            return null;
        }
    }

    // -------------------------
    // Meta persistence
    // -------------------------
    private static OracleMeta ReadOracleMeta()
    {
        string metaPath = FileSystem.NormalizeFilename( OracleMetaPath );

        if ( !FileSystem.Data.FileExists( metaPath ) )
            return null;

        try
        {
            using var stream = FileSystem.Data.OpenRead( metaPath );
            return JsonSerializer.Deserialize<OracleMeta>( stream );
        }
        catch
        {
            return null;
        }
    }

    private static async Task WriteOracleMetaAsync( string lastUpdatedAtFromIndex )
    {
        await MetaLock.WaitAsync();
        try
        {
            var meta = new OracleMeta
            {
                LastUpdatedAt    = lastUpdatedAtFromIndex,
                LastDownloadedAt = DateTimeOffset.UtcNow.ToString( "O" )
            };

            byte[] bytes    = JsonSerializer.SerializeToUtf8Bytes( meta );
            string metaPath = FileSystem.NormalizeFilename( OracleMetaPath );
            string tmpPath  = metaPath + ".tmp";

            // Write to tmp first, then atomically promote
            Stream tmpWrite = null;
            try
            {
                tmpWrite = FileSystem.Data.OpenWrite( tmpPath, FileMode.Create );
                await tmpWrite.WriteAsync( bytes, 0, bytes.Length );
                await tmpWrite.FlushAsync();
            }
            finally
            {
                TryDispose( tmpWrite );
            }

            if ( FileSystem.Data.FileExists( metaPath ) )
                FileSystem.Data.DeleteFile( metaPath );

            await StreamCopyFileAsync( FileSystem.Data, tmpPath, metaPath );
            FileSystem.Data.DeleteFile( tmpPath );
        }
        finally
        {
            MetaLock.Release();
        }
    }

    // -------------------------
    // Utilities
    // -------------------------
    private static bool TryParseIso8601Utc( string s, out DateTimeOffset dto )
        => DateTimeOffset.TryParse( s, null, System.Globalization.DateTimeStyles.RoundtripKind, out dto );

    private static void TryDispose( IDisposable disposable )
    {
        try { disposable?.Dispose(); }
        catch ( Exception e ) { Log.Warning( $"Dispose failed: {e.Message}" ); }
    }

    private static void TryDeleteFile( string path )
    {
        try
        {
            if ( FileSystem.Data.FileExists( path ) )
                FileSystem.Data.DeleteFile( path );
        }
        catch ( Exception e )
        {
            Log.Warning( $"Failed to delete file [{path}]: {e.Message}" );
        }
    }

    // -------------------------
    // DTOs
    // -------------------------
    private sealed class ScryfallBulkIndex
    {
        [JsonPropertyName( "data" )]
        public List<ScryfallBulkEntry> Data { get; set; }
    }

    private sealed class ScryfallBulkEntry
    {
        [JsonPropertyName( "type" )]             public string Type            { get; set; }
        [JsonPropertyName( "download_uri" )]     public string DownloadUri     { get; set; }
        [JsonPropertyName( "updated_at" )]       public string UpdatedAt       { get; set; }
        [JsonPropertyName( "size" )]             public long   Size            { get; set; }
        [JsonPropertyName( "content_type" )]     public string ContentType     { get; set; }
        [JsonPropertyName( "content_encoding" )] public string ContentEncoding { get; set; }
    }

    private sealed class OracleMeta
    {
        [JsonPropertyName( "last_updated_at" )]    public string LastUpdatedAt    { get; set; }
        [JsonPropertyName( "last_downloaded_at" )] public string LastDownloadedAt { get; set; }
    }
}