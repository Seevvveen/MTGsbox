#nullable enable
using Sandbox.Scryfall.Types.Components;
using Sandbox.Scryfall.Types.Enums;

namespace Sandbox.Game.Cards;

/// <summary>
/// Normalized, game-friendly card definition.
/// Goal: NO nulls, stable defaults, fast lookups.
/// This is what you cache/index and feed into CardInstance.
/// </summary>
public sealed record CardDefinition
{
	public Guid Id { get; init; }
	public Guid OracleId { get; init; }

	public string Name { get; init; } = string.Empty;
	public string TypeLine { get; init; } = string.Empty;
	public string OracleText { get; init; } = string.Empty;

	public decimal Cmc { get; init; }

	public string Set { get; init; } = string.Empty;
	public string CollectorNumber { get; init; } = string.Empty;
	public string ReleasedAt { get; init; } = string.Empty;

	public string Layout { get; init; } = string.Empty;
	public string Rarity { get; init; } = string.Empty;

	// Keep these as your existing bitmask type if you like.
	public ColorIdentity Colors { get; init; } = ColorIdentity.None;
	public ColorIdentity ColorIdentity { get; init; } = ColorIdentity.None;
	public ColorIdentity ProducedMana { get; init; } = ColorIdentity.None;

	public IReadOnlyList<string> Keywords { get; init; } = Array.Empty<string>();

	// Raw strings are fine; you can add helpers like IsStandardLegal below.
	public IReadOnlyDictionary<string, string> Legalities { get; init; } =
		new Dictionary<string, string>();

	public ImageStatus ImageStatus { get; init; } = ImageStatus.Unknown;
	public bool HighresImage { get; init; }

	// Optional convenience
	public bool IsCreature { get; init; }
	public bool IsLand { get; init; }

	// Image URIs (normalized)
	public IReadOnlyDictionary<string, string> ImageUris { get; init; } =
		new Dictionary<string, string>();

	public bool IsLegalIn( string format )
	{
		return Legalities.TryGetValue( format, out var v ) && v == "legal";
	}

	public string GetImageUriOrEmpty( string key )
	{
		return ImageUris.TryGetValue( key, out var v ) ? v : "";
	}
}
