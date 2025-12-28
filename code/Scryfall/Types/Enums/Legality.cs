namespace Sandbox.Scryfall.Types.Enums;

[JsonConverter( typeof( JsonStringEnumConverter ) )]
public enum Legality
{
	Legal,
	[JsonPropertyName( "not_legal" )] NotLegal,
	Restricted,
	Banned
}
