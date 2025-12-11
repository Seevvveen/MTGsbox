namespace Sandbox.Scryfall.Types;

/// <summary>Scryfalls API Response</summary>
public abstract class APIBase
{
	/// <summary>The Scryfall object type string, <i>for example: "card", "list", "error", etc.</i></summary>
	[JsonPropertyName( "object" )]
	public string Object { get; set; }
}

/// <summary>Base for indiviudal Items returned from Scryfall API</summary>
public abstract class ItemBase : APIBase
{
	/// <summary>A unique ID for this item in Scryfall’s database.</summary>
	[JsonPropertyName( "id" )]
	public Guid Id { get; set; }
}
