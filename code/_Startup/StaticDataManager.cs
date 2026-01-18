#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.Scryfall;

namespace Sandbox._Startup;

public sealed class StaticDataManager : GameObjectSystem<StaticDataManager>, ISceneStartup
{
	private static readonly object Gate = new();
	private static Task? _startupTask;
	private static Exception? _fatal;

	private static readonly Logger Log = new( "StaticDataManager" );

	private bool _releasedGameplay;

	public StaticDataManager( Scene scene ) : base( scene )
	{
		// Fallback: ensure startup begins even if ISceneStartup sequencing differs (editor/play).
		Listen( Stage.SceneLoaded, order: -1000, OnSceneLoaded, "StaticData:SceneLoaded" );

		// Gate your gameplay until ready (this does not block Scene.Load; it blocks your logic).
		Listen( Stage.StartUpdate, order: -10_000, GateGameplayStartUpdate, "StaticData:GateGameplay" );
	}

	// -------------------------
	// ISceneStartup
	// -------------------------

	void ISceneStartup.OnHostPreInitialize( SceneFile scene ) => EnsureStarted();
	void ISceneStartup.OnHostInitialize() => EnsureStarted();
	void ISceneStartup.OnClientInitialize() => EnsureStarted();

	// -------------------------
	// Public gate (THIS must start the pipeline)
	// -------------------------

	public static Task EnsureReadyAsync()
	{
		EnsureStarted();

		lock ( Gate )
		{
			if ( _fatal is not null )
				return Task.FromException( _fatal );

			return _startupTask ?? Task.CompletedTask;
		}
	}

	public static bool IsReady =>
		GlobalCatalogs.Symbols.IsReady &&
		GlobalCatalogs.Cards.IsReady;

	// -------------------------
	// Hooks
	// -------------------------

	private void OnSceneLoaded()
	{
		EnsureStarted();
	}

	private void GateGameplayStartUpdate()
	{
		if ( _releasedGameplay ) return;

		if ( _fatal is not null )
		{
			Log.Error( $"Static data failed: {_fatal}" );
			_releasedGameplay = true; // choose whether to hard-stop gameplay instead
			return;
		}

		if ( !IsReady )
			return;

		_releasedGameplay = true;
		OnStaticDataReadyForScene();
	}

	private void OnStaticDataReadyForScene()
	{
		Log.Info( "Static data ready: releasing gameplay." );
	}

	// -------------------------
	// Startup pipeline (static-owned)
	// -------------------------

	private static void EnsureStarted()
	{
		lock ( Gate )
		{
			if ( _startupTask is not null || _fatal is not null )
				return;

			_startupTask = RunStartupAsync();
		}
	}

	private static async Task RunStartupAsync()
	{
		try
		{
			Log.Info( "Static data startup begin" );

			// Fresh run semantics (optional)
			GlobalCatalogs.Symbols.Clear();
			GlobalCatalogs.Cards.Clear();

			var cache = new DefaultCacheService( ScryfallClient.Instance );

			// 1) Ensure bulk files exist
			await new ScryfallBulkSyncJob( cache ).RunAsync( progress: null, token: default );

			// 2) Ensure symbology exists
			await new ScryfallSymbologySyncJob( cache ).RunAsync( default );

			// 3) Build + publish symbols
			var symbols = await new BuildSymbolsCatalogJob( cache ).RunAsync( progress: null, token: default );
			GlobalCatalogs.Symbols.Publish( symbols.BySymbol );

			// 4) Build + publish cards
			var cards = await new BuildCardsCatalogJob( cache ).RunAsync( progress: null, token: default );
			GlobalCatalogs.Cards.Publish( cards.ById, cards.ByOracleId, cards.ByExactName );

			Log.Info( "Static data startup complete" );
		}
		catch ( Exception ex )
		{
			lock ( Gate ) _fatal = ex;

			Log.Error( $"Static data startup FAILED: {ex}" );

			GlobalCatalogs.Symbols.Clear();
			GlobalCatalogs.Cards.Clear();

			throw;
		}
	}

	private sealed class DefaultCacheService : CacheService
	{
		public DefaultCacheService( ScryfallClient api ) : base( api ) { }
	}
}
