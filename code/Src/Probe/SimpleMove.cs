using System.Diagnostics;

namespace Sandbox;

public class SimpleMove : Component
{
	private CameraComponent Camera {get; set;}

	[Property] private bool DrawDebug { get; set; } = false; 
	[Property] private bool TraceThrough { get; set; } = false; 
	
	protected override void OnStart()
	{
		Camera = Scene.Camera;
		Mouse.Visibility = MouseVisibility.Visible;
	}

	protected override void OnUpdate()
	{
		var ray = Camera.ScreenPixelToRay( Mouse.Position );
		var traceCard = Scene.Trace.Ray( ray, 500 ).WithTag( "card" );
		var traceThroughCard = Scene.Trace.Ray( ray, 300 ).WithoutTags( "card" );
		
		if ( Input.Down( "attack1" ) )
		{
			SceneTraceResult cardResult = traceCard.Run();
			SceneTraceResult throughCardResult;
			
			if ( TraceThrough )
				throughCardResult = traceThroughCard.Run();
			else
				throughCardResult = cardResult;

			if ( DrawDebug )
			{
				DebugOverlay.Trace( cardResult );
				DebugOverlay.Trace( throughCardResult );
			}
				
			
			if ( cardResult.Hit )
			{
				if ( !TraceThrough )
				{
					cardResult.GameObject.WorldPosition = cardResult.HitPosition;
				}
				else
				{
					cardResult.GameObject.WorldPosition = throughCardResult.HitPosition;
				}
			}
			
		}
		



		
		
		
	}
}
