#nullable enable


using System.Threading.Tasks;
using Sandbox.Scryfall;
using Sandbox.Diagnostics;

namespace Sandbox.Engine;

/// <summary>
/// 
/// </summary>
public sealed class GameStartupSystem : GameObjectSystem<GameStartupSystem>, ISceneStartup
{

	private readonly CacheService _cache = new(ScryfallClient.Instance);
	private readonly Logger _logger = new Logger("GameStartupSystem");
	
	
	public GameStartupSystem( Scene scene ) : base( scene )
	{
		_ = InitializeBulkAsync();
	}

	private async Task InitializeBulkAsync()
	{
		using var cts = TaskSource.CreateLinkedTokenSource();

		try
		{
			var job = new ScryfallBulkSyncJob( _cache );
			await job.RunAsync( cts.Token );
			_logger.Info( "Bulk Data Ready" );
		}
		catch ( Exception e )
		{
			_logger.Info( $"Bulk Sync Job Failed: {e}" );
		}
	}
	
	// Refactor When Multiplayer
	// Main System
	void ISceneStartup.OnHostPreInitialize( SceneFile scene )
	{

		
	}
}
