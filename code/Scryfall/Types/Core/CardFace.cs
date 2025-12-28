#nullable enable
using Sandbox.Scryfall.Types.Components;

namespace Sandbox.Scryfall.Types.Core;

/// <summary>
/// Represents a single face of a Magic card.
/// Used for multi-faced cards (split, flip, transform, modal DFC, etc.)
/// Also serves as the base class for Card since single-faced cards ARE card faces.
/// </summary>
public record CardFace
{
	[JsonPropertyName( "object" )]
	public string Object { get; init; } = "card_face";

	// Identity
	[JsonPropertyName( "name" )]
	public string Name { get; init; } = string.Empty;

	[JsonPropertyName( "oracle_id" )]
	public Guid? OracleId { get; init; }

	// Mana and costs
	[JsonPropertyName( "mana_cost" )]
	public string? ManaCost { get; init; }

	[JsonPropertyName( "cmc" )]
	public decimal? Cmc { get; init; }

	// Type and text
	[JsonPropertyName( "type_line" )]
	public string? TypeLine { get; init; }

	[JsonPropertyName( "oracle_text" )]
	public string? OracleText { get; init; }

	[JsonPropertyName( "flavor_text" )]
	public string? FlavorText { get; init; }

	// Colors
	[JsonPropertyName( "colors" )]
	public List<string> ColorsRaw { get; init; } = new();

	[JsonIgnore]
	public ColorIdentity Colors => ColorIdentity.FromScryfall( ColorsRaw );

	[JsonPropertyName( "color_indicator" )]
	public List<string>? ColorIndicatorRaw { get; init; }

	[JsonIgnore]
	public ColorIdentity? ColorIndicator => ColorIndicatorRaw != null
		? ColorIdentity.FromScryfall( ColorIndicatorRaw )
		: null;

	// Stats (can be non-numeric like * or X)
	[JsonPropertyName( "power" )]
	public string? Power { get; init; }

	[JsonPropertyName( "toughness" )]
	public string? Toughness { get; init; }

	[JsonPropertyName( "loyalty" )]
	public string? Loyalty { get; init; }

	[JsonPropertyName( "defense" )]
	public string? Defense { get; init; }

	// Art
	[JsonPropertyName( "artist" )]
	public string? Artist { get; init; }

	[JsonPropertyName( "artist_id" )]
	public Guid? ArtistId { get; init; }

	[JsonPropertyName( "illustration_id" )]
	public Guid? IllustrationId { get; init; }

	[JsonPropertyName( "image_uris" )]
	public ImageUris? ImageUris { get; init; }

	// Localized printing
	[JsonPropertyName( "printed_name" )]
	public string? PrintedName { get; init; }

	[JsonPropertyName( "printed_text" )]
	public string? PrintedText { get; init; }

	[JsonPropertyName( "printed_type_line" )]
	public string? PrintedTypeLine { get; init; }

	[JsonPropertyName( "watermark" )]
	public string? Watermark { get; init; }

	// Layout (only present on reversible_card faces)
	[JsonPropertyName( "layout" )]
	public string? Layout { get; init; }
}
