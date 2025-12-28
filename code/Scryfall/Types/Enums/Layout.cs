namespace Sandbox.Scryfall.Types.Enums;

[JsonConverter( typeof( JsonStringEnumConverter ) )]
public enum Layout
{
	Normal,
	Split,
	Flip,
	Transform,
	[JsonPropertyName( "modal_dfc" )] ModalDfc,
	Meld,
	Leveler,
	Class,
	Case,
	Saga,
	Adventure,
	Mutate,
	Prototype,
	Battle,
	Planar,
	Scheme,
	Vanguard,
	Token,
	[JsonPropertyName( "double_faced_token" )] DoubleFacedToken,
	Emblem,
	Augment,
	Host,
	[JsonPropertyName( "art_series" )] ArtSeries,
	[JsonPropertyName( "reversible_card" )] ReversibleCard
}
