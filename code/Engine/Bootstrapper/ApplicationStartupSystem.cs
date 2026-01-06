#nullable enable
using System.Threading.Tasks;
using Sandbox.Diagnostics;

namespace Sandbox.Engine.Bootstrapper;

public sealed class ApplicationStartupSystem : GameObjectSystem<ApplicationStartupSystem>, ISceneStartup
{
	private readonly Logger _logger = new( "ApplicationStartupSystem" );

	public ApplicationStartupSystem( Scene scene ) : base( scene ) { }

	void ISceneStartup.OnHostPreInitialize( SceneFile scene )
	{
		_logger.Info( $"OnHostPreInitialize Called" );

		// Safe to call every scene; only runs once per runtime instance.
		_ = ApplicationBootstrap.EnsureStartedAsync();
	}

	/// Optional: other scene code can await readiness via this system.
	public Task WhenReady => ApplicationBootstrap.EnsureStartedAsync();
}
