#nullable enable
using Sandbox.Game.Cards;
using Sandbox.Scryfall.Types.Components;
using Sandbox.Scryfall.Types.Enums;
using Sandbox.Scryfall.Types.Raw;

namespace Sandbox.Scryfall.Mapping;

public static class CardDefinitionMapper
{
	public static CardDefinition ToDefinition( ScryfallCard raw )
	{
		// Prefer top-level oracle_id, but fall back to first face oracle_id, else Id.
		var oracleId =
			raw.OracleId
			?? raw.CardFaces?.FirstOrDefault()?.OracleId
			?? raw.Id;

		var keywords = raw.Keywords ?? new List<string>();

		var legalities = raw.Legalities != null
			? NormalizeStringDict( raw.Legalities )
			: new Dictionary<string, string>();

		var imageUris = raw.ImageUris != null
			? NormalizeStringDict( raw.ImageUris )
			: new Dictionary<string, string>();

		var typeLine = raw.TypeLine ?? "";
		var name = raw.Name ?? "";

		return new CardDefinition
		{
			Id = raw.Id,
			OracleId = oracleId,

			Name = name,
			TypeLine = typeLine,
			OracleText = raw.OracleText ?? "",

			Cmc = raw.Cmc ?? 0m,

			Set = raw.Set ?? "",
			CollectorNumber = raw.CollectorNumber ?? "",
			ReleasedAt = raw.ReleasedAt ?? "",

			Layout = raw.Layout ?? "",
			Rarity = raw.Rarity ?? "",

			Colors = ColorIdentity.FromScryfall( raw.Colors ?? new() ),
			ColorIdentity = ColorIdentity.FromScryfall( raw.ColorIdentity ?? new() ),
			ProducedMana = ColorIdentity.FromScryfall( raw.ProducedMana ?? new() ),

			Keywords = keywords.Count == 0 ? Array.Empty<string>() : keywords.ToArray(),

			Legalities = legalities,
			ImageStatus = ParseImageStatus( raw.ImageStatus ),
			HighresImage = raw.HighresImage ?? false,
			ImageUris = imageUris,

			IsCreature = typeLine.Contains( "Creature", StringComparison.OrdinalIgnoreCase ),
			IsLand = typeLine.Contains( "Land", StringComparison.OrdinalIgnoreCase )
		};
	}

	private static Dictionary<string, string> NormalizeStringDict( Dictionary<string, string?> dict )
	{
		var result = new Dictionary<string, string>( dict.Count, StringComparer.OrdinalIgnoreCase );
		foreach ( var (k, v) in dict )
		{
			if ( string.IsNullOrWhiteSpace( k ) ) continue;
			result[k] = v ?? "";
		}
		return result;
	}

	private static ImageStatus ParseImageStatus( string? s )
	{
		if ( string.IsNullOrWhiteSpace( s ) ) return ImageStatus.Unknown;

		// Scryfall uses snake_case; make it forgiving.
		var squashed = s.Replace( "_", "" );

		return Enum.TryParse<ImageStatus>( squashed, ignoreCase: true, out var v )
			? v
			: ImageStatus.Unknown;
	}
}
