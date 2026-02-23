using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Sandbox.__Rewrite.Data;
using Sandbox.__Rewrite.Types;

namespace Sandbox.__Rewrite;

public class DiskDataSystem : GameObjectSystem, ISceneStartup
{
    // -------------------------
    // Constants
    // -------------------------
    private const string ScryfallBulkDataUrl = "https://api.scryfall.com/bulk-data";
    private const string ScryfallApiBase     = "https://api.scryfall.com";

    private const string OracleCardsType  = "oracle_cards";
    private const string DefaultCardsType = "default_cards";
    private const string RulingsType      = "rulings";

    private const string BulkIndexPath         = "scryfall/bulk-data.json";
    private const string OracleJsonPath        = "scryfall/oracle_cards.json";
    private const string OracleMetaPath        = "scryfall/oracle_cards.meta.json";
    private const string DefaultCardsPath      = "scryfall/default_cards.json";
    private const string DefaultCardsMetaPath  = "scryfall/default_cards.meta.json";
    private const string RulingsJsonPath       = "scryfall/rulings.json";
    private const string RulingsMetaPath       = "scryfall/rulings.meta.json";
    private const string CatalogsRoot          = "scryfall/catalogs";
    private const string CardBlobPath          = "scryfall/cards.blob";
    private const string CardBlobMetaPath      = "scryfall/cards.blob.meta.json";
    private const string PrintingBlobPath      = "scryfall/printings.blob";
    private const string PrintingBlobMetaPath  = "scryfall/printings.blob.meta.json";

    private static readonly string[] CatalogNames =
    {
        "card-names", "artist-names",
        "creature-types", "planeswalker-types", "land-types",
        "artifact-types", "enchantment-types", "spell-types",
        "keyword-actions", "keyword-abilities", "ability-words",
        "supertypes", "card-types"
    };

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
        try { await OnClientInitializeAsync(); }
        catch ( Exception e ) { Log.Error( $"DiskDataSystem startup failed: {e}" ); }
    }

    private async Task OnClientInitializeAsync()
    {
        // 1) Always refresh the bulk-data index from Scryfall
        if ( !await DownloadIndexAsync() )
            return;

        // 2) Parse index
        var oracle       = TryReadBulkItemFromIndex( BulkIndexPath, OracleCardsType );
        var defaultCards = TryReadBulkItemFromIndex( BulkIndexPath, DefaultCardsType );
        var rulings      = TryReadBulkItemFromIndex( BulkIndexPath, RulingsType );

        if ( oracle == null )
        {
            Log.Error( "bulk-data.json is missing the oracle_cards entry." );
            return;
        }

        // 3) Download oracle cards
        bool oracleDownloaded = await DownloadBulkEntryIfNeededAsync(
            entry:    oracle,
            jsonPath: OracleJsonPath,
            metaPath: OracleMetaPath,
            label:    "oracle_cards"
        );

        // 4) Download default cards
        bool defaultDownloaded = false;
        if ( defaultCards != null )
            defaultDownloaded = await DownloadBulkEntryIfNeededAsync(
                entry:    defaultCards,
                jsonPath: DefaultCardsPath,
                metaPath: DefaultCardsMetaPath,
                label:    "default_cards"
            );
        else
            Log.Warning( "bulk-data.json is missing the default_cards entry." );

        // 5) Download rulings
        if ( rulings != null )
            await DownloadBulkEntryIfNeededAsync(
                entry:    rulings,
                jsonPath: RulingsJsonPath,
                metaPath: RulingsMetaPath,
                label:    "rulings"
            );
        else
            Log.Warning( "bulk-data.json is missing the rulings entry." );

        // 6) Download catalogs
        await DownloadCatalogsAsync();

        // 7) Build card blob if oracle source changed or blob is missing
        string cardBlobNorm = FileSystem.NormalizeFilename( CardBlobPath );
        if ( oracleDownloaded || !FileSystem.Data.FileExists( cardBlobNorm ) )
        {
            if ( !await BuildCardBlobAsync( oracle.UpdatedAt ) )
                return;
        }
        else
        {
            Log.Info( "Card blob is up-to-date, skipping oracle normalization." );
        }

        // 8) Build printing blob if default_cards changed or blob is missing
        string printingBlobNorm = FileSystem.NormalizeFilename( PrintingBlobPath );
        if ( defaultDownloaded || !FileSystem.Data.FileExists( printingBlobNorm ) )
        {
            if ( defaultCards != null )
                await BuildPrintingBlobAsync( defaultCards.UpdatedAt );
            else
                Log.Warning( "Skipping printing blob — no default_cards entry available." );
        }
        else
        {
            Log.Info( "Printing blob is up-to-date, skipping default_cards normalization." );
        }

        // 9) Load card blob into runtime database
        var cardReader = await CardBlobReader.LoadAsync( FileSystem.Data, CardBlobPath );
        Log.Info( $"CardDatabase loaded — {cardReader.CardCount} oracle cards available." );
        CardDatabase.Initialize( cardReader );

        // 10) Load printing blob into runtime database (non-fatal if missing)
        if ( FileSystem.Data.FileExists( printingBlobNorm ) )
        {
            var printingReader = await PrintingBlobReader.LoadAsync( FileSystem.Data, PrintingBlobPath );
            CardDatabase.InitializePrintings( printingReader );
        }
        else
        {
            Log.Warning( "Printing blob not found — printing lookups will be unavailable." );
        }
    }


    // -------------------------
    // Card blob build  (oracle_cards → GameplayCard)
    // -------------------------
    private async Task<bool> BuildCardBlobAsync( string oracleUpdatedAt )
    {
        Log.Info( "Normalizing oracle cards and writing card blob…" );
        try
        {
            byte[] jsonBytes = await FileSystem.Data.ReadAllBytesAsync(
                FileSystem.NormalizeFilename( OracleJsonPath ) );

            if ( jsonBytes == null || jsonBytes.Length == 0 )
            {
                Log.Error( "Oracle JSON was empty — aborting card blob build." );
                return false;
            }

            var rawCards = JsonSerializer.Deserialize<List<ScryfallCard>>( jsonBytes );
            if ( rawCards == null || rawCards.Count == 0 )
            {
                Log.Error( "Oracle JSON deserialized to empty — aborting card blob build." );
                return false;
            }

            Log.Info( $"Deserialised {rawCards.Count} raw oracle cards, normalizing…" );

            var normalized = new List<GameplayCard>( rawCards.Count );
            foreach ( var raw in rawCards )
            {
                try   { normalized.Add( CardNormalizer.Normalize( raw ) ); }
                catch ( Exception e ) { Log.Warning( $"Failed to normalize oracle card [{raw?.Name}]: {e.Message}" ); }
            }

            Log.Info( $"Normalized {normalized.Count} cards, writing blob…" );

            string blobPath = FileSystem.NormalizeFilename( CardBlobPath );
            CardBlobWriter.Write( FileSystem.Data, blobPath, normalized );
            await WriteBulkMetaAsync( oracleUpdatedAt, CardBlobMetaPath );

            Log.Info( $"Card blob written ({FileSystem.Data.FileSize( blobPath )} bytes)." );
            return true;
        }
        catch ( Exception e )
        {
            Log.Error( $"Card blob build failed: {e}" );
            return false;
        }
    }


    // -------------------------
    // Printing blob build  (default_cards → GameplayPrinting)
    // -------------------------
    private async Task<bool> BuildPrintingBlobAsync( string defaultUpdatedAt )
    {
        Log.Info( "Normalizing default cards and writing printing blob…" );
        try
        {
            byte[] jsonBytes = await FileSystem.Data.ReadAllBytesAsync(
                FileSystem.NormalizeFilename( DefaultCardsPath ) );

            if ( jsonBytes == null || jsonBytes.Length == 0 )
            {
                Log.Error( "Default cards JSON was empty — aborting printing blob build." );
                return false;
            }

            var rawCards = JsonSerializer.Deserialize<List<ScryfallCard>>( jsonBytes );
            if ( rawCards == null || rawCards.Count == 0 )
            {
                Log.Error( "Default cards JSON deserialized to empty — aborting printing blob build." );
                return false;
            }

            Log.Info( $"Deserialised {rawCards.Count} raw printings, normalizing…" );

            var normalized = new List<GameplayPrinting>( rawCards.Count );
            foreach ( var raw in rawCards )
            {
                try   { normalized.Add( PrintingNormalizer.Normalize( raw ) ); }
                catch ( Exception e ) { Log.Warning( $"Failed to normalize printing [{raw?.Name}]: {e.Message}" ); }
            }

            Log.Info( $"Normalized {normalized.Count} printings, writing blob…" );

            string blobPath = FileSystem.NormalizeFilename( PrintingBlobPath );
            PrintingBlobWriter.Write( FileSystem.Data, blobPath, normalized );
            await WriteBulkMetaAsync( defaultUpdatedAt, PrintingBlobMetaPath );

            Log.Info( $"Printing blob written ({FileSystem.Data.FileSize( blobPath )} bytes)." );
            return true;
        }
        catch ( Exception e )
        {
            Log.Error( $"Printing blob build failed: {e}" );
            return false;
        }
    }


    // -------------------------
    // Bulk entry download
    // Returns true if a fresh download was performed.
    // -------------------------
    private async Task<bool> DownloadBulkEntryIfNeededAsync(
        ScryfallBulkEntry entry,
        string jsonPath,
        string metaPath,
        string label )
    {
        if ( ShouldDownloadBulk( entry, jsonPath, metaPath ) )
        {
            Log.Info( $"Downloading {label}… updated_at={entry.UpdatedAt}, size={entry.Size} bytes" );

            bool isGzip = string.Equals( entry.ContentEncoding, "gzip", StringComparison.OrdinalIgnoreCase );
            if ( !await DownloadBulkToFileAsync( entry.DownloadUri, jsonPath, decompressGzip: isGzip ) )
                return false;

            await WriteBulkMetaAsync( entry.UpdatedAt, metaPath );
            Log.Info( $"{label} saved ({FileSystem.Data.FileSize( FileSystem.NormalizeFilename( jsonPath ) )} bytes)" );
            return true;
        }

        Log.Info( $"{label} is up-to-date (index updated_at={entry.UpdatedAt})." );
        return false;
    }


    // -------------------------
    // Catalog download
    // -------------------------
    private async Task DownloadCatalogsAsync()
    {
        FileSystem.Data.CreateDirectory( CatalogsRoot );

        var headers = new Dictionary<string, string> { { "Accept", "application/json" } };

        foreach ( string catalog in CatalogNames )
        {
            string destPath = FileSystem.NormalizeFilename( $"{CatalogsRoot}/{catalog}.json" );

            if ( FileSystem.Data.FileExists( destPath ) )
            {
                Log.Info( $"Catalog {catalog} already on disk, skipping." );
                continue;
            }

            Log.Info( $"Fetching catalog/{catalog}…" );

            try
            {
                string url   = $"{ScryfallApiBase}/catalog/{catalog}";
                var response = await Sandbox.Http.RequestAsync( url, "GET", null, headers );

                if ( response == null || !response.IsSuccessStatusCode )
                {
                    Log.Warning( $"Catalog {catalog} returned HTTP {(int?)response?.StatusCode}" );
                    continue;
                }

                Stream netStream = null;
                Stream outStream = null;

                try
                {
                    netStream = await response.Content.ReadAsStreamAsync();
                    outStream = FileSystem.Data.OpenWrite( destPath, FileMode.Create );
                    await netStream.CopyToAsync( outStream, bufferSize: 256 * 1024 );
                    await outStream.FlushAsync();
                    Log.Info( $"Catalog {catalog} saved." );
                }
                finally
                {
                    TryDispose( outStream );
                    TryDispose( netStream );
                    TryDispose( response );
                }
            }
            catch ( Exception e )
            {
                Log.Error( $"Failed to download catalog/{catalog}: {e.Message}" );
            }

            await Task.Delay( 110 );
        }
    }


    // -------------------------
    // Index download
    // -------------------------
    private async Task<bool> DownloadIndexAsync()
    {
        Log.Info( "Fetching Scryfall bulk data index…" );

        if ( !await DownloadBulkToFileAsync( ScryfallBulkDataUrl, BulkIndexPath, decompressGzip: false ) )
            return false;

        Log.Info( $"Index saved ({FileSystem.Data.FileSize( FileSystem.NormalizeFilename( BulkIndexPath ) )} bytes)" );
        return true;
    }


    // -------------------------
    // Download decision logic
    // -------------------------
    private bool ShouldDownloadBulk( ScryfallBulkEntry entry, string jsonPath, string metaPath )
    {
        if ( !FileSystem.Data.FileExists( FileSystem.NormalizeFilename( jsonPath ) ) ) return true;
        if ( !TryGetLocalMeta( metaPath, out var meta ) )                               return true;
        if ( !IsRemoteNewer( entry, meta ) )                                            return false;
        if ( IsWithinRateLimitWindow( meta ) )                                          return false;
        return true;
    }

    private static bool TryGetLocalMeta( string metaPath, out OracleMeta meta )
    {
        meta = ReadMeta( metaPath );
        return meta != null && !string.IsNullOrWhiteSpace( meta.LastUpdatedAt );
    }

    private static bool IsRemoteNewer( ScryfallBulkEntry entry, OracleMeta meta )
    {
        if ( !TryParseIso8601Utc( entry.UpdatedAt, out var remoteUpdated ) )   return true;
        if ( !TryParseIso8601Utc( meta.LastUpdatedAt, out var localUpdated ) ) return true;
        return remoteUpdated > localUpdated;
    }

    private static bool IsWithinRateLimitWindow( OracleMeta meta )
    {
        if ( !TryParseIso8601Utc( meta.LastDownloadedAt, out var lastDownload ) )
            return false;

        var elapsed = DateTimeOffset.UtcNow - lastDownload;
        if ( elapsed < MinRedownloadInterval )
        {
            Log.Info( $"Remote is newer but last download was {elapsed.TotalMinutes:0} min ago; skipping." );
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
            foreach ( var enc in encodings )
                if ( string.Equals( enc, "gzip", StringComparison.OrdinalIgnoreCase ) )
                    return true;

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

            return index?.Data?.Find( e =>
                string.Equals( e.Type, type, StringComparison.OrdinalIgnoreCase ) );
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
    private static OracleMeta ReadMeta( string metaPath )
    {
        metaPath = FileSystem.NormalizeFilename( metaPath );

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

    private static async Task WriteBulkMetaAsync( string lastUpdatedAt, string metaPath )
    {
        await MetaLock.WaitAsync();
        try
        {
            var meta = new OracleMeta
            {
                LastUpdatedAt    = lastUpdatedAt,
                LastDownloadedAt = DateTimeOffset.UtcNow.ToString( "O" )
            };

            byte[] bytes          = JsonSerializer.SerializeToUtf8Bytes( meta );
            string normalizedMeta = FileSystem.NormalizeFilename( metaPath );
            string tmpPath        = normalizedMeta + ".tmp";

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

            if ( FileSystem.Data.FileExists( normalizedMeta ) )
                FileSystem.Data.DeleteFile( normalizedMeta );

            await StreamCopyFileAsync( FileSystem.Data, tmpPath, normalizedMeta );
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