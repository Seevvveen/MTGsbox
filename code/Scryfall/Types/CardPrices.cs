using System.Globalization;
namespace Sandbox.Scryfall.Types;

public struct CardPrices
{
	[JsonPropertyName( "usd" )] public string Usd { get; set; }
	[JsonPropertyName( "usd_foil" )] public string UsdFoil { get; set; }
	[JsonPropertyName( "usd_etched" )] public string UsdEtched { get; set; }
	[JsonPropertyName( "eur" )] public string Eur { get; set; }
	[JsonPropertyName( "eur_foil" )] public string EurFoil { get; set; }
	[JsonPropertyName( "tix" )] public string Tix { get; set; }

	// Optional numeric accessors
	[JsonIgnore] public float? UsdValue => TryParse( Usd );
	[JsonIgnore] public float? UsdFoilValue => TryParse( UsdFoil );
	[JsonIgnore] public float? UsdEtchedValue => TryParse( UsdEtched );
	[JsonIgnore] public float? EurValue => TryParse( Eur );
	[JsonIgnore] public float? EurFoilValue => TryParse( EurFoil );
	[JsonIgnore] public float? TixValue => TryParse( Tix );

	private static float? TryParse( string s )
	{
		if ( string.IsNullOrWhiteSpace( s ) )
			return null;

		return float.TryParse(
			s,
			NumberStyles.Float,
			CultureInfo.InvariantCulture,
			out var value
		)
			? value
			: (float?)null;
	}
}
