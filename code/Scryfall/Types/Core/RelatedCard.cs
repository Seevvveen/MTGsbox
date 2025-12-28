#nullable enable
namespace Sandbox.Scryfall.Types.Core;

/// <summary>
/// Represents a card that is related to another card.
/// Used in Card.AllParts for tokens, meld parts, combo pieces, etc.
/// </summary>
public record RelatedCard
{
	[JsonPropertyName( "object" )]
	public string Object { get; init; } = "related_card";

	[JsonPropertyName( "id" )]
	public Guid Id { get; init; }

	/// <summary>
	/// The relationship type: token, meld_part, meld_result, or combo_piece
	/// </summary>
	[JsonPropertyName( "component" )]
	public string Component { get; init; } = string.Empty;

	[JsonPropertyName( "name" )]
	public string Name { get; init; } = string.Empty;

	[JsonPropertyName( "type_line" )]
	public string TypeLine { get; init; } = string.Empty;

	[JsonPropertyName( "uri" )]
	public string? Uri { get; init; }
}
