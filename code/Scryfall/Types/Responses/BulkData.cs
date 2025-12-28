#nullable enable
namespace Sandbox.Scryfall.Types.Responses;

/// <summary>
/// Represents metadata about a bulk data file from Scryfall.
/// Bulk data files contain all cards, sets, or rulings in JSON format.
/// Files are updated every 12 hours.
/// </summary>
public record BulkData
{
	[JsonPropertyName( "object" )]
	public string Object { get; init; } = "bulk_data";

	[JsonPropertyName( "id" )]
	public Guid Id { get; init; }

	[JsonPropertyName( "uri" )]
	public string? Uri { get; init; }

	/// <summary>
	/// Type of bulk data:
	/// - oracle_cards: One card per Oracle ID
	/// - unique_artwork: One card per unique artwork
	/// - default_cards: All English cards (or only available language)
	/// - all_cards: Every card in every language
	/// - rulings: All rulings
	/// </summary>
	[JsonPropertyName( "type" )]
	public string Type { get; init; } = string.Empty;

	[JsonPropertyName( "name" )]
	public string Name { get; init; } = string.Empty;

	[JsonPropertyName( "description" )]
	public string Description { get; init; } = string.Empty;

	/// <summary>
	/// Direct download URI for the bulk file
	/// </summary>
	[JsonPropertyName( "download_uri" )]
	public string? DownloadUri { get; init; }

	[JsonPropertyName( "updated_at" )]
	public DateTimeOffset UpdatedAt { get; init; }

	/// <summary>
	/// Size of the file in bytes
	/// </summary>
	[JsonPropertyName( "size" )]
	public long Size { get; init; }

	[JsonPropertyName( "content_type" )]
	public string ContentType { get; init; } = string.Empty;

	[JsonPropertyName( "content_encoding" )]
	public string ContentEncoding { get; init; } = string.Empty;
}
