#nullable enable

using Sandbox.Zones;

namespace Sandbox.Players.Probe;

public sealed class ProbeSelection : Component
{
	[Property, ReadOnly] public ProbeCamera? Camera { get; set; }
	private CameraComponent _cameraComponent => Camera.GetComponent<CameraComponent>();
	
	[Property, ReadOnly, Group("Debug")] private GameObject? Hovered { get; set; }
	[Property, ReadOnly, Group("Debug")] private GameObject? Held { get; set; }

	[Property] private bool DrawTraces = true;
	[Property] private float Distance = 1000f;

	// Updated each frame from traces; applied in FixedUpdate for physics-friendly motion.
	private Vector3 _holdTargetPos;
	private Rotation _holdTargetRot;
	private bool _hasHoldTarget;



	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		Camera ??= ProbeCamera.Local;
		if ( Camera is not { IsValid: true } ) return;
		
		_hasHoldTarget = false;

		if (Input.Down("attack2"))
		{
			Held = null;
			return;
		}
		var mRay = _cameraComponent.ScreenPixelToRay(Mouse.Position);
		
		var trace = Scene.Trace.Ray( mRay, Distance );

		// Ignore our own pawn hierarchy (prefer the player's root, not the camera node).
		if ( Camera.Network?.RootGameObject is { IsValid: true } root )
		{
			trace = trace.IgnoreGameObjectHierarchy( root );
		}
		var baseResult = trace.Run();

		if ( DrawTraces )
		{
			DebugOverlay.Trace( baseResult, Time.Delta, false );
		}
		
		Hovered = baseResult.GameObject;

		if ( Hovered is not null && Input.Pressed( "attack1" ) )
		{
			if (Hovered.Tags.Contains("donttouchme")) return;
			if (Held is null)
			{
				var z = Hovered.GetComponent<Zone>();
				if (z is not null)
				{
					var c = z.GetCard();
					Held = c;
					return;
				}
			}
			Held = Hovered;
		}
		
		// While holding, trace "through" the held object so we can position it on surfaces behind it.
		var throughResult = trace.IgnoreGameObject( Held ).Run();
		
		if ( Held is not null && Input.Down( "attack1" ) )
		{
			if ( throughResult.Hit )
			{
				_holdTargetRot = throughResult.Normal.EulerAngles.ToRotation();
				_holdTargetPos = throughResult.HitPosition + throughResult.Normal;
				_hasHoldTarget = true;
			}
		}

		if ( Held is not null && Input.Released( "attack1" ) )
		{
			if (throughResult.Hit && throughResult.GameObject.Tags.Has("zone"))
			{
				var zone = throughResult.GameObject.GetComponent<Zone>();
				zone.AddCard(Held);
			}
			else
			{
				Held.WorldPosition = throughResult.HitPosition+throughResult.Normal.SnapToGrid(10,true,true,false);
			}
			
			
			Held = null;
			_hasHoldTarget = false;
		}
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy ) return;
		if ( Held is not { IsValid: true } ) return;
		if ( !_hasHoldTarget ) return;

		// If it has a Rigidbody, move it in physics time. Otherwise, fall back to transform move.
		var body = Held.GetComponent<Rigidbody>();
		if ( body is { IsValid: true } )
		{
			// Critically damped-ish "pull" by setting velocity toward target.
			var toTarget = _holdTargetPos - body.WorldPosition;
			var dt = Time.Delta;
			if ( dt > 0f )
			{
				body.AngularVelocity = Vector3.Zero;
				body.Velocity = toTarget / dt;
				body.WorldRotation = _holdTargetRot;
			}
		}
		else
		{
			Held.WorldRotation = _holdTargetRot;
			Held.WorldPosition = _holdTargetPos;
		}
	}	
	
}
