using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Sandbox.__Rewrite.Gameplay;
using Sandbox.__Rewrite.Scryfall;

namespace Sandbox.__Rewrite;

/// <summary>
/// Reads the raw Scryfall oracle_cards.json from disk, strips all non-gameplay
/// fields, and writes a compact gameplay_cards.json keyed by oracle_id.
///
/// Call <see cref="ProcessAsync"/> after DiskDataSystem has confirmed the
/// oracle cards file is present and up-to-date.
/// </summary>
public static class CardDataProcessor
{
    // -------------------------
    // Paths
    // -------------------------
    private const string OracleJsonPath    = "scryfall/oracle_cards.json";
    private const string GameplayJsonPath  = "scryfall/gameplay_cards.json";
    private const string GameplayMetaPath  = "scryfall/gameplay_cards.meta.json";

    private static readonly System.Threading.SemaphoreSlim WriteLock = new( 1, 1 );

    // -------------------------
    // Public entry point
    // -------------------------

    /// <summary>
    /// Processes oracle_cards.json into gameplay_cards.json.
    /// Skips processing if gameplay data is already current.
    /// </summary>
    /// <param name="oracleUpdatedAt">
    /// The updated_at timestamp from the Scryfall bulk index.
    /// Used to decide whether reprocessing is needed.
    /// </param>
    public static async Task ProcessAsync( string oracleUpdatedAt )
    {
        if ( !ShouldProcess( oracleUpdatedAt ) )
        {
            Log.Info( "Gameplay cards are already current; skipping processing." );
            return;
        }

        Log.Info( "Processing oracle cards into gameplay format…" );
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // 1) Deserialize the full oracle card list
        List<ScryfallCard> scryfallCards = await ReadOracleCardsAsync();
        if ( scryfallCards == null )
            return;

        Log.Info( $"Deserialized {scryfallCards.Count} oracle cards in {sw.ElapsedMilliseconds} ms." );

        // 2) Map to gameplay cards, keyed by oracle_id
        Dictionary<string, GameplayCard> gameplayCards = MapToGameplay( scryfallCards );

        Log.Info( $"Mapped {gameplayCards.Count} gameplay cards ({scryfallCards.Count - gameplayCards.Count} skipped, no oracle_id) in {sw.ElapsedMilliseconds} ms." );

        // 3) Write to disk atomically
        bool ok = await WriteGameplayCardsAsync( gameplayCards );
        if ( !ok )
            return;

        // 4) Record what we processed so future boots can compare
        await WriteGameplayMetaAsync( oracleUpdatedAt );

        sw.Stop();
        Log.Info( $"Gameplay cards written to {FileSystem.NormalizeFilename( GameplayJsonPath )} in {sw.ElapsedMilliseconds} ms." );
        Log.Info( $"File size: {FileSystem.Data.FileSize( FileSystem.NormalizeFilename( GameplayJsonPath ) )} bytes" );
    }

    // -------------------------
    // Should we reprocess?
    // -------------------------
    private static bool ShouldProcess( string oracleUpdatedAt )
    {
        // If output file doesn't exist yet, always process
        if ( !FileSystem.Data.FileExists( FileSystem.NormalizeFilename( GameplayJsonPath ) ) )
            return true;

        var meta = ReadGameplayMeta();
        if ( meta == null || string.IsNullOrWhiteSpace( meta.ProcessedFromOracleUpdatedAt ) )
            return true;

        // Only reprocess if the oracle data has changed
        return !string.Equals( meta.ProcessedFromOracleUpdatedAt, oracleUpdatedAt, StringComparison.Ordinal );
    }

    // -------------------------
    // Read
    // -------------------------
    private static async Task<List<ScryfallCard>> ReadOracleCardsAsync()
    {
        string path = FileSystem.NormalizeFilename( OracleJsonPath );

        if ( !FileSystem.Data.FileExists( path ) )
        {
            Log.Error( $"oracle_cards.json not found at {path}." );
            return null;
        }

        try
        {
            using var stream = FileSystem.Data.OpenRead( path );
            return await JsonSerializer.DeserializeAsync<List<ScryfallCard>>( stream );
        }
        catch ( Exception e )
        {
            Log.Error( $"Failed to deserialize oracle_cards.json: {e}" );
            return null;
        }
    }

    // -------------------------
    // Map
    // -------------------------
    private static Dictionary<string, GameplayCard> MapToGameplay( List<ScryfallCard> source )
    {
        var result = new Dictionary<string, GameplayCard>( source.Count );

        foreach ( var card in source )
        {
            // Cards without an oracle_id (e.g. reversible_card layout) cannot
            // be keyed by identity — skip them.
            if ( string.IsNullOrWhiteSpace( card.OracleId ) )
                continue;

            // Last write wins if somehow the same oracle_id appears twice.
            result[card.OracleId] = MapCard( card );
        }

        return result;
    }

    private static GameplayCard MapCard( ScryfallCard src ) => new()
    {
        // Identity
        Id       = src.Id,
        OracleId = src.OracleId,
        Lang     = src.Lang,
        Layout   = CardLayoutParser.Parse( src.Layout ),

        // Core gameplay
        Name       = src.Name,
        ManaCost   = ManaCost.Parse( src.ManaCost ),
        Cmc        = (int)src.Cmc,
        TypeLine   = src.TypeLine,
        OracleText = src.OracleText,

        // Colors
        Colors         = ManaColorExtensions.ParseList( src.Colors ),
        ColorIdentity  = ManaColorExtensions.ParseList( src.ColorIdentity ),
        ColorIndicator = ManaColorExtensions.ParseList( src.ColorIndicator ),
        ProducedMana   = ManaColorExtensions.ParseList( src.ProducedMana ),

        // Combat stats
        Power     = CombatValue.ParseOrNull( src.Power ),
        Toughness = CombatValue.ParseOrNull( src.Toughness ),

        // Loyalty / defense
        Loyalty = StartingValue.ParseOrNull( src.Loyalty ),
        Defense = StartingValue.ParseOrNull( src.Defense ),

        // Flags
        Reserved    = src.Reserved,
        GameChanger = src.GameChanger,

        // Vanguard
        HandModifier = src.HandModifier,
        LifeModifier = src.LifeModifier,

        // Keywords (read-only, already a list)
        Keywords = src.Keywords?.AsReadOnly(),

        // Legalities
        Legalities = Legalities.Parse( src.Legalities ),

        // Multi-face
        CardFaces = MapFaces( src.CardFaces ),
        AllParts  = MapRelatedCards( src.AllParts ),
    };

    private static IReadOnlyList<GameplayCardFace> MapFaces( List<ScryfallCardFace> faces )
    {
        if ( faces == null || faces.Count == 0 )
            return null;

        var result = new List<GameplayCardFace>( faces.Count );
        foreach ( var face in faces )
        {
            result.Add( new GameplayCardFace
            {
                Name           = face.Name,
                ManaCost       = ManaCost.Parse( face.ManaCost ),
                Cmc            = face.Cmc.HasValue ? (int)face.Cmc.Value : null,
                TypeLine       = face.TypeLine,
                OracleText     = face.OracleText,
                Colors         = ManaColorExtensions.ParseList( face.Colors ),
                ColorIndicator = ManaColorExtensions.ParseList( face.ColorIndicator ),
                Power          = CombatValue.ParseOrNull( face.Power ),
                Toughness      = CombatValue.ParseOrNull( face.Toughness ),
                Loyalty        = StartingValue.ParseOrNull( face.Loyalty ),
                Defense        = StartingValue.ParseOrNull( face.Defense ),
                OracleId       = face.OracleId,
                Layout         = CardLayoutParser.Parse( face.Layout ),
            } );
        }
        return result;
    }

    private static IReadOnlyList<GameplayRelatedCard> MapRelatedCards( List<ScryfallRelatedCard> parts )
    {
        if ( parts == null || parts.Count == 0 )
            return null;

        var result = new List<GameplayRelatedCard>( parts.Count );
        foreach ( var part in parts )
        {
            result.Add( new GameplayRelatedCard
            {
                Id        = part.Id,
                Component = part.Component,
                Name      = part.Name,
                TypeLine  = part.TypeLine,
            } );
        }
        return result;
    }

    // -------------------------
    // Write
    // -------------------------
    private static async Task<bool> WriteGameplayCardsAsync( Dictionary<string, GameplayCard> cards )
    {
        await WriteLock.WaitAsync();
        try
        {
            string destPath = FileSystem.NormalizeFilename( GameplayJsonPath );
            string tmpPath  = destPath + ".tmp";

            // Serialize to tmp
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes( cards );

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

            // Atomic promote: delete old, copy tmp -> dest, delete tmp
            if ( FileSystem.Data.FileExists( destPath ) )
                FileSystem.Data.DeleteFile( destPath );

            await StreamCopyFileAsync( FileSystem.Data, tmpPath, destPath );
            FileSystem.Data.DeleteFile( tmpPath );
            return true;
        }
        catch ( Exception e )
        {
            Log.Error( $"Failed to write gameplay_cards.json: {e}" );
            return false;
        }
        finally
        {
            WriteLock.Release();
        }
    }

    // -------------------------
    // Meta persistence
    // -------------------------
    private static GameplayMeta ReadGameplayMeta()
    {
        string metaPath = FileSystem.NormalizeFilename( GameplayMetaPath );

        if ( !FileSystem.Data.FileExists( metaPath ) )
            return null;

        try
        {
            using var stream = FileSystem.Data.OpenRead( metaPath );
            return JsonSerializer.Deserialize<GameplayMeta>( stream );
        }
        catch
        {
            return null;
        }
    }

    private static async Task WriteGameplayMetaAsync( string oracleUpdatedAt )
    {
        try
        {
            var meta = new GameplayMeta
            {
                ProcessedFromOracleUpdatedAt = oracleUpdatedAt,
                ProcessedAt                 = DateTimeOffset.UtcNow.ToString( "O" ),
            };

            byte[] bytes    = JsonSerializer.SerializeToUtf8Bytes( meta );
            string metaPath = FileSystem.NormalizeFilename( GameplayMetaPath );
            string tmpPath  = metaPath + ".tmp";

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
        catch ( Exception e )
        {
            // Non-fatal — worst case we reprocess on next boot
            Log.Warning( $"Failed to write gameplay meta: {e.Message}" );
        }
    }

    // -------------------------
    // Shared file utilities (mirrors DiskDataSystem helpers)
    // -------------------------
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

    private static void TryDispose( IDisposable disposable )
    {
        try { disposable?.Dispose(); }
        catch ( Exception e ) { Log.Warning( $"Dispose failed: {e.Message}" ); }
    }

    // -------------------------
    // Meta DTO
    // -------------------------
    private sealed class GameplayMeta
    {
        [JsonPropertyName( "processed_from_oracle_updated_at" )]
        public string ProcessedFromOracleUpdatedAt { get; set; }

        [JsonPropertyName( "processed_at" )]
        public string ProcessedAt { get; set; }
    }
}