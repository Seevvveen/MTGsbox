using System.Numerics;
using Sandbox.Diagnostics;

namespace Sandbox.GameNetworking;

/// <summary>
/// A Factory type of class that:
/// Produces Network Objects
/// ...
/// </summary>
public class GameNetworkManager : Component, Component.INetworkListener
{
	[Sync] public NetList<Connection> PlayerList { get; private set; } = new();
	[Property] public GameObject PlayerPrefab { get; set; }
	//DEBUG
	private readonly Logger _log = new Logger("MatchManager");
	
	// When Someone is loaded
	public void OnActive(Connection channel)
	{
		PlayerList.Add(channel);

		var player = PlayerPrefab.Clone(Vector3.Random);
		player.GetComponent<Player>().Name = channel.DisplayName;
		MoveToSpawnPointTag(player);
		
		player.NetworkSpawn(channel);
	}

	//TODO Add "Seat Full Spawn Spector Camera"
	private void MoveToSpawnPointTag(GameObject player)
	{
		if (!Networking.IsHost)
			return;

		foreach (var obj in Scene.GetAllObjects(true))
		{
			if ( !obj.Tags.Has("spawnpoint") )
				continue;
			
			if ( !obj.Enabled )
				continue;
			
			player.WorldPosition = obj.WorldPosition;
			player.WorldRotation = obj.WorldRotation;
			obj.Enabled = false;
			break;
		}
	}
	
	
	
	
	public void OnDisconnected(Connection channel)
	{
		PlayerList.Remove(channel);
	}
	
	
}
