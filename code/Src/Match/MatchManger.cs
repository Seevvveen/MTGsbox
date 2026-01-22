#nullable enable

using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Sandbox.Components;
using Sandbox.Match.MatchServices;

namespace Sandbox.GameNetworking;

/// <summary>
/// One Component Per Match in the Scene
/// Exists on the Host then is replicated to clients
/// - replicated gets [Sync] Properties
/// </summary>
public class MatchManager : Component, Component.INetworkListener
{
	public static MatchManager? Instance { get; private set; }
	
	[Property] public GameObject PlayerPawnPrefab { get; private set; } = null;
	[Property, ReadOnly, Group("DataDebug")] public List<Guid> MatchPlayers;
	public PlayerService Players { get; private set; }

	
	
	[Property, ReadOnly, Group("DataDebug")] public List<Seat> MatchSeats;
	public SeatService Seats {get; private set;}


	protected override Task OnLoad(LoadingContext context)
	{
		context.Title = "Match Manager Loading";
		Instance = this;
		Seats = new SeatService(this);
		Players = new PlayerService(this);
		
		if (!Networking.IsHost) return Task.CompletedTask; // Client has everything they need
		
		if (PlayerPawnPrefab is null)
			throw new NullReferenceException("MatchManager PlayerPawnPrefab is null");
		
		MatchSeats = Seats.GetAll() ?? [];
		
		return Task.CompletedTask;
	}

	public void OnActive(Connection channel)
	{
		Instance = this;
		if (!Networking.IsHost) return;
		Players.Add(channel);
		GameObject.NetworkSpawn(channel); // Host Sends Update
	}
	
	public void OnDisconnected(Connection channel)
	{
		if (!Networking.IsHost) return;
		Players.Remove(channel);
	}
}