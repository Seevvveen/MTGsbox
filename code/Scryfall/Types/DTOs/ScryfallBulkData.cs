#nullable enable
namespace Sandbox.Scryfall.Types.DTOs;

/// <summary>
/// Scryfall Meta Data for Bulk Items
/// </summary>
public sealed record ScryfallBulkData
{
	[JsonPropertyName( "object" )] public string? Object { get; init; }
	[JsonPropertyName( "id" )] public Guid? Id { get; init; }
	[JsonPropertyName( "type" )] public string? Type { get; init; } 
	[JsonPropertyName( "updated_at" )] public DateTimeOffset UpdatedAt { get; init; }
	[JsonPropertyName( "uri" )] public string? Uri { get; init; }
	[JsonPropertyName( "name" )] public string? Name { get; init; }
	[JsonPropertyName( "description" )] public string? Description { get; init; }
	[JsonPropertyName( "size" )] public int Size { get; init; }
	[JsonPropertyName( "download_uri" )] public string? DownloadUri { get; init; }
	[JsonPropertyName( "content_type" )] public string? ContentType { get; init; }
	[JsonPropertyName( "content_encoding" )] public string? ContentEncoding { get; init; }
}
