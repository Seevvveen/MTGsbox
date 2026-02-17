namespace Sandbox.Components.Card;

public sealed class WorldCardFactory
{
	// Not a component no property
	[Property] public GameObject WorldCardPrefab { get; set; }
	
	
	public WorldCard? GenerateWorldCard(Connection owner, Guid definitionId)
	{
		if (!Networking.IsHost) return null;
	}
	
}