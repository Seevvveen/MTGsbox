namespace Sandbox;

public sealed class Spin : Component
{
	[Property] public float SlerpSpeed { get; set; } = 1.5f;
	[Property] public float TimeBetweenTargets { get; set; } = 2.0f;

	private Rotation _targetRotation;
	private TimeSince _timeSinceTargetChange;

	protected override void OnStart()
	{
		_targetRotation = Rotation.Random;
	}

	protected override void OnUpdate()
	{
		// Pick a new random rotation every few seconds
		if ( _timeSinceTargetChange > TimeBetweenTargets )
		{
			_targetRotation = Rotation.Random;
			_timeSinceTargetChange = 0;
		}

		// Smoothly slerp toward the target rotation
		WorldRotation = WorldRotation.SlerpTo(
			_targetRotation,
			Time.Delta * SlerpSpeed
		);
	}
}