namespace Sandbox.Game.Types.Symbols;

/// <summary>
/// Payment Requirements for Mana
/// </summary>
public sealed record ManaCost
{
	public static readonly ManaCost Empty =
		new ManaCost { Symbols = Array.Empty<ManaSymbol>() };

	// ManaCost.Symbols
	public IReadOnlyList<ManaSymbol> Symbols { get; init; } = Array.Empty<ManaSymbol>();

	// Converted Manacost
	public decimal ManaValue => Symbols.Sum(s => s.ManaValue);
}
