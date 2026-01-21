using Sandbox.GameNetworking;
using Sandbox.GameNetworking.MatchServices;
using Sandbox.Match.MatchServices;

namespace Sandbox.Components;

public class Player : Component
{
	[ReadOnly] public MatchManager? Match = MatchManager.Instance;
	
	// Global Identifier
	[Property, ReadOnly] public SteamId SteamId { get; private set; }
	
	[Property] public string Name { get; set; } = "unknown";
	[Property] public int LifeTotal { get; set; } = 40;
	[Property] public Seat Seat { get; set; } = null;
	[Property] public bool IsReady { get; set; } = false;
	
	/// <summary>
	/// Called into by the network manager to configure Identity
	/// </summary>
	public void HostSetIdentity(PlayerData data)
	{
		if (!Networking.IsHost) return;
		
		SteamId = data.SteamId;
		Name = data.DisplayName ?? "Player";
		Seat = data.seat;
		IsReady = false;
	}

	// Todo Kinda Jank
	protected override void OnStart()
	{
		if (IsProxy)
		{
			GetComponentInChildren<CameraComponent>().Enabled = false;
			GetComponentInChildren<SimpleMove>().Enabled = false;
		}
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