#nullable enable

namespace Sandbox.Scryfall.Types.DTOs;

/// <summary>Matches the "preview" object.</summary>
public sealed record ScryfallPreview
{
	[JsonPropertyName( "source" )] public string? Source { get; init; }
	[JsonPropertyName( "source_uri" )] public string? SourceUri { get; init; }
	[JsonPropertyName( "previewed_at" )] public string? PreviewedAt { get; init; }
}
