#nullable enable

using Sandbox.GameNetworking.MatchServices;

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

	/// <summary>
	/// People Participating in the match
	/// </summary>
	[Sync(SyncFlags.FromHost)] public NetList<PlayerData> MatchParticipants { get; private set; } = new();
	
	
	/// <summary>
	/// Services to Keep Main Match Logic Clean
	/// </summary>
	public PlayerService Players { get; private set; }
	public SeatService Seats {get; private set;}
	
	
	protected override void OnAwake()
	{
		Instance = this;
		Seats = new SeatService(this);
		Players = new PlayerService(this);
		
		if (!Networking.IsHost) return;
		Seats.HostCacheSeats();
	}
	
	protected override void OnDestroy()
	{
		if ( Instance == this )
			Instance = null;
	}
	
}