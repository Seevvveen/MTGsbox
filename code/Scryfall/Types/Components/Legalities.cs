#nullable enable
using Sandbox.Scryfall.Types.Enums;

namespace Sandbox.Scryfall.Types.Components;

/// <summary>
/// Legality information for a card across various formats.
/// </summary>
public record Legalities
{
	[JsonPropertyName( "standard" )]
	public Legality Standard { get; init; }

	[JsonPropertyName( "future" )]
	public Legality Future { get; init; }

	[JsonPropertyName( "historic" )]
	public Legality Historic { get; init; }

	[JsonPropertyName( "timeless" )]
	public Legality Timeless { get; init; }

	[JsonPropertyName( "gladiator" )]
	public Legality Gladiator { get; init; }

	[JsonPropertyName( "pioneer" )]
	public Legality Pioneer { get; init; }

	[JsonPropertyName( "modern" )]
	public Legality Modern { get; init; }

	[JsonPropertyName( "legacy" )]
	public Legality Legacy { get; init; }

	[JsonPropertyName( "pauper" )]
	public Legality Pauper { get; init; }

	[JsonPropertyName( "vintage" )]
	public Legality Vintage { get; init; }

	[JsonPropertyName( "penny" )]
	public Legality Penny { get; init; }

	[JsonPropertyName( "commander" )]
	public Legality Commander { get; init; }

	[JsonPropertyName( "oathbreaker" )]
	public Legality Oathbreaker { get; init; }

	[JsonPropertyName( "standardbrawl" )]
	public Legality Standardbrawl { get; init; }

	[JsonPropertyName( "brawl" )]
	public Legality Brawl { get; init; }

	[JsonPropertyName( "alchemy" )]
	public Legality Alchemy { get; init; }

	[JsonPropertyName( "paupercommander" )]
	public Legality Paupercommander { get; init; }

	[JsonPropertyName( "duel" )]
	public Legality Duel { get; init; }

	[JsonPropertyName( "oldschool" )]
	public Legality Oldschool { get; init; }

	[JsonPropertyName( "premodern" )]
	public Legality Premodern { get; init; }

	[JsonPropertyName( "predh" )]
	public Legality Predh { get; init; }
}
