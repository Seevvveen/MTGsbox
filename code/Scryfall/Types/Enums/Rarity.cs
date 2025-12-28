namespace Sandbox.Scryfall.Types.Enums;

[JsonConverter( typeof( JsonStringEnumConverter ) )]
public enum Rarity
{
	Common,
	Uncommon,
	Rare,
	Mythic,
	Special,
	Bonus
}
