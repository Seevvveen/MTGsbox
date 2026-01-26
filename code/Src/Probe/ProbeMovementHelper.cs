namespace Sandbox.Probe;

public readonly record struct MoveSettings(
	float Speed,
	float AccelerationRate,
	float MaxVelocity,
	float PerpFriction
);

public readonly record struct LookSettings(
	float SensX,
	float SensY,
	float MinPitch,
	float MaxPitch
);

public static class ProbeLook
{
	/// <summary>
	/// Applies analog look input to angles + pitch cache and returns a rotation.
	/// State lives in the caller (angles, pitch).
	/// </summary>
	public static (Angles angles, float pitch, Rotation rotation)
		ApplyAnalogLook( Angles angles, float pitch, in LookSettings s, float dt )
	{
		var mouseX = Input.AnalogLook.yaw   * dt * s.SensX;
		var mouseY = Input.AnalogLook.pitch * dt * s.SensY;

		angles.yaw += mouseX;

		pitch = (pitch + mouseY).Clamp( s.MinPitch, s.MaxPitch );
		angles.pitch = pitch;

		return (angles, pitch, Rotation.From( angles ));
	}
}

public static class ProbeMove
{
	/// <summary>
	/// Builds a normalized wish direction from an orientation basis and analog move input.
	/// </summary>
	public static Vector3 GetWishDir( Rotation basis, Vector3 input )
	{
		var wish =
			(basis.Forward * input.x)  +
			(basis.Right   * -input.y) +
			(basis.Up      * input.z);

		var len = wish.Length;
		return (len > 0.001f) ? (wish / len) : Vector3.Zero;
	}

	/// <summary>
	/// Pure velocity integration: decel if no input, otherwise apply perp friction + accel toward target speed.
	/// </summary>
	public static Vector3 IntegrateVelocity( Vector3 velocity, Vector3 wishDir, float dt, in MoveSettings s )
	{
		if ( dt <= 0f ) return velocity;

		if ( wishDir.IsNearlyZero() )
			return DecelerateToStop( velocity, dt, s.AccelerationRate );

		// reduce sideways slide
		var velInWish = wishDir * Vector3.Dot( velocity, wishDir );
		var velPerp   = velocity - velInWish;

		velPerp *= MathF.Max( 0f, 1f - s.PerpFriction * dt );
		velocity = velInWish + velPerp;

		// accelerate toward desired speed along wish dir
		var currentInWish = Vector3.Dot( velocity, wishDir );
		var addSpeed = s.Speed - currentInWish;
		if ( addSpeed <= 0f ) return Clamp( velocity, s.MaxVelocity );

		var accel = s.AccelerationRate * s.Speed * dt;
		if ( accel > addSpeed ) accel = addSpeed;

		velocity += wishDir * accel;

		return Clamp( velocity, s.MaxVelocity );
	}

	public static Vector3 DecelerateToStop( Vector3 velocity, float dt, float accelRate )
	{
		var speed = velocity.Length;
		if ( speed < 0.1f ) return Vector3.Zero;

		var decel = accelRate * speed * dt;
		var newSpeed = MathF.Max( 0f, speed - decel );

		return velocity * (newSpeed / speed);
	}

	public static Vector3 Clamp( Vector3 velocity, float maxSpeed )
	{
		if ( maxSpeed <= 0f ) return Vector3.Zero;

		var speed = velocity.Length;
		if ( speed <= maxSpeed ) return velocity;

		return velocity * (maxSpeed / speed);
	}
}

public static class ProbeRigidbody
{
	public static void ApplyMovement( this Rigidbody body, Vector3 wishDir, float dt, in MoveSettings s, bool zeroAngular = true )
	{
		if ( zeroAngular ) body.AngularVelocity = 0;

		var vel = body.Velocity;
		vel = ProbeMove.IntegrateVelocity( vel, wishDir, dt, s );
		body.Velocity = vel;
	}
}