#nullable enable
namespace Sandbox.Catalog;

public sealed class GameCatalogs
{
	public SymbolsCatalog Symbols { get; } = new();
	public CardsCatalog Cards { get; } = new();
	
	public void Clear()
	{
		Symbols.Clear();
		Cards.Clear();
	}

	public bool IsReady => Symbols.IsReady && Cards.IsReady;
}
