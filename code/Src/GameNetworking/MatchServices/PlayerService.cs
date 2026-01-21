namespace Sandbox.GameNetworking.MatchServices;

/// <summary>
/// Simple Service that handles players for the match
/// WIP
/// </summary>
public sealed class PlayerService
{
	// We are composed into the MatchManager
	private readonly MatchManager _match;
	internal PlayerService(MatchManager match) => _match = match;

	
	public bool DoesPlayerExist( SteamId steamId )
	{
		for ( var i = 0; i < _match.MatchParticipants.Count; i++ )
			if ( _match.MatchParticipants[i].SteamId == steamId )
				return true;
		return false;
	}
	
	public void HostAddPlayer( Connection channel )
	{
		if ( !Networking.IsHost ) return;
		if ( DoesPlayerExist( channel.SteamId ) ) return;

		_match.MatchParticipants.Add( new PlayerData
		{
			SteamId = channel.SteamId
		} );
	}

	public void HostRemovePlayer( Connection channel )
	{
		if ( !Networking.IsHost ) return;

		for ( var i = 0; i < _match.MatchParticipants.Count; i++ )
		{
			if (_match.MatchParticipants[i].SteamId != channel.SteamId) continue;
			_match.MatchParticipants.RemoveAt( i );
			return;
		}
	}

	
	

}

/// <summary>
/// What we store and Sync between Clients
/// </summary>
public struct PlayerData
{
	public SteamId SteamId;
}