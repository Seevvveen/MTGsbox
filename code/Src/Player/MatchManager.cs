namespace Sandbox.Player;

public sealed class MatchManager : Component, Component.INetworkListener
{
	public static MatchManager Instance { get; private set; }
	
	[Property, Group("Prefabs")] public GameObject PlayerPrefab { get; set; }
	
	[Sync( SyncFlags.FromHost )] public int PlayerCount { get; set; }

	public PlayerState[] Players => Scene.GetAllComponents<PlayerState>().ToArray();

	protected override void OnAwake()
	{
		Instance = this;
	}

	public void OnActive( Connection channel )
	{
		if ( !Networking.IsHost ) return;
		if ( PlayerPrefab is null ) return;

		SpawnPlayerFor( channel );
	}

	private void SpawnPlayerFor( Connection channel )
	{
		var seat = Players.Length;
		
		var go = PlayerPrefab.Clone( new CloneConfig { Name = channel.DisplayName, } );

		go.Network.SetOrphanedMode( NetworkOrphaned.ClearOwner );
		go.NetworkSpawn( channel );
		
		var ps = go.Components.Get<PlayerState>();
		ps.SeatIndex = seat;
		ps.Life = 40;
		ps.IsReady = false;

		PlayerCount = Players.Length;
		Log.Info( $"Host spawned player seat={seat} owner={channel.DisplayName} ownerId={channel.Id}" );
	}
	
}
