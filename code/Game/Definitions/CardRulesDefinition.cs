using Sandbox.Game.Enums;
using Sandbox.Game.Enums.Cards;
using Sandbox.Game.Types;

namespace Sandbox.Game.Cards;

/// <summary>
/// Source of truth for Functionality based per card off OracleKey
/// - Definitions are pure facts about the card, no dependency on anything other than knowng what the card is.
/// </summary>
public sealed record CardRuleDefinition
{
	// Identity
	public Guid OracleId { get; init; }
	public string Name { get; init; }
	public string? BackName { get; init; } // Back face name (DFC)
	
	// Layout / Faces
	public Layout Layout { get; init; }
	public List<CardFace> CardFaces { get; init; } = [];
	
	// Cost / Color
	public ManaCost ManaCost { get; init; }
	public float ConvertedManaCost { get; init; }
	public float CMC => ConvertedManaCost; // Alias
	public ColorIdentity ColorIdentity { get; init; }
	
	// Rules Text / Types
	public string TypeLine { get; init; }
	public string OracleText { get; init; }
	
	// Stats (presence-based)
	public string? Power { get; init; }
	public string? Toughness { get; init; }
	public Loyalty Loyalty { get; init; }
	public int? Defense { get; init; } // Battle-only
	
	// Keywords / Abilities
	public List<string> KeywordAbilities { get; init; } = []; // TODO: typed keywords
	
	// Mana Production
	public string? ProducedMana { get; init; } // TODO: strong type
	
	// Special Rule Modifiers
	public int? HandModifier { get; init; } // Vanguard
	public int? LifeModifier { get; init; } // Vanguard
}
