#nullable enable

namespace Sandbox.GameNetworking;

/// <summary>
/// One Component Per Match in the Scene
/// Exists on the Host then is replicated to clients
/// - replicated gets [Sync] Properties
/// </summary>
public class MatchManager : Component
{
	
	// Provide a Instance Variable
	// Instance varible will exist on both clients and the host
	// when a client runs MatchManager.Instance it will point to their replicated copy from the host
	// When the host runs MatchManager.Instance it will point to the authoritative copy which we can change.
	// Treat it as the local handle to whatever represents the match - for the host its their authortive copy - for clients its what they read from.
	public static MatchManager? Instance { get; private set; }
	protected override void OnAwake()
	{
		Instance = this;

		if (!Networking.IsHost) return;
		HostCacheSeats();
	}

	protected override void OnDestroy()
	{
		if ( Instance == this )
			Instance = null;
	}


	/// <summary>
	/// Host writes this list, clients will observer this list
	/// </summary>
	[Sync] public NetList<PlayerInfo> PlayerList { get; private set; } = new();
	
	// Only Host Adds Players
	public void HostAddPlayer(PlayerInfo player)
	{
		if (!Networking.IsHost) return;

		//No Duplicates - Make Better with Contains?
		for (int i = 0; i < PlayerList.Count; i++)
			if (PlayerList[i].SteamId == player.SteamId)
				return;
		
		PlayerList.Add( player );
	}

	// Only Host Removes Players
	public void HostRemovePlayerBySteamId( long steamId )
	{
		if ( !Networking.IsHost ) return;

		for ( int i = 0; i < PlayerList.Count; i++ )
			if ( PlayerList[i].SteamId == steamId )
			{
				PlayerList.RemoveAt( i );
				return;
			}
	}


	/// <summary>
	/// Seat Stuff
	/// </summary>
	private List<Seat> _seats = new();
	
	
	/// <summary>
	/// Get Seat Anchors within scene we are in
	/// </summary>
	private void HostCacheSeats()
	{
		_seats = Scene.GetAllComponents<Seat>().ToList();
		_seats.Sort( (a,b) => a.Order.CompareTo(b.Order)  );
	}

	/// <summary>
	/// For GameNetwork Manager to call into to know if we put them into a seat or make them spectator
	/// </summary>
	public Seat? HostTryClaimSeat(Connection channel)
	{
		if (!Networking.IsHost) return null;

		foreach (var seat in _seats)
		{
			if (seat.IsFull) continue;
			seat.HostAssign(channel);
			return seat;
		}
		return null;
	}
	
}

/// <summary>
/// Smaller Player Identity 
/// </summary>
public struct PlayerInfo
{
	public long SteamId {get; set;}
	public string Name {get; set;}
	public int Seat {get; set;}
	public bool Ready {get; set;}
}