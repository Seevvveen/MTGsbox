namespace Sandbox.Player;

/// <summary>
/// The "State" of the player, this is not the instance of them in world just data
/// </summary>
public class PlayerState : Component
{
	
	//Sets up a Local variable - each client will resolve local to their instance of it
	private static PlayerState _local;
	public static PlayerState Local
	{
		get
		{
			if ( !_local.IsValid() )
			{
				_local = Game.ActiveScene
				             .GetAllComponents<PlayerState>()
				             .FirstOrDefault( p => p.Network.IsOwner );
			}
			return _local;
		}
	}
	
	//Establish a list of all OTHER players per each player
	public static PlayerState[] All =>
		Game.ActiveScene.GetAllComponents<PlayerState>().Where( p => p.IsValid ).ToArray();
	
	//Properties consistent across all PlayerStates
	[Sync] public int SeatIndex { get; set; }
	[Sync] public int Life { get; set; } = 40;
	[Sync] public bool IsReady { get; set; }

	protected override void OnStart()
	{
		Log.Info( $"PlayerState OnStart: seat={SeatIndex} ownerId={Network.OwnerId} isOwner={Network.IsOwner} isProxy={IsProxy}" );
	}
}
