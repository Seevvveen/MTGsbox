using System.Text.RegularExpressions;
using Sandbox.Engine.StartUp;
using Sandbox.Game.Enums;

namespace Sandbox.Engine;

public static class ManaCostParser
{
	public static ManaCost? Parse(string? manaCostString)
	{
		if (string.IsNullOrWhiteSpace(manaCostString))
			return null;
            
		var symbols = new List<CardSymbol>();
		var registry = SymbolRegistry.Instance;
        
		// Extract all {...} tokens
		var matches = Regex.Matches(manaCostString, @"\{([^}]+)\}");
        
		foreach (Match match in matches)
		{
			var symbolText = match.Groups[1].Value;
			var symbol = registry.GetSymbol(symbolText);
            
			if (symbol == null)
			{
				throw new Exception(
					$"Unknown symbol '{symbolText}' in mana cost '{manaCostString}'");
			}
            
			symbols.Add(symbol);
		}
        
		return new ManaCost(manaCostString, symbols);
	}
}
