namespace Sandbox.GameNetworking;

/// <summary>
/// Represent a Seat Anchor in the scene
/// MatchManagers will look for these components on objects to know where to place players ingame
/// </summary>
public sealed class Seat : Component
{
	[Property] public int Order { get; set; }
	[Sync] public SteamId OccupantSteamId { get; private set; } = default;
	public bool IsFull => OccupantSteamId != default;

	public bool IsOccupiedBy( SteamId steamId )
		=> IsFull && OccupantSteamId == steamId;

	
	public bool HostTryAssign( Connection channel )
	{
		if ( !Networking.IsHost ) return false;
		if ( IsFull ) return false;

		OccupantSteamId = channel.SteamId;
		return true;
	}

	
	public bool HostClearIfMatches( SteamId steamId )
	{
		if ( !Networking.IsHost ) return false;
		if ( !IsOccupiedBy( steamId ) ) return false;

		OccupantSteamId = default;
		return true;
	}
}
