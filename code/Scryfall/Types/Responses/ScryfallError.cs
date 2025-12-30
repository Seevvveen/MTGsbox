#nullable enable
namespace Sandbox.Scryfall.Types.Responses;

/// <summary>
/// Scryfall Error so capture it using this type
/// </summary>
public record ScryfallError
{
	[JsonPropertyName( "object" )]
	public string Object { get; init; } = "error";
	
	[JsonPropertyName( "status" )]
	public int Status { get; init; }
	
	// Https Status code
	[JsonPropertyName( "code" )]
	public string Code { get; init; } = string.Empty;

	// Human-readable
	[JsonPropertyName( "details" )]
	public string Details { get; init; } = string.Empty;
	
	// Additional context for the error (e.g., "ambiguous" for 404s)
	[JsonPropertyName( "type" )]
	public string? Type { get; init; }
	
	// Non-fatal warnings that also occurred
	[JsonPropertyName( "warnings" )]
	public List<string>? Warnings { get; init; }
}


// Reponse Example from website
/*
*	HTTP 400 error
*	Content-Type: application/json; charset=utf-8
*
*	{
*		"object": "error",
*		"code": "bad_request",
*		"status": 400,
*		"warnings": [
*			"Invalid expression “is:slick” was ignored. Checking if cards are “slick” is not supported",
*			"Invalid expression “cmc>cmc” was ignored. The sides of your comparison must be different."
*		],
*		"details": "All of your terms were ignored."
*	}
*/
