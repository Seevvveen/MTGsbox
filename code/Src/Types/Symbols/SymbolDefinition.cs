#nullable enable

namespace Sandbox.Types.Symbols;

/// <summary>
/// Gameplay Ready version of ScryfallCardSymbol.cs
/// Goal: no null collections, stable defaults, parsed types, validated required fields.
/// 
/// </summary>
public sealed record SymbolDefinition
{
	// Identity
	public string Symbol { get; init; } = string.Empty;      // "{G}", "{2/W}", etc.
	public string English { get; init; } = string.Empty;

	// Flags
	public bool Transposable { get; init; }
	public bool RepresentsMana { get; init; }
	public bool AppearsInManaCosts { get; init; }
	public bool Hybrid { get; init; }
	public bool Phyrexian { get; init; }
	public bool Funny { get; init; }

	// Numeric
	public float ManaValue { get; init; }
	public float ConvertedManaCost { get; init; }

	// Optional “nice-to-have” parsed types
	public Uri? SvgUri { get; init; }
	public string? LooseVariant { get; init; }

	// Parsed/normalized collections
	public ColorMask Colors { get; init; }
	public IReadOnlyList<string> GathererAlternatives { get; init; } = [];
}
