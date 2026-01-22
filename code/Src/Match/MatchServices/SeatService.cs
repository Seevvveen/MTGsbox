using Sandbox.Components;
using Sandbox.GameNetworking;

namespace Sandbox.Match.MatchServices;

/// <summary>
/// Manages Seats in the match
/// </summary>
public sealed class SeatService
{
	// Match Owns us
	private readonly MatchManager _match;
	internal SeatService(MatchManager match) => _match = match;
	
	public List<Seat> All { get; private set; } = null;
	
	public List<Seat>? GetAll()
	{
		if ( !Networking.IsHost ) return null;

		All = _match.Scene.GetAllComponents<Seat>().ToList();
		All.Sort( (a, b) => a.Order.CompareTo( b.Order ) );
		return All;
	}

	public Seat GetNextAvailable()
	{
		foreach (var seat in All)
		{
			if (seat.IsOccupied) continue;
			return seat;
		}
		return null;
	}

	public void FreeSeat(Guid id)
	{
		foreach (var seat in All)
		{
			if (seat.Occupent != id) continue;
			seat.ClearOccupent();
		}
	}
	
	
}