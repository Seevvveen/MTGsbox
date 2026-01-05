namespace Sandbox.Game.Enums;

[Flags]
public enum CardFinish : byte
{
	None     = 0,

	NonFoil  = 1 << 0, // "nonfoil"
	Foil     = 1 << 1, // "foil"
	Etched   = 1 << 2, // "etched"
}
