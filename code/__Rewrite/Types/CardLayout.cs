namespace Sandbox.__Rewrite.Gameplay;

/// <summary>All known Scryfall card layout codes.</summary>
public enum CardLayout : byte
{
	Unknown = 0,

	// Single-face
	Normal,
	Leveler,
	Class,
	Case,
	Saga,
	Mutate,
	Prototype,
	Token,
	Emblem,

	// Two-face
	Split,
	Flip,
	Transform,
	ModalDfc,
	Adventure,
	Planar,
	Scheme,
	Vanguard,

	// Meld
	Meld,

	// Other
	DoubleFacedToken,
	ArtSeries,
	ReversibleCard,
	Host,
	Augment,
}

public static class CardLayoutParser
{
	public static CardLayout Parse( string raw ) => raw switch
	{
		"normal"             => CardLayout.Normal,
		"split"              => CardLayout.Split,
		"flip"               => CardLayout.Flip,
		"transform"          => CardLayout.Transform,
		"modal_dfc"          => CardLayout.ModalDfc,
		"meld"               => CardLayout.Meld,
		"leveler"            => CardLayout.Leveler,
		"class"              => CardLayout.Class,
		"case"               => CardLayout.Case,
		"saga"               => CardLayout.Saga,
		"adventure"          => CardLayout.Adventure,
		"mutate"             => CardLayout.Mutate,
		"prototype"          => CardLayout.Prototype,
		"planar"             => CardLayout.Planar,
		"scheme"             => CardLayout.Scheme,
		"vanguard"           => CardLayout.Vanguard,
		"token"              => CardLayout.Token,
		"double_faced_token" => CardLayout.DoubleFacedToken,
		"emblem"             => CardLayout.Emblem,
		"augment"            => CardLayout.Augment,
		"host"               => CardLayout.Host,
		"art_series"         => CardLayout.ArtSeries,
		"reversible_card"    => CardLayout.ReversibleCard,
		_                    => CardLayout.Unknown
	};
}