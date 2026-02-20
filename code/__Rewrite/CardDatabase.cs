using System;
using Sandbox;
using Sandbox.__Rewrite.Gameplay;

namespace Sandbox.__Rewrite;

public static class CardDatabase
{
    private const string GameplayBlobPath = "scryfall/gameplay_cards.blob";

    private static GameplayCardsBlob _blob;
    private static bool _loaded;

    public static bool IsLoaded => _loaded;

    public static bool TryLoadFromDisk()
    {
        try
        {
            var path = FileSystem.NormalizeFilename( GameplayBlobPath );
            if ( !FileSystem.Data.FileExists( path ) )
            {
                Log.Warning( $"Gameplay blob missing at {path}" );
                _loaded = false;
                return false;
            }

            var bytes = FileSystem.Data.ReadAllBytes( path );
            Log.Info( $"Loading gameplay blob bytes: {bytes.Length}" );
            if ( bytes == null || bytes.Length == 0 )
            {
                Log.Warning( "Gameplay blob file empty." );
                _loaded = false;
                return false;
            }

            _blob = new GameplayCardsBlob();

            // Our simple file format: [int version][blob payload...]
            var bs = ByteStream.CreateReader( bytes );
            int dataVersion = bs.Read<int>();

            var reader = new BlobData.Reader { Stream = bs, DataVersion = dataVersion };

            if ( dataVersion < _blob.Version )
                _blob.Upgrade( ref reader, dataVersion );
            else
                _blob.Deserialize( ref reader );

            // Make sure index is ready.
            _blob.RebuildOracleIndex();

            _loaded = true;
            Log.Info( $"CardDatabase loaded: {_blob.Cards.Count} records." );
            return true;
        }
        catch ( Exception e )
        {
            Log.Error( $"CardDatabase load failed: {e}" );
            _loaded = false;
            return false;
        }
    }

    public static bool TryGetRecordByOracleId( string oracleId, out GameplayCardsBlob.CardRecord rec )
    {
        rec = default;

        if ( !_loaded || _blob == null )
            return false;

        if ( !_blob.TryGetIndexByOracleId( oracleId, out var index ) )
            return false;

        rec = _blob.Cards[index];
        return true;
    }

    /// <summary>
    /// Convenience: returns common string fields without allocating a full GameplayCard.
    /// </summary>
    public static bool TryGetTextFieldsByOracleId(
        string oracleId,
        out string name,
        out string typeLine,
        out string oracleText )
    {
        name = typeLine = oracleText = null;

        if ( !TryGetRecordByOracleId( oracleId, out var rec ) )
            return false;

        name = _blob.GetString( rec.NameSid );
        typeLine = _blob.GetString( rec.TypeLineSid );
        oracleText = _blob.GetString( rec.OracleTextSid );
        return true;
    }
}
