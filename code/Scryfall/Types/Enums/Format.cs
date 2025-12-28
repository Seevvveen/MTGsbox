namespace Sandbox.Scryfall.Types.Enums;

[JsonConverter( typeof( JsonStringEnumConverter ) )]
public enum Format
{
	Standard,
	Future,
	Historic,
	Timeless,
	Gladiator,
	Pioneer,
	Modern,
	Legacy,
	Pauper,
	Vintage,
	Penny,
	Commander,
	Oathbreaker,
	Standardbrawl,
	Brawl,
	Alchemy,
	Paupercommander,
	Duel,
	Oldschool,
	Premodern,
	Predh
}
