namespace Sandbox.Scryfall.Types;

public class Card : CardFace
{
	[JsonPropertyName( "lang" )] public string Lang { get; set; }
	[JsonPropertyName( "artist_ids" )] public List<Guid> ArtistIds { get; set; }
	[JsonPropertyName( "all_parts" )] public List<RelatedCard> AllParts { get; set; }
	[JsonPropertyName( "card_faces" )] public List<CardFace> CardFaces { get; set; }
	[JsonPropertyName( "arena_id" )] public int? ArenaId { get; set; }
	[JsonPropertyName( "cardmarket_id" )] public int? CardmarketId { get; set; }
	[JsonPropertyName( "mtgo_foil_id" )] public int? MtgoFoilId { get; set; }
	[JsonPropertyName( "mtgo_id" )] public int? MtgoId { get; set; }
	[JsonPropertyName( "multiverse_ids" )] public List<int> MultiverseIds { get; set; }
	[JsonPropertyName( "resource_id" )] public string ResourceId { get; set; }
	[JsonPropertyName( "tcgplayer_etched_id" )] public int? TcgplayerEtchedId { get; set; }
	[JsonPropertyName( "tcgplayer_id" )] public int? TcgplayerId { get; set; }
	[JsonPropertyName( "prints_search_uri" )] public Uri PrintsSearchUri { get; set; }
	[JsonPropertyName( "rulings_uri" )] public Uri RulingsUri { get; set; }
	[JsonPropertyName( "scryfall_set_uri" )] public Uri ScryfallSetUri { get; set; }
	[JsonPropertyName( "scryfall_uri" )] public Uri ScryfallUri { get; set; }
	[JsonPropertyName( "set_search_uri" )] public Uri SetSearchUri { get; set; }
	[JsonPropertyName( "set_uri" )] public Uri SetUri { get; set; }
	[JsonPropertyName( "uri" )] public Uri Uri { get; set; }
	[JsonPropertyName( "produced_mana" )] public List<string> ProducedManaRaw { get; set; }
	[JsonIgnore] public ColorIdentity ProducedMana => ColorIdentity.FromScryfall( ProducedManaRaw );
	[JsonPropertyName( "hand_modifier" )] public string HandModifier { get; set; }
	[JsonPropertyName( "life_modifier" )] public string LifeModifier { get; set; }
	[JsonPropertyName( "keywords" )] public List<string> Keywords { get; set; }
	[JsonPropertyName( "legalities" )] public LegalitiesType Legalities { get; set; }
	[JsonPropertyName( "edhrec_rank" )] public int? EdhrecRank { get; set; }
	[JsonPropertyName( "game_changer" )] public bool GameChanger { get; set; }
	[JsonPropertyName( "penny_rank" )] public int? PennyRank { get; set; }
	[JsonPropertyName( "reserved" )] public bool Reserved { get; set; }
	[JsonPropertyName( "flavor_name" )] public string FlavorName { get; set; }
	[JsonPropertyName( "attraction_lights" )] public List<int> AttractionLights { get; set; }
	[JsonPropertyName( "border_color" )] public string BorderColorRaw { get; set; }
	[JsonIgnore] public bool HasBlackBorder => BorderColorRaw == "black";
	[JsonIgnore] public bool HasWhiteBorder => BorderColorRaw == "white";
	[JsonIgnore] public bool IsBorderless => BorderColorRaw == "borderless";
	[JsonPropertyName( "card_back_id" )] public string CardBackId { get; set; }
	[JsonPropertyName( "content_warning" )] public bool ContentWarning { get; set; }
	[JsonPropertyName( "finishes" )] public List<string> FinishesRaw { get; set; }
	[JsonIgnore] public bool HasFoil => FinishesRaw?.Contains( "foil" ) == true;
	[JsonIgnore] public bool HasNonfoil => FinishesRaw?.Contains( "nonfoil" ) == true;
	[JsonIgnore] public bool HasEtched => FinishesRaw?.Contains( "etched" ) == true;
	[JsonPropertyName( "frame" )] public string Frame { get; set; }
	[JsonPropertyName( "frame_effects" )] public List<string> FrameEffects { get; set; }
	[JsonPropertyName( "full_art" )] public bool FullArt { get; set; }
	[JsonPropertyName( "highres_image" )] public bool HighresImage { get; set; }
	[JsonPropertyName( "image_status" )] public string ImageStatus { get; set; }
	[JsonPropertyName( "oversized" )] public bool Oversized { get; set; }
	[JsonPropertyName( "security_stamp" )] public string SecurityStamp { get; set; }
	[JsonPropertyName( "textless" )] public bool Textless { get; set; }
	[JsonPropertyName( "collector_number" )] public string CollectorNumber { get; set; }
	[JsonPropertyName( "rarity" )] public string Rarity { get; set; }
	[JsonPropertyName( "released_at" )] public string ReleasedAt { get; set; }
	[JsonPropertyName( "reprint" )] public bool Reprint { get; set; }
	[JsonPropertyName( "set" )] public string Set { get; set; }
	[JsonPropertyName( "set_id" )] public Guid SetId { get; set; }
	[JsonPropertyName( "set_name" )] public string SetName { get; set; }
	[JsonPropertyName( "set_type" )] public string SetType { get; set; }
	[JsonPropertyName( "story_spotlight" )] public bool StorySpotlight { get; set; }
	[JsonPropertyName( "booster" )] public bool Booster { get; set; }
	[JsonPropertyName( "digital" )] public bool Digital { get; set; }
	[JsonPropertyName( "games" )] public List<string> Games { get; set; }
	[JsonPropertyName( "promo" )] public bool Promo { get; set; }
	[JsonPropertyName( "promo_types" )] public List<string> PromoTypes { get; set; }
	[JsonPropertyName( "variation" )] public bool Variation { get; set; }
	[JsonPropertyName( "variation_of" )] public string VariationOf { get; set; }
	[JsonPropertyName( "prices" )] public CardPrices Prices { get; set; }
	[JsonPropertyName( "purchase_uris" )] public Dictionary<string, string> PurchaseUris { get; set; }
	[JsonPropertyName( "related_uris" )] public Dictionary<string, string> RelatedUris { get; set; }
	[JsonPropertyName( "preview" )] public ScryfallPreview Preview { get; set; }

	public class ScryfallPreview
	{
		[JsonPropertyName( "previewed_at" )] public string PreviewedAt { get; set; }
		[JsonPropertyName( "source" )] public string Source { get; set; }
		[JsonPropertyName( "source_uri" )] public string SourceUri { get; set; }
	}
}
