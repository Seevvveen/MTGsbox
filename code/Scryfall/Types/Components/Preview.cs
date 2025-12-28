#nullable enable
namespace Sandbox.Scryfall.Types.Components;

/// <summary>
/// Information about where and when a card was previewed/spoiled.
/// </summary>
public record Preview
{
	// DateOnly not allowed by whitelist - issue opened
	[JsonPropertyName( "previewed_at" )]
	public string? PreviewedAt { get; init; } = string.Empty;

	[JsonPropertyName( "source" )]
	public string? Source { get; init; }

	[JsonPropertyName( "source_uri" )]
	public string? SourceUri { get; init; }
}
