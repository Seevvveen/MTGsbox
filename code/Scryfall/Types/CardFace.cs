namespace Sandbox.Scryfall.Types;

public class CardFace : CardBase
{
	[JsonPropertyName( "oracle_id" )] public Guid OracleId { get; set; }
	[JsonPropertyName( "layout" )] public string Layout { get; set; }
	[JsonPropertyName( "illustration_id" )] public Guid IllustrationId { get; set; }
	[JsonPropertyName( "artist" )] public string Artist { get; set; }
	[JsonPropertyName( "artist_id" )] public Guid ArtistId { get; set; }
	[JsonPropertyName( "mana_cost" )] public string ManaCost { get; set; } // Parse into tokens if you want
	[JsonPropertyName( "cmc" )] public float Cmc { get; set; }
	[JsonPropertyName( "colors" )] public List<string> ColorsRaw { get; set; }
	[JsonIgnore] public ColorIdentity Colors => ColorIdentity.FromScryfall( ColorsRaw );
	[JsonPropertyName( "color_indicator" )] public List<string> ColorIndicatorRaw { get; set; }
	[JsonIgnore] public ColorIdentity ColorIndicator => ColorIdentity.FromScryfall( ColorIndicatorRaw );
	[JsonPropertyName( "power" )] public string Power { get; set; }
	[JsonPropertyName( "toughness" )] public string Toughness { get; set; }
	[JsonPropertyName( "defense" )] public string Defense { get; set; }
	[JsonPropertyName( "loyalty" )] public string Loyalty { get; set; }
	[JsonPropertyName( "oracle_text" )] public string OracleText { get; set; }
	[JsonPropertyName( "flavor_text" )] public string FlavorText { get; set; }
	[JsonPropertyName( "printed_name" )] public string PrintedName { get; set; }
	[JsonPropertyName( "printed_text" )] public string PrintedText { get; set; }
	[JsonPropertyName( "printed_type_line" )] public string PrintedTypeLine { get; set; }
	[JsonPropertyName( "image_uris" )] public CardImages ImageUris { get; set; }
	[JsonPropertyName( "watermark" )] public string Watermark { get; set; }
}
