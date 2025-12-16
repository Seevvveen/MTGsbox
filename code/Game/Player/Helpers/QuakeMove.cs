// Standard Calculations for quake like movement
public static class QuakeMove
{
	/// <summary>
	/// Build a camera/relative direction that were trying to move in
	/// </summary>
	/// <param name="basis">Input is relative to this rotation</param>
	/// <param name="input">Input containing desired direction, input.y = left</param>
	/// <returns>Wish Direction</returns>
	public static Vector3 CreateWishDirection( Rotation basis, Vector3 input )
	{
		//Relative Direction * Toggle to include it
		var wish = basis.Forward * input.x
				 + basis.Right * -input.y
				 + basis.Up * input.z;

		//Require minimum length, no input == zero
		var length = wish.Length;
		if ( length <= 0.001f )
			return Vector3.Zero;

		// Normalize
		return wish / length;
	}


	public static Vector3 ApplyAccelerationAndFriction(
		Vector3 currentVelocity,
		Vector3 wishDir,
		float speed,
		float accelRate,
		float perpFriction,
		float deltaTime )
	{
		// No input: decelerate smoothly
		if ( wishDir.IsNearlyZero() )
		{
			var currentSpeed = currentVelocity.Length;
			if ( currentSpeed < 0.1f )
				return Vector3.Zero;

			float decel = accelRate * currentSpeed * deltaTime;
			float newSpeed = MathF.Max( 0f, currentSpeed - decel );

			return currentVelocity * (newSpeed / currentSpeed);
		}

		// Apply friction to velocity perpendicular to input direction
		var velocityInWishDir = wishDir * Vector3.Dot( currentVelocity, wishDir );
		var velocityPerpendicular = currentVelocity - velocityInWishDir;

		velocityPerpendicular *= MathF.Max( 0f, 1f - perpFriction * deltaTime );
		currentVelocity = velocityInWishDir + velocityPerpendicular;

		// Standard acceleration in wish direction
		float currentSpeedInWishDir = currentVelocity.Dot( wishDir );
		float addSpeed = speed - currentSpeedInWishDir;

		if ( addSpeed <= 0f )
			return currentVelocity;

		float accelAmount = accelRate * speed * deltaTime;
		if ( accelAmount > addSpeed )
			accelAmount = addSpeed;

		return currentVelocity + wishDir * accelAmount;
	}

	public static Vector3 ClampSpeed( Vector3 velocity, float maxSpeed )
	{
		float currentSpeed = velocity.Length;
		if ( currentSpeed <= maxSpeed )
			return velocity;

		return velocity * (maxSpeed / currentSpeed);
	}
}
