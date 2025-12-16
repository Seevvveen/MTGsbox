/// <summary>
/// Generic "free look" camera: takes a look input vector each frame
/// and updates local rotation + exposes ViewRotation.
/// </summary>
public sealed class FirstPersonCamera : Component
{
	[RequireComponent, Hide] public CameraComponent Camera { get; set; }
	[Property] public GameObject CameraObject { get; set; }

	[Property] public float Sensitivity { get; set; } = 150f;
	[Property] public float MinPitch { get; set; } = -89f;
	[Property] public float MaxPitch { get; set; } = 89f;
	[Property] public bool InvertY { get; set; } = false;

	/// <summary>External look input provider (e.g., PlayerInput).</summary>
	[Property] public PlayerInput InputSource { get; set; }

	private Angles _viewAngles;

	public Rotation ViewRotation => Rotation.From( _viewAngles );

	protected override void OnStart()
	{
		Camera ??= GetComponent<CameraComponent>();

		if ( Camera == null )
		{
			Log.Error( "[FreeLookCamera] No CameraComponent found." );
			Enabled = false;
			return;
		}

		CameraObject ??= Camera.GameObject;
		_viewAngles = CameraObject.WorldRotation.Angles();

		Camera.FieldOfView = 90;
	}

	protected override void OnUpdate()
	{
		if ( InputSource == null )
		{
			// Fallback so this still "just works" if you drop it in.
			UpdateFromLookInput( Input.AnalogLook );
		}
		else
		{
			UpdateFromLookInput( InputSource.Look );
		}

		Camera.LocalRotation = ViewRotation;
	}

	private void UpdateFromLookInput( Angles lookInput )
	{
		var scaled = lookInput * (Time.Delta * Sensitivity);

		if ( InvertY )
			scaled.pitch = -scaled.pitch;

		_viewAngles.pitch += scaled.pitch;
		_viewAngles.pitch = MathX.Clamp( _viewAngles.pitch, MinPitch, MaxPitch );

		_viewAngles.yaw += scaled.yaw;
		_viewAngles.roll = 0f;
	}

	protected override void OnDestroy()
	{
		Camera.Destroy();
	}
}
