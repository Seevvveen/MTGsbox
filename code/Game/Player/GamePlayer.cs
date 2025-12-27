#nullable enable
/// <summary>
/// High-level player class that orchestrates camera and other game systems
/// </summary>
public sealed class GamePlayer : Component
{
	[Property, RequireComponent] public required StrategyCameraController CameraController { get; set; }
	public WorldCard? HoverCard { get; private set; } = null;
	public WorldCard? LastHoverCard { get; private set; } = null;

	protected override void OnStart()
	{
		CameraController.Camera?.SetPivotPosition( this.WorldPosition.WithZ( 0 ) );
	}

	protected override void OnFixedUpdate()
	{
		var r = CameraController.Camera.TraceCursor();
		DebugOverlay.Sphere( new Sphere( r.HitPosition, 10 ) );
		if ( r.Hit && r.GameObject.Components.TryGet<WorldCard>( out var worldCard ) )
		{
			HoverCard = worldCard;
		}
		else if ( HoverCard is not null )
		{
			LastHoverCard = HoverCard;
			HoverCard = null;
		}

		if ( Input.Down( "Attack1" ) & !Input.Down( "Attack2" ) )
		{
			HoverCard?.WorldPosition = CameraController.Camera.TraceCursor().HitPosition;
		}

		if ( Input.Down( "Reload" ) && HoverCard is not null )
		{
			FocusOnPosition( HoverCard.WorldPosition );
		}

	}

	// Prototype
	public void FocusOnPosition( Vector3 position )
	{
		CameraController.Camera?.SetPivotPosition( position.WithZ( 0 ) );
	}
}
