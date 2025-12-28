namespace Sandbox.Scryfall.Types.Enums;

[JsonConverter( typeof( JsonStringEnumConverter ) )]
public enum Frame
{
	[JsonPropertyName( "1993" )] Frame1993,
	[JsonPropertyName( "1997" )] Frame1997,
	[JsonPropertyName( "2003" )] Frame2003,
	[JsonPropertyName( "2015" )] Frame2015,
	Future
}
