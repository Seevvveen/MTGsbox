namespace Sandbox.Probe;

/// <summary>
/// Configuration for first-person movement physics.
/// </summary>
public readonly record struct MoveSettings(
    float Speed,
    float AccelerationRate,
    float MaxVelocity,
    float PerpFriction
);

/// <summary>
/// Configuration for first-person camera look controls.
/// </summary>
public readonly record struct LookSettings(
    float SensitivityX,
    float SensitivityY,
    float MinPitch,
    float MaxPitch
);

public static class ProbeLook
{
    /// <summary>
    /// Applies analog look input to update camera angles and pitch.
    /// </summary>
    /// <param name="angles">Current camera angles</param>
    /// <param name="pitch">Current pitch value (cached separately for clamping)</param>
    /// <param name="settings">Look sensitivity and constraints</param>
    /// <param name="deltaTime">Time step for framerate-independent input</param>
    /// <returns>Updated angles, pitch, and resulting rotation</returns>
    public static (Angles angles, float pitch, Rotation rotation) ApplyAnalogLook(
        Angles angles,
        float pitch,
        in LookSettings settings,
        float deltaTime)
    {
        var deltaYaw = Input.AnalogLook.yaw * deltaTime * settings.SensitivityX;
        var deltaPitch = Input.AnalogLook.pitch * deltaTime * settings.SensitivityY;

        angles.yaw += deltaYaw;
        pitch = (pitch + deltaPitch).Clamp(settings.MinPitch, settings.MaxPitch);
        angles.pitch = pitch;

        return (angles, pitch, Rotation.From(angles));
    }
}

public static class ProbeMove
{
    private const float MinVelocityThreshold = 0.001f;
    private const float StopThreshold = 0.1f;

    /// <summary>
    /// Converts input vector into a world-space movement direction.
    /// </summary>
    /// <param name="basis">Orientation to use as reference frame</param>
    /// <param name="input">Input vector (x=forward, y=right, z=up)</param>
    /// <returns>Normalized wish direction or zero if input is negligible</returns>
    public static Vector3 GetWishDirection(Rotation basis, Vector3 input)
    {
        var worldSpaceWish =
            (basis.Forward * input.x) +
            (basis.Right * -input.y) +
            (basis.Up * input.z);

        var length = worldSpaceWish.Length;
        return length > MinVelocityThreshold
            ? worldSpaceWish / length
            : Vector3.Zero;
    }

    /// <summary>
    /// Updates velocity based on wish direction and movement settings.
    /// Handles deceleration, perpendicular friction, and acceleration.
    /// </summary>
    public static Vector3 IntegrateVelocity(
        Vector3 velocity,
        Vector3 wishDirection,
        float deltaTime,
        in MoveSettings settings)
    {
        if (deltaTime <= 0f)
            return velocity;

        // No input - decelerate to stop
        if (wishDirection.IsNearlyZero())
            return DecelerateToStop(velocity, deltaTime, settings.AccelerationRate);

        // Apply perpendicular friction to reduce sliding
        velocity = ApplyPerpendicularFriction(velocity, wishDirection, deltaTime, settings.PerpFriction);

        // Accelerate toward target speed
        velocity = AccelerateAlongDirection(velocity, wishDirection, deltaTime, settings);

        return ClampVelocity(velocity, settings.MaxVelocity);
    }

    /// <summary>
    /// Gradually reduces velocity to zero.
    /// </summary>
    public static Vector3 DecelerateToStop(Vector3 velocity, float deltaTime, float accelerationRate)
    {
        var speed = velocity.Length;
        if (speed < StopThreshold)
            return Vector3.Zero;

        var decelerationAmount = accelerationRate * speed * deltaTime;
        var newSpeed = MathF.Max(0f, speed - decelerationAmount);

        return velocity * (newSpeed / speed);
    }

    /// <summary>
    /// Reduces velocity perpendicular to the wish direction to prevent sliding.
    /// </summary>
    private static Vector3 ApplyPerpendicularFriction(
        Vector3 velocity,
        Vector3 wishDirection,
        float deltaTime,
        float perpFriction)
    {
        var velocityAlongWish = wishDirection * Vector3.Dot(velocity, wishDirection);
        var velocityPerpendicular = velocity - velocityAlongWish;

        var frictionScale = MathF.Max(0f, 1f - perpFriction * deltaTime);
        velocityPerpendicular *= frictionScale;

        return velocityAlongWish + velocityPerpendicular;
    }

    /// <summary>
    /// Accelerates velocity toward the target speed along the wish direction.
    /// </summary>
    private static Vector3 AccelerateAlongDirection(
        Vector3 velocity,
        Vector3 wishDirection,
        float deltaTime,
        in MoveSettings settings)
    {
        var currentSpeedInDirection = Vector3.Dot(velocity, wishDirection);
        var speedDeficit = settings.Speed - currentSpeedInDirection;

        if (speedDeficit <= 0f)
            return velocity;

        var maxAcceleration = settings.AccelerationRate * settings.Speed * deltaTime;
        var actualAcceleration = MathF.Min(maxAcceleration, speedDeficit);

        return velocity + wishDirection * actualAcceleration;
    }

    /// <summary>
    /// Clamps velocity magnitude to maximum speed.
    /// </summary>
    private static Vector3 ClampVelocity(Vector3 velocity, float maxSpeed)
    {
        if (maxSpeed <= 0f)
            return Vector3.Zero;

        var speed = velocity.Length;
        if (speed <= maxSpeed)
            return velocity;

        return velocity * (maxSpeed / speed);
    }
}

public static class ProbeRigidbodyExtensions
{
    /// <summary>
    /// Applies movement physics to a rigidbody based on wish direction and settings.
    /// </summary>
    /// <param name="zeroAngularVelocity">Whether to reset angular velocity (prevents rotation)</param>
    public static void ApplyMovement(
        this Rigidbody body,
        Vector3 wishDirection,
        float deltaTime,
        in MoveSettings settings,
        bool zeroAngularVelocity = true)
    {
        if (zeroAngularVelocity)
            body.AngularVelocity = Vector3.Zero;

        body.Velocity = ProbeMove.IntegrateVelocity(
            body.Velocity,
            wishDirection,
            deltaTime,
            settings);
    }
}