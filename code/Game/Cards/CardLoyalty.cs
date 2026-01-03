namespace Sandbox.Game.Cards;

public class CardLoyalty
{
	public string RawValue { get; set; }
    
	public bool IsNumeric => int.TryParse(RawValue, out _);
	public bool IsVariable => RawValue is "*" or "X";
	public bool IsDiceNotation => RawValue?.Contains("d") == true;
    
	public int? GetNumericValue() => int.TryParse(RawValue, out var val) ? val : null;
    
	public static implicit operator CardLoyalty(string value) => new() { RawValue = value };
	public override string ToString() => RawValue;
}
