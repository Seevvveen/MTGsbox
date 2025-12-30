#nullable enable

namespace Sandbox.Scryfall.Types.DTOs;

/// <summary>Matches the "legalities" object (string values like "legal", "not_legal").</summary>
public sealed record ScryfallLegalities
{
	[JsonPropertyName( "standard" )] public string? Standard { get; init; }
	[JsonPropertyName( "future" )] public string? Future { get; init; }
	[JsonPropertyName( "historic" )] public string? Historic { get; init; }
	[JsonPropertyName( "timeless" )] public string? Timeless { get; init; }
	[JsonPropertyName( "gladiator" )] public string? Gladiator { get; init; }
	[JsonPropertyName( "pioneer" )] public string? Pioneer { get; init; }
	[JsonPropertyName( "modern" )] public string? Modern { get; init; }
	[JsonPropertyName( "legacy" )] public string? Legacy { get; init; }
	[JsonPropertyName( "pauper" )] public string? Pauper { get; init; }
	[JsonPropertyName( "vintage" )] public string? Vintage { get; init; }
	[JsonPropertyName( "penny" )] public string? Penny { get; init; }
	[JsonPropertyName( "commander" )] public string? Commander { get; init; }
	[JsonPropertyName( "oathbreaker" )] public string? Oathbreaker { get; init; }
	[JsonPropertyName( "standardbrawl" )] public string? Standardbrawl { get; init; }
	[JsonPropertyName( "brawl" )] public string? Brawl { get; init; }
	[JsonPropertyName( "alchemy" )] public string? Alchemy { get; init; }
	[JsonPropertyName( "paupercommander" )] public string? Paupercommander { get; init; }
	[JsonPropertyName( "duel" )] public string? Duel { get; init; }
	[JsonPropertyName( "oldschool" )] public string? Oldschool { get; init; }
	[JsonPropertyName( "premodern" )] public string? Premodern { get; init; }
	[JsonPropertyName( "predh" )] public string? Predh { get; init; }
}
