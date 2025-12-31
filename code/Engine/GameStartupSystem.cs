#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Diagnostics;
using Sandbox.Scryfall;

namespace Sandbox.Engine;

public sealed class GameStartupSystem : GameObjectSystem<GameStartupSystem>, ISceneStartup
{
	private readonly CacheService _cache = new( ScryfallClient.Instance );
	private readonly Logger _logger = new( "GameStartupSystem" );

	// Persist for the whole game runtime
	public CardCatalog Catalog { get; } = new();

	// Optional: expose a task for “wait until ready”
	private Task? _startupTask;

	public GameStartupSystem( Scene scene ) : base( scene )
	{
		// Don’t do work here; let startup hook trigger it.
	}

	void ISceneStartup.OnHostPreInitialize( SceneFile scene )
	{
		_logger.Info( "GameStartupSystem OnHostPreInitialize called" );

		// Kick off startup once
		_startupTask ??= RunStartupAsync();
	}

	private async Task RunStartupAsync()
	{
		using var cts = TaskSource.CreateLinkedTokenSource();
		var ts = TaskSource.Create( cts.Token );

		try
		{
			// 1) Ensure bulk files exist and are valid
			var bulkJob = new ScryfallBulkSyncJob( _cache );
			await bulkJob.RunAsync( cts.Token );
			_logger.Info( "Bulk Data Ready" );

			// 2) Build indexes from known-good file
			var indexJob = new CardIndexBuildJob( ts, "default_cards.json" );
			var result = await indexJob.RunAsync( cts.Token );

			// 3) Publish atomically to long-lived catalog
			Catalog.Set( result );

			_logger.Info( "Card catalog ready" );
		}
		catch ( OperationCanceledException )
		{
			_logger.Warning( "Startup cancelled." );
			throw;
		}
		catch ( Exception e )
		{
			_logger.Error( $"Startup failed: {e}" );
			// Decide: rethrow (hard fail) or swallow and keep game running without cards.
			// I’d usually rethrow in dev builds.
		}
	}
}
