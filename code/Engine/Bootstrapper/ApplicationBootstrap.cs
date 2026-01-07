#nullable enable
using System.Threading.Tasks;
using Sandbox.Catalog;
using Sandbox.Catalog.Builders;
using Sandbox.Diagnostics;
using Sandbox.Engine.Jobs;
using Sandbox.Scryfall;

namespace Sandbox.Engine.Bootstrapper;

public static class ApplicationBootstrap
{
	private static readonly object _lock = new();
	private static Task? _startupTask;

	private static readonly Logger _logger = new( "ApplicationBootstrap" );
	private static readonly CacheService _cache = new( ScryfallClient.Instance );

	public static GameCatalogs Catalogs { get; } = new();

	public static Task EnsureStartedAsync()
	{
		lock ( _lock )
		{
			_startupTask ??= RunStartupWithResetOnFailureAsync();
			return _startupTask;
		}
	}

	private static async Task RunStartupWithResetOnFailureAsync()
	{
		try
		{
			await RunStartupOnceAsync();
		}
		catch ( System.Exception e )
		{
			_logger.Error( $"Startup: failed: {e}" );

			// Ensure we don't leave partial publish state
			Catalogs.Clear();

			// Allow a manual re-call of EnsureStartedAsync() to try again
			lock ( _lock )
			{
				_startupTask = null;
			}

			throw;
		}
	}

	private static async Task RunStartupOnceAsync()
	{
		using var cts = TaskSource.CreateLinkedTokenSource();
		var token = cts.Token;

		_logger.Info( "Startup: begin" );

		await new ScryfallBulkSyncJob( _cache ).RunAsync( token );
		_logger.Info( "Startup: bulk ready" );

		var symbolsJob = new ScryfallSymbologySyncJob( _cache );
		await symbolsJob.RunAsync( token );
		_logger.Info( "Startup: symbology ready" );

		// Build first, publish after build succeeds (reduces partial state risk)
		var symbolsBySymbol = await new BuildSymbolsCatalogJob( _cache ).RunAsync( token );
		var cardsResult = await new BuildCardsCatalogJob( _cache ).RunAsync( token );

		Catalogs.Symbols.Publish( symbolsBySymbol );
		_logger.Info( $"Startup: symbols ready ({Catalogs.Symbols.Count})" );

		Catalogs.Cards.Publish( cardsResult.ById, cardsResult.ByOracleId, cardsResult.ByExactName );
		_logger.Info( $"Startup: cards ready ({Catalogs.Cards.Count})" );

		_logger.Info( "Startup: complete" );
	}
}
