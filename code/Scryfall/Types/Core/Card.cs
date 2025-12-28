#nullable enable
using Sandbox.Scryfall.Types.Components;
using Sandbox.Scryfall.Types.Enums;

namespace Sandbox.Scryfall.Types.Core;

/// <summary>
/// Represents a Magic: The Gathering card.
/// Inherits from CardFace because single-faced cards ARE card faces.
/// Multi-faced cards have a CardFaces property with additional faces.
/// </summary>
public record Card : CardFace
{
	// Override object type
	[JsonPropertyName( "object" )]
	public new string Object { get; init; } = "card";

	// ============================================================================
	// CORE IDENTIFIERS
	// ============================================================================

	[JsonPropertyName( "id" )]
	public Guid Id { get; init; }

	[JsonPropertyName( "lang" )]
	public string Lang { get; init; } = "en";

	//[JsonPropertyName( "oracle_id" )]
	//public new Guid? OracleId { get; init; }

	// Platform-specific IDs
	[JsonPropertyName( "arena_id" )]
	public int? ArenaId { get; init; }

	[JsonPropertyName( "mtgo_id" )]
	public int? MtgoId { get; init; }

	[JsonPropertyName( "mtgo_foil_id" )]
	public int? MtgoFoilId { get; init; }

	[JsonPropertyName( "multiverse_ids" )]
	public List<int> MultiverseIds { get; init; } = new();

	[JsonPropertyName( "resource_id" )]
	public string? ResourceId { get; init; }

	[JsonPropertyName( "tcgplayer_id" )]
	public int? TcgplayerId { get; init; }

	[JsonPropertyName( "tcgplayer_etched_id" )]
	public int? TcgplayerEtchedId { get; init; }

	[JsonPropertyName( "cardmarket_id" )]
	public int? CardmarketId { get; init; }

	// ============================================================================
	// MULTI-FACE CARDS
	// ============================================================================

	[JsonPropertyName( "card_faces" )]
	public List<CardFace>? CardFaces { get; init; }

	[JsonPropertyName( "all_parts" )]
	public List<RelatedCard>? AllParts { get; init; }

	[JsonPropertyName( "layout" )]
	public Layout Layout { get; init; }

	// ============================================================================
	// GAMEPLAY
	// ============================================================================

	[JsonPropertyName( "cmc" )]
	public new decimal Cmc { get; init; }

	[JsonPropertyName( "color_identity" )]
	public List<string> ColorIdentityRaw { get; init; } = new();

	[JsonIgnore]
	public ColorIdentity ColorIdentity => ColorIdentity.FromScryfall( ColorIdentityRaw );

	[JsonPropertyName( "keywords" )]
	public List<string> Keywords { get; init; } = new();

	[JsonPropertyName( "legalities" )]
	public Legalities Legalities { get; init; } = new();

	[JsonPropertyName( "reserved" )]
	public bool Reserved { get; init; }

	[JsonPropertyName( "edhrec_rank" )]
	public int? EdhrecRank { get; init; }

	[JsonPropertyName( "penny_rank" )]
	public int? PennyRank { get; init; }

	[JsonPropertyName( "game_changer" )]
	public bool GameChanger { get; init; }

	[JsonPropertyName( "produced_mana" )]
	public List<string> ProducedManaRaw { get; init; } = new();

	[JsonIgnore]
	public ColorIdentity ProducedMana => ColorIdentity.FromScryfall( ProducedManaRaw );

	[JsonPropertyName( "hand_modifier" )]
	public string? HandModifier { get; init; }

	[JsonPropertyName( "life_modifier" )]
	public string? LifeModifier { get; init; }

	// ============================================================================
	// SET AND PRINT INFORMATION
	// ============================================================================

	[JsonPropertyName( "set" )]
	public string Set { get; init; } = string.Empty;

	[JsonPropertyName( "set_id" )]
	public Guid SetId { get; init; }

	[JsonPropertyName( "set_name" )]
	public string SetName { get; init; } = string.Empty;

	[JsonPropertyName( "set_type" )]
	public SetType SetType { get; init; }

	[JsonPropertyName( "collector_number" )]
	public string CollectorNumber { get; init; } = string.Empty;

	[JsonPropertyName( "rarity" )]
	public Rarity Rarity { get; init; }

	// DateOnly not allowed with whitelist - issue opened
	[JsonPropertyName( "released_at" )]
	public string ReleasedAt { get; init; } = string.Empty;

	[JsonPropertyName( "reprint" )]
	public bool Reprint { get; init; }

	[JsonPropertyName( "digital" )]
	public bool Digital { get; init; }

	[JsonPropertyName( "promo" )]
	public bool Promo { get; init; }

	[JsonPropertyName( "promo_types" )]
	public List<string>? PromoTypes { get; init; }

	[JsonPropertyName( "variation" )]
	public bool Variation { get; init; }

	[JsonPropertyName( "variation_of" )]
	public string? VariationOf { get; init; }

	[JsonPropertyName( "booster" )]
	public bool Booster { get; init; }

	[JsonPropertyName( "story_spotlight" )]
	public bool StorySpotlight { get; init; }

	[JsonPropertyName( "games" )]
	public List<string> Games { get; init; } = new();

	// ============================================================================
	// PHYSICAL PROPERTIES
	// ============================================================================

	[JsonPropertyName( "border_color" )]
	public string BorderColorRaw { get; init; } = string.Empty;

	[JsonIgnore]
	public bool HasBlackBorder => BorderColorRaw == "black";

	[JsonIgnore]
	public bool HasWhiteBorder => BorderColorRaw == "white";

	[JsonIgnore]
	public bool IsBorderless => BorderColorRaw == "borderless";

	[JsonPropertyName( "frame" )]
	public string Frame { get; init; } = string.Empty;

	[JsonPropertyName( "frame_effects" )]
	public List<string> FrameEffects { get; init; } = new();

	[JsonPropertyName( "security_stamp" )]
	public string? SecurityStamp { get; init; }

	[JsonPropertyName( "finishes" )]
	public List<string> Finishes { get; init; } = new();

	[JsonIgnore]
	public bool HasFoil => Finishes.Contains( "foil" );

	[JsonIgnore]
	public bool HasNonfoil => Finishes.Contains( "nonfoil" );

	[JsonIgnore]
	public bool HasEtched => Finishes.Contains( "etched" );

	[JsonPropertyName( "full_art" )]
	public bool FullArt { get; init; }

	[JsonPropertyName( "oversized" )]
	public bool Oversized { get; init; }

	[JsonPropertyName( "textless" )]
	public bool Textless { get; init; }

	[JsonPropertyName( "card_back_id" )]
	public string? CardBackId { get; init; }

	[JsonPropertyName( "content_warning" )]
	public bool ContentWarning { get; init; }

	[JsonPropertyName( "flavor_name" )]
	public string? FlavorName { get; init; }

	[JsonPropertyName( "attraction_lights" )]
	public List<int>? AttractionLights { get; init; }

	// ============================================================================
	// IMAGES
	// ============================================================================

	[JsonPropertyName( "image_status" )]
	public string? ImageStatusRaw { get; init; }

	[JsonIgnore]
	public ImageStatus ImageStatus =>
		Enum.TryParse<ImageStatus>( ImageStatusRaw?.Replace( "_", "" ), true, out var v )
			? v
			: ImageStatus.Unknown;

	[JsonPropertyName( "highres_image" )]
	public bool HighresImage { get; init; }

	[JsonPropertyName( "image_uris" )]
	public new ImageUris? ImageUris { get; init; }

	// ============================================================================
	// ARTIST (override from CardFace for list)
	// ============================================================================

	[JsonPropertyName( "artist_ids" )]
	public List<Guid> ArtistIds { get; init; } = new();

	// ============================================================================
	// PRICES AND URIS
	// ============================================================================

	[JsonPropertyName( "prices" )]
	public Prices Prices { get; init; } = new();

	[JsonPropertyName( "purchase_uris" )]
	public Dictionary<string, string> PurchaseUris { get; init; } = new();

	[JsonPropertyName( "related_uris" )]
	public Dictionary<string, string> RelatedUris { get; init; } = new();

	[JsonPropertyName( "uri" )]
	public string? Uri { get; init; }

	[JsonPropertyName( "scryfall_uri" )]
	public string? ScryfallUri { get; init; }

	[JsonPropertyName( "prints_search_uri" )]
	public string? PrintsSearchUri { get; init; }

	[JsonPropertyName( "rulings_uri" )]
	public string? RulingsUri { get; init; }

	[JsonPropertyName( "scryfall_set_uri" )]
	public string? ScryfallSetUri { get; init; }

	[JsonPropertyName( "set_search_uri" )]
	public string? SetSearchUri { get; init; }

	[JsonPropertyName( "set_uri" )]
	public string? SetUri { get; init; }

	[JsonPropertyName( "preview" )]
	public Preview? Preview { get; init; }
}
