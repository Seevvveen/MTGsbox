namespace Sandbox.Scryfall.Types;

public class CardBase : ItemBase
{
	/// <summary>The name of this card. If this card has multiple faces, this field will contain both names separated by '//'.</summary>
	[JsonPropertyName( "name" )]
	public string Name { get; set; }
	/// <summary>The type line of this card.</summary>
	[JsonPropertyName( "type_line" )]
	public string TypeLine { get; set; }
}
