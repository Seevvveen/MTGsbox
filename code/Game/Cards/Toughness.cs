using Sandbox.Game.Enums;

namespace Sandbox.Game.Cards;

public class Toughness : CreatureStat
{
	private Toughness() { }
    
	public static Toughness Parse(string value)
	{
		var toughness = new Toughness { RawValue = value };
		var (numeric, type, isModifier) = ParseCore(value);
        
		toughness.NumericValue = numeric;
		toughness.Type = type;
		toughness.IsModifier = isModifier;
        
		return toughness;
	}
    
	// Toughness-specific: Check if creature should die from marked damage
	public bool IsLethalDamage(int damageMarked, int? variableValue = null)
	{
		var effectiveValue = GetEffectiveValue(variableValue);
        
		if (!effectiveValue.HasValue)
			return false; // Unknown toughness, can't determine lethality
        
		if (Type == StatType.Infinity)
			return false; // Infinite toughness can't be killed by damage
        
		return damageMarked >= effectiveValue.Value;
	}
    
	// Calculate remaining "health"
	public decimal? GetRemainingToughness(int damageMarked, int? variableValue = null)
	{
		var effectiveValue = GetEffectiveValue(variableValue);
        
		if (!effectiveValue.HasValue)
			return null;
        
		return Math.Max(0, effectiveValue.Value - damageMarked);
	}
    
	// Check if toughness can be reduced to 0 or less by effects
	public bool WouldDieFromToughnessReduction(int reduction, int? variableValue = null)
	{
		var effectiveValue = GetEffectiveValue(variableValue);
        
		if (!effectiveValue.HasValue)
			return false;
        
		return effectiveValue.Value - reduction <= 0;
	}
}
