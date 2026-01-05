#nullable enable
using Sandbox.Game.Types.Symbols;

namespace Sandbox.Game.Definitions;

/// <summary>
/// Source of truth for Functionality based per card off OracleKey
/// - Definitions are pure facts about the card, no dependency on anything other than knowng what the card is.
/// </summary>
public abstract record CardDefinition
{
	public Guid Id { get; init; }
	public Guid OracleId { get; init; }

	public string Name { get; init; } = string.Empty;
	public string TypeLine { get; init; } = string.Empty;
	public string OracleText { get; init; } = string.Empty;

	// Authority on cost + mana value
	public ManaCost Cost { get; init; } = ManaCost.Empty;

	// Colors are classification, NOT cost
	public ManaColor ColorIdentity { get; init; }
	public ManaColor? Colors { get; init; }
	public ManaColor? ColorIndicator { get; init; }

	// Convenience ONLY (cannot disagree with Cost)
	public decimal ManaValue => Cost.ManaValue;
}
