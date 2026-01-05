namespace Sandbox.Game.Types.Symbols;

/// <summary>
/// - What Kind is it
/// - How much does it add
/// - What colors match this symbol
/// - how much generic
/// - Bool options for life and Variable
/// </summary>
public sealed record ManaSymbol
{
	public ManaSymbolKind Kind;

	// How much this symbol contributes to mana value (CMC)
	public decimal ManaValue;

	// What colors can satisfy this symbol (if any)
	public ManaColor AllowedColors;

	// Generic requirement (for {2}, {2/W})
	public int GenericAmount;

	// Special payment options
	public bool CanPayLife; // Phyrexian
	public bool IsVariable; // X
}

