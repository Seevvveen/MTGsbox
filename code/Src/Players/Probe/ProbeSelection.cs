#nullable enable

using Sandbox.Zones;

namespace Sandbox.Players.Probe;

public sealed class ProbeSelection : Component
{
	[Property, ReadOnly] public ProbeCamera? Camera { get; set; }
	[Property, ReadOnly] private bool HasCamera {get; set;} = false;
	
	
	[Property, ReadOnly, Group("Debug")] private GameObject? Hovered { get; set; }
	[Property, ReadOnly, Group("Debug")] private GameObject? Held { get; set; }
	
	[Property] private bool DrawTraces = true; 
	[Property] private float Distance = 1000f;
	private Ray Ray => new Ray(WorldPosition,Camera.WorldRotation.Forward);
	private SceneTrace TraceBase => Scene.Trace.Ray( Ray, Distance ).IgnoreGameObjectHierarchy(Camera?.Network.RootGameObject);



	protected override void OnUpdate()
	{
		if ( IsProxy ) return;
		if (!HasCamera)
		{
			if ( Camera is not { IsValid: true } )
				Camera = ProbeCamera.Local;
			if ( Camera == null ) return;
			HasCamera = true;
		}

		var BaseResult = TraceBase.Run();
		
		//DEBUG
		if (DrawTraces)
		{
			DebugOverlay.Trace(BaseResult,Time.Delta,false);
		}

		Hovered = BaseResult.GameObject;

		if (Hovered is not null && Input.Pressed("attack1"))
		{
			Held = Hovered;
		}

		if (Held is not null && Input.Down("attack1"))
		{
			var ThroughResult = TraceBase.IgnoreGameObject(Held).Run();
			if (!ThroughResult.Hit) return;
			Held.WorldRotation = ThroughResult.Normal.EulerAngles.ToRotation();
			Held.WorldPosition = ThroughResult.HitPosition + Vector3.Up;
		}

		if (Held is not null && Input.Released("attack1"))
		{
			Held = null;
		}
		
		

	}
	
	
}
