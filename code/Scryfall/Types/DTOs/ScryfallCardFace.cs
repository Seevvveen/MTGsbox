#nullable enable
namespace Sandbox.Scryfall.Types.DTOs;

public sealed record ScryfallCardFace
{
	[JsonPropertyName("object")] public string? Object { get; init; }
	[JsonPropertyName("name")] public string? Name { get; init; }
	[JsonPropertyName("mana_cost")] public string? ManaCost { get; init; }
	[JsonPropertyName("type_line")] public string? TypeLine { get; init; }
	[JsonPropertyName("oracle_text")] public string? OracleText { get; init; }
	[JsonPropertyName("colors")] public List<string>? Colors { get; init; }

	[JsonPropertyName("power")] public string? Power { get; init; }
	[JsonPropertyName("toughness")] public string? Toughness { get; init; }
	[JsonPropertyName("defense")] public string? Defense { get; init; }
	[JsonPropertyName("loyalty")] public string? Loyalty { get; init; }

	[JsonPropertyName("artist")] public string? Artist { get; init; }
	[JsonPropertyName("artist_ids")] public List<Guid>? ArtistIds { get; init; }
	[JsonPropertyName("illustration_id")] public Guid? IllustrationId { get; init; }

	[JsonPropertyName("image_uris")] public ScryfallImageUris? ImageUris { get; init; }
}
