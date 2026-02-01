using System.Threading.Tasks;
using Sandbox.Match;

namespace Sandbox.Players;

public class ConnectionHandler : Component, Component.INetworkListener
{
	[Property] private GameObject PlayerPrefab { get; set; }
	[Property] private GameObject SpectatorPrefab { get; set; }

	protected override Task OnLoad()
	{
		GameObject.NetworkMode = NetworkMode.Never;
		return Task.CompletedTask;
	}

	
	public void OnActive(Connection channel)
	{
		
		if (!MatchManager.Instance.IsFull)
		{
			CreatePlayer(channel);
			MatchManager.Instance.RequestJoin(channel.Id);
		}
		else
		{
			CreateSpectator(channel);	
		}
	}

	public void OnDisconnected(Connection channel)
	{
		MatchManager.Instance.RemovePlayer(channel.Id);
	}

	
	
	private void CreatePlayer(Connection channel)
	{
		if (!Networking.IsHost)
			return;

		var obj = PlayerPrefab.Clone(new CloneConfig()
		{
			Name =  "Player_Test",
		});
		obj.NetworkSpawn(new NetworkSpawnOptions()
		{
			Owner = channel,
		});
	}

	private void CreateSpectator(Connection channel)
	{
	}
	
}