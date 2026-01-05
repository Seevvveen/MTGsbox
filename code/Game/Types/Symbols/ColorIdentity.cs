namespace Sandbox.Game.Enums;

/// <summary>
/// Same as ManaColor But Saving Just in case
/// </summary>
[Flags]
public enum ColorIdentity : byte
{
	None = 0,

	// Flags (use these for bitwise operations)
	W = 1 << 0,
	U = 1 << 1,
	B = 1 << 2,
	R = 1 << 3,
	G = 1 << 4,

	Colorless = None,
    
	// Aliases for readability (these reference the same values)
	White = W,
	Blue = U,
	Black = B,
	Red = R,
	Green = G,
    
	// Common multicolor combinations
	Azorius = W  | U,
	Dimir = U    | B,
	Rakdos = B   | R,
	Gruul = R    | G,
	Selesnya = G | W,
	Orzhov = W   | B,
	Izzet = U    | R,
	Golgari = B  | G,
	Boros = R    | W,
	Simic = G    | U,
    
	// Shards
	Esper = W  | U | B,
	Grixis = U | B | R,
	Jund = B   | R | G,
	Naya = R   | G | W,
	Bant = G   | W | U,
    
	// Wedges
	Abzan = W  | B | G,
	Jeskai = U | R | W,
	Sultai = B | G | U,
	Mardu = R  | W | B,
	Temur = G  | U | R,
    
	// 4-color
	NonWhite = U | B | R | G,
	NonBlue = W  | B | R | G,
	NonBlack = W | U | R | G,
	NonRed = W   | U | B | G,
	NonGreen = W | U | B | R,
    
	// 5-color
	WUBRG = W | U | B | R | G,
	FiveColor = WUBRG,
}
