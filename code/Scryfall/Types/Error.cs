namespace Sandbox.Scryfall.Types;

/// <summary>
/// Scryfall Threw Error
/// </summary>
public class Error : APIBase
{
	[JsonPropertyName( "status" )] public int? Status { get; set; }
	[JsonPropertyName( "code" )] public string Code { get; set; }
	[JsonPropertyName( "details" )] public string Details { get; set; }
	[JsonPropertyName( "type" )] public string Type { get; set; }
	[JsonPropertyName( "warnings" )] public List<string> Warnings { get; set; }
}
