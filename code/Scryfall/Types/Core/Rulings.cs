#nullable enable
namespace Sandbox.Scryfall.Types.Core;

/// <summary>
/// Represents a ruling about a Magic card.
/// Rulings can come from Wizards of the Coast or Scryfall.
/// </summary>
public record Ruling
{
	[JsonPropertyName( "object" )]
	public string Object { get; init; } = "ruling";

	[JsonPropertyName( "oracle_id" )]
	public Guid OracleId { get; init; }

	/// <summary>
	/// Source of the ruling: "wotc" or "scryfall"
	/// </summary>
	[JsonPropertyName( "source" )]
	public string Source { get; init; } = string.Empty;

	//DateOnly is not allowed by whitelist - issue opened
	[JsonPropertyName( "published_at" )]
	public string PublishedAt { get; init; } = string.Empty;

	[JsonPropertyName( "comment" )]
	public string Comment { get; init; } = string.Empty;
}
