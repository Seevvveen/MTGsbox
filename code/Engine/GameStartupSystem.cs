#nullable enable

using System.Threading.Tasks;
using Sandbox.Diagnostics;
using Sandbox.Engine;
using Sandbox.Scryfall;

namespace Sandbox.Engine;

public sealed class GameStartupSystem : GameObjectSystem<GameStartupSystem>, ISceneStartup
{
	private readonly CacheService _cache = new( ScryfallClient.Instance );
	private readonly Logger _logger = new( "GameStartupSystem" );

	// Persist for the whole game runtime
	public CardCatalog Catalog { get; } = new();

	// expose “wait until ready”
	public Task? StartupTask;

	public GameStartupSystem( Scene scene ) : base( scene ) { }

	void ISceneStartup.OnHostPreInitialize( SceneFile scene )
	{
		_logger.Info( "Started" );

		// Kick off startup once
		StartupTask ??= RunStartupAsync();
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

			// Symbols
			var symbolsJob = new ScryfallSymbologySyncJob( _cache );
			await symbolsJob.RunAsync( cts.Token );
			
			
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
			throw;
		}
	}
}
