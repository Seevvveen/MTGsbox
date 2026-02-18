using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sandbox.__Rewrite.Gameplay;

/// <summary>
/// All formats Scryfall reports legality for.
/// </summary>
public enum Format
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

/// <summary>
/// Legal status of a card in a given format.
/// </summary>
public enum LegalityStatus
{
    Unknown = 0,
    Legal,
    NotLegal,
    Banned,
    Restricted,     // Vintage only
}

/// <summary>
/// Per-format legality for a card. Replaces Dictionary&lt;string, string&gt;.
/// All formats are explicit properties — no string key lookups at call sites.
/// </summary>
public readonly record struct Legalities
{
    public LegalityStatus Standard       { get; init; }
    public LegalityStatus Pioneer        { get; init; }
    public LegalityStatus Modern         { get; init; }
    public LegalityStatus Legacy         { get; init; }
    public LegalityStatus Vintage        { get; init; }
    public LegalityStatus Commander      { get; init; }
    public LegalityStatus Oathbreaker    { get; init; }
    public LegalityStatus StandardBrawl  { get; init; }
    public LegalityStatus Brawl          { get; init; }
    public LegalityStatus Alchemy        { get; init; }
    public LegalityStatus Explorer       { get; init; }
    public LegalityStatus Historic       { get; init; }
    public LegalityStatus Timeless       { get; init; }
    public LegalityStatus Pauper         { get; init; }
    public LegalityStatus PauperCommander{ get; init; }
    public LegalityStatus Penny          { get; init; }
    public LegalityStatus OldSchool      { get; init; }
    public LegalityStatus PreModern      { get; init; }
    public LegalityStatus PreDh          { get; init; }

    /// <summary>Returns the legality status for any format by enum value.</summary>
    public LegalityStatus Get( Format format ) => format switch
    {
        Format.Standard        => Standard,
        Format.Pioneer         => Pioneer,
        Format.Modern          => Modern,
        Format.Legacy          => Legacy,
        Format.Vintage         => Vintage,
        Format.Commander       => Commander,
        Format.Oathbreaker     => Oathbreaker,
        Format.StandardBrawl   => StandardBrawl,
        Format.Brawl           => Brawl,
        Format.Alchemy         => Alchemy,
        Format.Explorer        => Explorer,
        Format.Historic        => Historic,
        Format.Timeless        => Timeless,
        Format.Pauper          => Pauper,
        Format.PauperCommander => PauperCommander,
        Format.Penny           => Penny,
        Format.OldSchool       => OldSchool,
        Format.PreModern       => PreModern,
        Format.PreDh           => PreDh,
        _                      => LegalityStatus.Unknown,
    };

    /// <summary>True if the card is legal or restricted in the given format.</summary>
    public bool IsPlayableIn( Format format )
    {
        var status = Get( format );
        return status == LegalityStatus.Legal || status == LegalityStatus.Restricted;
    }

    /// <summary>
    /// Parses a Scryfall legalities dictionary into the typed struct.
    /// </summary>
    public static Legalities Parse( Dictionary<string, string> raw )
    {
        if ( raw == null )
            return default;

        return new Legalities
        {
            Standard        = ParseStatus( raw, "standard" ),
            Pioneer         = ParseStatus( raw, "pioneer" ),
            Modern          = ParseStatus( raw, "modern" ),
            Legacy          = ParseStatus( raw, "legacy" ),
            Vintage         = ParseStatus( raw, "vintage" ),
            Commander       = ParseStatus( raw, "commander" ),
            Oathbreaker     = ParseStatus( raw, "oathbreaker" ),
            StandardBrawl   = ParseStatus( raw, "standardbrawl" ),
            Brawl           = ParseStatus( raw, "brawl" ),
            Alchemy         = ParseStatus( raw, "alchemy" ),
            Explorer        = ParseStatus( raw, "explorer" ),
            Historic        = ParseStatus( raw, "historic" ),
            Timeless        = ParseStatus( raw, "timeless" ),
            Pauper          = ParseStatus( raw, "pauper" ),
            PauperCommander = ParseStatus( raw, "paupercommander" ),
            Penny           = ParseStatus( raw, "penny" ),
            OldSchool       = ParseStatus( raw, "oldschool" ),
            PreModern       = ParseStatus( raw, "premodern" ),
            PreDh           = ParseStatus( raw, "predh" ),
        };
    }

    private static LegalityStatus ParseStatus( Dictionary<string, string> raw, string key )
    {
        if ( !raw.TryGetValue( key, out var value ) )
            return LegalityStatus.Unknown;

        return value switch
        {
            "legal"      => LegalityStatus.Legal,
            "not_legal"  => LegalityStatus.NotLegal,
            "banned"     => LegalityStatus.Banned,
            "restricted" => LegalityStatus.Restricted,
            _            => LegalityStatus.Unknown,
        };
    }
}