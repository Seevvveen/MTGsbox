[Group( "SevensStuff" )]
[Title( "Noclip Movement Controller" )]

public sealed class NoclipMovementController : Component
{
	[Property] public Rigidbody RigidBody { get; set; }
	[Property] public GameObject MovementObject { get; set; }
	[Property] public CameraComponent Camera { get; set; }

	[Property] public float Speed { get; set; } = 400f;
	[Property] public float AccelerationRate { get; set; } = 15f;
	[Property] public float MaxVelocity { get; set; } = 1000f;

	private Vector3 WishDirection;

	protected override void OnStart()
	{
		WorldRotation = Rotation.Identity;
		RigidBody.Gravity = false;
	}

	protected override void OnFixedUpdate()
	{
		RigidBody.AngularVelocity = 0;

		ProcessInput();
		ApplyMovement();
		ClampVelocity();
	}

	void ProcessInput()
	{
		Input.ClearActions();
		var input = Input.AnalogMove;


		var camRotation = Camera.LocalRotation;

		WishDirection = (camRotation.Forward * input.x) +
						(camRotation.Right * -input.y) +
						(camRotation.Up * input.z);

		float length = WishDirection.Length;
		if ( length > 0.001f )
		{
			WishDirection /= length;
		}
		else
		{
			WishDirection = Vector3.Zero;
		}

	}

	void ApplyMovement()
	{
		// Not pressing anything - decelerate
		if ( WishDirection.IsNearlyZero() )
		{
			var currentSpeed = RigidBody.Velocity.Length;
			if ( currentSpeed < 0.1f )
			{
				RigidBody.Velocity = Vector3.Zero;
				return;
			}
			float decel = AccelerationRate * currentSpeed * Time.Delta;
			float newSpeed = MathF.Max( 0f, currentSpeed - decel );
			RigidBody.Velocity *= (newSpeed / currentSpeed);
			return;
		}

		// Apply friction to velocity perpendicular to input direction
		float perpFriction = 10f; // Adjust this value (higher = less sliding)
		var velocityInWishDir = WishDirection * Vector3.Dot( RigidBody.Velocity, WishDirection );
		var velocityPerpendicular = RigidBody.Velocity - velocityInWishDir;

		// Reduce perpendicular velocity
		velocityPerpendicular *= MathF.Max( 0f, 1f - perpFriction * Time.Delta );
		RigidBody.Velocity = velocityInWishDir + velocityPerpendicular;

		// Standard acceleration in wish direction
		float currentSpeedInWishDir = RigidBody.Velocity.Dot( WishDirection );
		float addSpeed = Speed - currentSpeedInWishDir;

		if ( addSpeed <= 0 )
			return;

		float accelAmount = AccelerationRate * Speed * Time.Delta;
		if ( accelAmount > addSpeed )
			accelAmount = addSpeed;

		RigidBody.Velocity += WishDirection * accelAmount;
	}

	void ClampVelocity()
	{
		float currentSpeed = RigidBody.Velocity.Length;
		if ( currentSpeed > MaxVelocity )
		{
			RigidBody.Velocity *= (MaxVelocity / currentSpeed);
		}
	}
}
