#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;

namespace Sandbox.Match;

public enum MatchPhase
{
	Lobby,
	Starting,
	InGame,
	Ended
}

/// <summary>
/// Host-authoritative match orchestrator.
/// - Dynamic host registry of connections -> PlayerState
/// - Spawns a PlayerState object per connection (host only)
/// - Drives match phase transitions
/// </summary>
public sealed class MatchManager : Component, Component.INetworkListener
{
	public static MatchManager? Instance { get; private set; }

	// -------------------------
	// Inspector
	// -------------------------

	[Property] public bool AutoCreateLobby { get; set; } = true;
	[Property] public int MinPlayersToStart { get; set; } = 2;

	/// <summary>
	/// Prefab that MUST contain a PlayerState component.
	/// </summary>
	[Property] public GameObject? PlayerStatePrefab { get; set; }

	// -------------------------
	// Replicated match state
	// -------------------------

	[Sync] public MatchPhase Phase { get; private set; } = MatchPhase.Lobby;
	[Sync] public int ActiveSeat { get; private set; } = -1;
	[Sync] public int TurnNumber { get; private set; } = 0;

	// -------------------------
	// Host-only runtime registry
	// -------------------------

	private readonly Dictionary<Guid, PlayerState> _playersByConnId = new();
	private readonly Dictionary<Guid, int> _seatByConnId = new();

	private bool IsHost => Networking.IsHost;

	// -------------------------
	// Lifecycle
	// -------------------------

	protected override void OnAwake()
	{
		Instance = this;
	}

	protected override void OnDestroy()
	{
		if ( Instance == this )
			Instance = null;
	}

	protected override async Task OnLoad()
	{
		if ( Scene.IsEditor )
			return;

		if ( AutoCreateLobby && !Networking.IsActive )
		{
			LoadingScreen.Title = "Creating Lobby";
			await Task.DelayRealtimeSeconds( 0.1f );
			Networking.CreateLobby( new() );
		}
	}

	// -------------------------
	// INetworkListener
	// -------------------------

	public bool AcceptConnection( Connection connection )
	{
		// Host gate. Add password/banlist/etc later.
		return true;
	}

	public void OnConnected( Connection connection )
	{
		if ( !IsHost )
			return;

		RegisterOrRebuildPlayerFor( connection );
		TryAutoStartFromLobby();
	}

	public void OnDisconnected( Connection connection )
	{
		if ( !IsHost )
			return;

		UnregisterPlayerFor( connection );

		// If someone leaves mid-game, decide your policy.
		// For now: if fewer than MinPlayers remain, end the match.
		if ( Phase == MatchPhase.InGame )
		{
			var alive = _playersByConnId.Values.Count( p => p.IsValid() );
			if ( alive < MinPlayersToStart )
				EndMatchHost();
		}
		else
		{
			// Lobby: re-evaluate autostart conditions
			TryAutoStartFromLobby();
		}
	}

	/// <summary>
	/// Called when someone is all loaded and entered the game.
	/// Host only.
	/// Useful to (re)spawn per-connection objects if you want to wait until fully loaded.
	/// </summary>
	public void OnActive( Connection connection )
	{
		if ( !IsHost )
			return;

		// Ensure registry entry exists even if Connected fired before components were ready.
		RegisterOrRebuildPlayerFor( connection );
	}

	/// <summary>
	/// Called when previous host left and you became host.
	/// You should rebuild host-only registries from replicated scene objects.
	/// </summary>
	public void OnBecameHost()
	{
		RebuildHostRegistryFromScene();
		TryAutoStartFromLobby();
	}

	// -------------------------
	// Client-visible helpers
	// -------------------------

	public IReadOnlyList<PlayerState> GetPlayersSnapshot()
	{
		// Safe for clients: they see replicated PlayerState objects.
		return Scene.GetAllComponents<PlayerState>().ToList();
	}

	public PlayerState? GetLocalPlayer()
	{
		var local = Connection.Local;
		if ( local is null )
			return null;

		var id = local.Id;
		return Scene.GetAllComponents<PlayerState>().FirstOrDefault( p => p.ConnectionId == id );
	}

	// -------------------------
	// Host registry + spawn
	// -------------------------

	private void RegisterOrRebuildPlayerFor( Connection connection )
	{
		var connId = connection.Id;

		// If we already have a valid PlayerState, keep it.
		if ( _playersByConnId.TryGetValue( connId, out var existing ) && existing.IsValid() )
			return;

		_playersByConnId.Remove( connId );

		var seat = AllocateSeat( connId );
		var ps = SpawnPlayerState( connection, seat );

		_playersByConnId[connId] = ps;
	}

	private void UnregisterPlayerFor( Connection connection )
	{
		var connId = connection.Id;

		if ( _playersByConnId.TryGetValue( connId, out var ps ) )
		{
			_playersByConnId.Remove( connId );
			_seatByConnId.Remove( connId );

			if ( ps.IsValid() )
				ps.GameObject.Destroy();
		}
	}

	private int AllocateSeat( Guid connId )
	{
		if ( _seatByConnId.TryGetValue( connId, out var existing ) )
			return existing;

		var used = new HashSet<int>( _seatByConnId.Values );
		var seat = 0;
		while ( used.Contains( seat ) )
			seat++;

		_seatByConnId[connId] = seat;
		return seat;
	}

	private PlayerState SpawnPlayerState( Connection owner, int seat )
	{
		if ( PlayerStatePrefab is null )
			throw new InvalidOperationException( $"{nameof(MatchManager)}: {nameof(PlayerStatePrefab)} not set." );

		var go = PlayerStatePrefab.Clone( Transform.World );

		// Owner determines who may call OwnerOnly RPCs etc.
		// Snapshot/Object networking is controlled by the GameObject's NetworkMode in editor.
		go.NetworkSpawn( owner );

		var ps = go.Components.Get<PlayerState>( includeDisabled: true );
		if ( ps is null )
			throw new InvalidOperationException( "PlayerStatePrefab must contain a PlayerState component." );

		ps.InitializeHost(
			connectionId: owner.Id,
			displayName: owner.DisplayName ?? "Player",
			seatIndex: seat
		);

		return ps;
	}

	private void RebuildHostRegistryFromScene()
	{
		_playersByConnId.Clear();
		_seatByConnId.Clear();

		foreach ( var ps in Scene.GetAllComponents<PlayerState>() )
		{
			if ( !ps.IsValid() )
				continue;

			_playersByConnId[ps.ConnectionId] = ps;
			_seatByConnId[ps.ConnectionId] = ps.SeatIndex;
		}
	}

	// -------------------------
	// Phase machine (host-only)
	// -------------------------

	private void TryAutoStartFromLobby()
	{
		if ( !IsHost )
			return;

		if ( Phase != MatchPhase.Lobby )
			return;

		var players = _playersByConnId.Values.Where( p => p.IsValid() ).ToList();
		if ( players.Count < MinPlayersToStart )
			return;

		if ( players.Any( p => !p.Ready ) )
			return;

		StartMatchHost();
	}

	private void StartMatchHost()
	{
		if ( !IsHost )
			return;

		if ( Phase != MatchPhase.Lobby )
			return;

		Phase = MatchPhase.Starting;

		var ordered = _playersByConnId.Values
			.Where( p => p.IsValid() )
			.OrderBy( p => p.SeatIndex )
			.ToList();

		if ( ordered.Count == 0 )
		{
			Phase = MatchPhase.Lobby;
			return;
		}

		ActiveSeat = ordered[0].SeatIndex;
		TurnNumber = 1;

		Phase = MatchPhase.InGame;
	}

	private void EndMatchHost()
	{
		if ( !IsHost )
			return;

		Phase = MatchPhase.Ended;
	}

	private void AdvanceTurnHost()
	{
		if ( !IsHost || Phase != MatchPhase.InGame )
			return;

		var orderedSeats = _playersByConnId.Values
			.Where( p => p.IsValid() )
			.Select( p => p.SeatIndex )
			.Distinct()
			.OrderBy( x => x )
			.ToList();

		if ( orderedSeats.Count == 0 )
			return;

		var idx = orderedSeats.IndexOf( ActiveSeat );
		var nextIdx = (idx >= 0) ? (idx + 1) % orderedSeats.Count : 0;

		ActiveSeat = orderedSeats[nextIdx];
		TurnNumber++;
	}

	// -------------------------
	// RPC: client -> host requests
	// -------------------------

	/// <summary>
	/// Client requests toggling Ready. Host validates caller and applies to their PlayerState only.
	/// </summary>
	[Rpc.Host]
	public void RpcSetReady( bool ready )
	{
		if ( !IsHost )
			return;

		var caller = Rpc.Caller;
		if ( caller is null )
			return;

		if ( _playersByConnId.TryGetValue( caller.Id, out var ps ) && ps.IsValid() )
		{
			ps.SetReadyHost( ready );
			TryAutoStartFromLobby();
		}
	}

	/// <summary>
	/// Client requests ending turn. Host validates it's caller's turn (seat matches ActiveSeat).
	/// </summary>
	[Rpc.Host]
	public void RpcEndTurn()
	{
		if ( !IsHost || Phase != MatchPhase.InGame )
			return;

		var caller = Rpc.Caller;
		if ( caller is null )
			return;

		if ( !_playersByConnId.TryGetValue( caller.Id, out var ps ) || !ps.IsValid() )
			return;

		if ( ps.SeatIndex != ActiveSeat )
			return;

		AdvanceTurnHost();
	}
}

/// <summary>
/// Minimal per-player replicated state.
/// Lives on PlayerStatePrefab.
/// </summary>
public sealed class PlayerState : Component
{
	[Sync] public Guid ConnectionId { get; private set; }
	[Sync] public string DisplayName { get; private set; } = string.Empty;
	[Sync] public int SeatIndex { get; private set; } = -1;
	[Sync] public bool Ready { get; private set; } = false;

	public void InitializeHost( Guid connectionId, string displayName, int seatIndex )
	{
		if ( !Networking.IsHost )
			return;

		ConnectionId = connectionId;
		DisplayName = displayName;
		SeatIndex = seatIndex;
		Ready = false;
	}

	public void SetReadyHost( bool ready )
	{
		if ( !Networking.IsHost )
			return;

		Ready = ready;
	}
}
