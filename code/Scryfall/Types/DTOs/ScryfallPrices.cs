#nullable enable

namespace Sandbox.Scryfall.Types.DTOs;

/// <summary>Matches the "prices" object. These are strings in the API (often null).</summary>
public sealed record ScryfallPrices
{
	[JsonPropertyName("usd")] public string? Usd { get; init; }
	[JsonPropertyName("usd_foil")] public string? UsdFoil { get; init; }
	[JsonPropertyName("usd_etched")] public string? UsdEtched { get; init; }
	[JsonPropertyName("eur")] public string? Eur { get; init; }
	[JsonPropertyName("eur_foil")] public string? EurFoil { get; init; }
	[JsonPropertyName("tix")] public string? Tix { get; init; }
}
