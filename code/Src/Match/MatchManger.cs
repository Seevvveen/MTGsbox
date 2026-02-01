
using System.Threading.Tasks;
using Sandbox.Seating;

namespace Sandbox.Match;

/// <summary>
/// Orchestrates the match lifecycle and wires connection events into services.
/// </summary>
public sealed class MatchManager : Component
{
	public static MatchManager Instance { get; private set; }

	//Players
	[Property] public byte MaxPlayers { get; private set; } = 4;
	[Property, ReadOnly] public bool IsFull => Players.Count >= MaxPlayers;
	[Sync(SyncFlags.FromHost)] public NetList<Guid> Players {get; private set;} = new NetList<Guid>();
	
	[Property, ReadOnly] public Dictionary<Guid,bool> PlayerReady {get; private set;} = new Dictionary<Guid, bool>();
	
	//Game
	[Property] private byte TurnNumber { get; set; } = 0;
	[Property] private bool HasStarted {get; set;}
	
	
	
	protected override Task OnLoad( LoadingContext context )
	{
		context.Title = "Match Manager Loading";
		GameObject.NetworkMode = NetworkMode.Object;
		GameObject.NetworkSpawn(new NetworkSpawnOptions()
		{
			AlwaysTransmit = true,
			Owner = Connection.Host,
			StartEnabled = true,
			OwnerTransfer = OwnerTransfer.Fixed,
			Flags = NetworkFlags.NoTransformSync,
		});
		Instance = this;
		
		return Task.CompletedTask;
	}

	
	[Button("CheckReady")]
	public void CheckReady()
	{
		foreach (var ply in PlayerReady)
		{
			if (!ply.Value)
			{
				Log.Info("Someone Not Ready");
				break;
			}
				
			StartGame();
		}
	}
	
	public void StartGame()
	{
		throw new  NotImplementedException();
	}
	
	
	[Rpc.Host]
	public void RequestJoin(Guid id)
	{
		foreach (var seat in Scene.GetAll<SeatAnchor>())
		{
			if (!seat.IsOccupied)
			{
				seat.SetOccupant(id);
				break;
			}
		}
		
		Players.Add(id);
		PlayerReady.Add(id, false);
	}

	[Rpc.Host]
	public void RemovePlayer(Guid id)
	{
		foreach (var seat in Scene.GetAll<SeatAnchor>())
		{
			if (seat.Occupent == id)
			{
				seat.Clear();
			}
		}
		Players.RemoveAt(Players.IndexOf(id));
		PlayerReady.Remove(id);
	}
	

	
}