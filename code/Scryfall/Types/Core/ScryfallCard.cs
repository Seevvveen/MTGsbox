#nullable enable
namespace Sandbox.Scryfall.Types.Raw;

/// <summary>
/// Raw Scryfall JSON shape for bulk parsing.
/// Keep this permissive: strings instead of enums, nullable where Scryfall can omit.
/// Goal: NEVER fail to deserialize due to new/unknown values.
/// </summary>
public sealed record ScryfallCard
{
	// --- Core identifiers ---
	[JsonPropertyName( "id" )]
	public Guid Id { get; init; }

	// Scryfall says oracle_id can be absent for reversible_card (present on faces instead).
	[JsonPropertyName( "oracle_id" )]
	public Guid? OracleId { get; init; }

	[JsonPropertyName( "lang" )]
	public string? Lang { get; init; }

	[JsonPropertyName( "name" )]
	public string? Name { get; init; }

	[JsonPropertyName( "type_line" )]
	public string? TypeLine { get; init; }

	[JsonPropertyName( "oracle_text" )]
	public string? OracleText { get; init; }

	// Decimal (mana value); can be missing.
	[JsonPropertyName( "cmc" )]
	public decimal? Cmc { get; init; }

	// --- Enum-like strings (keep as string to avoid breaking) ---
	[JsonPropertyName( "layout" )]
	public string? Layout { get; init; }

	[JsonPropertyName( "rarity" )]
	public string? Rarity { get; init; }

	[JsonPropertyName( "set" )]
	public string? Set { get; init; }

	[JsonPropertyName( "collector_number" )]
	public string? CollectorNumber { get; init; }

	[JsonPropertyName( "released_at" )]
	public string? ReleasedAt { get; init; }

	// --- Colors ---
	[JsonPropertyName( "colors" )]
	public List<string>? Colors { get; init; }

	[JsonPropertyName( "color_identity" )]
	public List<string>? ColorIdentity { get; init; }

	[JsonPropertyName( "produced_mana" )]
	public List<string>? ProducedMana { get; init; }

	// --- Keywords ---
	[JsonPropertyName( "keywords" )]
	public List<string>? Keywords { get; init; }

	// --- Legalities are strings like "legal", "not_legal", "banned", "restricted" ---
	[JsonPropertyName( "legalities" )]
	public Dictionary<string, string?>? Legalities { get; init; }

	// --- Images ---
	[JsonPropertyName( "image_status" )]
	public string? ImageStatus { get; init; }

	[JsonPropertyName( "highres_image" )]
	public bool? HighresImage { get; init; }

	[JsonPropertyName( "image_uris" )]
	public Dictionary<string, string?>? ImageUris { get; init; } // raw map: "normal", "png", etc.

	// --- Multifaced (optional) ---
	[JsonPropertyName( "card_faces" )]
	public List<ScryfallCardFace>? CardFaces { get; init; }
}

/// <summary>
/// Raw face object. Again: strings + nullable.
/// </summary>
public sealed record ScryfallCardFace
{
	[JsonPropertyName( "name" )]
	public string? Name { get; init; }

	[JsonPropertyName( "oracle_id" )]
	public Guid? OracleId { get; init; }

	[JsonPropertyName( "mana_cost" )]
	public string? ManaCost { get; init; }

	[JsonPropertyName( "type_line" )]
	public string? TypeLine { get; init; }

	[JsonPropertyName( "oracle_text" )]
	public string? OracleText { get; init; }

	[JsonPropertyName( "colors" )]
	public List<string>? Colors { get; init; }

	[JsonPropertyName( "image_uris" )]
	public Dictionary<string, string?>? ImageUris { get; init; }
}
