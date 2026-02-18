using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sandbox.__Rewrite.Gameplay;

/// <summary>
/// Possible kinds of power/toughness value.
/// </summary>
public enum CombatValueKind
{
    /// <summary>A fixed integer e.g. 3</summary>
    Numeric,

    /// <summary>Purely variable e.g. * (Tarmogoyf toughness, Unbound Flourishing)</summary>
    Variable,

    /// <summary>Base integer plus variable e.g. 1+* or 2+*</summary>
    Sum,
}

/// <summary>
/// Typed power or toughness value. Replaces raw Scryfall strings like "3", "*", "1+*".
/// </summary>
[JsonConverter( typeof( CombatValueJsonConverter ) )]
public readonly record struct CombatValue
{
    public CombatValueKind Kind { get; init; }

    /// <summary>The integer component for <see cref="CombatValueKind.Numeric"/> and <see cref="CombatValueKind.Sum"/>. Zero for Variable.</summary>
    public int Base { get; init; }

    /// <summary>True when the value has a variable (*) component.</summary>
    public bool IsVariable => Kind == CombatValueKind.Variable || Kind == CombatValueKind.Sum;

    /// <summary>True when the value is a fixed integer.</summary>
    public bool IsFixed => Kind == CombatValueKind.Numeric;

    /// <summary>
    /// Resolves the value assuming * = 0.
    /// For fixed numeric values this is always exact.
    /// </summary>
    public int BaseValue => Base;

    public override string ToString() => Kind switch
    {
        CombatValueKind.Numeric  => Base.ToString(),
        CombatValueKind.Variable => "*",
        CombatValueKind.Sum      => $"{Base}+*",
        _                        => "?",
    };

    // -------------------------
    // Parsing
    // -------------------------

    public static bool TryParse( string raw, out CombatValue value )
    {
        value = default;

        if ( string.IsNullOrEmpty( raw ) )
            return false;

        // "1+*" or "2+*"
        int plusStar = raw.IndexOf( "+*", StringComparison.Ordinal );
        if ( plusStar >= 0 )
        {
            var basePart = raw.Substring( 0, plusStar );
            if ( int.TryParse( basePart, out int baseVal ) )
            {
                value = new CombatValue { Kind = CombatValueKind.Sum, Base = baseVal };
                return true;
            }
            return false;
        }

        // "*"
        if ( raw == "*" )
        {
            value = new CombatValue { Kind = CombatValueKind.Variable, Base = 0 };
            return true;
        }

        // Plain integer (including negatives like "-1")
        if ( int.TryParse( raw, out int n ) )
        {
            value = new CombatValue { Kind = CombatValueKind.Numeric, Base = n };
            return true;
        }

        return false;
    }

    public static CombatValue? ParseOrNull( string raw )
    {
        if ( raw == null )
            return null;
        return TryParse( raw, out var v ) ? v : null;
    }
}

// -------------------------
// JSON serialization — round-trips as the canonical string
// -------------------------

public sealed class CombatValueJsonConverter : JsonConverter<CombatValue>
{
    public override CombatValue Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
    {
        var raw = reader.GetString();
        return CombatValue.TryParse( raw, out var value ) ? value : default;
    }

    public override void Write( Utf8JsonWriter writer, CombatValue value, JsonSerializerOptions options )
        => writer.WriteStringValue( value.ToString() );
}

/// <summary>Nullable CombatValue JSON converter — writes null for null, delegates otherwise.</summary>
public sealed class NullableCombatValueJsonConverter : JsonConverter<CombatValue?>
{
    public override CombatValue? Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
    {
        if ( reader.TokenType == JsonTokenType.Null )
            return null;
        var raw = reader.GetString();
        return CombatValue.TryParse( raw, out var v ) ? v : null;
    }

    public override void Write( Utf8JsonWriter writer, CombatValue? value, JsonSerializerOptions options )
    {
        if ( value == null )
            writer.WriteNullValue();
        else
            writer.WriteStringValue( value.Value.ToString() );
    }
}