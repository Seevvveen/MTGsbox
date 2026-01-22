using Sandbox.Components;
using Sandbox.GameNetworking;

namespace Sandbox.Match.MatchServices;

/// <summary>
/// What the match uses to understand players
/// </summary>
public sealed class PlayerService
{
	// We are composed into the MatchManager
	private readonly MatchManager _match;
	internal PlayerService(MatchManager match) => _match = match;

	private GameObject CreatePlayerPawn(Connection channel)
	{
		var seat = _match.Seats.GetNextAvailable();
		
		var plyObj = _match.PlayerPawnPrefab.Clone(cloneConfig: new CloneConfig()
		{
			Name =  channel.DisplayName,
			Parent = _match.GameObject,
			StartEnabled = true,
			Transform = seat.WorldTransform,
		});
		plyObj.GetComponent<Player>().SetPlayer(channel);
		seat.SetOccupent(channel.Id);
		plyObj.NetworkSpawn(channel);
		return plyObj;
	}
	
	public void Add(Connection channel)
	{
		CreatePlayerPawn( channel );
		_match.MatchPlayers.Add( channel.Id );
	}

	public void Remove(Connection channel)
	{
		foreach (var id in _match.MatchPlayers.ToList())
		{
			if (id != channel.Id) continue;
			_match.Seats.FreeSeat(id);
			_match.MatchPlayers.Remove(id);
		}
	}
	
}
