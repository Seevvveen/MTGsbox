namespace Sandbox.GameNetworking.MatchServices;

/// <summary>
/// Manages Seats in the match
/// </summary>
public sealed class SeatService
{
	private readonly MatchManager _match;
	private List<Seat> _seats = new();
	
	internal SeatService(MatchManager match) => _match = match;

	public int SeatCount => _seats.Count;
	
	
	public void HostCacheSeats()
	{
		if ( !Networking.IsHost ) return;

		_seats = _match.Scene.GetAllComponents<Seat>().ToList();
		_seats.Sort( (a, b) => a.Order.CompareTo( b.Order ) );
	}

	
	public Seat? HostClaimSeat(Connection channel)
	{
		if ( !Networking.IsHost ) return null;

		// Ensure cached
		if ( _seats.Count == 0 )
			HostCacheSeats();

		return _seats.FirstOrDefault(seat => seat.HostTryAssign(channel));
	}

	
	public void HostReleaseSeat( SteamId steamId )
	{
		if ( !Networking.IsHost ) return;

		if (_seats.Any(t => t.HostClearIfMatches( steamId )))
		{
			return;
		}
	}
	
	
	public int HostGetSeatIndexFor( SteamId steamId )
	{
		if ( !Networking.IsHost ) return -1;

		for ( var i = 0; i < _seats.Count; i++ )
			if ( _seats[i].IsOccupiedBy( steamId ) )
				return i;

		return -1;
	}
	
	
	public Seat? HostGetSeatByIndex( int index )
	{
		if ( !Networking.IsHost ) return null;
		if ( index < 0 || index >= _seats.Count ) return null;
		return _seats[index];
	}
	
}