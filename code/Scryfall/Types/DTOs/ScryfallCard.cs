#nullable enable

using Sandbox.Scryfall.Types.DTOs;

namespace Sandbox.Scryfall.Types.Dtos;

/// <summary>
/// Direct mirror of Scryfall "card" JSON.
/// Everything is nullable / permissive by design.
/// </summary>
public sealed record ScryfallCard
{
	// Core identity
	[JsonPropertyName( "object" )] public string? Object { get; init; }
	[JsonPropertyName( "id" )] public Guid? Id { get; init; }
	[JsonPropertyName( "oracle_id" )] public Guid? OracleId { get; init; }

	// IDs
	[JsonPropertyName( "multiverse_ids" )] public int[]? MultiverseIds { get; init; }
	[JsonPropertyName( "mtgo_id" )] public int? MtgoId { get; init; }
	[JsonPropertyName( "mtgo_foil_id" )] public int? MtgoFoilId { get; init; }
	[JsonPropertyName( "tcgplayer_id" )] public int? TcgplayerId { get; init; }
	[JsonPropertyName( "cardmarket_id" )] public int? CardmarketId { get; init; }

	// Basics
	[JsonPropertyName( "name" )] public string? Name { get; init; }
	[JsonPropertyName( "lang" )] public string? Lang { get; init; }
	[JsonPropertyName( "released_at" )] public string? ReleasedAt { get; init; }

	// Links
	[JsonPropertyName( "uri" )] public string? Uri { get; init; }
	[JsonPropertyName( "scryfall_uri" )] public string? ScryfallUri { get; init; }

	// Layout & image
	[JsonPropertyName( "layout" )] public string? Layout { get; init; }
	[JsonPropertyName( "highres_image" )] public bool? HighresImage { get; init; }
	[JsonPropertyName( "image_status" )] public string? ImageStatus { get; init; }
	[JsonPropertyName( "image_uris" )] public ScryfallImageUris? ImageUris { get; init; }

	// Rules-ish
	[JsonPropertyName( "mana_cost" )] public string? ManaCost { get; init; }
	[JsonPropertyName( "cmc" )] public decimal? Cmc { get; init; }
	[JsonPropertyName( "type_line" )] public string? TypeLine { get; init; }
	[JsonPropertyName( "oracle_text" )] public string? OracleText { get; init; }
	[JsonPropertyName( "power" )] public string? Power { get; init; }
	[JsonPropertyName( "toughness" )] public string? Toughness { get; init; }

	// Colors
	[JsonPropertyName( "colors" )] public string[]? Colors { get; init; }
	[JsonPropertyName( "color_identity" )] public string[]? ColorIdentity { get; init; }

	// Keywords / parts
	[JsonPropertyName( "keywords" )] public string[]? Keywords { get; init; }
	[JsonPropertyName( "all_parts" )] public ScryfallRelatedCard[]? AllParts { get; init; }

	// Legality / availability
	[JsonPropertyName( "legalities" )] public ScryfallLegalities? Legalities { get; init; }
	[JsonPropertyName( "games" )] public string[]? Games { get; init; }

	// Flags
	[JsonPropertyName( "reserved" )] public bool? Reserved { get; init; }
	[JsonPropertyName( "game_changer" )] public bool? GameChanger { get; init; }
	[JsonPropertyName( "foil" )] public bool? Foil { get; init; }
	[JsonPropertyName( "nonfoil" )] public bool? Nonfoil { get; init; }
	[JsonPropertyName( "finishes" )] public string[]? Finishes { get; init; }
	[JsonPropertyName( "oversized" )] public bool? Oversized { get; init; }
	[JsonPropertyName( "promo" )] public bool? Promo { get; init; }
	[JsonPropertyName( "reprint" )] public bool? Reprint { get; init; }
	[JsonPropertyName( "variation" )] public bool? Variation { get; init; }

	// Set / printing
	[JsonPropertyName( "set_id" )] public Guid? SetId { get; init; }
	[JsonPropertyName( "set" )] public string? Set { get; init; }
	[JsonPropertyName( "set_name" )] public string? SetName { get; init; }
	[JsonPropertyName( "set_type" )] public string? SetType { get; init; }

	[JsonPropertyName( "set_uri" )] public string? SetUri { get; init; }
	[JsonPropertyName( "set_search_uri" )] public string? SetSearchUri { get; init; }
	[JsonPropertyName( "scryfall_set_uri" )] public string? ScryfallSetUri { get; init; }

	[JsonPropertyName( "rulings_uri" )] public string? RulingsUri { get; init; }
	[JsonPropertyName( "prints_search_uri" )] public string? PrintsSearchUri { get; init; }

	[JsonPropertyName( "collector_number" )] public string? CollectorNumber { get; init; }
	[JsonPropertyName( "digital" )] public bool? Digital { get; init; }
	[JsonPropertyName( "rarity" )] public string? Rarity { get; init; }

	// Flavor / art / frame
	[JsonPropertyName( "watermark" )] public string? Watermark { get; init; }
	[JsonPropertyName( "flavor_text" )] public string? FlavorText { get; init; }

	[JsonPropertyName( "card_back_id" )] public Guid? CardBackId { get; init; }

	[JsonPropertyName( "artist" )] public string? Artist { get; init; }
	[JsonPropertyName( "artist_ids" )] public Guid[]? ArtistIds { get; init; }
	[JsonPropertyName( "illustration_id" )] public Guid? IllustrationId { get; init; }

	[JsonPropertyName( "border_color" )] public string? BorderColor { get; init; }
	[JsonPropertyName( "frame" )] public string? Frame { get; init; }
	[JsonPropertyName( "frame_effects" )] public string[]? FrameEffects { get; init; }
	[JsonPropertyName( "security_stamp" )] public string? SecurityStamp { get; init; }

	[JsonPropertyName( "full_art" )] public bool? FullArt { get; init; }
	[JsonPropertyName( "textless" )] public bool? Textless { get; init; }
	[JsonPropertyName( "booster" )] public bool? Booster { get; init; }
	[JsonPropertyName( "story_spotlight" )] public bool? StorySpotlight { get; init; }

	// Popularity / preview
	[JsonPropertyName( "edhrec_rank" )] public int? EdhrecRank { get; init; }
	[JsonPropertyName( "preview" )] public ScryfallPreview? Preview { get; init; }

	// Commerce / related links
	[JsonPropertyName( "prices" )] public ScryfallPrices? Prices { get; init; }
	[JsonPropertyName( "related_uris" )] public Dictionary<string, string?>? RelatedUris { get; init; }
	[JsonPropertyName( "purchase_uris" )] public Dictionary<string, string?>? PurchaseUris { get; init; }
}
