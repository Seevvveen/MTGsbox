using System;

namespace Sandbox.__Rewrite.Gameplay;

/// <summary>
/// MTG colors as a flags enum. Replaces all List&lt;string&gt; color fields.
/// Supports bitwise operations for identity checks, subset tests, and multicolor detection.
/// </summary>
[Flags]
public enum ManaColor
{
    None      = 0,
    White     = 1 << 0,   // W
    Blue      = 1 << 1,   // U
    Black     = 1 << 2,   // B
    Red       = 1 << 3,   // R
    Green     = 1 << 4,   // G
    Colorless = 1 << 5,   // C  (Wastes, Devoid cards)
}

public static class ManaColorExtensions
{
    /// <summary>Number of colors in this identity (ignores Colorless).</summary>
    public static int ColorCount( this ManaColor color )
    {
        var chromatic = color & ~ManaColor.Colorless;
        int count = 0;
        while ( chromatic != 0 )
        {
            count  += (int)chromatic & 1;
            chromatic = (ManaColor)( (int)chromatic >> 1 );
        }
        return count;
    }

    public static bool IsMonoColor( this ManaColor color ) => color.ColorCount() == 1;
    public static bool IsMultiColor( this ManaColor color ) => color.ColorCount() > 1;
    public static bool IsColorless( this ManaColor color )  => color == ManaColor.None || color == ManaColor.Colorless;

    /// <summary>Returns true if this identity contains all colors in <paramref name="subset"/>.</summary>
    public static bool Contains( this ManaColor identity, ManaColor subset ) => ( identity & subset ) == subset;

    /// <summary>Parses a single Scryfall color code: "W", "U", "B", "R", "G", "C".</summary>
    public static ManaColor ParseSymbol( string symbol ) => symbol?.ToUpperInvariant() switch
    {
        "W" => ManaColor.White,
        "U" => ManaColor.Blue,
        "B" => ManaColor.Black,
        "R" => ManaColor.Red,
        "G" => ManaColor.Green,
        "C" => ManaColor.Colorless,
        _   => ManaColor.None,
    };

    /// <summary>Parses a Scryfall color list e.g. ["W", "U"] into a combined ManaColor.</summary>
    public static ManaColor ParseList( System.Collections.Generic.IEnumerable<string> symbols )
    {
        if ( symbols == null )
            return ManaColor.None;

        var result = ManaColor.None;
        foreach ( var s in symbols )
            result |= ParseSymbol( s );

        return result;
    }
}