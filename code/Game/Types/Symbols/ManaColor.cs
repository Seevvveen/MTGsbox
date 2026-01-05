namespace Sandbox.Game.Types.Symbols;

/// <summary>
/// COLOR IDENTITY
/// </summary>
[Flags]
public enum ManaColor
{
	None = 0,
	W = 1 << 0,
	U = 1 << 1,
	B = 1 << 2,
	R = 1 << 3,
	G = 1 << 4,
	C = 1 << 5, // colorless
	S = 1 << 6  // snow
}
