namespace Sandbox.Enums.Cards.Visual;

/// <summary>
/// Visual border color of a Magic card.
/// </summary>
public enum BorderColor : byte
{
	Unknown = 0,

	Black,      // "black"
	White,      // "white"
	Borderless, // "borderless"
	Yellow,     // "yellow" (old cards)
	Silver,     // "silver" (Un-sets)
	Gold        // "gold" (multicolor)
}
