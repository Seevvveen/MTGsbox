using System;
using System.Collections.Generic;

namespace Sandbox.__Rewrite.Gameplay;

public enum Format : byte
{
    Unknown = 0,
    Standard,
    Pioneer,
    Modern,
    Legacy,
    Vintage,
    Commander,
    Oathbreaker,
    StandardBrawl,
    Brawl,
    Alchemy,
    Explorer,
    Historic,
    Timeless,
    Pauper,
    PauperCommander,
    Penny,
    OldSchool,
    PreModern,
    PreDh,
}

public enum LegalityStatus : byte
{
    Unknown = 0,
    Legal = 1,
    NotLegal = 2,
    Banned = 3,
    Restricted = 4,
}

/// <summary>
/// Packed legalities: 3 bits per format, stored in a ulong (supports up to 21 formats).
/// </summary>
public readonly record struct Legalities
{
    private const int BitsPer = 3;
    private const ulong Mask = (1UL << BitsPer) - 1UL;

    // Format enum values are contiguous; we use the enum numeric value as the index.
    private readonly ulong _bits;

    private Legalities( ulong bits ) => _bits = bits;

    public LegalityStatus Get( Format format )
    {
        int idx = (int)format;
        if ( idx <= 0 ) return LegalityStatus.Unknown;
        int shift = idx * BitsPer;
        if ( shift >= 64 ) return LegalityStatus.Unknown;

        return (LegalityStatus)( ( _bits >> shift ) & Mask );
    }

    public bool IsPlayableIn( Format format )
    {
        var s = Get( format );
        return s == LegalityStatus.Legal || s == LegalityStatus.Restricted;
    }

    public Legalities With( Format format, LegalityStatus status )
    {
        int idx = (int)format;
        if ( idx <= 0 ) return this;

        int shift = idx * BitsPer;
        if ( shift >= 64 ) return this;

        ulong cleared = _bits & ~(Mask << shift);
        ulong set = cleared | ( ( (ulong)status & Mask ) << shift );
        return new Legalities( set );
    }

    public static Legalities Parse( Dictionary<string, string> raw )
    {
        if ( raw == null ) return default;

        ulong bits = 0;

        foreach ( var kv in raw )
        {
            var fmt = ParseFormatKey( kv.Key );
            if ( fmt == Format.Unknown ) continue;

            var st = ParseStatusValue( kv.Value );

            int shift = (int)fmt * BitsPer;
            if ( shift >= 64 ) continue;

            bits &= ~(Mask << shift);
            bits |= ( ( (ulong)st & Mask ) << shift );
        }

        return new Legalities( bits );
    }

    private static LegalityStatus ParseStatusValue( string value ) => value switch
    {
        "legal"      => LegalityStatus.Legal,
        "not_legal"  => LegalityStatus.NotLegal,
        "banned"     => LegalityStatus.Banned,
        "restricted" => LegalityStatus.Restricted,
        _            => LegalityStatus.Unknown,
    };

    private static Format ParseFormatKey( string key ) => key switch
    {
        "standard"        => Format.Standard,
        "pioneer"         => Format.Pioneer,
        "modern"          => Format.Modern,
        "legacy"          => Format.Legacy,
        "vintage"         => Format.Vintage,
        "commander"       => Format.Commander,
        "oathbreaker"     => Format.Oathbreaker,
        "standardbrawl"   => Format.StandardBrawl,
        "brawl"           => Format.Brawl,
        "alchemy"         => Format.Alchemy,
        "explorer"        => Format.Explorer,
        "historic"        => Format.Historic,
        "timeless"        => Format.Timeless,
        "pauper"          => Format.Pauper,
        "paupercommander" => Format.PauperCommander,
        "penny"           => Format.Penny,
        "oldschool"       => Format.OldSchool,
        "premodern"       => Format.PreModern,
        "predh"           => Format.PreDh,
        _                 => Format.Unknown,
    };
}
