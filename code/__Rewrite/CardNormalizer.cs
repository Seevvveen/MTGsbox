using System;
using Sandbox.__Rewrite.Types;

namespace Sandbox.__Rewrite;

// ═══════════════════════════════════════════════════════════════════
//  SHARED PARSE HELPERS
// ═══════════════════════════════════════════════════════════════════

internal static class ScryfallParsers
{
    internal static readonly HashSet<string> KnownSupertypes = new( StringComparer.OrdinalIgnoreCase )
    {
        "Legendary", "Basic", "Snow", "World", "Ongoing", "Elite", "Host", "Token"
    };

    internal static Guid ParseGuid( string s )
        => Guid.TryParse( s, out var g ) ? g : Guid.Empty;

    internal static Guid? TryParseGuid( string s )
        => Guid.TryParse( s, out var g ) ? g : null;

    internal static (List<string> Supers, List<string> Types, List<string> Subs) ParseTypeLine( string typeLine )
    {
        if ( string.IsNullOrWhiteSpace( typeLine ) )
            return (new(), new(), new());

        string[] halves     = typeLine.Split( '—', 2 );
        string[] leftTokens = halves[0].Trim().Split( ' ', StringSplitOptions.RemoveEmptyEntries );

        var supers = new List<string>();
        var types  = new List<string>();

        foreach ( var token in leftTokens )
        {
            if ( KnownSupertypes.Contains( token ) ) supers.Add( token );
            else                                      types.Add( token );
        }

        var subs = halves.Length > 1
            ? new List<string>( halves[1].Trim().Split( ' ', StringSplitOptions.RemoveEmptyEntries ) )
            : new List<string>();

        return (supers, types, subs);
    }

    internal static List<ManaCostSymbol> ParseManaCost( string manaCost )
    {
        var result = new List<ManaCostSymbol>();
        if ( string.IsNullOrWhiteSpace( manaCost ) ) return result;

        int i = 0;
        while ( i < manaCost.Length )
        {
            if ( manaCost[i] != '{' ) { i++; continue; }
            int close = manaCost.IndexOf( '}', i );
            if ( close < 0 ) break;
            result.Add( TokenToSymbol( manaCost.Substring( i + 1, close - i - 1 ) ) );
            i = close + 1;
        }
        return result;
    }

    private static ManaCostSymbol TokenToSymbol( string token )
    {
        bool isHybrid    = token.Contains( '/' ) && !token.StartsWith( "H" );
        bool isPhyrexian = token.StartsWith( "H" ) || token.EndsWith( "/P" );
        bool isVariable  = token is "X" or "Y" or "Z";
        string clean     = token.Replace( "/P", "" ).Replace( "H", "" );

        if ( isHybrid )
        {
            var parts     = clean.Split( '/' );
            var primary   = ParseSingleColor( parts[0] );
            var secondary = parts.Length > 1 ? ParseSingleColor( parts[1] ) : null;
            return new ManaCostSymbol( $"{{{token}}}", primary, secondary, 1f, false, true, isPhyrexian, false );
        }

        if ( int.TryParse( clean, out int generic ) )
            return new ManaCostSymbol( $"{{{token}}}", null, null, generic, true, false, false, false );

        if ( isVariable )
            return new ManaCostSymbol( $"{{{token}}}", null, null, 0f, false, false, false, true );

        return new ManaCostSymbol( $"{{{token}}}", ParseSingleColor( clean ), null, 1f, false, false, isPhyrexian, false );
    }

    internal static MtgColor? ParseSingleColor( string s ) => s switch
    {
        "W" => MtgColor.White,
        "U" => MtgColor.Blue,
        "B" => MtgColor.Black,
        "R" => MtgColor.Red,
        "G" => MtgColor.Green,
        "C" => MtgColor.Colorless,
        _   => null
    };

    internal static ColorSet ParseColorSet( List<string> colors )
    {
        if ( colors == null ) return ColorSet.None;
        var result = ColorSet.None;
        foreach ( var c in colors )
            result |= c switch
            {
                "W" => ColorSet.White,
                "U" => ColorSet.Blue,
                "B" => ColorSet.Black,
                "R" => ColorSet.Red,
                "G" => ColorSet.Green,
                _   => ColorSet.None
            };
        return result;
    }

    internal static MtgLayout ParseLayout( string s ) => s switch
    {
        "normal"             => MtgLayout.Normal,
        "split"              => MtgLayout.Split,
        "flip"               => MtgLayout.Flip,
        "transform"          => MtgLayout.Transform,
        "modal_dfc"          => MtgLayout.ModalDfc,
        "meld"               => MtgLayout.Meld,
        "leveler"            => MtgLayout.Leveler,
        "class"              => MtgLayout.Class,
        "case"               => MtgLayout.Case,
        "saga"               => MtgLayout.Saga,
        "adventure"          => MtgLayout.Adventure,
        "mutate"             => MtgLayout.Mutate,
        "prototype"          => MtgLayout.Prototype,
        "battle"             => MtgLayout.Battle,
        "planar"             => MtgLayout.Planar,
        "scheme"             => MtgLayout.Scheme,
        "vanguard"           => MtgLayout.Vanguard,
        "token"              => MtgLayout.Token,
        "double_faced_token" => MtgLayout.DoubleFacedToken,
        "emblem"             => MtgLayout.Emblem,
        "augment"            => MtgLayout.Augment,
        "host"               => MtgLayout.Host,
        "art_series"         => MtgLayout.ArtSeries,
        "reversible_card"    => MtgLayout.ReversibleCard,
        _                    => MtgLayout.Unknown
    };

    internal static MtgRarity ParseRarity( string s ) => s switch
    {
        "common"   => MtgRarity.Common,
        "uncommon" => MtgRarity.Uncommon,
        "rare"     => MtgRarity.Rare,
        "mythic"   => MtgRarity.Mythic,
        "special"  => MtgRarity.Special,
        "bonus"    => MtgRarity.Bonus,
        _          => MtgRarity.Common
    };

    internal static MtgBorderColor ParseBorderColor( string s ) => s switch
    {
        "white"      => MtgBorderColor.White,
        "borderless" => MtgBorderColor.Borderless,
        "yellow"     => MtgBorderColor.Yellow,
        "silver"     => MtgBorderColor.Silver,
        "gold"       => MtgBorderColor.Gold,
        _            => MtgBorderColor.Black
    };

    internal static MtgImageStatus ParseImageStatus( string s ) => s switch
    {
        "placeholder"  => MtgImageStatus.Placeholder,
        "lowres"       => MtgImageStatus.LowRes,
        "highres_scan" => MtgImageStatus.HighResScan,
        _              => MtgImageStatus.Missing
    };

    internal static MtgSecurityStamp ParseSecurityStamp( string s ) => s switch
    {
        "oval"     => MtgSecurityStamp.Oval,
        "triangle" => MtgSecurityStamp.Triangle,
        "acorn"    => MtgSecurityStamp.Acorn,
        "circle"   => MtgSecurityStamp.Circle,
        "arena"    => MtgSecurityStamp.Arena,
        "heart"    => MtgSecurityStamp.Heart,
        _          => MtgSecurityStamp.None
    };

    internal static List<MtgFinish> ParseFinishes( List<string> raw )
    {
        if ( raw == null ) return new();
        var result = new List<MtgFinish>( raw.Count );
        foreach ( var f in raw )
            result.Add( f switch
            {
                "foil"   => MtgFinish.Foil,
                "etched" => MtgFinish.Etched,
                _        => MtgFinish.Nonfoil
            } );
        return result;
    }

    internal static List<MtgGame> ParseGames( List<string> raw )
    {
        if ( raw == null ) return new();
        var result = new List<MtgGame>( raw.Count );
        foreach ( var g in raw )
            result.Add( g switch
            {
                "arena"  => MtgGame.Arena,
                "mtgo"   => MtgGame.Mtgo,
                "astral" => MtgGame.Astral,
                "sega"   => MtgGame.Sega,
                _        => MtgGame.Paper
            } );
        return result;
    }
}


// ═══════════════════════════════════════════════════════════════════
//  CARD NORMALIZER  —  oracle data only
// ═══════════════════════════════════════════════════════════════════

public static class CardNormalizer
{
    public static GameplayCard Normalize( ScryfallCard raw )
    {
        bool isMultiFace = raw.CardFaces is { Count: > 1 };

        var faces = isMultiFace
            ? raw.CardFaces.ConvertAll( NormalizeFace )
            : new List<GameplayFace> { NormalizeSingleFace( raw ) };

        var (supers, types, subs) = ScryfallParsers.ParseTypeLine( raw.TypeLine );

        return new GameplayCard
        {
            ScryfallId    = ScryfallParsers.ParseGuid( raw.Id ),
            OracleId      = ScryfallParsers.TryParseGuid( raw.OracleId ),
            Name          = raw.Name ?? "",
            Layout        = ScryfallParsers.ParseLayout( raw.Layout ),

            Faces         = faces,

            ColorIdentity  = ScryfallParsers.ParseColorSet( raw.ColorIdentity ),
            ProducedMana   = ScryfallParsers.ParseColorSet( raw.ProducedMana ),

            ManaCostRaw    = raw.ManaCost ?? "",
            ManaCost       = ScryfallParsers.ParseManaCost( raw.ManaCost ),
            Cmc            = raw.Cmc,

            TypeLine       = raw.TypeLine ?? "",
            Supertypes     = supers,
            CardTypes      = types,
            Subtypes       = subs,

            OracleText     = raw.OracleText ?? "",
            Keywords       = raw.Keywords ?? new(),

            Power          = raw.Power     != null ? CardStat.Parse( raw.Power )     : null,
            Toughness      = raw.Toughness != null ? CardStat.Parse( raw.Toughness ) : null,
            Loyalty        = raw.Loyalty   != null ? CardStat.Parse( raw.Loyalty )   : null,
            Defense        = raw.Defense   != null ? CardStat.Parse( raw.Defense )   : null,

            HandModifier   = raw.HandModifier ?? "",
            LifeModifier   = raw.LifeModifier ?? "",

            Legalities     = LegalityMap.Build( raw.Legalities ?? new() ),

            RelatedCards   = raw.AllParts?.ConvertAll( NormalizeRelated ) ?? new(),

            IsReserved     = raw.Reserved,
            IsGameChanger  = raw.GameChanger ?? false,

            EdhrecRank     = raw.EdhrecRank,
            PennyRank      = raw.PennyRank,
        };
    }

    private static GameplayFace NormalizeSingleFace( ScryfallCard c )
    {
        var (supers, types, subs) = ScryfallParsers.ParseTypeLine( c.TypeLine );
        return new GameplayFace
        {
            Name           = c.Name ?? "",
            ManaCostRaw    = c.ManaCost ?? "",
            ManaCost       = ScryfallParsers.ParseManaCost( c.ManaCost ),
            Cmc            = c.Cmc,
            TypeLine       = c.TypeLine ?? "",
            Supertypes     = supers,
            CardTypes      = types,
            Subtypes       = subs,
            OracleText     = c.OracleText ?? "",
            Colors         = ScryfallParsers.ParseColorSet( c.Colors ),
            ColorIndicator = ScryfallParsers.ParseColorSet( c.ColorIndicator ),
            Power          = c.Power     != null ? CardStat.Parse( c.Power )     : null,
            Toughness      = c.Toughness != null ? CardStat.Parse( c.Toughness ) : null,
            Loyalty        = c.Loyalty   != null ? CardStat.Parse( c.Loyalty )   : null,
            Defense        = c.Defense   != null ? CardStat.Parse( c.Defense )   : null,
        };
    }

    private static GameplayFace NormalizeFace( ScryfallCardFace f )
    {
        var (supers, types, subs) = ScryfallParsers.ParseTypeLine( f.TypeLine );
        return new GameplayFace
        {
            OracleId       = ScryfallParsers.TryParseGuid( f.OracleId ),
            Name           = f.Name ?? "",
            ManaCostRaw    = f.ManaCost ?? "",
            ManaCost       = ScryfallParsers.ParseManaCost( f.ManaCost ),
            Cmc            = f.Cmc ?? 0f,
            TypeLine       = f.TypeLine ?? "",
            Supertypes     = supers,
            CardTypes      = types,
            Subtypes       = subs,
            OracleText     = f.OracleText ?? "",
            Colors         = ScryfallParsers.ParseColorSet( f.Colors ),
            ColorIndicator = ScryfallParsers.ParseColorSet( f.ColorIndicator ),
            Power          = f.Power     != null ? CardStat.Parse( f.Power )     : null,
            Toughness      = f.Toughness != null ? CardStat.Parse( f.Toughness ) : null,
            Loyalty        = f.Loyalty   != null ? CardStat.Parse( f.Loyalty )   : null,
            Defense        = f.Defense   != null ? CardStat.Parse( f.Defense )   : null,
            Layout         = f.Layout    != null ? ScryfallParsers.ParseLayout( f.Layout ) : null,
        };
    }

    private static RelatedCard NormalizeRelated( ScryfallRelatedCard r ) => new()
    {
        ScryfallId = ScryfallParsers.ParseGuid( r.Id ),
        Component  = r.Component switch
        {
            "token"       => RelatedCardComponent.Token,
            "meld_part"   => RelatedCardComponent.MeldPart,
            "meld_result" => RelatedCardComponent.MeldResult,
            _             => RelatedCardComponent.ComboPiece
        },
        Name     = r.Name ?? "",
        TypeLine = r.TypeLine ?? ""
    };
}


// ═══════════════════════════════════════════════════════════════════
//  PRINTING NORMALIZER  —  per-printing data only
// ═══════════════════════════════════════════════════════════════════

public static class PrintingNormalizer
{
    public static GameplayPrinting Normalize( ScryfallCard raw )
    {
        var faceArt = new List<GameplayFaceArt>();

        if ( raw.CardFaces is { Count: > 1 } )
        {
            foreach ( var face in raw.CardFaces )
                faceArt.Add( new GameplayFaceArt
                {
                    Artist         = face.Artist ?? "",
                    ArtistId       = ScryfallParsers.TryParseGuid( face.ArtistId ),
                    IllustrationId = ScryfallParsers.TryParseGuid( face.IllustrationId ),
                    FlavorText     = face.FlavorText ?? "",
                    Watermark      = face.Watermark ?? "",
                    ImageUris      = face.ImageUris ?? new()
                } );
        }
        else
        {
            // Single face — wrap card-level art in a face art entry for consistency
            faceArt.Add( new GameplayFaceArt
            {
                Artist         = raw.Artist ?? "",
                ArtistId       = raw.ArtistIds is { Count: > 0 }
                                     ? ScryfallParsers.TryParseGuid( raw.ArtistIds[0] )
                                     : null,
                IllustrationId = ScryfallParsers.TryParseGuid( raw.IllustrationId ),
                FlavorName     = raw.FlavorName ?? "",
                FlavorText     = raw.FlavorText ?? "",
                Watermark      = raw.Watermark ?? "",
                ImageUris      = raw.ImageUris ?? new()
            } );
        }

        return new GameplayPrinting
        {
            ScryfallId      = ScryfallParsers.ParseGuid( raw.Id ),
            OracleId        = ScryfallParsers.TryParseGuid( raw.OracleId ),

            Set             = raw.Set ?? "",
            SetId           = ScryfallParsers.ParseGuid( raw.SetId ),
            SetName         = raw.SetName ?? "",
            CollectorNumber = raw.CollectorNumber ?? "",
            ReleasedAt      = raw.ReleasedAt ?? "",

            Rarity          = ScryfallParsers.ParseRarity( raw.Rarity ),
            BorderColor     = ScryfallParsers.ParseBorderColor( raw.BorderColor ),
            ImageStatus     = ScryfallParsers.ParseImageStatus( raw.ImageStatus ),
            SecurityStamp   = ScryfallParsers.ParseSecurityStamp( raw.SecurityStamp ),
            Finishes        = ScryfallParsers.ParseFinishes( raw.Finishes ),
            Games           = ScryfallParsers.ParseGames( raw.Games ),

            ImageUris       = raw.CardFaces is { Count: > 1 } ? new() : raw.ImageUris ?? new(),
            FaceArt         = faceArt,

            IsPromo           = raw.Promo,
            IsReprint         = raw.Reprint,
            IsFullArt         = raw.FullArt,
            IsOversized       = raw.Oversized,
            IsTextless        = raw.Textless,
            IsStorySpotlight  = raw.StorySpotlight,
            IsBooster         = raw.Booster,
            IsDigital         = raw.Digital,
            IsVariation       = raw.Variation,
            HasContentWarning = raw.ContentWarning ?? false,
            HasHighResImage   = raw.HighresImage,
            PromoTypes        = raw.PromoTypes ?? new(),

            ArenaId           = raw.ArenaId,
            MtgoId            = raw.MtgoId,
            TcgPlayerId       = raw.TcgplayerId,
            CardmarketId      = raw.CardmarketId,
            CardBackId        = ScryfallParsers.ParseGuid( raw.CardBackId ),
            VariationOf       = ScryfallParsers.TryParseGuid( raw.VariationOf ),

            PriceUsd          = raw.Prices?.Usd ?? "",
            PriceUsdFoil      = raw.Prices?.UsdFoil ?? "",
            PriceUsdEtched    = raw.Prices?.UsdEtched ?? "",
            PriceEur          = raw.Prices?.Eur ?? "",
            PriceEurFoil      = raw.Prices?.EurFoil ?? "",
            PriceTix          = raw.Prices?.Tix ?? "",
        };
    }
}