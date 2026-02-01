#nullable enable
using Sandbox.Probe;
namespace Sandbox.Players.Probe;



/// <summary>
/// Players Movement
/// </summary>
[Title( "Probe Movement")]
public sealed class ProbeMovement : Component
{
	public static ProbeMovement? Local { get; private set; }

	[Property, RequireComponent, ReadOnly] public required Rigidbody RigidBody { get; set; }

	/// <summary>
	/// Cached camera reference (should be a child of this movement object).
	/// </summary>
	[ReadOnly, Property] public ProbeCamera? Camera { get; set; }

	[Property,Group("Tuning")] private float Speed { get; set; } = 400f;
	[Property,Group("Tuning")] private float AccelerationRate { get; set; } = 15f;
	[Property,Group("Tuning")] private float MaxVelocity { get; set; } = 1000f;
	
	private const float PerpFriction = 10f;
	
	private Vector3 _wishDir;

	private MoveSettings MoveCfg => new(
		Speed,
		AccelerationRate,
		MaxVelocity,
		PerpFriction
	);
	
	
	protected override void OnAwake()
	{
		base.OnAwake();
		RigidBody = GetComponent<Rigidbody>();

		// Cache local movement instance for other components that need to pair up lazily.
		if ( !IsProxy )
			Local = this;
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( IsProxy )
			return;

		// Cache camera once (it should live under the player/pawn root).
		// We can re-resolve later if it gets destroyed.
		Camera ??= GetComponentInChildren<ProbeCamera>( true );

		// If we don't already have a camera create a local-only camera under this pawn.
		if ( Camera is not { IsValid: true } )
		{
			var camGo = new GameObject( true, "ProbeCamera" );
			camGo.Parent = GameObject;
			camGo.WorldPosition = WorldPosition;
			camGo.WorldRotation = WorldRotation;

			camGo.Components.Create<CameraComponent>();
			Camera = camGo.Components.Create<ProbeCamera>();
		}

		// If the camera exists, make sure it's parented to this movement object.
		if ( Camera is { IsValid: true } && Camera.GameObject.Parent != GameObject )
		{
			Camera.GameObject.Parent = GameObject;
		}
	}
	
	protected override void OnFixedUpdate()
	{
		if ( IsProxy ) return;
		if ( RigidBody is not { IsValid: true } ) return;
		
		if ( Camera is not { IsValid: true } )
		{
			Camera = GetComponentInChildren<ProbeCamera>( true );
			if ( Camera is not { IsValid: true } )
				return;
		}
		
		_wishDir = ProbeMove.GetWishDirection( Camera.WorldRotation, Input.AnalogMove );
		RigidBody.ApplyMovement( _wishDir, Time.Delta, MoveCfg );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ( Local == this )
			Local = null;
	}
}
