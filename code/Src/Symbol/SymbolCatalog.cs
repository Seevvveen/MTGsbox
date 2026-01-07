namespace Sandbox.Symbol;

/// <summary>
/// Global
/// </summary>
public sealed class CardSymbolCatalog
{
	/// <summary>
	/// Internal Only Search Authority
	/// </summary>
	private readonly IReadOnlyDictionary<string, SymbolDefinition> _bySymbol;

	/// <summary>
	/// Create our source of truth on creation
	/// </summary>
	/// <param name="defs"></param>
	public CardSymbolCatalog( IEnumerable<SymbolDefinition> defs )
	{
		_bySymbol = defs.ToDictionary( d => d.Symbol );
	}

	/// <summary>
	/// Try to get a symbol out of it
	/// </summary>
	/// <param name="symbol"></param>
	/// <param name="def"></param>
	/// <returns></returns>
	public bool TryGet( string symbol, out SymbolDefinition def )
		=> _bySymbol.TryGetValue( symbol, out def );
}
