#nullable enable
using Sandbox._Startup;
using Sandbox.Diagnostics;
using CardComp = Sandbox.Components.Card;

namespace Sandbox.Zones;

[Tag("Zone")]
public class Zone : Component
{
	[Property, ReadOnly] public Connection Owner { get; set; } = Connection.Host;
	
	[Property] private GameObject CardPrefab { get; set; } = null;
	
	[Property,ReadOnly]
	public List<Guid> Cards { get; protected set; } = new();

	
	public void AddCard(GameObject card)
	{
		var cardComp = card.GetComponent<CardComp>();
		Cards.Add( cardComp.Definition.Id );
		card.Destroy();
	}

	public GameObject GetCard()
	{
		if (Cards.Count < 1)
			throw new Exception("Zone has no Cards");
		var id = Cards.Last();
		var obj = GenerateCardObject(id);
		Cards.Remove(id);
		return obj;
	}
	
	
	private GameObject GenerateCardObject(Guid id)
	{
		var def = GlobalCatalogs.Cards.ById.GetOrThrow(id);
		
		var card = CardPrefab.Clone(new CloneConfig()
		{
			Name =  $"Card: {def.Name}",
			StartEnabled =  true,
		});
		card.NetworkSpawn(new NetworkSpawnOptions()
		{
			Owner = Owner,
			StartEnabled =  true,
		});
		card.GetComponent<CardComp>().TrySetDefinition(def);
		card.Network.ClearInterpolation();
		
		return card;
	}
	
}
