namespace Sandbox.__Rewrite.Types;

// Represents a single symbol in a mana cost e.g. {2}, {W}, {W/U}, {H/W}
public readonly struct ManaCostSymbol
{
	public readonly string     Raw;         // exactly as Scryfall gives it e.g. "{W/U}"
	public readonly MtgColor?  PrimaryColor;
	public readonly MtgColor?  SecondaryColor; // present on hybrid
	public readonly float      CmcValue;
	public readonly bool       IsGeneric;
	public readonly bool       IsHybrid;
	public readonly bool       IsPhyrexian;
	public readonly bool       IsVariable;   // X, Y, Z

	public ManaCostSymbol(
		string raw,
		MtgColor? primary,
		MtgColor? secondary,
		float cmc,
		bool isGeneric,
		bool isHybrid,
		bool isPhyrexian,
		bool isVariable )
	{
		Raw            = raw;
		PrimaryColor   = primary;
		SecondaryColor = secondary;
		CmcValue       = cmc;
		IsGeneric      = isGeneric;
		IsHybrid       = isHybrid;
		IsPhyrexian    = isPhyrexian;
		IsVariable     = isVariable;
	}
}