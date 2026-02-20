using System;
using System.Text.Json;

namespace Sandbox.__Rewrite.Gameplay;

public enum StartingValueKind : byte
{
	Unknown = 0,
	Fixed,
	X
}

public readonly record struct StartingValue : IJsonConvert
{
	public StartingValueKind Kind { get; init; }
	public int Value { get; init; } // only valid if Fixed

	public bool IsX     => Kind == StartingValueKind.X;
	public bool IsFixed => Kind == StartingValueKind.Fixed;
	public bool IsKnown => Kind != StartingValueKind.Unknown;

	public override string ToString() => Kind switch
	{
		StartingValueKind.Fixed => Value.ToString(),
		StartingValueKind.X     => "X",
		_                       => "?"
	};

	public static bool TryParse( string raw, out StartingValue result )
	{
		result = default;
		if ( string.IsNullOrEmpty( raw ) ) return false;

		if ( raw.Length == 1 && (raw[0] == 'X' || raw[0] == 'x') )
		{
			result = new StartingValue { Kind = StartingValueKind.X };
			return true;
		}

		if ( int.TryParse( raw.AsSpan(), out int n ) )
		{
			result = new StartingValue { Kind = StartingValueKind.Fixed, Value = n };
			return true;
		}

		return false;
	}

	public static StartingValue ParseOrUnknown( string raw )
		=> TryParse( raw, out var v ) ? v : new StartingValue { Kind = StartingValueKind.Unknown };

	public static StartingValue? ParseOrNull( string raw )
	{
		if ( string.IsNullOrWhiteSpace( raw ) )
			return null;

		return TryParse( raw, out var v )
			? v
			: new StartingValue { Kind = StartingValueKind.Unknown };
	}
	
	// IJsonConvert
	public static object JsonRead( ref Utf8JsonReader reader, Type typeToConvert )
	{
		if ( reader.TokenType == JsonTokenType.Null )
			return null;

		var raw = reader.GetString();
		return TryParse( raw, out var v ) ? v : new StartingValue { Kind = StartingValueKind.Unknown };
	}

	public static void JsonWrite( object value, Utf8JsonWriter writer )
	{
		if ( value is null ) { writer.WriteNullValue(); return; }
		writer.WriteStringValue( ( (StartingValue)value ).ToString() );
	}
}