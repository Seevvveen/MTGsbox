using Sandbox.Diagnostics;

namespace Sandbox.GameNetworking;

/// <summary>
/// Exists within the scene. Inert for Clients, Allows host to manage connections and spawn pawns
/// 
/// </summary>
public class GameNetworkManager : Component, Component.INetworkListener
{
	//DEBUG
	private readonly Logger _log = new Logger("MatchManager");
	
	
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public GameObject SpectatorPrefab { get; set; }
	[Property] public GameObject MatchPrefab {get; set;}

	private MatchManager Match => EnsureMatch();
	
	
	// When Someone is loaded
	public void OnActive(Connection channel)
	{
		if (!Networking.IsHost) return;
		
		_ = EnsureMatch();
		
		var seat = Match.HostTryClaimSeat(channel);
		
		_log.Info(seat);
		
		if (seat != null)
		{
			// Register New Connection Into Match
			Match.HostAddPlayer(new PlayerInfo
			{
				SteamId = channel.SteamId,
				Name = channel.DisplayName,
				Seat = seat.Order,
				Ready = false
			});
			
			// Give them their pawn
			SpawnPlayerPawn(channel, seat);
		}
		else
		{
			SpawnSpectorPawn(channel);
		}
	}

	// TODO - Tell Match Manager to UnRegister Them
	public void OnDisconnected(Connection channel)
	{
		Match.HostRemovePlayerBySteamId(channel.SteamId);
	}
	
	// Player is in Match Give them Player Pawn
	public void SpawnPlayerPawn(Connection channel, Seat seat)
	{
		var go = PlayerPrefab.Clone();
		go.WorldPosition = seat.WorldPosition;
		go.WorldRotation = seat.WorldRotation;
		go.NetworkSpawn( channel );
	}
	
	// Player is NOT part of match, put them in spectator
	public void SpawnSpectorPawn(Connection channel)
	{
		var go = SpectatorPrefab.Clone();
		go.WorldPosition = Vector3.Up * 100;
		go.WorldRotation = Rotation.LookAt(Vector3.Down);
		go.NetworkSpawn( channel );
		//throw new NotImplementedException();
	}

	// Make Sure that we have an instance of a MatchManager to work with
	private MatchManager EnsureMatch()
	{
		if (MatchManager.Instance is not null)
			return MatchManager.Instance;

		if (!Networking.IsHost)
			throw new Exception("MatchManager is not initialized on NonHost"); //TODO Stop Throwing
		
		var go = MatchPrefab.Clone();
		go.NetworkSpawn();
		return go.GetComponent<MatchManager>();
	}
	
}
