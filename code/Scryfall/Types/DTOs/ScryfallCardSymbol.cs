#nullable enable
namespace Sandbox.Scryfall.Types.DTOs;

/// <summary>
/// Never Instanced, Deserialization target for Symbol Reader
/// </summary>
public sealed record ScryfallCardSymbol
{
	[JsonPropertyName( "object" )] public string? Object { get; init; }
	[JsonPropertyName( "symbol" )] public string? Symbol { get; init; }
	[JsonPropertyName( "svg_uri" )] public string? SvgUri { get; init; }
	[JsonPropertyName( "loose_variant" )] public string? LooseVariant { get; init; }
	[JsonPropertyName( "english" )] public string? English { get; init; }
	[JsonPropertyName( "transposable" )] public bool Transposable { get; init; }
	[JsonPropertyName( "represents_mana" )] public bool RepresentsMana { get; init; }
	[JsonPropertyName( "appears_in_mana_costs" )] public bool AppearsInManaCosts { get; init; }
	[JsonPropertyName( "mana_value" )] public float? ManaValue { get; init; }
	[JsonPropertyName( "hybrid" )] public bool Hybrid { get; init; }
	[JsonPropertyName( "phyrexian" )] public bool Phyrexian { get; init; }
	[JsonPropertyName( "cmc" )] public float? ConvertedManaCost { get; init; }
	[JsonPropertyName( "funny" )] public bool Funny { get; init; }
	[JsonPropertyName( "colors" )] public List<string>? Colors { get; init; }
	[JsonPropertyName( "gatherer_alternatives" )] public List<string>? GathererAlternatives { get; init; }
}
