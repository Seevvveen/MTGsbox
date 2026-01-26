
using System.Threading.Tasks;
using Sandbox.Seating;

namespace Sandbox.Match;

/// <summary>
/// Orchestrates the match lifecycle and wires connection events into services.
/// </summary>
public sealed class MatchManager : Component
{
	public static MatchManager Instance { get; private set; }
	[Sync(SyncFlags.FromHost)] public NetList<Guid> Players {get; private set;} = new NetList<Guid>();
	
	
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
	}
	

	
}