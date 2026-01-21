namespace Sandbox.GameNetworking;

public class Seat : Component
{
	[Property] public int Order {get; set;}

	[Sync] public long OccupantSteamId { get; private set; } = 0;
	
	public bool IsFull => OccupantSteamId != 0;

	
	public void HostAssign(Connection channel)
	{
		if (!Networking.IsHost) return;
		OccupantSteamId = channel.SteamId;
	}

	public void HostClear(Connection channel)
	{
		if (!Networking.IsHost) return;
		OccupantSteamId = 0;
	}
}
