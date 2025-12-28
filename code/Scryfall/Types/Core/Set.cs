#nullable enable
using Sandbox.Scryfall.Types.Enums;

namespace Sandbox.Scryfall.Types.Core;

/// <summary>
/// Represents a Magic: The Gathering set.
/// Sets are groups of related cards (expansions, promos, tokens, etc.)
/// </summary>
public record Set
{
	[JsonPropertyName( "object" )]
	public string Object { get; init; } = "set";

	// Identity
	[JsonPropertyName( "id" )]
	public Guid Id { get; init; }

	[JsonPropertyName( "code" )]
	public string Code { get; init; } = string.Empty;

	[JsonPropertyName( "mtgo_code" )]
	public string? MtgoCode { get; init; }

	[JsonPropertyName( "arena_code" )]
	public string? ArenaCode { get; init; }

	[JsonPropertyName( "tcgplayer_id" )]
	public int? TcgplayerId { get; init; }

	[JsonPropertyName( "name" )]
	public string Name { get; init; } = string.Empty;

	// Type and metadata
	[JsonPropertyName( "set_type" )]
	public SetType SetType { get; init; }

	// DateOnly not allowed by whitelist - issue opened 
	[JsonPropertyName( "released_at" )]
	public string? ReleasedAt { get; init; } = string.Empty;

	[JsonPropertyName( "block_code" )]
	public string? BlockCode { get; init; }

	[JsonPropertyName( "block" )]
	public string? Block { get; init; }

	[JsonPropertyName( "parent_set_code" )]
	public string? ParentSetCode { get; init; }

	// Size
	[JsonPropertyName( "card_count" )]
	public int CardCount { get; init; }

	[JsonPropertyName( "printed_size" )]
	public int? PrintedSize { get; init; }

	// Format flags
	[JsonPropertyName( "digital" )]
	public bool Digital { get; init; }

	[JsonPropertyName( "foil_only" )]
	public bool FoilOnly { get; init; }

	[JsonPropertyName( "nonfoil_only" )]
	public bool NonfoilOnly { get; init; }

	// URIs
	[JsonPropertyName( "scryfall_uri" )]
	public string? ScryfallUri { get; init; }

	[JsonPropertyName( "uri" )]
	public string? Uri { get; init; }

	[JsonPropertyName( "icon_svg_uri" )]
	public string? IconSvgUri { get; init; }

	[JsonPropertyName( "search_uri" )]
	public string? SearchUri { get; init; }
}
