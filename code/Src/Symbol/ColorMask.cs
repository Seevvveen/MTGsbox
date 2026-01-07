namespace Sandbox.Symbol;

/// <summary>
/// Compact gameplay representation of MTG colors (bitmask).
/// </summary>
[Flags]
public enum ColorMask : byte
{
	None  = 0,
	White = 1 << 0,
	Blue  = 1 << 1,
	Black = 1 << 2,
	Red   = 1 << 3,
	Green = 1 << 4,
}
