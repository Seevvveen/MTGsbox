#nullable enable
namespace Sandbox.Scryfall.Types.Core;

/// <summary>
/// Represents a catalog of Magic data points (card names, types, etc.)
/// Used for autocomplete and validation.
/// </summary>
public record Catalog
{
	[JsonPropertyName( "object" )]
	public string Object { get; init; } = "catalog";

	[JsonPropertyName( "uri" )]
	public string? Uri { get; init; }

	[JsonPropertyName( "total_values" )]
	public int TotalValues { get; init; }

	[JsonPropertyName( "data" )]
	public List<string> Data { get; init; } = new();
}
