#nullable enable
using Sandbox.Game.Enums;

namespace Sandbox.Engine;

/// <summary>
/// Singleton Owner for Card Symbols, Loaded at StartUp?
/// TODO Cleanup 
/// </summary>
public sealed class SymbolRegistry
{
    private static readonly Lazy<SymbolRegistry> _instance = new(() => new SymbolRegistry());
    public static SymbolRegistry Instance => _instance.Value;
    
    private readonly Dictionary<string, CardSymbol> _symbolsByFull;  // "{W}" -> symbol
    private readonly Dictionary<string, CardSymbol> _symbolsByShort; // "W" -> symbol
    
    private SymbolRegistry()
    {
        var symbols = LoadSymbolsFromCache();
        _symbolsByFull = symbols.ToDictionary(s => s.Symbol, s => s);
        _symbolsByShort = symbols
            .Where(s => s.Symbol.StartsWith("{") && s.Symbol.EndsWith("}"))
            .ToDictionary(s => s.Symbol.Trim('{', '}'), s => s);
    }
    
    public CardSymbol? GetSymbol(string symbol)
    {
        // Try full format first: "{W}"
        if (_symbolsByFull.TryGetValue(symbol, out var result))
            return result;
            
        // Try short format: "W"
        if (_symbolsByShort.TryGetValue(symbol, out result))
            return result;
            
        return null;
    }
    
    public bool TryGetSymbol(string symbol, out CardSymbol? result)
    {
        result = GetSymbol(symbol);
        return result != null;
    }
    
    private static List<CardSymbol> LoadSymbolsFromCache()
    {
        // Your JSON deserialization from the Scryfall cache
        // This is just structure - you'd implement actual loading
        return new List<CardSymbol>();
    }
    
    // Predefined constants for common symbols (fast access without dictionary lookup)
    public static class Common
    {
        // Tap symbols
        public static readonly CardSymbol Tap = new()
        {
            Symbol = "{T}",
            English = "tap this permanent",
            RepresentsMana = false,
            AppearsInManaCosts = false
        };
        
        public static readonly CardSymbol Untap = new()
        {
            Symbol = "{Q}",
            English = "untap this permanent",
            RepresentsMana = false,
            AppearsInManaCosts = false
        };
        
        // Generic mana
        public static readonly CardSymbol Zero = new()
        {
            Symbol = "{0}",
            English = "zero mana",
            ManaValue = 0,
            RepresentsMana = true,
            AppearsInManaCosts = true
        };
        
        public static readonly CardSymbol One = new()
        {
            Symbol = "{1}",
            English = "one generic mana",
            ManaValue = 1,
            RepresentsMana = true,
            AppearsInManaCosts = true
        };
        
        public static readonly CardSymbol Two = new()
        {
            Symbol = "{2}",
            English = "two generic mana",
            ManaValue = 2,
            RepresentsMana = true,
            AppearsInManaCosts = true
        };
        
        // Variables
        public static readonly CardSymbol X = new()
        {
            Symbol = "{X}",
            English = "X generic mana",
            ManaValue = 0,
            RepresentsMana = true,
            AppearsInManaCosts = true
        };
        
        // Colored mana
        public static readonly CardSymbol White = new()
        {
            Symbol = "{W}",
            English = "one white mana",
            ManaValue = 1,
            RepresentsMana = true,
            AppearsInManaCosts = true,
            Colors = new[] { "W" }
        };
        
        public static readonly CardSymbol Blue = new()
        {
            Symbol = "{U}",
            English = "one blue mana",
            ManaValue = 1,
            RepresentsMana = true,
            AppearsInManaCosts = true,
            Colors = new[] { "U" }
        };
        
        public static readonly CardSymbol Black = new()
        {
            Symbol = "{B}",
            English = "one black mana",
            ManaValue = 1,
            RepresentsMana = true,
            AppearsInManaCosts = true,
            Colors = ["B"]
        };
        
        public static readonly CardSymbol Red = new()
        {
            Symbol = "{R}",
            English = "one red mana",
            ManaValue = 1,
            RepresentsMana = true,
            AppearsInManaCosts = true,
            Colors = ["R"]
        };
        
        public static readonly CardSymbol Green = new()
        {
            Symbol = "{G}",
            English = "one green mana",
            ManaValue = 1,
            RepresentsMana = true,
            AppearsInManaCosts = true,
            Colors = ["G"]
        };
        
        public static readonly CardSymbol Colorless = new()
        {
            Symbol = "{C}",
            English = "one colorless mana",
            ManaValue = 1,
            RepresentsMana = true,
            AppearsInManaCosts = true
        };
    }
}
