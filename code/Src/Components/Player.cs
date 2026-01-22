using Sandbox.GameNetworking;
using Sandbox.Match.MatchServices;

namespace Sandbox.Components;

public class Player : Component
{
	[ReadOnly] public MatchManager? Match = MatchManager.Instance;
	
	// Global Identifier
	[Property, ReadOnly] public SteamId? SteamId { get; private set; }
	[Property, ReadOnly] public bool HasBeenGivenIdentity { get; private set; } = false;
	
	[Property] public string Name { get; set; } = "unknown";
	[Property] public int LifeTotal { get; set; } = 40;
	[Property] public Seat Seat { get; set; } = null;
	[Property] public bool IsReady { get; set; } = false;
	
	public void SetPlayer(Connection channel)
	{
		if (HasBeenGivenIdentity) return;
		
		SteamId = channel.SteamId;
		
		
		HasBeenGivenIdentity =  true;
	}
	
	protected override void OnStart()
	{
		if (!IsProxy) return;
		GetComponentInChildren<CameraComponent>().Enabled = false;
		GetComponentInChildren<SimpleMove>().Enabled = false;
	}


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