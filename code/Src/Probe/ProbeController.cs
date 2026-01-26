#nullable enable

using Sandbox.Probe;

namespace Sandbox.Components;

[Title( "Probe Controller" )]
public sealed class ProbeController : Component
{
	[Property] public Rigidbody? RigidBody { get; set; }

	// The object we rotate for facing (often the player root, or a camera pivot)
	[Property] public GameObject? MovementObject { get; set; }
	[Property] public CameraComponent? Camera { get; set; }

	[Property] private float Speed { get; set; } = 400f;
	[Property] private float AccelerationRate { get; set; } = 15f;
	[Property] private float MaxVelocity { get; set; } = 1000f;

	[Property] private float SensX { get; set; } = 100f;
	[Property] private float SensY { get; set; } = 100f;

	// You can expose this as a [Property] if you want it configurable.
	private const float PerpFriction = 10f;

	private float _pitch;
	private Angles _angles;
	private Vector3 _wishDir;

	private MoveSettings MoveCfg => new(
		Speed,
		AccelerationRate,
		MaxVelocity,
		PerpFriction
	);

	private LookSettings LookCfg => new(
		SensX,
		SensY,
		MinPitch: -90f,
		MaxPitch:  90f
	);

	protected override void OnStart()
	{
		if ( IsProxy )
		{
			Camera?.Enabled = false;
			return;
		}

		var lookSource = MovementObject ?? GameObject;

		_angles = lookSource.WorldRotation.Angles();
		_pitch = _angles.pitch;

		if ( RigidBody is not null )
			RigidBody.Gravity = false;
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		var lookSource = MovementObject ?? GameObject;

		(_angles, _pitch, var rot) = ProbeLook.ApplyAnalogLook( _angles, _pitch, LookCfg, Time.Delta );
		lookSource.WorldRotation = rot;
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy ) return;
		if ( RigidBody is null ) return;

		ProcessMoveInput();

		RigidBody.ApplyMovement(_wishDir, Time.Delta, MoveCfg, zeroAngular: true );
	}

	private void ProcessMoveInput()
	{
		var basis = (MovementObject ?? GameObject).WorldRotation;
		_wishDir = ProbeMove.GetWishDir( basis, Input.AnalogMove );
	}
}
