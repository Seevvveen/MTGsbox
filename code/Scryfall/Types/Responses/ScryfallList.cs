#nullable enable
namespace Sandbox.Scryfall.Types.Responses;

/// <summary>
/// Generic wrapper for paginated list responses from the Scryfall API.
/// Used for search results, rulings lists, etc.
/// </summary>
/// <typeparam name="T">The type of objects in the data array</typeparam>
public record ScryfallList<T>
{
	[JsonPropertyName( "object" )]
	public string Object { get; init; } = "list";

	/// <summary>
	/// Array of the requested objects
	/// </summary>
	[JsonPropertyName( "data" )]
	public List<T> Data { get; init; } = new();

	/// <summary>
	/// True if there is another page of results available
	/// </summary>
	[JsonPropertyName( "has_more" )]
	public bool HasMore { get; init; }

	/// <summary>
	/// URI to the next page of results, if HasMore is true
	/// </summary>
	[JsonPropertyName( "next_page" )]
	public string? NextPage { get; init; }

	/// <summary>
	/// Total number of cards found (only present for card searches)
	/// </summary>
	[JsonPropertyName( "total_cards" )]
	public int? TotalCards { get; init; }

	/// <summary>
	/// Non-fatal warnings about the request
	/// </summary>
	[JsonPropertyName( "warnings" )]
	public List<string>? Warnings { get; init; } = new();
}
