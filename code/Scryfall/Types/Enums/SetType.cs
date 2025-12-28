namespace Sandbox.Scryfall.Types.Enums;

[JsonConverter( typeof( JsonStringEnumConverter ) )]
public enum SetType
{
	Core,
	Expansion,
	Masters,
	Eternal,
	Alchemy,
	Masterpiece,
	Arsenal,
	[JsonPropertyName( "from_the_vault" )] FromTheVault,
	Spellbook,
	[JsonPropertyName( "premium_deck" )] PremiumDeck,
	[JsonPropertyName( "duel_deck" )] DuelDeck,
	[JsonPropertyName( "draft_innovation" )] DraftInnovation,
	[JsonPropertyName( "treasure_chest" )] TreasureChest,
	Commander,
	Planechase,
	Archenemy,
	Vanguard,
	Funny,
	Starter,
	Box,
	Promo,
	Token,
	Memorabilia,
	Minigame
}
