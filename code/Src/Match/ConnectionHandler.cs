namespace Sandbox.Match;

public class ConnectionHandler : Component, Component.INetworkListener
{
	public void OnActive(Connection channel)
	{
		MatchManager.Instance.RequestJoin(channel.Id);
	}

	public void OnDisconnected(Connection channel)
	{
		MatchManager.Instance.RemovePlayer(channel.Id);
	}
}