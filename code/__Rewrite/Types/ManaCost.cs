using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sandbox.__Rewrite.Gameplay;

/// <summary>
/// A parsed mana cost such as {2}{G}{G} or {X}{B}{B}.
/// Immutable value type — all parsing happens in <see cref="Parse"/>.
/// </summary>
[JsonConverter( typeof( ManaCostJsonConverter ) )]
public readonly record struct ManaCost
{
    /// <summary>The original Scryfall cost string e.g. "{2}{G}{G}". Empty string means explicitly no cost.</summary>
    public string Source { get; init; }

    /// <summary>Total mana value (CMC). Excludes X and half-mana.</summary>
    public int Cmc { get; init; }

    /// <summary>Which colors appear as pips (excludes hybrid/generic).</summary>
    public ManaColor Colors { get; init; }

    /// <summary>Raw generic mana component {N}.</summary>
    public int Generic { get; init; }

    /// <summary>True if the cost contains {X}.</summary>
    public bool HasX { get; init; }

    /// <summary>True if the cost contains any hybrid symbols e.g. {W/U}.</summary>
    public bool HasHybrid { get; init; }

    /// <summary>True if the cost contains any Phyrexian mana symbols e.g. {W/P}.</summary>
    public bool HasPhyrexian { get; init; }

    /// <summary>True if the cost contains {S} (snow mana).</summary>
    public bool HasSnow { get; init; }

    /// <summary>
    /// Total colored pips of the given color, including hybrid symbols that contain it.
    /// Used for devotion calculations.
    /// </summary>
    public int GetDevotion( ManaColor color )
    {
        if ( string.IsNullOrEmpty( Source ) )
            return 0;

        int devotion = 0;
        foreach ( var sym in EnumerateSymbols( Source ) )
            devotion += CountColorInSymbol( sym, color );

        return devotion;
    }

    /// <summary>Null represents a cost stored on faces (multi-face cards). Empty string is explicitly no cost.</summary>
    public bool IsNull     => Source == null;
    public bool IsNoCost   => Source == string.Empty;
    public bool IsFree     => !IsNull && !IsNoCost && Cmc == 0 && !HasX;   // {0}
    public bool IsColored  => Colors != ManaColor.None;

    public override string ToString() => Source ?? "(null)";

    // -------------------------
    // Parsing
    // -------------------------

    /// <summary>
    /// Parses a Scryfall mana cost string.
    /// <para>Pass <c>null</c> for multi-face cards where cost is stored per-face.</para>
    /// <para>Pass <c>""</c> for cards that explicitly have no mana cost (e.g. land).</para>
    /// </summary>
    public static ManaCost Parse( string raw )
    {
        // null → cost is on faces; preserve the distinction
        if ( raw == null )
            return new ManaCost { Source = null };

        // "" → explicitly no cost (e.g. some lands, Ancestral Vision)
        if ( raw == string.Empty )
            return new ManaCost { Source = string.Empty };

        int     generic     = 0;
        int     cmc         = 0;
        bool    hasX        = false;
        bool    hasHybrid   = false;
        bool    hasPhyrexian = false;
        bool    hasSnow     = false;
        var     colors      = ManaColor.None;

        foreach ( var symbol in EnumerateSymbols( raw ) )
        {
            if ( symbol.Equals( "X", StringComparison.OrdinalIgnoreCase ) )
            {
                hasX = true;
                continue;
            }

            if ( symbol.Equals( "S", StringComparison.OrdinalIgnoreCase ) )
            {
                hasSnow = true;
                cmc++;
                continue;
            }

            // Hybrid: W/U, B/G, 2/W …
            if ( symbol.Contains( '/' ) )
            {
                hasHybrid = true;
                var parts = symbol.Split( '/' );

                // Phyrexian hybrid: W/P
                if ( parts.Length == 2 && parts[1].Equals( "P", StringComparison.OrdinalIgnoreCase ) )
                    hasPhyrexian = true;

                // Add all color components found in the hybrid symbol
                foreach ( var part in parts )
                    colors |= ManaColorExtensions.ParseSymbol( part );

                cmc++;
                continue;
            }

            // Generic numeric: 2, 10, 15 …
            if ( int.TryParse( symbol, out int n ) )
            {
                generic += n;
                cmc     += n;
                continue;
            }

            // Single color pip: W, U, B, R, G, C
            var pip = ManaColorExtensions.ParseSymbol( symbol );
            if ( pip != ManaColor.None )
            {
                colors |= pip;
                cmc++;
                continue;
            }
        }

        return new ManaCost
        {
            Source       = raw,
            Cmc          = cmc,
            Colors       = colors,
            Generic      = generic,
            HasX         = hasX,
            HasHybrid    = hasHybrid,
            HasPhyrexian = hasPhyrexian,
            HasSnow      = hasSnow,
        };
    }

    // -------------------------
    // Helpers
    // -------------------------

    /// <summary>Splits "{2}{G}{G}" into ["2", "G", "G"].</summary>
    private static IEnumerable<string> EnumerateSymbols( string cost )
    {
        int i = 0;
        while ( i < cost.Length )
        {
            if ( cost[i] == '{' )
            {
                int close = cost.IndexOf( '}', i + 1 );
                if ( close > i )
                {
                    yield return cost.Substring( i + 1, close - i - 1 );
                    i = close + 1;
                    continue;
                }
            }
            i++;
        }
    }

    private static int CountColorInSymbol( string symbol, ManaColor color )
    {
        // Hybrid: each part of the symbol that matches counts once
        if ( symbol.Contains( '/' ) )
        {
            int count = 0;
            foreach ( var part in symbol.Split( '/' ) )
                if ( ManaColorExtensions.ParseSymbol( part ) == color )
                    count++;
            return count;
        }

        return ManaColorExtensions.ParseSymbol( symbol ) == color ? 1 : 0;
    }
}

// -------------------------
// JSON serialization — round-trips as the source string
// -------------------------

public sealed class ManaCostJsonConverter : JsonConverter<ManaCost>
{
    public override ManaCost Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
    {
        if ( reader.TokenType == JsonTokenType.Null )
            return ManaCost.Parse( null );

        return ManaCost.Parse( reader.GetString() );
    }

    public override void Write( Utf8JsonWriter writer, ManaCost value, JsonSerializerOptions options )
    {
        if ( value.IsNull )
            writer.WriteNullValue();
        else
            writer.WriteStringValue( value.Source );
    }
}