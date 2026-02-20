using System;
using System.Text.Json;

namespace Sandbox.__Rewrite.Gameplay;

/// <summary>
/// A parsed mana cost such as {2}{G}{G} or {X}{B}{B}.
/// Round-trips to/from JSON as the raw Scryfall string (or null / "").
/// Parsing is allocation-free (no Substring/Split) and Span-safe (no yield over Span).
/// </summary>
public readonly record struct ManaCost : IJsonConvert
{
    /// <summary>
    /// Original Scryfall cost string e.g. "{2}{G}{G}".
    /// Null means "stored per-face" (multi-face cards).
    /// Empty string means "explicitly no cost".
    /// </summary>
    public string Source { get; init; }

    /// <summary>Total mana value. Excludes X and half-mana.</summary>
    public ushort Cmc { get; init; }

    /// <summary>Which colors appear as pips (excludes generic).</summary>
    public ManaColor Colors { get; init; }

    /// <summary>The generic mana component {N}.</summary>
    public ushort Generic { get; init; }

    public ManaCostFlags Flags { get; init; }

    public bool IsNull        => Source == null;
    public bool IsNoCost      => Source == string.Empty;
    public bool HasX          => (Flags & ManaCostFlags.HasX) != 0;
    public bool HasHybrid     => (Flags & ManaCostFlags.HasHybrid) != 0;
    public bool HasPhyrexian  => (Flags & ManaCostFlags.HasPhyrexian) != 0;
    public bool HasSnow       => (Flags & ManaCostFlags.HasSnow) != 0;

    /// <summary>{0} — zero cost, distinct from no cost.</summary>
    public bool IsFree => !IsNull && !IsNoCost && Cmc == 0 && !HasX;

    public bool IsColored => Colors != ManaColor.None;

    public override string ToString() => Source ?? "(null)";

    /// <summary>
    /// Total colored pips of the given color, including hybrid symbols that contain it.
    /// Used for devotion calculations.
    /// </summary>
    public int GetDevotion( ManaColor color )
    {
        if ( string.IsNullOrEmpty( Source ) )
            return 0;

        int devotion = 0;

        EnumerateSymbols( Source.AsSpan(), sym =>
        {
            devotion += CountColorInSymbol( sym, color );
        } );

        return devotion;
    }

    /// <summary>
    /// Parses a Scryfall mana cost string.
    /// Pass null for multi-face cards where cost is stored per-face.
    /// Pass "" for cards that explicitly have no mana cost.
    /// </summary>
    public static ManaCost Parse( string raw )
    {
        if ( raw == null ) return new ManaCost { Source = null };
        if ( raw.Length == 0 ) return new ManaCost { Source = string.Empty };

        ushort generic = 0;
        ushort cmc = 0;
        ManaColor colors = ManaColor.None;
        ManaCostFlags flags = 0;

        EnumerateSymbols( raw.AsSpan(), sym =>
        {
            // X
            if ( sym.Length == 1 && (sym[0] == 'X' || sym[0] == 'x') )
            {
                flags |= ManaCostFlags.HasX;
                return;
            }

            // Snow
            if ( sym.Length == 1 && (sym[0] == 'S' || sym[0] == 's') )
            {
                flags |= ManaCostFlags.HasSnow;
                cmc += 1;
                return;
            }

            // Hybrid / phyrexian etc: contains '/'
            if ( sym.IndexOf( '/' ) >= 0 )
            {
                flags |= ManaCostFlags.HasHybrid;

                int start = 0;
                while ( start < sym.Length )
                {
                    int slash = sym.Slice( start ).IndexOf( '/' );
                    ReadOnlySpan<char> part = slash >= 0 ? sym.Slice( start, slash ) : sym.Slice( start );

                    // phyrexian marker: {W/P} etc (part "P")
                    if ( part.Length == 1 && (part[0] == 'P' || part[0] == 'p') )
                    {
                        flags |= ManaCostFlags.HasPhyrexian;
                    }
                    else
                    {
                        colors |= ParseSymbolSpan( part );
                    }

                    if ( slash < 0 ) break;
                    start += slash + 1;
                }

                cmc += 1;
                return;
            }

            // Generic number
            if ( ushort.TryParse( sym, out ushort n ) )
            {
                generic += n;
                cmc += n;
                return;
            }

            // Colored pip
            var pip = ParseSymbolSpan( sym );
            if ( pip != ManaColor.None )
            {
                colors |= pip;
                cmc += 1;
            }
        } );

        return new ManaCost
        {
            Source = raw,
            Cmc = cmc,
            Colors = colors,
            Generic = generic,
            Flags = flags
        };
    }

    // -------------------------
    // IJsonConvert — round-trips as the source string.
    // -------------------------
    public static object JsonRead( ref Utf8JsonReader reader, Type typeToConvert )
    {
        if ( reader.TokenType == JsonTokenType.Null )
            return Parse( null );

        return Parse( reader.GetString() );
    }

    public static void JsonWrite( object value, Utf8JsonWriter writer )
    {
        // Note: for non-nullable ManaCost, value will not be null here.
        // For ManaCost?, the factory may pass null.
        if ( value is null )
        {
            writer.WriteNullValue();
            return;
        }

        var mc = (ManaCost)value;

        if ( mc.IsNull )
            writer.WriteNullValue();
        else
            writer.WriteStringValue( mc.Source );
    }

    // -------------------------
    // Helpers (Span-safe, allocation-free)
    // -------------------------

    /// <summary>
    /// Enumerates symbols in "{2}{G}{G}" as spans: "2", "G", "G".
    /// Does not allocate and does not use iterators (ref-struct safe).
    /// </summary>
    private static void EnumerateSymbols( ReadOnlySpan<char> cost, Action<ReadOnlySpan<char>> visitor )
    {
        int i = 0;

        while ( i < cost.Length )
        {
            if ( cost[i] == '{' )
            {
                int close = cost.Slice( i + 1 ).IndexOf( '}' );
                if ( close >= 0 )
                {
                    visitor( cost.Slice( i + 1, close ) );
                    i += close + 2; // skip past "}"
                    continue;
                }
            }

            i++;
        }
    }

    private static ManaColor ParseSymbolSpan( ReadOnlySpan<char> s )
    {
        if ( s.Length != 1 ) return ManaColor.None;

        return s[0] switch
        {
            'W' or 'w' => ManaColor.White,
            'U' or 'u' => ManaColor.Blue,
            'B' or 'b' => ManaColor.Black,
            'R' or 'r' => ManaColor.Red,
            'G' or 'g' => ManaColor.Green,
            'C' or 'c' => ManaColor.Colorless,
            _ => ManaColor.None
        };
    }

    private static int CountColorInSymbol( ReadOnlySpan<char> sym, ManaColor color )
    {
        // Hybrid: "{W/U}", "{2/R}", "{G/P}", etc.
        if ( sym.IndexOf( '/' ) >= 0 )
        {
            int count = 0;
            int start = 0;

            while ( start < sym.Length )
            {
                int slash = sym.Slice( start ).IndexOf( '/' );
                var part = slash >= 0 ? sym.Slice( start, slash ) : sym.Slice( start );

                if ( ParseSymbolSpan( part ) == color )
                    count++;

                if ( slash < 0 ) break;
                start += slash + 1;
            }

            return count;
        }

        return ParseSymbolSpan( sym ) == color ? 1 : 0;
    }
}

[Flags]
public enum ManaCostFlags : byte
{
    None         = 0,
    HasX         = 1 << 0,
    HasHybrid    = 1 << 1,
    HasPhyrexian = 1 << 2,
    HasSnow      = 1 << 3,
}
