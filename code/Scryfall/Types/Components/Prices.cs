#nullable enable
namespace Sandbox.Scryfall.Types.Components;

/// <summary>
/// Price information for a card in various currencies and finishes.
/// Prices are strings as they can be null or decimal values.
/// Note: Prices in bulk data are stale after 24 hours.
/// </summary>
public record Prices
{
	[JsonPropertyName( "usd" )]
	public string? Usd { get; init; }

	[JsonPropertyName( "usd_foil" )]
	public string? UsdFoil { get; init; }

	[JsonPropertyName( "usd_etched" )]
	public string? UsdEtched { get; init; }

	[JsonPropertyName( "eur" )]
	public string? Eur { get; init; }

	[JsonPropertyName( "eur_foil" )]
	public string? EurFoil { get; init; }

	[JsonPropertyName( "eur_etched" )]
	public string? EurEtched { get; init; }

	[JsonPropertyName( "tix" )]
	public string? Tix { get; init; }
}
