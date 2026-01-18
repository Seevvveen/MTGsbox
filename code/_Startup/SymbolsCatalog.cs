#nullable enable
using Sandbox.Symbol;

namespace Sandbox._Startup;

/// <summary>
/// Catalog for MTG mana symbols and card symbols.
/// Provides fast lookups and symbol parsing utilities.
/// </summary>
public sealed class SymbolsCatalog
{
    public Catalog<string, SymbolDefinition> BySymbol { get; } = new();
    
    public bool IsReady => BySymbol.IsReady;
    public int Count => BySymbol.Count;

    /// <summary>
    /// Publishes the symbol catalog.
    /// </summary>
    public void Publish(IReadOnlyDictionary<string, SymbolDefinition> bySymbol)
    {
        ArgumentNullException.ThrowIfNull(bySymbol);
        BySymbol.Set(bySymbol);
    }

    /// <summary>
    /// Clears the catalog.
    /// </summary>
    public void Clear()
        => BySymbol.Clear();

    // ===== Direct Symbol Lookups =====

    /// <summary>
    /// Gets a symbol definition by its symbol string (e.g., "{G}", "{2/W}").
    /// </summary>
    public SymbolDefinition? GetSymbol(string symbol)
    {
        return BySymbol.TryGet(symbol, out var def) ? def : null;
    }

    /// <summary>
    /// Gets a symbol definition, throwing if not found.
    /// </summary>
    public SymbolDefinition GetSymbolOrThrow(string symbol)
    {
        return BySymbol.GetOrThrow(symbol);
    }

    /// <summary>
    /// Tries to get a symbol definition.
    /// </summary>
    public bool TryGetSymbol(string symbol, out SymbolDefinition definition)
    {
        return BySymbol.TryGet(symbol, out definition);
    }

    /// <summary>
    /// Checks if a symbol exists in the catalog.
    /// </summary>
    public bool ContainsSymbol(string symbol)
    {
        return BySymbol.ContainsKey(symbol);
    }

    // ===== Mana Cost Parsing =====

    /// <summary>
    /// Parses a mana cost string (e.g., "{2}{G}{G}") into individual symbols.
    /// Returns empty list if any symbol is invalid.
    /// </summary>
    public IReadOnlyList<SymbolDefinition> ParseManaCost(string manaCost)
    {
        if (string.IsNullOrWhiteSpace(manaCost))
            return Array.Empty<SymbolDefinition>();

        var symbols = ExtractSymbols(manaCost);
        var definitions = new List<SymbolDefinition>(symbols.Count);

        foreach (var symbol in symbols)
        {
            if (!BySymbol.TryGet(symbol, out var def))
                return Array.Empty<SymbolDefinition>(); // Invalid symbol found
            
            definitions.Add(def);
        }

        return definitions;
    }

    /// <summary>
    /// Tries to parse a mana cost string into individual symbols.
    /// </summary>
    public bool TryParseManaCost(string manaCost, out IReadOnlyList<SymbolDefinition> symbols)
    {
        symbols = ParseManaCost(manaCost);
        return symbols.Count > 0 || string.IsNullOrWhiteSpace(manaCost);
    }

    /// <summary>
    /// Calculates the total mana value (CMC) of a mana cost string.
    /// </summary>
    public float CalculateManaValue(string manaCost)
    {
        var symbols = ParseManaCost(manaCost);
        return symbols.Sum(s => s.ManaValue);
    }

    /// <summary>
    /// Gets the color identity from a mana cost string.
    /// </summary>
    public ColorMask GetColorIdentity(string manaCost)
    {
        var symbols = ParseManaCost(manaCost);
        var colors = ColorMask.None;
        
        foreach (var symbol in symbols)
        {
            colors |= symbol.Colors;
        }
        
        return colors;
    }

    // ===== Filtered Lookups =====

    /// <summary>
    /// Gets all symbols that represent mana.
    /// </summary>
    public IEnumerable<SymbolDefinition> GetManaSymbols()
    {
        return BySymbol.Where(s => s.RepresentsMana);
    }

    /// <summary>
    /// Gets all symbols that can appear in mana costs.
    /// </summary>
    public IEnumerable<SymbolDefinition> GetManaCostSymbols()
    {
        return BySymbol.Where(s => s.AppearsInManaCosts);
    }

    /// <summary>
    /// Gets all hybrid mana symbols.
    /// </summary>
    public IEnumerable<SymbolDefinition> GetHybridSymbols()
    {
        return BySymbol.Where(s => s.Hybrid);
    }

    /// <summary>
    /// Gets all Phyrexian mana symbols.
    /// </summary>
    public IEnumerable<SymbolDefinition> GetPhyrexianSymbols()
    {
        return BySymbol.Where(s => s.Phyrexian);
    }

    /// <summary>
    /// Gets all symbols of a specific color.
    /// </summary>
    public IEnumerable<SymbolDefinition> GetSymbolsByColor(ColorMask color)
    {
        return BySymbol.Where(s => s.Colors.HasFlag(color));
    }

    /// <summary>
    /// Gets all monocolored symbols.
    /// </summary>
    public IEnumerable<SymbolDefinition> GetMonocoloredSymbols()
    {
        return BySymbol.Where(s => 
            s.Colors != ColorMask.None && 
            IsSingleColor(s.Colors));
    }

    /// <summary>
    /// Gets all colorless symbols.
    /// </summary>
    public IEnumerable<SymbolDefinition> GetColorlessSymbols()
    {
        return BySymbol.Where(s => s.Colors == ColorMask.None);
    }

    /// <summary>
    /// Gets all funny/Un-set symbols.
    /// </summary>
    public IEnumerable<SymbolDefinition> GetFunnySymbols()
    {
        return BySymbol.Where(s => s.Funny);
    }

    // ===== Utility Methods =====

    /// <summary>
    /// Gets all unique symbols from the catalog.
    /// </summary>
    public IReadOnlyList<SymbolDefinition> GetAllSymbols()
    {
        return BySymbol.GetAll();
    }

    /// <summary>
    /// Extracts individual symbol strings from a mana cost (e.g., "{2}{G}{G}" → ["{2}", "{G}", "{G}"]).
    /// </summary>
    private static List<string> ExtractSymbols(string manaCost)
    {
        var symbols = new List<string>();
        var current = 0;

        while (current < manaCost.Length)
        {
            var start = manaCost.IndexOf('{', current);
            if (start == -1) break;

            var end = manaCost.IndexOf('}', start);
            if (end == -1) break;

            var symbol = manaCost.Substring(start, end - start + 1);
            symbols.Add(symbol);
            current = end + 1;
        }

        return symbols;
    }

    /// <summary>
    /// Checks if a color mask represents exactly one color.
    /// </summary>
    private static bool IsSingleColor(ColorMask colors)
    {
        // Check if only one bit is set
        return colors != ColorMask.None && (colors & (colors - 1)) == 0;
    }
}
