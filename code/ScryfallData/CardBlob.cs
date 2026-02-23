using System;
using System.Threading.Tasks;
using Sandbox.ScryfallData.Types;

namespace Sandbox.ScryfallData;

// ═══════════════════════════════════════════════════════════════════
//  CARD BLOB  (oracle data only)
//
//  [ HEADER     ]  magic(4) + version(2) + cardCount(4) + dataOffset(4)
//  [ INDEX      ]  cardCount × ( Guid(16) + offset(4) )
//  [ CATALOGS   ]  keyword strings + type word strings
//  [ CARD DATA  ]  variable-length card records
// ═══════════════════════════════════════════════════════════════════

public static class CardBlobWriter
{
    private const uint   Magic      = 0x4D544743; // "MTGC"
    private const ushort Version    = 2;           // bumped — layout changed
    private const int    HeaderSize = 14;

    public static void Write( BaseFileSystem fs, string path, List<GameplayCard> cards )
    {
        var sorted = new List<GameplayCard>( cards.Count );
        foreach ( var c in cards )
            if ( c.OracleId.HasValue )
                sorted.Add( c );
        sorted.Sort( ( a, b ) => a.OracleId!.Value.CompareTo( b.OracleId!.Value ) );

        int cardCount = sorted.Count;

        var keywordCatalog  = BuildCatalog( sorted, c => c.Keywords );
        var typeWordCatalog = BuildCatalog( sorted, c => EnumerateTypeWords( c ) );

        byte[] catalogBytes = SerializeCatalog( keywordCatalog, typeWordCatalog );

        var    offsets  = new int[cardCount];
        byte[] cardData = SerializeCards( sorted, keywordCatalog, typeWordCatalog, offsets );

        int indexSize  = cardCount * ( 16 + 4 );
        int dataOffset = HeaderSize + indexSize + catalogBytes.Length;
        int totalSize  = dataOffset + cardData.Length;

        var blob = ByteStream.Create( totalSize );
        try
        {
            blob.Write( Magic );
            blob.Write( Version );
            blob.Write( cardCount );
            blob.Write( dataOffset );

            for ( int i = 0; i < sorted.Count; i++ )
            {
                WriteGuid( ref blob, sorted[i].OracleId!.Value );
                blob.Write( offsets[i] );
            }

            blob.Write( catalogBytes );
            blob.Write( cardData );

            fs.WriteAllBytes( path, blob.ToArray() );
        }
        finally { blob.Dispose(); }
    }

    // ── Catalog ──────────────────────────────────────────────────

    private static Dictionary<string, ushort> BuildCatalog(
        List<GameplayCard> cards,
        Func<GameplayCard, IEnumerable<string>> selector )
    {
        var catalog = new Dictionary<string, ushort>( StringComparer.Ordinal );
        foreach ( var card in cards )
            foreach ( var word in selector( card ) )
                if ( !catalog.ContainsKey( word ) )
                    catalog[word] = (ushort)catalog.Count;
        return catalog;
    }

    private static IEnumerable<string> EnumerateTypeWords( GameplayCard c )
    {
        foreach ( var s in c.Supertypes ) yield return s;
        foreach ( var s in c.CardTypes  ) yield return s;
        foreach ( var s in c.Subtypes   ) yield return s;
        foreach ( var face in c.Faces )
        {
            foreach ( var s in face.Supertypes ) yield return s;
            foreach ( var s in face.CardTypes   ) yield return s;
            foreach ( var s in face.Subtypes    ) yield return s;
        }
    }

    private static byte[] SerializeCatalog(
        Dictionary<string, ushort> keywords,
        Dictionary<string, ushort> typeWords )
    {
        var kw = new string[keywords.Count];
        foreach ( var kv in keywords )  kw[kv.Value] = kv.Key;

        var tw = new string[typeWords.Count];
        foreach ( var kv in typeWords ) tw[kv.Value] = kv.Key;

        var s = ByteStream.Create( 64 * 1024 );
        try
        {
            s.Write( (ushort)kw.Length );
            foreach ( var word in kw ) s.Write( word );

            s.Write( (ushort)tw.Length );
            foreach ( var word in tw ) s.Write( word );

            return s.ToArray();
        }
        finally { s.Dispose(); }
    }

    // ── Card data ────────────────────────────────────────────────

    private static byte[] SerializeCards(
        List<GameplayCard> sorted,
        Dictionary<string, ushort> keywords,
        Dictionary<string, ushort> typeWords,
        int[] offsets )
    {
        var s = ByteStream.Create( 8 * 1024 * 1024 );
        try
        {
            for ( int i = 0; i < sorted.Count; i++ )
            {
                offsets[i] = s.Position;
                WriteCard( ref s, sorted[i], keywords, typeWords );
            }
            return s.ToArray();
        }
        finally { s.Dispose(); }
    }

    private static void WriteCard(
        ref ByteStream s,
        GameplayCard c,
        Dictionary<string, ushort> keywords,
        Dictionary<string, ushort> typeWords )
    {
        // Identity
        WriteGuid( ref s, c.ScryfallId );
        WriteGuid( ref s, c.OracleId!.Value );
        s.Write( c.Name );
        s.Write( (byte)c.Layout );

        // Colors — oracle-level only (no ColorIndicator, that's face-level)
        s.Write( (byte)c.ColorIdentity );
        s.Write( (byte)c.ProducedMana );

        // Mana
        s.Write( c.ManaCostRaw );
        WriteManaCost( ref s, c.ManaCost );
        s.Write( c.Cmc );

        // Types
        s.Write( c.TypeLine );
        WriteStringIndices( ref s, c.Supertypes, typeWords );
        WriteStringIndices( ref s, c.CardTypes,  typeWords );
        WriteStringIndices( ref s, c.Subtypes,   typeWords );

        // Rules
        s.Write( c.OracleText );
        WriteStringIndices( ref s, c.Keywords, keywords );

        // Stats
        WriteNullableStat( ref s, c.Power );
        WriteNullableStat( ref s, c.Toughness );
        WriteNullableStat( ref s, c.Loyalty );
        WriteNullableStat( ref s, c.Defense );

        // Vanguard
        s.Write( c.HandModifier );
        s.Write( c.LifeModifier );

        // Legalities
        s.Write( c.Legalities.Packed );

        // Related cards
        WriteRelatedCards( ref s, c.RelatedCards );

        // Flags — oracle-level only
        ushort flags = 0;
        if ( c.IsReserved   ) flags |= 1 << 0;
        if ( c.IsGameChanger ) flags |= 1 << 1;
        s.Write( flags );

        // Ranks
        WriteNullableInt( ref s, c.EdhrecRank );
        WriteNullableInt( ref s, c.PennyRank );

        // Faces
        s.Write( (byte)c.Faces.Count );
        foreach ( var face in c.Faces )
            WriteFace( ref s, face, keywords, typeWords );
    }

    private static void WriteFace(
        ref ByteStream s,
        GameplayFace f,
        Dictionary<string, ushort> keywords,
        Dictionary<string, ushort> typeWords )
    {
        WriteNullableGuid( ref s, f.OracleId );
        s.Write( f.Name );
        s.Write( f.ManaCostRaw );
        WriteManaCost( ref s, f.ManaCost );
        s.Write( f.Cmc );
        s.Write( f.TypeLine );
        WriteStringIndices( ref s, f.Supertypes, typeWords );
        WriteStringIndices( ref s, f.CardTypes,  typeWords );
        WriteStringIndices( ref s, f.Subtypes,   typeWords );
        s.Write( f.OracleText );
        s.Write( (byte)f.Colors );
        s.Write( (byte)f.ColorIndicator );
        WriteNullableStat( ref s, f.Power );
        WriteNullableStat( ref s, f.Toughness );
        WriteNullableStat( ref s, f.Loyalty );
        WriteNullableStat( ref s, f.Defense );
        s.Write( f.Layout.HasValue ? (byte)f.Layout.Value : (byte)0xFF );
    }

    // ── Primitives ───────────────────────────────────────────────

    private static void WriteGuid( ref ByteStream s, Guid g )
    {
        Span<byte> bytes = stackalloc byte[16];
        g.TryWriteBytes( bytes );
        s.Write( bytes.ToArray() );
    }

    private static void WriteNullableGuid( ref ByteStream s, Guid? g )
    {
        s.Write( g.HasValue ? (byte)1 : (byte)0 );
        if ( g.HasValue ) WriteGuid( ref s, g.Value );
    }

    private static void WriteNullableInt( ref ByteStream s, int? v )
        => s.Write( v ?? -1 );

    private static void WriteNullableStat( ref ByteStream s, CardStat? stat )
    {
        if ( !stat.HasValue ) { s.Write( (byte)0 ); return; }
        s.Write( (byte)1 );
        s.Write( (byte)stat.Value.Type );
        s.Write( stat.Value.Value );
        s.Write( stat.Value.Modifier );
    }

    private static void WriteStringIndices(
        ref ByteStream s,
        List<string> words,
        Dictionary<string, ushort> catalog )
    {
        s.Write( (byte)words.Count );
        foreach ( var word in words )
            s.Write( catalog.TryGetValue( word, out ushort idx ) ? idx : (ushort)0 );
    }

    private static void WriteManaCost( ref ByteStream s, List<ManaCostSymbol> cost )
    {
        s.Write( (byte)cost.Count );
        foreach ( var sym in cost )
        {
            s.Write( sym.Raw );
            s.Write( sym.PrimaryColor.HasValue   ? (byte)sym.PrimaryColor.Value   : (byte)0xFF );
            s.Write( sym.SecondaryColor.HasValue ? (byte)sym.SecondaryColor.Value : (byte)0xFF );
            s.Write( sym.CmcValue );
            byte flags = 0;
            if ( sym.IsGeneric   ) flags |= 1;
            if ( sym.IsHybrid    ) flags |= 2;
            if ( sym.IsPhyrexian ) flags |= 4;
            if ( sym.IsVariable  ) flags |= 8;
            s.Write( flags );
        }
    }

    private static void WriteRelatedCards( ref ByteStream s, List<RelatedCard> related )
    {
        s.Write( (byte)related.Count );
        foreach ( var r in related )
        {
            WriteGuid( ref s, r.ScryfallId );
            s.Write( (byte)r.Component );
            s.Write( r.Name );
            s.Write( r.TypeLine );
        }
    }
}


public sealed class CardBlobReader
{
    private byte[]                _blob;
    private int                   _dataOffset;
    private Dictionary<Guid, int> _index;
    private string[]              _keywords;
    private string[]              _typeWords;

    public int CardCount => _index.Count;

    public static async Task<CardBlobReader> LoadAsync( BaseFileSystem fs, string path )
    {
        byte[] data = await fs.ReadAllBytesAsync( path );
        var reader  = new CardBlobReader();
        reader.Initialize( data );
        return reader;
    }

    private void Initialize( byte[] data )
    {
        _blob = data;
        var s = ByteStream.CreateReader( data );

        uint   magic     = s.Read<uint>();
        ushort version   = s.Read<ushort>();
        int    cardCount = s.Read<int>();
        _dataOffset      = s.Read<int>();

        if ( magic != 0x4D544743 )
            throw new Exception( "Invalid card blob — bad magic bytes." );
        if ( version != 2 )
            throw new Exception( $"Card blob version mismatch — expected 2, got {version}. Rebuild the blob." );

        _index = new Dictionary<Guid, int>( cardCount );
        for ( int i = 0; i < cardCount; i++ )
        {
            var id     = ReadGuid( ref s );
            int offset = s.Read<int>();
            _index[id] = offset;
        }

        ushort kwCount = s.Read<ushort>();
        _keywords      = new string[kwCount];
        for ( int i = 0; i < kwCount; i++ )
            _keywords[i] = s.Read<string>( "" ) ?? "";

        ushort twCount = s.Read<ushort>();
        _typeWords     = new string[twCount];
        for ( int i = 0; i < twCount; i++ )
            _typeWords[i] = s.Read<string>( "" ) ?? "";
    }

    public GameplayCard Fetch( Guid oracleId )
    {
        if ( !_index.TryGetValue( oracleId, out int offset ) )
            return null;

        var s = ByteStream.CreateReader( _blob );
        s.Position = _dataOffset + offset;
        return ReadCard( ref s );
    }

    public List<GameplayCard> FetchBatch( IEnumerable<Guid> oracleIds )
    {
        var result = new List<GameplayCard>();
        foreach ( var id in oracleIds )
        {
            var card = Fetch( id );
            if ( card != null ) result.Add( card );
        }
        return result;
    }

    public bool Contains( Guid oracleId ) => _index.ContainsKey( oracleId );

    // ── Deserialization ──────────────────────────────────────────

    private GameplayCard ReadCard( ref ByteStream s )
    {
        var scryfallId = ReadGuid( ref s );
        var oracleId   = ReadGuid( ref s );
        var name       = s.Read<string>( "" ) ?? "";
        var layout     = (MtgLayout)s.Read<byte>();

        var colorIdentity = (ColorSet)s.Read<byte>();
        var producedMana  = (ColorSet)s.Read<byte>();

        var manaCostRaw = s.Read<string>( "" ) ?? "";
        var manaCost    = ReadManaCost( ref s );
        var cmc         = s.Read<float>();

        var typeLine   = s.Read<string>( "" ) ?? "";
        var supertypes = ReadStringIndices( ref s, _typeWords );
        var cardTypes  = ReadStringIndices( ref s, _typeWords );
        var subtypes   = ReadStringIndices( ref s, _typeWords );

        var oracleText = s.Read<string>( "" ) ?? "";
        var keywords   = ReadStringIndices( ref s, _keywords );

        var power     = ReadNullableStat( ref s );
        var toughness = ReadNullableStat( ref s );
        var loyalty   = ReadNullableStat( ref s );
        var defense   = ReadNullableStat( ref s );

        var handModifier = s.Read<string>( "" ) ?? "";
        var lifeModifier = s.Read<string>( "" ) ?? "";

        var legalities = new LegalityMap( s.Read<ulong>() );

        var relatedCards = ReadRelatedCards( ref s );

        var flags = s.Read<ushort>();

        var edhrecRank = ReadNullableInt( ref s );
        var pennyRank  = ReadNullableInt( ref s );

        int faceCount = s.Read<byte>();
        var faces     = new List<GameplayFace>( faceCount );
        for ( int i = 0; i < faceCount; i++ )
            faces.Add( ReadFace( ref s ) );

        return new GameplayCard
        {
            ScryfallId    = scryfallId,
            OracleId      = oracleId,
            Name          = name,
            Layout        = layout,
            Faces         = faces,
            ColorIdentity = colorIdentity,
            ProducedMana  = producedMana,
            ManaCostRaw   = manaCostRaw,
            ManaCost      = manaCost,
            Cmc           = cmc,
            TypeLine      = typeLine,
            Supertypes    = supertypes,
            CardTypes     = cardTypes,
            Subtypes      = subtypes,
            OracleText    = oracleText,
            Keywords      = keywords,
            Power         = power,
            Toughness     = toughness,
            Loyalty       = loyalty,
            Defense       = defense,
            HandModifier  = handModifier,
            LifeModifier  = lifeModifier,
            Legalities    = legalities,
            RelatedCards  = relatedCards,
            IsReserved    = ( flags & ( 1 << 0 ) ) != 0,
            IsGameChanger = ( flags & ( 1 << 1 ) ) != 0,
            EdhrecRank    = edhrecRank,
            PennyRank     = pennyRank,
        };
    }

    private GameplayFace ReadFace( ref ByteStream s ) => new()
    {
        OracleId       = ReadNullableGuid( ref s ),
        Name           = s.Read<string>( "" ) ?? "",
        ManaCostRaw    = s.Read<string>( "" ) ?? "",
        ManaCost       = ReadManaCost( ref s ),
        Cmc            = s.Read<float>(),
        TypeLine       = s.Read<string>( "" ) ?? "",
        Supertypes     = ReadStringIndices( ref s, _typeWords ),
        CardTypes      = ReadStringIndices( ref s, _typeWords ),
        Subtypes       = ReadStringIndices( ref s, _typeWords ),
        OracleText     = s.Read<string>( "" ) ?? "",
        Colors         = (ColorSet)s.Read<byte>(),
        ColorIndicator = (ColorSet)s.Read<byte>(),
        Power          = ReadNullableStat( ref s ),
        Toughness      = ReadNullableStat( ref s ),
        Loyalty        = ReadNullableStat( ref s ),
        Defense        = ReadNullableStat( ref s ),
        Layout         = ReadNullableLayout( ref s ),
    };

    // ── Primitives ───────────────────────────────────────────────

    private static Guid ReadGuid( ref ByteStream s )
    {
        var bytes = new byte[16];
        s.Read( bytes, 0, 16 );
        return new Guid( bytes );
    }

    private static Guid? ReadNullableGuid( ref ByteStream s )
        => s.Read<byte>() == 1 ? ReadGuid( ref s ) : null;

    private static int? ReadNullableInt( ref ByteStream s )
    {
        int v = s.Read<int>();
        return v == -1 ? null : v;
    }

    private static CardStat? ReadNullableStat( ref ByteStream s )
    {
        if ( s.Read<byte>() == 0 ) return null;
        var type     = (StatType)s.Read<byte>();
        int value    = s.Read<int>();
        int modifier = s.Read<int>();
        return type switch
        {
            StatType.Numeric  => CardStat.FromNumeric( value ),
            StatType.Combined => CardStat.FromCombined( modifier ),
            StatType.Variable => CardStat.Variable,
            StatType.X        => CardStat.X,
            _                 => CardStat.Variable
        };
    }

    private static List<string> ReadStringIndices( ref ByteStream s, string[] catalog )
    {
        int count  = s.Read<byte>();
        var result = new List<string>( count );
        for ( int i = 0; i < count; i++ )
        {
            ushort idx = s.Read<ushort>();
            result.Add( idx < catalog.Length ? catalog[idx] : "" );
        }
        return result;
    }

    private static List<ManaCostSymbol> ReadManaCost( ref ByteStream s )
    {
        int count  = s.Read<byte>();
        var result = new List<ManaCostSymbol>( count );
        for ( int i = 0; i < count; i++ )
        {
            var   raw   = s.Read<string>( "" ) ?? "";
            byte  pc    = s.Read<byte>();
            byte  sc    = s.Read<byte>();
            float cmc   = s.Read<float>();
            byte  flags = s.Read<byte>();
            result.Add( new ManaCostSymbol(
                raw,
                pc == 0xFF ? null : (MtgColor?)pc,
                sc == 0xFF ? null : (MtgColor?)sc,
                cmc,
                ( flags & 1 ) != 0,
                ( flags & 2 ) != 0,
                ( flags & 4 ) != 0,
                ( flags & 8 ) != 0
            ) );
        }
        return result;
    }

    private static List<RelatedCard> ReadRelatedCards( ref ByteStream s )
    {
        int count  = s.Read<byte>();
        var result = new List<RelatedCard>( count );
        for ( int i = 0; i < count; i++ )
            result.Add( new RelatedCard
            {
                ScryfallId = ReadGuid( ref s ),
                Component  = (RelatedCardComponent)s.Read<byte>(),
                Name       = s.Read<string>( "" ) ?? "",
                TypeLine   = s.Read<string>( "" ) ?? "",
            } );
        return result;
    }

    private static MtgLayout? ReadNullableLayout( ref ByteStream s )
    {
        byte b = s.Read<byte>();
        return b == 0xFF ? null : (MtgLayout)b;
    }

    private static Dictionary<string, string> ReadImageUris( ref ByteStream s )
    {
        int count  = s.Read<byte>();
        var result = new Dictionary<string, string>( count );
        for ( int i = 0; i < count; i++ )
            result[s.Read<string>( "" ) ?? ""] = s.Read<string>( "" ) ?? "";
        return result;
    }

    private static List<MtgFinish> UnpackFinishes( byte b )
    {
        var result = new List<MtgFinish>( 3 );
        if ( ( b & 1 ) != 0 ) result.Add( MtgFinish.Nonfoil );
        if ( ( b & 2 ) != 0 ) result.Add( MtgFinish.Foil );
        if ( ( b & 4 ) != 0 ) result.Add( MtgFinish.Etched );
        return result;
    }

    private static List<MtgGame> UnpackGames( byte b )
    {
        var result = new List<MtgGame>( 5 );
        if ( ( b & 1  ) != 0 ) result.Add( MtgGame.Paper );
        if ( ( b & 2  ) != 0 ) result.Add( MtgGame.Arena );
        if ( ( b & 4  ) != 0 ) result.Add( MtgGame.Mtgo );
        if ( ( b & 8  ) != 0 ) result.Add( MtgGame.Astral );
        if ( ( b & 16 ) != 0 ) result.Add( MtgGame.Sega );
        return result;
    }
}


// ═══════════════════════════════════════════════════════════════════
//  PRINTING BLOB  (per-printing data from default_cards)
//
//  [ HEADER          ]  magic(4) + version(2) + count(4) + dataOffset(4)
//  [ PRIMARY INDEX   ]  count × ( ScryfallId(16) + offset(4) )
//  [ SECONDARY INDEX ]  oracleCount × ( OracleId(16) + scryfallIdCount(2) + ScryfallId(16)... )
//  [ PRINTING DATA   ]  variable-length printing records
// ═══════════════════════════════════════════════════════════════════

public static class PrintingBlobWriter
{
    private const uint   Magic      = 0x4D544750; // "MTGP"
    private const ushort Version    = 1;
    private const int    HeaderSize = 14;

    public static void Write( BaseFileSystem fs, string path, List<GameplayPrinting> printings )
    {
        // Sort by ScryfallId for deterministic layout
        var sorted = new List<GameplayPrinting>( printings );
        sorted.Sort( ( a, b ) => a.ScryfallId.CompareTo( b.ScryfallId ) );

        int count = sorted.Count;

        // Build secondary index: OracleId → List<ScryfallId>
        var secondaryIndex = new Dictionary<Guid, List<Guid>>();
        foreach ( var p in sorted )
        {
            if ( !p.OracleId.HasValue ) continue;
            if ( !secondaryIndex.TryGetValue( p.OracleId.Value, out var list ) )
            {
                list = new List<Guid>();
                secondaryIndex[p.OracleId.Value] = list;
            }
            list.Add( p.ScryfallId );
        }

        // Serialize secondary index
        byte[] secondaryBytes = SerializeSecondaryIndex( secondaryIndex );

        // Serialize printing records
        var    offsets       = new int[count];
        byte[] printingData  = SerializePrintings( sorted, offsets );

        int primaryIndexSize   = count * ( 16 + 4 );
        int dataOffset         = HeaderSize + primaryIndexSize + secondaryBytes.Length;
        int totalSize          = dataOffset + printingData.Length;

        var blob = ByteStream.Create( totalSize );
        try
        {
            blob.Write( Magic );
            blob.Write( Version );
            blob.Write( count );
            blob.Write( dataOffset );

            // Primary index
            for ( int i = 0; i < sorted.Count; i++ )
            {
                WriteGuid( ref blob, sorted[i].ScryfallId );
                blob.Write( offsets[i] );
            }

            blob.Write( secondaryBytes );
            blob.Write( printingData );

            fs.WriteAllBytes( path, blob.ToArray() );
        }
        finally { blob.Dispose(); }
    }

    private static byte[] SerializeSecondaryIndex( Dictionary<Guid, List<Guid>> index )
    {
        var s = ByteStream.Create( 256 * 1024 );
        try
        {
            s.Write( index.Count );
            foreach ( var kv in index )
            {
                WriteGuid( ref s, kv.Key );
                s.Write( (ushort)kv.Value.Count );
                foreach ( var id in kv.Value )
                    WriteGuid( ref s, id );
            }
            return s.ToArray();
        }
        finally { s.Dispose(); }
    }

    private static byte[] SerializePrintings( List<GameplayPrinting> sorted, int[] offsets )
    {
        var s = ByteStream.Create( 16 * 1024 * 1024 );
        try
        {
            for ( int i = 0; i < sorted.Count; i++ )
            {
                offsets[i] = s.Position;
                WritePrinting( ref s, sorted[i] );
            }
            return s.ToArray();
        }
        finally { s.Dispose(); }
    }

    private static void WritePrinting( ref ByteStream s, GameplayPrinting p )
    {
        // Identity
        WriteGuid( ref s, p.ScryfallId );
        WriteNullableGuid( ref s, p.OracleId );

        // Set info
        s.Write( p.Set );
        WriteGuid( ref s, p.SetId );
        s.Write( p.SetName );
        s.Write( p.CollectorNumber );
        s.Write( p.ReleasedAt );

        // Print characteristics
        s.Write( (byte)p.Rarity );
        s.Write( (byte)p.BorderColor );
        s.Write( (byte)p.ImageStatus );
        s.Write( (byte)p.SecurityStamp );
        s.Write( PackFinishes( p.Finishes ) );
        s.Write( PackGames( p.Games ) );

        // Card-level image uris (absent for multi-face)
        WriteImageUris( ref s, p.ImageUris );

        // Face art
        s.Write( (byte)p.FaceArt.Count );
        foreach ( var fa in p.FaceArt )
            WriteFaceArt( ref s, fa );

        // Flags
        s.Write( PackFlags( p ) );

        // External IDs
        WriteNullableInt( ref s, p.ArenaId );
        WriteNullableInt( ref s, p.MtgoId );
        WriteNullableInt( ref s, p.TcgPlayerId );
        WriteNullableInt( ref s, p.CardmarketId );
        WriteGuid( ref s, p.CardBackId );
        WriteNullableGuid( ref s, p.VariationOf );

        // Promo types
        s.Write( (byte)p.PromoTypes.Count );
        foreach ( var pt in p.PromoTypes ) s.Write( pt );

        // Prices
        s.Write( p.PriceUsd );
        s.Write( p.PriceUsdFoil );
        s.Write( p.PriceUsdEtched );
        s.Write( p.PriceEur );
        s.Write( p.PriceEurFoil );
        s.Write( p.PriceTix );
    }

    private static void WriteFaceArt( ref ByteStream s, GameplayFaceArt fa )
    {
        s.Write( fa.Artist );
        WriteNullableGuid( ref s, fa.ArtistId );
        WriteNullableGuid( ref s, fa.IllustrationId );
        s.Write( fa.FlavorName );
        s.Write( fa.FlavorText );
        s.Write( fa.Watermark );
        WriteImageUris( ref s, fa.ImageUris );
    }

    // ── Primitives ───────────────────────────────────────────────

    private static void WriteGuid( ref ByteStream s, Guid g )
    {
        Span<byte> bytes = stackalloc byte[16];
        g.TryWriteBytes( bytes );
        s.Write( bytes.ToArray() );
    }

    private static void WriteNullableGuid( ref ByteStream s, Guid? g )
    {
        s.Write( g.HasValue ? (byte)1 : (byte)0 );
        if ( g.HasValue ) WriteGuid( ref s, g.Value );
    }

    private static void WriteNullableInt( ref ByteStream s, int? v )
        => s.Write( v ?? -1 );

    private static void WriteImageUris( ref ByteStream s, Dictionary<string, string> uris )
    {
        if ( uris == null || uris.Count == 0 ) { s.Write( (byte)0 ); return; }
        s.Write( (byte)uris.Count );
        foreach ( var kv in uris )
        {
            s.Write( kv.Key );
            s.Write( kv.Value );
        }
    }

    private static byte PackFinishes( List<MtgFinish> finishes )
    {
        byte b = 0;
        foreach ( var f in finishes )
            b |= f switch
            {
                MtgFinish.Nonfoil => 1,
                MtgFinish.Foil    => 2,
                MtgFinish.Etched  => 4,
                _                 => 0
            };
        return b;
    }

    private static byte PackGames( List<MtgGame> games )
    {
        byte b = 0;
        foreach ( var g in games )
            b |= g switch
            {
                MtgGame.Paper  => 1,
                MtgGame.Arena  => 2,
                MtgGame.Mtgo   => 4,
                MtgGame.Astral => 8,
                MtgGame.Sega   => 16,
                _              => 0
            };
        return b;
    }

    private static ushort PackFlags( GameplayPrinting p )
    {
        ushort f = 0;
        if ( p.IsPromo           ) f |= 1 << 0;
        if ( p.IsReprint         ) f |= 1 << 1;
        if ( p.IsFullArt         ) f |= 1 << 2;
        if ( p.IsOversized       ) f |= 1 << 3;
        if ( p.IsTextless        ) f |= 1 << 4;
        if ( p.IsStorySpotlight  ) f |= 1 << 5;
        if ( p.IsBooster         ) f |= 1 << 6;
        if ( p.IsDigital         ) f |= 1 << 7;
        if ( p.IsVariation       ) f |= 1 << 8;
        if ( p.HasContentWarning ) f |= 1 << 9;
        if ( p.HasHighResImage   ) f |= 1 << 10;
        return f;
    }
}


public sealed class PrintingBlobReader
{
    private byte[]                        _blob;
    private int                           _dataOffset;
    private Dictionary<Guid, int>         _primaryIndex;   // ScryfallId → offset
    private Dictionary<Guid, List<Guid>>  _secondaryIndex; // OracleId   → ScryfallIds

    public int PrintingCount => _primaryIndex.Count;

    public static async Task<PrintingBlobReader> LoadAsync( BaseFileSystem fs, string path )
    {
        byte[] data = await fs.ReadAllBytesAsync( path );
        var reader  = new PrintingBlobReader();
        reader.Initialize( data );
        return reader;
    }

    private void Initialize( byte[] data )
    {
        _blob = data;
        var s = ByteStream.CreateReader( data );

        uint   magic = s.Read<uint>();
        ushort ver   = s.Read<ushort>();
        int    count = s.Read<int>();
        _dataOffset  = s.Read<int>();

        if ( magic != 0x4D544750 )
            throw new Exception( "Invalid printing blob — bad magic bytes." );
        if ( ver != 1 )
            throw new Exception( $"Printing blob version mismatch — expected 1, got {ver}. Rebuild the blob." );

        // Primary index
        _primaryIndex = new Dictionary<Guid, int>( count );
        for ( int i = 0; i < count; i++ )
        {
            var id     = ReadGuid( ref s );
            int offset = s.Read<int>();
            _primaryIndex[id] = offset;
        }

        // Secondary index
        int oracleCount  = s.Read<int>();
        _secondaryIndex  = new Dictionary<Guid, List<Guid>>( oracleCount );
        for ( int i = 0; i < oracleCount; i++ )
        {
            var oracleId    = ReadGuid( ref s );
            ushort idCount  = s.Read<ushort>();
            var ids         = new List<Guid>( idCount );
            for ( int j = 0; j < idCount; j++ )
                ids.Add( ReadGuid( ref s ) );
            _secondaryIndex[oracleId] = ids;
        }
    }

    /// Fetch a single printing by Scryfall ID.
    public GameplayPrinting Fetch( Guid scryfallId )
    {
        if ( !_primaryIndex.TryGetValue( scryfallId, out int offset ) )
            return null;

        var s = ByteStream.CreateReader( _blob );
        s.Position = _dataOffset + offset;
        return ReadPrinting( ref s );
    }

    /// Fetch all printings for a given oracle ID.
    public List<GameplayPrinting> FetchAllForOracle( Guid oracleId )
    {
        if ( !_secondaryIndex.TryGetValue( oracleId, out var ids ) )
            return new List<GameplayPrinting>();

        var result = new List<GameplayPrinting>( ids.Count );
        foreach ( var id in ids )
        {
            var p = Fetch( id );
            if ( p != null ) result.Add( p );
        }
        return result;
    }

    /// Fetch the preferred printing for a given oracle ID.
    /// Preference order: non-promo highres paper > any paper > anything.
    public GameplayPrinting FetchPreferred( Guid oracleId )
    {
        var all = FetchAllForOracle( oracleId );
        if ( all.Count == 0 ) return null;
        if ( all.Count == 1 ) return all[0];

        return all
            .OrderBy( p => p.IsPromo ? 1 : 0 )
            .ThenByDescending( p => p.HasHighResImage ? 1 : 0 )
            .ThenBy( p => p.Games.Contains( MtgGame.Paper ) ? 0 : 1 )
            .First();
    }

    public bool Contains( Guid scryfallId ) => _primaryIndex.ContainsKey( scryfallId );

    // ── Deserialization ──────────────────────────────────────────

    private GameplayPrinting ReadPrinting( ref ByteStream s )
    {
        var scryfallId = ReadGuid( ref s );
        var oracleId   = ReadNullableGuid( ref s );

        var set             = s.Read<string>( "" ) ?? "";
        var setId           = ReadGuid( ref s );
        var setName         = s.Read<string>( "" ) ?? "";
        var collectorNumber = s.Read<string>( "" ) ?? "";
        var releasedAt      = s.Read<string>( "" ) ?? "";

        var rarity        = (MtgRarity)s.Read<byte>();
        var borderColor   = (MtgBorderColor)s.Read<byte>();
        var imageStatus   = (MtgImageStatus)s.Read<byte>();
        var securityStamp = (MtgSecurityStamp)s.Read<byte>();
        var finishes      = UnpackFinishes( s.Read<byte>() );
        var games         = UnpackGames( s.Read<byte>() );
        var imageUris     = ReadImageUris( ref s );

        int faceArtCount = s.Read<byte>();
        var faceArt      = new List<GameplayFaceArt>( faceArtCount );
        for ( int i = 0; i < faceArtCount; i++ )
            faceArt.Add( ReadFaceArt( ref s ) );

        var flags = s.Read<ushort>();

        var arenaId      = ReadNullableInt( ref s );
        var mtgoId       = ReadNullableInt( ref s );
        var tcgPlayerId  = ReadNullableInt( ref s );
        var cardmarketId = ReadNullableInt( ref s );
        var cardBackId   = ReadGuid( ref s );
        var variationOf  = ReadNullableGuid( ref s );

        int promoTypeCount = s.Read<byte>();
        var promoTypes     = new List<string>( promoTypeCount );
        for ( int i = 0; i < promoTypeCount; i++ )
            promoTypes.Add( s.Read<string>( "" ) ?? "" );

        var priceUsd       = s.Read<string>( "" ) ?? "";
        var priceUsdFoil   = s.Read<string>( "" ) ?? "";
        var priceUsdEtched = s.Read<string>( "" ) ?? "";
        var priceEur       = s.Read<string>( "" ) ?? "";
        var priceEurFoil   = s.Read<string>( "" ) ?? "";
        var priceTix       = s.Read<string>( "" ) ?? "";

        return new GameplayPrinting
        {
            ScryfallId        = scryfallId,
            OracleId          = oracleId,
            Set               = set,
            SetId             = setId,
            SetName           = setName,
            CollectorNumber   = collectorNumber,
            ReleasedAt        = releasedAt,
            Rarity            = rarity,
            BorderColor       = borderColor,
            ImageStatus       = imageStatus,
            SecurityStamp     = securityStamp,
            Finishes          = finishes,
            Games             = games,
            ImageUris         = imageUris,
            FaceArt           = faceArt,
            IsPromo           = ( flags & ( 1 << 0  ) ) != 0,
            IsReprint         = ( flags & ( 1 << 1  ) ) != 0,
            IsFullArt         = ( flags & ( 1 << 2  ) ) != 0,
            IsOversized       = ( flags & ( 1 << 3  ) ) != 0,
            IsTextless        = ( flags & ( 1 << 4  ) ) != 0,
            IsStorySpotlight  = ( flags & ( 1 << 5  ) ) != 0,
            IsBooster         = ( flags & ( 1 << 6  ) ) != 0,
            IsDigital         = ( flags & ( 1 << 7  ) ) != 0,
            IsVariation       = ( flags & ( 1 << 8  ) ) != 0,
            HasContentWarning = ( flags & ( 1 << 9  ) ) != 0,
            HasHighResImage   = ( flags & ( 1 << 10 ) ) != 0,
            ArenaId           = arenaId,
            MtgoId            = mtgoId,
            TcgPlayerId       = tcgPlayerId,
            CardmarketId      = cardmarketId,
            CardBackId        = cardBackId,
            VariationOf       = variationOf,
            PromoTypes        = promoTypes,
            PriceUsd          = priceUsd,
            PriceUsdFoil      = priceUsdFoil,
            PriceUsdEtched    = priceUsdEtched,
            PriceEur          = priceEur,
            PriceEurFoil      = priceEurFoil,
            PriceTix          = priceTix,
        };
    }

    private GameplayFaceArt ReadFaceArt( ref ByteStream s ) => new()
    {
        Artist         = s.Read<string>( "" ) ?? "",
        ArtistId       = ReadNullableGuid( ref s ),
        IllustrationId = ReadNullableGuid( ref s ),
        FlavorName     = s.Read<string>( "" ) ?? "",
        FlavorText     = s.Read<string>( "" ) ?? "",
        Watermark      = s.Read<string>( "" ) ?? "",
        ImageUris      = ReadImageUris( ref s ),
    };

    // ── Primitives ───────────────────────────────────────────────

    private static Guid ReadGuid( ref ByteStream s )
    {
        var bytes = new byte[16];
        s.Read( bytes, 0, 16 );
        return new Guid( bytes );
    }

    private static Guid? ReadNullableGuid( ref ByteStream s )
        => s.Read<byte>() == 1 ? ReadGuid( ref s ) : null;

    private static int? ReadNullableInt( ref ByteStream s )
    {
        int v = s.Read<int>();
        return v == -1 ? null : v;
    }

    private static Dictionary<string, string> ReadImageUris( ref ByteStream s )
    {
        int count  = s.Read<byte>();
        var result = new Dictionary<string, string>( count );
        for ( int i = 0; i < count; i++ )
            result[s.Read<string>( "" ) ?? ""] = s.Read<string>( "" ) ?? "";
        return result;
    }

    private static List<MtgFinish> UnpackFinishes( byte b )
    {
        var result = new List<MtgFinish>( 3 );
        if ( ( b & 1 ) != 0 ) result.Add( MtgFinish.Nonfoil );
        if ( ( b & 2 ) != 0 ) result.Add( MtgFinish.Foil );
        if ( ( b & 4 ) != 0 ) result.Add( MtgFinish.Etched );
        return result;
    }

    private static List<MtgGame> UnpackGames( byte b )
    {
        var result = new List<MtgGame>( 5 );
        if ( ( b & 1  ) != 0 ) result.Add( MtgGame.Paper );
        if ( ( b & 2  ) != 0 ) result.Add( MtgGame.Arena );
        if ( ( b & 4  ) != 0 ) result.Add( MtgGame.Mtgo );
        if ( ( b & 8  ) != 0 ) result.Add( MtgGame.Astral );
        if ( ( b & 16 ) != 0 ) result.Add( MtgGame.Sega );
        return result;
    }
}