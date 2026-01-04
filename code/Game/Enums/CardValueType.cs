namespace Sandbox.Game.Enums;

/// <summary>
/// Represents format type of any value across the card
/// </summary>
public enum CardValueType
{
	Empty,
	Numeric,  // "1", "2", "20"
	Variable, // "X"
	Special,  // "*"
	Formula,  // "1d4+1"
	Unknown
}
