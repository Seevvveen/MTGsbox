#nullable enable

namespace Sandbox.Scryfall.Types.DTOs;

public sealed class ScryfallCard
{
	// Core
	[JsonPropertyName("object")] public string? Object { get; init; }
	[JsonPropertyName("id")] public Guid? Id { get; init; }
	[JsonPropertyName("oracle_id")] public Guid? OracleId { get; init; }

	[JsonPropertyName("arena_id")] public int? ArenaId { get; init; }
	[JsonPropertyName("mtgo_id")] public int? MtgoId { get; init; }
	[JsonPropertyName("mtgo_foil_id")] public int? MtgoFoilId { get; init; }
	[JsonPropertyName("multiverse_ids")] public List<int>? MultiverseIds { get; init; }

	[JsonPropertyName("tcgplayer_id")] public int? TcgplayerId { get; init; }
	[JsonPropertyName("tcgplayer_etched_id")] public int? TcgplayerEtchedId { get; init; }
	[JsonPropertyName("cardmarket_id")] public int? CardmarketId { get; init; }

	// Identity
	[JsonPropertyName("name")] public string? Name { get; init; }
	[JsonPropertyName("lang")] public string? Lang { get; init; }
	[JsonPropertyName("released_at")] public string? ReleasedAt { get; init; }

	[JsonPropertyName("uri")] public string? Uri { get; init; }
	[JsonPropertyName("scryfall_uri")] public string? ScryfallUri { get; init; }

	// Layout / Images
	[JsonPropertyName("layout")] public string? Layout { get; init; }
	[JsonPropertyName("highres_image")] public bool? HighresImage { get; init; }
	[JsonPropertyName("image_status")] public string? ImageStatus { get; init; }
	[JsonPropertyName("image_uris")] public ScryfallImageUris? ImageUris { get; init; }
	[JsonPropertyName("card_faces")] public List<ScryfallCardFace>? CardFaces { get; init; }

	// Gameplay
	[JsonPropertyName("mana_cost")] public string? ManaCost { get; init; }
	[JsonPropertyName("cmc")] public decimal? Cmc { get; init; }
	[JsonPropertyName("type_line")] public string? TypeLine { get; init; }
	[JsonPropertyName("oracle_text")] public string? OracleText { get; init; }

	[JsonPropertyName("power")] public string? Power { get; init; }
	[JsonPropertyName("toughness")] public string? Toughness { get; init; }
	[JsonPropertyName("defense")] public string? Defense { get; init; }
	[JsonPropertyName("loyalty")] public string? Loyalty { get; init; }

	[JsonPropertyName("colors")] public List<string>? Colors { get; init; }
	[JsonPropertyName("color_indicator")] public List<string>? ColorIndicator { get; init; }
	[JsonPropertyName("color_identity")] public List<string>? ColorIdentity { get; init; }
	[JsonPropertyName("keywords")] public List<string>? Keywords { get; init; }
	[JsonPropertyName("produced_mana")] public List<string>? ProducedMana { get; init; }

	// Legality / play
	[JsonPropertyName("legalities")] public ScryfallLegalities? Legalities { get; init; }
	[JsonPropertyName("games")] public List<string>? Games { get; init; }

	// Print flags
	[JsonPropertyName("reserved")] public bool? Reserved { get; init; }
	[JsonPropertyName("game_changer")] public bool? GameChanger { get; init; }

	[JsonPropertyName("foil")] public bool? Foil { get; init; }
	[JsonPropertyName("nonfoil")] public bool? Nonfoil { get; init; }
	[JsonPropertyName("finishes")] public List<string>? Finishes { get; init; }

	[JsonPropertyName("oversized")] public bool? Oversized { get; init; }
	[JsonPropertyName("promo")] public bool? Promo { get; init; }
	[JsonPropertyName("promo_types")] public List<string>? PromoTypes { get; init; }

	[JsonPropertyName("reprint")] public bool? Reprint { get; init; }
	[JsonPropertyName("variation")] public bool? Variation { get; init; }
	[JsonPropertyName("variation_of")] public Guid? VariationOf { get; init; }

	// Set metadata
	[JsonPropertyName("set_id")] public Guid? SetId { get; init; }
	[JsonPropertyName("set")] public string? Set { get; init; }
	[JsonPropertyName("set_name")] public string? SetName { get; init; }
	[JsonPropertyName("set_type")] public string? SetType { get; init; }

	[JsonPropertyName("set_uri")] public string? SetUri { get; init; }
	[JsonPropertyName("set_search_uri")] public string? SetSearchUri { get; init; }
	[JsonPropertyName("scryfall_set_uri")] public string? ScryfallSetUri { get; init; }
	[JsonPropertyName("rulings_uri")] public string? RulingsUri { get; init; }
	[JsonPropertyName("prints_search_uri")] public string? PrintsSearchUri { get; init; }

	[JsonPropertyName("collector_number")] public string? CollectorNumber { get; init; }
	[JsonPropertyName("digital")] public bool? Digital { get; init; }
	[JsonPropertyName("rarity")] public string? Rarity { get; init; }

	[JsonPropertyName("flavor_text")] public string? FlavorText { get; init; }
	[JsonPropertyName("flavor_name")] public string? FlavorName { get; init; }

	[JsonPropertyName("card_back_id")] public Guid? CardBackId { get; init; }

	[JsonPropertyName("artist")] public string? Artist { get; init; }
	[JsonPropertyName("artist_ids")] public List<Guid>? ArtistIds { get; init; }
	[JsonPropertyName("illustration_id")] public Guid? IllustrationId { get; init; }

	[JsonPropertyName("border_color")] public string? BorderColor { get; init; }
	[JsonPropertyName("frame")] public string? Frame { get; init; }
	[JsonPropertyName("frame_effects")] public List<string>? FrameEffects { get; init; }
	[JsonPropertyName("security_stamp")] public string? SecurityStamp { get; init; }

	[JsonPropertyName("full_art")] public bool? FullArt { get; init; }
	[JsonPropertyName("textless")] public bool? Textless { get; init; }
	[JsonPropertyName("booster")] public bool? Booster { get; init; }
	[JsonPropertyName("story_spotlight")] public bool? StorySpotlight { get; init; }

	[JsonPropertyName("edhrec_rank")] public int? EdhrecRank { get; init; }
	[JsonPropertyName("penny_rank")] public int? PennyRank { get; init; }

	// Extras
	[JsonPropertyName("attraction_lights")] public List<int>? AttractionLights { get; init; }
	[JsonPropertyName("content_warning")] public bool? ContentWarning { get; init; }

	[JsonPropertyName("printed_name")] public string? PrintedName { get; init; }
	[JsonPropertyName("printed_text")] public string? PrintedText { get; init; }
	[JsonPropertyName("printed_type_line")] public string? PrintedTypeLine { get; init; }

	// Finance / links
	[JsonPropertyName("prices")] public ScryfallPrices? Prices { get; init; }
	[JsonPropertyName("related_uris")] public ScryfallRelatedUris? RelatedUris { get; init; }
	[JsonPropertyName("purchase_uris")] public ScryfallPurchaseUris? PurchaseUris { get; init; }

	// Preview
	[JsonPropertyName("preview")] public ScryfallPreview? Preview { get; init; }
}
