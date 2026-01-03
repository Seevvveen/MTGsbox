#nullable enable
namespace Sandbox.Scryfall.Types.DTOs;

public sealed record ScryfallPurchaseUris
{
	[JsonPropertyName("tcgplayer")] public string? Tcgplayer { get; set; }
	[JsonPropertyName("cardmarket")] public string? Cardmarket { get; set; }
	[JsonPropertyName("cardhoarder")] public string? Cardhoarder { get; set; }
}
