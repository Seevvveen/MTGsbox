using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Sandbox.GameNetworking;

public class Player : Component
{
	[ReadOnly] public MatchManager? Match = MatchManager.Instance;
	
	// Global Identifier
	[Property, ReadOnly] public SteamId SteamId { get; private set; }
	
	
	[Property] public string Name { get; set; } = "unknown";
	[Property] public int LifeTotal { get; set; } = 40;
	[Property] public Seat Seat { get; set; } = null;
	
	
	/// <summary>
	/// Called into by the network manager to our identity based off connection
	/// </summary>
	public void HostSetIdentity(Connection channel, Seat seat)
	{
		if (!Networking.IsHost) return;
		
		SteamId = channel.SteamId;
		Name = channel.DisplayName ?? "Player";
		Seat = seat;
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