using System.Threading.Tasks;

namespace Sandbox.GameNetworking;

public class Player : Component
{
	[Property] public string Name { get; set; } = "unknown";
	[Property] public int LifeTotal { get; set; } = 40;
	
	
	
	
	[Property] public GameObject CardPrefabTest { get; set; }
	[Button("TestSpawn")]
	public void CloneCard()
	{
		var NewCard = CardPrefabTest.Clone(WorldPosition + Vector3.Up*10);
		var CardComponenet = NewCard.GetComponentInChildren<Components.Card>();
		CardComponenet.SetRandomCard();
		NewCard.NetworkSpawn(Network.Owner);
	}
	
}