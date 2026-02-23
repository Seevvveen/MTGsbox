using System;
using System.Text.Json.Serialization;

namespace Sandbox.ScryfallData.Types;

public sealed class ScryfallCard
{
    // ---- Identity ----
    [JsonPropertyName( "id" )]        public string Id       { get; set; }
    [JsonPropertyName( "oracle_id" )] public string OracleId { get; set; }
    [JsonPropertyName( "lang" )]      public string Lang     { get; set; }
    [JsonPropertyName( "layout" )]    public string Layout   { get; set; }

    // ---- Gameplay ----
    [JsonPropertyName( "name" )]             public string       Name           { get; set; }
    [JsonPropertyName( "mana_cost" )]        public string       ManaCost       { get; set; }
    [JsonPropertyName( "cmc" )]              public float        Cmc            { get; set; }
    [JsonPropertyName( "type_line" )]        public string       TypeLine       { get; set; }
    [JsonPropertyName( "oracle_text" )]      public string       OracleText     { get; set; }
    [JsonPropertyName( "colors" )]           public List<string> Colors         { get; set; }
    [JsonPropertyName( "color_identity" )]   public List<string> ColorIdentity  { get; set; }
    [JsonPropertyName( "color_indicator" )]  public List<string> ColorIndicator { get; set; }
    [JsonPropertyName( "keywords" )]         public List<string> Keywords       { get; set; }
    [JsonPropertyName( "legalities" )]       public Dictionary<string, string> Legalities { get; set; }
    [JsonPropertyName( "power" )]            public string       Power          { get; set; }
    [JsonPropertyName( "toughness" )]        public string       Toughness      { get; set; }
    [JsonPropertyName( "loyalty" )]          public string       Loyalty        { get; set; }
    [JsonPropertyName( "defense" )]          public string       Defense        { get; set; }
    [JsonPropertyName( "produced_mana" )]    public List<string> ProducedMana   { get; set; }
    [JsonPropertyName( "reserved" )]         public bool         Reserved       { get; set; }
    [JsonPropertyName( "game_changer" )]     public bool?        GameChanger    { get; set; }
    [JsonPropertyName( "edhrec_rank" )]      public int?         EdhrecRank     { get; set; }
    [JsonPropertyName( "penny_rank" )]       public int?         PennyRank      { get; set; }
    [JsonPropertyName( "hand_modifier" )]    public string       HandModifier   { get; set; }
    [JsonPropertyName( "life_modifier" )]    public string       LifeModifier   { get; set; }
    [JsonPropertyName( "all_parts" )]        public List<ScryfallRelatedCard> AllParts  { get; set; }
    [JsonPropertyName( "card_faces" )]       public List<ScryfallCardFace>   CardFaces { get; set; }

    // ---- Print-Specific ----
    [JsonPropertyName( "set" )]               public string       Set             { get; set; }
    [JsonPropertyName( "set_id" )]            public string       SetId           { get; set; }
    [JsonPropertyName( "set_name" )]          public string       SetName         { get; set; }
    [JsonPropertyName( "set_type" )]          public string       SetType         { get; set; }
    [JsonPropertyName( "collector_number" )]  public string       CollectorNumber { get; set; }
    [JsonPropertyName( "rarity" )]            public string       Rarity          { get; set; }
    [JsonPropertyName( "released_at" )]       public string       ReleasedAt      { get; set; }
    [JsonPropertyName( "reprint" )]           public bool         Reprint         { get; set; }
    [JsonPropertyName( "promo" )]             public bool         Promo           { get; set; }
    [JsonPropertyName( "promo_types" )]       public List<string> PromoTypes      { get; set; }
    [JsonPropertyName( "variation" )]         public bool         Variation       { get; set; }
    [JsonPropertyName( "variation_of" )]      public string       VariationOf     { get; set; }
    [JsonPropertyName( "digital" )]           public bool         Digital         { get; set; }
    [JsonPropertyName( "games" )]             public List<string> Games           { get; set; }
    [JsonPropertyName( "finishes" )]          public List<string> Finishes        { get; set; }
    [JsonPropertyName( "booster" )]           public bool         Booster         { get; set; }
    [JsonPropertyName( "oversized" )]         public bool         Oversized       { get; set; }
    [JsonPropertyName( "border_color" )]      public string       BorderColor     { get; set; }
    [JsonPropertyName( "frame" )]             public string       Frame           { get; set; }
    [JsonPropertyName( "frame_effects" )]     public List<string> FrameEffects    { get; set; }
    [JsonPropertyName( "full_art" )]          public bool         FullArt         { get; set; }
    [JsonPropertyName( "textless" )]          public bool         Textless        { get; set; }
    [JsonPropertyName( "story_spotlight" )]   public bool         StorySpotlight  { get; set; }
    [JsonPropertyName( "content_warning" )]   public bool?        ContentWarning  { get; set; }
    [JsonPropertyName( "highres_image" )]     public bool         HighresImage    { get; set; }
    [JsonPropertyName( "image_status" )]      public string       ImageStatus     { get; set; }
    [JsonPropertyName( "image_uris" )]        public Dictionary<string, string> ImageUris { get; set; }
    [JsonPropertyName( "card_back_id" )]      public string       CardBackId      { get; set; }
    [JsonPropertyName( "security_stamp" )]    public string       SecurityStamp   { get; set; }
    [JsonPropertyName( "watermark" )]         public string       Watermark       { get; set; }
    [JsonPropertyName( "artist" )]            public string       Artist          { get; set; }
    [JsonPropertyName( "artist_ids" )]        public List<string> ArtistIds       { get; set; }
    [JsonPropertyName( "illustration_id" )]   public string       IllustrationId  { get; set; }
    [JsonPropertyName( "flavor_name" )]       public string       FlavorName      { get; set; }
    [JsonPropertyName( "flavor_text" )]       public string       FlavorText      { get; set; }
    [JsonPropertyName( "printed_name" )]      public string       PrintedName     { get; set; }
    [JsonPropertyName( "printed_text" )]      public string       PrintedText     { get; set; }
    [JsonPropertyName( "printed_type_line" )] public string       PrintedTypeLine { get; set; }
    [JsonPropertyName( "prices" )]            public ScryfallPrices Prices        { get; set; }

    // ---- Platform IDs ----
    [JsonPropertyName( "arena_id" )]            public int?      ArenaId           { get; set; }
    [JsonPropertyName( "mtgo_id" )]             public int?      MtgoId            { get; set; }
    [JsonPropertyName( "mtgo_foil_id" )]        public int?      MtgoFoilId        { get; set; }
    [JsonPropertyName( "multiverse_ids" )]      public List<int> MultiverseIds     { get; set; }
    [JsonPropertyName( "tcgplayer_id" )]        public int?      TcgplayerId       { get; set; }
    [JsonPropertyName( "tcgplayer_etched_id" )] public int?      TcgplayerEtchedId { get; set; }
    [JsonPropertyName( "cardmarket_id" )]       public int?      CardmarketId      { get; set; }
}

public sealed class ScryfallCardFace
{
    [JsonPropertyName( "object" )]            public string       Object          { get; set; }
    [JsonPropertyName( "oracle_id" )]         public string       OracleId        { get; set; }
    [JsonPropertyName( "layout" )]            public string       Layout          { get; set; }
    [JsonPropertyName( "name" )]              public string       Name            { get; set; }
    [JsonPropertyName( "mana_cost" )]         public string       ManaCost        { get; set; }
    [JsonPropertyName( "cmc" )]               public float?       Cmc             { get; set; }
    [JsonPropertyName( "type_line" )]         public string       TypeLine        { get; set; }
    [JsonPropertyName( "oracle_text" )]       public string       OracleText      { get; set; }
    [JsonPropertyName( "colors" )]            public List<string> Colors          { get; set; }
    [JsonPropertyName( "color_indicator" )]   public List<string> ColorIndicator  { get; set; }
    [JsonPropertyName( "power" )]             public string       Power           { get; set; }
    [JsonPropertyName( "toughness" )]         public string       Toughness       { get; set; }
    [JsonPropertyName( "loyalty" )]           public string       Loyalty         { get; set; }
    [JsonPropertyName( "defense" )]           public string       Defense         { get; set; }
    [JsonPropertyName( "artist" )]            public string       Artist          { get; set; }
    [JsonPropertyName( "artist_id" )]         public string       ArtistId        { get; set; }
    [JsonPropertyName( "illustration_id" )]   public string       IllustrationId  { get; set; }
    [JsonPropertyName( "image_uris" )]        public Dictionary<string, string> ImageUris { get; set; }
    [JsonPropertyName( "flavor_text" )]       public string       FlavorText      { get; set; }
    [JsonPropertyName( "watermark" )]         public string       Watermark       { get; set; }
    [JsonPropertyName( "printed_name" )]      public string       PrintedName     { get; set; }
    [JsonPropertyName( "printed_text" )]      public string       PrintedText     { get; set; }
    [JsonPropertyName( "printed_type_line" )] public string       PrintedTypeLine { get; set; }
}

public sealed class ScryfallRelatedCard
{
    [JsonPropertyName( "object" )]    public string Object    { get; set; }
    [JsonPropertyName( "id" )]        public string Id        { get; set; }
    [JsonPropertyName( "component" )] public string Component { get; set; }
    [JsonPropertyName( "name" )]      public string Name      { get; set; }
    [JsonPropertyName( "type_line" )] public string TypeLine  { get; set; }
    [JsonPropertyName( "uri" )]       public string Uri       { get; set; }
}

public sealed class ScryfallPrices
{
    [JsonPropertyName( "usd" )]        public string Usd       { get; set; }
    [JsonPropertyName( "usd_foil" )]   public string UsdFoil   { get; set; }
    [JsonPropertyName( "usd_etched" )] public string UsdEtched { get; set; }
    [JsonPropertyName( "eur" )]        public string Eur       { get; set; }
    [JsonPropertyName( "eur_foil" )]   public string EurFoil   { get; set; }
    [JsonPropertyName( "eur_etched" )] public string EurEtched { get; set; }
    [JsonPropertyName( "tix" )]        public string Tix       { get; set; }
}

public sealed class RelatedCard
{
    public Guid                  ScryfallId  { get; init; }
    public RelatedCardComponent  Component   { get; init; }
    public string                Name        { get; init; }
    public string                TypeLine    { get; init; }
}