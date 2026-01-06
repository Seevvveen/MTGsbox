#nullable enable

using System.Threading.Tasks;
using Sandbox.Diagnostics;
using Sandbox.Engine;
using Sandbox.Engine.Jobs;
using Sandbox.Scryfall;

namespace Sandbox.Engine;

/// <summary>
/// Orchestrates the order of jobs to ensure a local cache of game information
/// </summary>
public sealed class ApplicationStartupSystem : GameObjectSystem<ApplicationStartupSystem>, ISceneStartup
{
	private readonly CacheService _cache = new( ScryfallClient.Instance );
	private readonly Logger _logger = new( "GameStartupSystem" );
	
	
	// expose “wait until ready”
	public Task? StartupTask;

	public ApplicationStartupSystem( Scene scene ) : base( scene ) { }

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
			// Bulk Step
			var bulkJob = new ScryfallBulkSyncJob( _cache );
			await bulkJob.RunAsync( cts.Token );
			_logger.Info( "Bulk Data Ready" );

			// Symbols Step
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
