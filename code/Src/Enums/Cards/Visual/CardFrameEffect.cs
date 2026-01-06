namespace Sandbox.Enums.Cards.Visual;

/// <summary>
/// Additional frame artwork layered on top of the base frame.
/// Maps to the `frame_effects` array.
/// </summary>
[Flags]
public enum FrameEffect
{
	None = 0,

	Legendary               = 1 << 0,
	Miracle                 = 1 << 1,
	Enchantment             = 1 << 2,
	Draft                   = 1 << 3,
	Devoid                  = 1 << 4,
	Tombstone               = 1 << 5,
	ColorShifted            = 1 << 6,
	Inverted                = 1 << 7,

	SunMoonDFC              = 1 << 8,
	CompassLandDFC          = 1 << 9,
	OriginPlaneswalkerDFC   = 1 << 10,
	MoonEldraziDFC          = 1 << 11,
	WaxingWaningMoonDFC     = 1 << 12,

	Showcase                = 1 << 13,
	ExtendedArt             = 1 << 14,
	Companion               = 1 << 15,
	Etched                  = 1 << 16,
	Snow                    = 1 << 17,
	Lesson                  = 1 << 18,
	ShatteredGlass          = 1 << 19,

	ConvertDFC              = 1 << 20,
	FanDFC                  = 1 << 21,
	UpsideDownDFC           = 1 << 22,

	Spree                   = 1 << 23
}

