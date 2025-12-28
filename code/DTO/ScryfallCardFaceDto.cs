#nullable enable
public sealed class ScryfallCardFaceDto
{
	[JsonPropertyName( "object" )]
	public string? Object { get; set; }

	[JsonPropertyName( "name" )]
	public string? Name { get; set; }

	[JsonPropertyName( "mana_cost" )]
	public string? ManaCost { get; set; }

	[JsonPropertyName( "type_line" )]
	public string? TypeLine { get; set; }

	[JsonPropertyName( "oracle_text" )]
	public string? OracleText { get; set; }

	[JsonPropertyName( "power" )]
	public string? Power { get; set; }

	[JsonPropertyName( "toughness" )]
	public string? Toughness { get; set; }

	[JsonPropertyName( "loyalty" )]
	public string? Loyalty { get; set; }

	[JsonPropertyName( "colors" )]
	public string[]? Colors { get; set; }

	[JsonPropertyName( "color_indicator" )]
	public string[]? ColorIndicator { get; set; }

	[JsonPropertyName( "oracle_id" )]
	public string? OracleId { get; set; }
}
