using Sandbox.Game.Enums;

namespace Sandbox.Game.Cards;

public class Power : CreatureStat
{
	private Power() { }
    
	public static Power Parse(string value)
	{
		var power = new Power { RawValue = value };
		var (numeric, type, isModifier) = ParseCore(value);
        
		power.NumericValue = numeric;
		power.Type = type;
		power.IsModifier = isModifier;
        
		return power;
	}
    
	// Power-specific: Calculate combat damage dealt
	public int CalculateDamage(int? variableValue = null)
	{
		var effectiveValue = GetEffectiveValue(variableValue);
        
		if (!effectiveValue.HasValue)
			return 0;
        
		// Power can't be negative for damage calculation
		return Math.Max(0, (int)Math.Floor(effectiveValue.Value));
	}
    
	// Check if this creature can deal lethal damage to target
	public bool CanDealLethalTo(Toughness targetToughness, int targetDamageMarked, 
	                            int? variableValue = null)
	{
		var damageDealt = CalculateDamage(variableValue);
		var effectiveToughness = targetToughness.GetEffectiveValue();
        
		if (!effectiveToughness.HasValue)
			return false;
        
		return (targetDamageMarked + damageDealt) >= effectiveToughness.Value;
	}
    
	public bool IsInfinite() => Type == StatType.Infinity;
}
