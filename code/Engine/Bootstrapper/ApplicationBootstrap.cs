#nullable enable
using System.Threading.Tasks;
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

	public static Task EnsureStartedAsync()
	{
		lock ( _lock )
		{
			_startupTask ??= RunStartupAsync();
			return _startupTask;
		}
	}

	private static async Task RunStartupAsync()
	{
		using var cts = TaskSource.CreateLinkedTokenSource();
		var token = cts.Token;

		try
		{
			_logger.Info( "Startup: begin" );

			var bulkJob = new ScryfallBulkSyncJob( _cache );
			await bulkJob.RunAsync( token );
			_logger.Info( "Startup: bulk ready" );

			var symbolsJob = new ScryfallSymbologySyncJob( _cache );
			await symbolsJob.RunAsync( token );
			_logger.Info( "Startup: symbology ready" );

			_logger.Info( "Startup: complete" );
		}
		catch ( System.Exception e )
		{
			_logger.Error( $"Startup: failed: {e}" );
			throw;
		}
	}
}
