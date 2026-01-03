#nullable enable
namespace Sandbox.Scryfall.Types.DTOs;

public sealed record ScryfallRelatedUris
{
	[JsonPropertyName("gatherer")] public string? Gatherer { get; init; }
	[JsonPropertyName("tcgplayer_infinite_articles")] public string? TcgplayerInfiniteArticles { get; init; }
	[JsonPropertyName("tcgplayer_infinite_decks")] public string? TcgplayerInfiniteDecks { get; init; }
	[JsonPropertyName("edhrec")] public string? Edhrec { get; init; }
}
