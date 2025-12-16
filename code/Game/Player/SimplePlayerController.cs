[Title( "SimplePlayerController" )]
public sealed class SimplePlayerController : Component
{
	[RequireComponent, Hide] public Rigidbody Body { get; set; }
	[RequireComponent, Hide] public SphereCollider SphereCollider { get; set; }
	[RequireComponent, Hide] public FirstPersonCamera LookCamera { get; set; }
	[RequireComponent, Hide] public PlayerInput InputSource { get; set; }

	[Property] public float Speed { get; set; } = 400f;
	[Property] public float AccelerationRate { get; set; } = 15f;
	[Property] public float MaxVelocity { get; set; } = 1000f;
	[Property] public float PerpendicularFriction { get; set; } = 10f;

	protected override void OnStart()
	{
		Body ??= GetComponent<Rigidbody>();

		if ( Body == null )
		{
			Log.Error( "[FreeflyNoclipController] Missing Rigidbody." );
			Enabled = false;
			return;
		}

		LookCamera ??= GameObject.GetComponentInChildren<FirstPersonCamera>();
		InputSource ??= GameObject.GetComponent<PlayerInput>();

		WorldRotation = Rotation.Identity;
		Body.Gravity = false;
	}

	protected override void OnFixedUpdate()
	{
		Body.AngularVelocity = 0f;

		// Fallback: if we don't have a LookCamera, just use our own rotation.
		var orientation = LookCamera != null
			? LookCamera.ViewRotation
			: WorldRotation;

		var moveInput = InputSource != null
			? InputSource.Move
			: Input.AnalogMove;

		var wishDir = QuakeMove.CreateWishDirection( orientation, moveInput );

		var newVelocity = QuakeMove.ApplyAccelerationAndFriction(
			Body.Velocity,
			wishDir,
			Speed,
			AccelerationRate,
			PerpendicularFriction,
			Time.Delta
		);

		newVelocity = QuakeMove.ClampSpeed( newVelocity, MaxVelocity );

		Body.Velocity = newVelocity;
	}

	protected override void OnDestroy()
	{
		Body?.Destroy();
		SphereCollider?.Destroy();
		LookCamera?.Destroy();
		InputSource?.Destroy();
	}
}
