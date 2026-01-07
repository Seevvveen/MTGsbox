#nullable enable
using Sandbox.Symbol;

namespace Sandbox.Catalog;

/// <summary>
/// 
/// </summary>
public sealed class SymbolsCatalog
{
	public Catalog<string, SymbolDefinition> BySymbol { get; } = new();
	public bool IsReady => BySymbol.IsReady;
	public int Count => BySymbol.Count;

	public void Clear()
		=> BySymbol.Clear();
	
	public void Publish( IReadOnlyDictionary<string, SymbolDefinition> bySymbol )
	{
		BySymbol.Set( bySymbol );
	}
}

