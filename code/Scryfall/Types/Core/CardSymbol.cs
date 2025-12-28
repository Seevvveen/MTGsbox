#nullable enable
namespace Sandbox.Scryfall.Types.Core;

/// <summary>
/// Represents a Magic card symbol (mana symbols, tap symbols, etc.)
/// </summary>
public record CardSymbol
{
	[JsonPropertyName( "object" )]
	public string Object { get; init; } = "card_symbol";

	/// <summary>
	/// The plaintext symbol, often in curly braces like {W} or {2/U}
	/// </summary>
	[JsonPropertyName( "symbol" )]
	public string Symbol { get; init; } = string.Empty;

	/// <summary>
	/// Alternate version without curly braces, if applicable
	/// </summary>
	[JsonPropertyName( "loose_variant" )]
	public string? LooseVariant { get; init; }

	/// <summary>
	/// English description of the symbol
	/// </summary>
	[JsonPropertyName( "english" )]
	public string English { get; init; } = string.Empty;

	/// <summary>
	/// True if this symbol can be written backwards
	/// </summary>
	[JsonPropertyName( "transposable" )]
	public bool Transposable { get; init; }

	/// <summary>
	/// True if this is a mana symbol
	/// </summary>
	[JsonPropertyName( "represents_mana" )]
	public bool RepresentsMana { get; init; }

	/// <summary>
	/// The mana value (CMC) this symbol represents, if applicable
	/// </summary>
	[JsonPropertyName( "mana_value" )]
	public decimal? ManaValue { get; init; }

	/// <summary>
	/// True if this symbol appears in mana costs
	/// </summary>
	[JsonPropertyName( "appears_in_mana_costs" )]
	public bool AppearsInManaCosts { get; init; }

	/// <summary>
	/// True if this symbol is only used on funny/Un-cards
	/// </summary>
	[JsonPropertyName( "funny" )]
	public bool Funny { get; init; }

	/// <summary>
	/// Colors this symbol represents (W, U, B, R, G, C)
	/// </summary>
	[JsonPropertyName( "colors" )]
	public List<string> Colors { get; init; } = new();

	/// <summary>
	/// True if this is a hybrid mana symbol
	/// </summary>
	[JsonPropertyName( "hybrid" )]
	public bool Hybrid { get; init; }

	/// <summary>
	/// True if this is a Phyrexian mana symbol (can be paid with 2 life)
	/// </summary>
	[JsonPropertyName( "phyrexian" )]
	public bool Phyrexian { get; init; }

	/// <summary>
	/// Alternate representations used by Gatherer
	/// </summary>
	[JsonPropertyName( "gatherer_alternates" )]
	public List<string>? GathererAlternates { get; init; }

	/// <summary>
	/// URI to an SVG image of this symbol
	/// </summary>
	[JsonPropertyName( "svg_uri" )]
	public string? SvgUri { get; init; }
}
