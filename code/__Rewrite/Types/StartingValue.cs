using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sandbox.__Rewrite.Gameplay;

/// <summary>
/// Starting numeric value for planeswalker loyalty or battle defense.
/// Always a non-negative integer or X (e.g. Everquill Phoenix).
/// </summary>
[JsonConverter( typeof( StartingValueJsonConverter ) )]
public readonly record struct StartingValue
{
    /// <summary>Fixed value e.g. 4 for a planeswalker with 4 starting loyalty.</summary>
    public int? Value { get; init; }

    /// <summary>True if the starting value is X (determined at cast time).</summary>
    public bool IsX => Value == null;

    public override string ToString() => IsX ? "X" : Value!.Value.ToString();

    // -------------------------
    // Parsing
    // -------------------------

    public static bool TryParse( string raw, out StartingValue result )
    {
        result = default;

        if ( string.IsNullOrEmpty( raw ) )
            return false;

        if ( raw.Equals( "X", StringComparison.OrdinalIgnoreCase ) )
        {
            result = new StartingValue { Value = null };
            return true;
        }

        if ( int.TryParse( raw, out int n ) )
        {
            result = new StartingValue { Value = n };
            return true;
        }

        return false;
    }

    public static StartingValue? ParseOrNull( string raw )
    {
        if ( raw == null )
            return null;
        return TryParse( raw, out var v ) ? v : null;
    }
}

// -------------------------
// JSON serialization
// -------------------------

public sealed class StartingValueJsonConverter : JsonConverter<StartingValue>
{
    public override StartingValue Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
    {
        var raw = reader.GetString();
        return StartingValue.TryParse( raw, out var value ) ? value : default;
    }

    public override void Write( Utf8JsonWriter writer, StartingValue value, JsonSerializerOptions options )
        => writer.WriteStringValue( value.ToString() );
}

public sealed class NullableStartingValueJsonConverter : JsonConverter<StartingValue?>
{
    public override StartingValue? Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
    {
        if ( reader.TokenType == JsonTokenType.Null )
            return null;
        var raw = reader.GetString();
        return StartingValue.TryParse( raw, out var v ) ? v : null;
    }

    public override void Write( Utf8JsonWriter writer, StartingValue? value, JsonSerializerOptions options )
    {
        if ( value == null )
            writer.WriteNullValue();
        else
            writer.WriteStringValue( value.Value.ToString() );
    }
}