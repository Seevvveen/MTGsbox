using System;
using System.Text.Json;

namespace Sandbox.__Rewrite.Gameplay;

public enum CombatValueKind : byte
{
    Unknown = 0,
    Numeric,
    Variable,
    Sum
}

public readonly record struct CombatValue : IJsonConvert
{
    public CombatValueKind Kind { get; init; }
    public int Base { get; init; } // Numeric/Sum

    public bool IsVariable => Kind is CombatValueKind.Variable or CombatValueKind.Sum;
    public bool IsFixed    => Kind == CombatValueKind.Numeric;
    public bool IsKnown    => Kind != CombatValueKind.Unknown;

    public override string ToString() => Kind switch
    {
        CombatValueKind.Numeric  => Base.ToString(),
        CombatValueKind.Variable => "*",
        CombatValueKind.Sum      => $"{Base}+*",
        _                        => "?"
    };

    public static bool TryParse( string raw, out CombatValue value )
    {
        value = default;
        if ( string.IsNullOrEmpty( raw ) ) return false;

        var s = raw.AsSpan();

        // "*"
        if ( s.Length == 1 && s[0] == '*' )
        {
            value = new CombatValue { Kind = CombatValueKind.Variable };
            return true;
        }

        // "N+*"
        int plus = s.IndexOf( "+*".AsSpan(), StringComparison.Ordinal );
        if ( plus >= 0 )
        {
            if ( int.TryParse( s[..plus], out int baseVal ) )
            {
                value = new CombatValue { Kind = CombatValueKind.Sum, Base = baseVal };
                return true;
            }
            return false;
        }

        // integer
        if ( int.TryParse( s, out int n ) )
        {
            value = new CombatValue { Kind = CombatValueKind.Numeric, Base = n };
            return true;
        }

        return false;
    }

    public static CombatValue ParseOrUnknown( string raw )
        => TryParse( raw, out var v ) ? v : new CombatValue { Kind = CombatValueKind.Unknown };

    public static CombatValue? ParseOrNull( string raw )
    {
        if ( string.IsNullOrWhiteSpace( raw ) )
            return null;

        return TryParse( raw, out var v )
            ? v
            : new CombatValue { Kind = CombatValueKind.Unknown };
    }
    
    // IJsonConvert
    public static object JsonRead( ref Utf8JsonReader reader, Type typeToConvert )
    {
        if ( reader.TokenType == JsonTokenType.Null )
            return null;

        var raw = reader.GetString();
        return TryParse( raw, out var v ) ? v : new CombatValue { Kind = CombatValueKind.Unknown };
    }

    public static void JsonWrite( object value, Utf8JsonWriter writer )
    {
        if ( value is null ) { writer.WriteNullValue(); return; }
        writer.WriteStringValue( ( (CombatValue)value ).ToString() );
    }
}
