using Sandbox.Game.Enums;

namespace Sandbox.Game.Cards;

public abstract class CreatureStat
{
    public string RawValue { get; protected set; }
    public decimal? NumericValue { get; protected set; }
    public StatType Type { get; protected set; }
    public bool IsModifier { get; protected set; }
    
    protected static (decimal?, StatType, bool) ParseCore(string value)
    {
        if (string.IsNullOrEmpty(value))
            return (null, StatType.Empty, false);
            
        // Handle modifiers (+1, +2, -0, +0)
        if (value.StartsWith("+") || (value.StartsWith("-") && value != "-1"))
        {
            var numericPart = value.StartsWith("+") ? value.Substring(1) : value;
            if (decimal.TryParse(numericPart, out decimal modValue))
                return (modValue, StatType.Numeric, true);
        }
        
        // Special symbols
        switch (value)
        {
            case "*": return (null, StatType.Variable, false);
            case "*²": return (null, StatType.VariableSquared, false);
            case "?": return (null, StatType.Unknown, false);
            case "∞": return (null, StatType.Infinity, false);
        }
        
        // Formulas (1+*, 2+*, 7-*, *+1)
        if (value.Contains("*"))
            return (null, StatType.Formula, false);
        
        // Standard numeric
        if (decimal.TryParse(value, out decimal numericValue))
            return (numericValue, StatType.Numeric, false);
        
        return (null, StatType.Unknown, false);
    }
    
    public decimal? GetEffectiveValue(int? variableValue = null)
    {
        switch (Type)
        {
            case StatType.Numeric:
                return NumericValue;
                
            case StatType.Variable:
                return variableValue;
                
            case StatType.VariableSquared:
                return variableValue.HasValue ? variableValue.Value * variableValue.Value : null;
                
            case StatType.Formula:
                return ParseFormula(variableValue);
                
            case StatType.Infinity:
                return decimal.MaxValue;
                
            default:
                return null;
        }
    }
    
    private decimal? ParseFormula(int? variableValue)
    {
        if (!variableValue.HasValue) return null;
        
        // Handle "1+*", "2+*", "*+1"
        var addMatch = System.Text.RegularExpressions.Regex.Match(
            RawValue, @"(\d+\.?\d*|\*)\s*\+\s*(\d+\.?\d*|\*)");
        if (addMatch.Success)
        {
            var left = addMatch.Groups[1].Value == "*" ? variableValue.Value : 
                       decimal.Parse(addMatch.Groups[1].Value);
            var right = addMatch.Groups[2].Value == "*" ? variableValue.Value : 
                        decimal.Parse(addMatch.Groups[2].Value);
            return left + right;
        }
        
        // Handle "7-*"
        var subMatch = System.Text.RegularExpressions.Regex.Match(
            RawValue, @"(\d+\.?\d*)\s*-\s*\*");
        if (subMatch.Success)
        {
            var baseValue = decimal.Parse(subMatch.Groups[1].Value);
            return baseValue - variableValue.Value;
        }
        
        return null;
    }
    
    public bool IsNumeric() => Type == StatType.Numeric;
    public bool IsVariable() => Type == StatType.Variable || Type == StatType.VariableSquared;
    public bool IsFormula() => Type == StatType.Formula;
    
    public override string ToString() => RawValue ?? "null";
}
