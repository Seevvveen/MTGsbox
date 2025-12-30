// ColorIdentity.cs
#nullable enable

namespace Sandbox.Game.Enums;

/// <summary>
/// Bitmask for MTG colors (game-side). Designed for fast checks.
/// </summary>
[Flags]
public enum ColorIdentity : byte
{
	None = 0,

	W = 1 << 0,
	U = 1 << 1,
	B = 1 << 2,
	R = 1 << 3,
	G = 1 << 4,

	// Optional convenience
	WUBRG = W | U | B | R | G
}
