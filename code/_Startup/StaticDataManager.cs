namespace Sandbox._Startup;

/// <summary>
/// </summary>
public sealed class StaticDataManager : GameObjectSystem<StaticDataManager>, ISceneStartup
{
	public StaticDataManager( Scene scene ) : base( scene )
	{
		
	}

	
	
	
	
	
	// Before Scene Load
	void ISceneStartup.OnHostPreInitialize(SceneFile scene)
	{
		
	}
	
	// Scene Load Computer in Charge of the game - Not Called on connecting clients
	void ISceneStartup.OnHostInitialize()
	{
		
	}
	
	// Scene Load on Host(non-dedicated) and connecting clients
	void ISceneStartup.OnClientInitialize()
	{
		
	}

}
