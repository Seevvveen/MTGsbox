using Sandbox.Game.Cards;

namespace Sandbox.Game.Instances;

/// <summary>
/// Mutable gameplay object representing one physical card in a match.
/// References immutable definitions and tracks runtime state only.
/// </summary>
public sealed class CardInstance
{
	// Definitions (read-only)
	public CardRuleDefinition Rules { get; }
	
	// State
	public bool IsTapped { get; set; }
	
	public CardInstance( CardRuleDefinition rules )
	{
		Rules = rules;
	}
}
