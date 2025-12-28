namespace Sandbox.Scryfall.Types.Enums;

[JsonConverter( typeof( JsonStringEnumConverter ) )]
public enum BorderColor
{
	Black,
	White,
	Borderless,
	Silver,
	Gold
}
