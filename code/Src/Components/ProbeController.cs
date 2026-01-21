#nullable enable

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

	private float _pitch;
	private Angles _angles;
	private Vector3 _wishDir;

	protected override void OnStart()
	{
		if (IsProxy)
		{
			Camera?.Enabled = false;
			return;
		}
		
		_angles = WorldRotation.Angles();
		_pitch = _angles.pitch;

		if ( RigidBody is not null )
			RigidBody.Gravity = false;
	}

	protected override void OnUpdate()
	{
		if (IsProxy)  return;
		
		// LOOK (frame-rate)
		var mouseX = Input.AnalogLook.yaw * Time.Delta * SensX;
		var mouseY = Input.AnalogLook.pitch * Time.Delta * SensY;

		_angles.yaw += mouseX;
		_pitch = (_pitch + mouseY).Clamp( -90f, 90f );
		_angles.pitch = _pitch;

		var rot = Rotation.From( _angles );

		// Apply rotation to the thing that represents facing direction
		if ( MovementObject is not null )
			MovementObject.WorldRotation = rot;
		else
			WorldRotation = rot;

		// If you want camera separate from body, you can also set camera local rotation here.
		// For a simple setup, keeping them the same is fine.
		if ( Camera is not null )
			Camera.WorldRotation = rot;
	}

	protected override void OnFixedUpdate()
	{
		if (IsProxy)  return;
		
		if ( RigidBody is null )
			return;

		RigidBody.AngularVelocity = 0;

		ProcessMoveInput();
		ApplyMovement();
		ClampVelocity();
	}

	private void ProcessMoveInput()
	{
		if (IsProxy)  return;
		
		// MOVE (physics tick)
		var input = Input.AnalogMove;

		// Use the rotation that represents facing direction
		var basisRot =
			MovementObject?.WorldRotation ?? (Camera?.WorldRotation ?? WorldRotation);

		// Typical convention: input.y = forward, input.x = right, input.z = up (noclip ascend/descend)
		_wishDir =
			(basisRot.Forward * input.x) +
			(basisRot.Right * -input.y) +
			(basisRot.Up * input.z);

		var len = _wishDir.Length;
		_wishDir = (len > 0.001f) ? (_wishDir / len) : Vector3.Zero;
	}

	private void ApplyMovement()
	{
		if (IsProxy)  return;
		
		if ( _wishDir.IsNearlyZero() )
		{
			// Decelerate to stop
			var currentSpeed = RigidBody!.Velocity.Length;
			if ( currentSpeed < 0.1f )
			{
				RigidBody.Velocity = Vector3.Zero;
				return;
			}

			var decel = AccelerationRate * currentSpeed * Time.Delta;
			var newSpeed = MathF.Max( 0f, currentSpeed - decel );
			RigidBody.Velocity *= (newSpeed / currentSpeed);
			return;
		}

		// Reduce sideways slide (optional)
		const float perpFriction = 10f;
		var vel = RigidBody!.Velocity;

		var velInWish = _wishDir * Vector3.Dot( vel, _wishDir );
		var velPerp = vel - velInWish;

		velPerp *= MathF.Max( 0f, 1f - perpFriction * Time.Delta );
		vel = velInWish + velPerp;

		// Accelerate toward target speed in wish direction
		var currentInWish = Vector3.Dot( vel, _wishDir );
		var addSpeed = Speed - currentInWish;
		if ( addSpeed <= 0f )
		{
			RigidBody.Velocity = vel;
			return;
		}

		var accel = AccelerationRate * Speed * Time.Delta;
		if ( accel > addSpeed ) accel = addSpeed;

		vel += _wishDir * accel;
		RigidBody.Velocity = vel;
	}

	private void ClampVelocity()
	{
		if (IsProxy)  return;
		
		var speed = RigidBody!.Velocity.Length;
		if ( speed > MaxVelocity )
			RigidBody.Velocity *= (MaxVelocity / speed);
	}
}
