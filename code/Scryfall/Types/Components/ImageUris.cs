#nullable enable
namespace Sandbox.Scryfall.Types.Components;

/// <summary>
/// Image URIs for various sizes and crops of a card image.
/// Used by Card, CardFace, and Set objects.
/// </summary>
public record ImageUris
{
	[JsonPropertyName( "small" )]
	public string? Small { get; init; }

	[JsonPropertyName( "normal" )]
	public string? Normal { get; init; }

	[JsonPropertyName( "large" )]
	public string? Large { get; init; }

	[JsonPropertyName( "png" )]
	public string? Png { get; init; }

	[JsonPropertyName( "art_crop" )]
	public string? ArtCrop { get; init; }

	[JsonPropertyName( "border_crop" )]
	public string? BorderCrop { get; init; }
}
