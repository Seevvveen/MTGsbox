#nullable enable
namespace Sandbox.Scryfall.Types.Responses;

/// <summary>
/// Represents an error response from the Scryfall API.
/// Always accompanied by a 4XX or 5XX HTTP status code.
/// </summary>
public record ScryfallError
{
	[JsonPropertyName( "object" )]
	public string Object { get; init; } = "error";

	/// <summary>
	/// The HTTP status code (e.g., 404, 500)
	/// </summary>
	[JsonPropertyName( "status" )]
	public int Status { get; init; }

	/// <summary>
	/// Computer-readable HTTP status code string
	/// </summary>
	[JsonPropertyName( "code" )]
	public string Code { get; init; } = string.Empty;

	/// <summary>
	/// Human-readable explanation of the error
	/// </summary>
	[JsonPropertyName( "details" )]
	public string Details { get; init; } = string.Empty;

	/// <summary>
	/// Additional context for the error (e.g., "ambiguous" for 404s)
	/// </summary>
	[JsonPropertyName( "type" )]
	public string? Type { get; init; }

	/// <summary>
	/// Non-fatal warnings that also occurred
	/// </summary>
	[JsonPropertyName( "warnings" )]
	public List<string>? Warnings { get; init; }
}
