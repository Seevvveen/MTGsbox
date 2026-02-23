namespace Sandbox.ScryfallData.Types;

// Represents a single symbol in a mana cost e.g. {2}, {W}, {W/U}, {H/W}
public readonly struct ManaCostSymbol(
	string raw,
	MtgColor? primary,
	MtgColor? secondary,
	float cmc,
	bool isGeneric,
	bool isHybrid,
	bool isPhyrexian,
	bool isVariable)
{
	public readonly string     Raw = raw;         // exactly as Scryfall gives it e.g. "{W/U}"
	public readonly MtgColor?  PrimaryColor = primary;
	public readonly MtgColor?  SecondaryColor = secondary; // present on hybrid
	public readonly float      CmcValue = cmc;
	public readonly bool       IsGeneric = isGeneric;
	public readonly bool       IsHybrid = isHybrid;
	public readonly bool       IsPhyrexian = isPhyrexian;
	public readonly bool       IsVariable = isVariable;   // X, Y, Z
}