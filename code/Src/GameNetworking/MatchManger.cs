#nullable enable

namespace Sandbox.GameNetworking;

public class MatchManager : Component
{
	
	// Provide a Instance Variable
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
	/// Communicate Who is in the game
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
/// Match Information
/// </summary>
public struct PlayerInfo
{
	public long SteamId {get; set;}
	public string Name {get; set;}
	public int Seat {get; set;}
	public bool Ready {get; set;}
}