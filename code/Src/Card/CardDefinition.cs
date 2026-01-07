namespace Sandbox.Card;

public class CardDefinition
{
	public Guid Id { get; init; } = Guid.Empty;
	public string Name { get; init; }
	public CardImageUris ImageUris { get; init; }
}
