#nullable enable
namespace Sandbox.Scryfall.Types.DTOs;

public record ScryfallRelatedCard()
{
	[JsonPropertyName( "object" )] public string? Object { get; init; }
	[JsonPropertyName( "id" )] public Guid? Id { get; init; }
	[JsonPropertyName( "component" )] public string? Component { get; init; }
	[JsonPropertyName( "name" )] public string? Name { get; init; }
	[JsonPropertyName( "type_line" )] public string? TypeLine { get; init; }
	[JsonPropertyName( "uri" )] public string? Uri { get; init; }
}
