namespace Sandbox.Scryfall.Types;

public class RelatedCard : CardBase
{
	[JsonPropertyName( "component" )] public string Component { get; set; }
	[JsonPropertyName( "uri" )] public Uri Uri { get; set; }
}
