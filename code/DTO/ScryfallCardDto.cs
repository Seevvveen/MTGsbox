#nullable enable
public sealed class ScryfallCardDto
{
	// Core
	[JsonPropertyName( "object" )]
	public string? Object { get; set; }

	[JsonPropertyName( "id" )]
	public string? Id { get; set; }

	[JsonPropertyName( "oracle_id" )]
	public string? OracleId { get; set; }

	[JsonPropertyName( "lang" )]
	public string? Lang { get; set; }

	[JsonPropertyName( "layout" )]
	public string? Layout { get; set; }

	[JsonPropertyName( "name" )]
	public string? Name { get; set; }

	[JsonPropertyName( "type_line" )]
	public string? TypeLine { get; set; }

	[JsonPropertyName( "oracle_text" )]
	public string? OracleText { get; set; }

	[JsonPropertyName( "mana_cost" )]
	public string? ManaCost { get; set; }

	[JsonPropertyName( "cmc" )]
	public decimal? Cmc { get; set; }

	[JsonPropertyName( "power" )]
	public string? Power { get; set; }

	[JsonPropertyName( "toughness" )]
	public string? Toughness { get; set; }

	[JsonPropertyName( "loyalty" )]
	public string? Loyalty { get; set; }

	[JsonPropertyName( "colors" )]
	public string[]? Colors { get; set; }

	[JsonPropertyName( "color_identity" )]
	public string[]? ColorIdentity { get; set; }

	[JsonPropertyName( "keywords" )]
	public string[]? Keywords { get; set; }

	// Multi-face / relations
	[JsonPropertyName( "card_faces" )]
	public ScryfallCardFaceDto[]? CardFaces { get; set; }

	[JsonPropertyName( "all_parts" )]
	public ScryfallRelatedCardDto[]? AllParts { get; set; }

	// URIs (strings on purpose)
	[JsonPropertyName( "uri" )]
	public string? Uri { get; set; }

	[JsonPropertyName( "scryfall_uri" )]
	public string? ScryfallUri { get; set; }

	[JsonPropertyName( "prints_search_uri" )]
	public string? PrintsSearchUri { get; set; }

	[JsonPropertyName( "rulings_uri" )]
	public string? RulingsUri { get; set; }

	// Printing info
	[JsonPropertyName( "set" )]
	public string? Set { get; set; }

	[JsonPropertyName( "set_name" )]
	public string? SetName { get; set; }

	[JsonPropertyName( "set_type" )]
	public string? SetType { get; set; }

	[JsonPropertyName( "collector_number" )]
	public string? CollectorNumber { get; set; }

	[JsonPropertyName( "rarity" )]
	public string? Rarity { get; set; }

	[JsonPropertyName( "artist" )]
	public string? Artist { get; set; }

	[JsonPropertyName( "released_at" )]
	public string? ReleasedAt { get; set; }

	[JsonPropertyName( "reprint" )]
	public bool? Reprint { get; set; }

	[JsonPropertyName( "digital" )]
	public bool? Digital { get; set; }

	[JsonPropertyName( "reserved" )]
	public bool? Reserved { get; set; }
}
