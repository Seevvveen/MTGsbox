#nullable enable
using System;
using System.Threading.Tasks;
using Sandbox.Diagnostics;

namespace Sandbox.Engine.Bootstrapper;

public sealed class ApplicationStartupSystem( Scene scene )
	: GameObjectSystem<ApplicationStartupSystem>( scene ), ISceneStartup
{
	private readonly Logger _logger = new( "ApplicationStartupSystem" );

	void ISceneStartup.OnHostPreInitialize( SceneFile scene )
	{
		_logger.Info( "OnHostPreInitialize Called" );
		_ = TryStartAsync();
	}

	private async Task TryStartAsync()
	{
		try
		{
			await ApplicationBootstrap.EnsureStartedAsync();
			_logger.Info( "Startup: ready" );
		}
		catch ( Exception e )
		{
			_logger.Error( $"Startup: failed (will retry next time EnsureStartedAsync is called): {e}" );
		}
	}

	/// Optional: other scene code can await readiness via this system.
	public Task WhenReady => ApplicationBootstrap.EnsureStartedAsync();
}
