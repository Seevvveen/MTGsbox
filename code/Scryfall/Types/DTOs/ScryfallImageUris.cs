#nullable enable

namespace Sandbox.Scryfall.Types.DTOs;

/// <summary>Matches the "image_uris" object.</summary>
public sealed record ScryfallImageUris
{
	[JsonPropertyName( "small" )] public string? Small { get; init; }
	[JsonPropertyName( "normal" )] public string? Normal { get; init; }
	[JsonPropertyName( "large" )] public string? Large { get; init; }
	[JsonPropertyName( "png" )] public string? Png { get; init; }
	[JsonPropertyName( "art_crop" )] public string? ArtCrop { get; init; }
	[JsonPropertyName( "border_crop" )] public string? BorderCrop { get; init; }
}
