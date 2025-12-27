/// <summary>
/// Controller - Detect Inputs and Translate to Commands passed to Camera Logic
/// </summary>
public sealed class StrategyCameraController : Component
{
	//Implement Camera
	[Property, RequireComponent] public StrategyCamera2 Camera { get; set; }

	// Input configuration
	[Property, Group( "Input" )] public float LookSensitivity { get; set; } = 150f;
	[Property, Group( "Input" )] public float PanSpeed { get; set; } = 125f;
	[Property, Group( "Input" )] public bool ScalePanByDistance { get; set; } = true;
	[Property, Group( "Input" )] public float PanDistanceReference { get; set; } = 150f;
	[Property, Group( "Input" )] public float ZoomStep { get; set; } = 0.15f;
	[Property, Group( "Input" )] public bool EnablePanMomentum { get; set; } = false;
	[Property, Group( "Input" ), ShowIf( nameof( EnablePanMomentum ), true )]
	public float PanDrag { get; set; } = 0.9f;

	private Vector3 _panVelocity;
	private bool _wasRotating;

	protected override void OnUpdate()
	{
		if ( Camera == null ) return;

		bool isRotating = Input.Down( "attack2" );

		// Toggle mouse visibility
		if ( isRotating != _wasRotating )
		{
			Mouse.Visibility = isRotating ? MouseVisibility.Hidden : MouseVisibility.Visible;
			_wasRotating = isRotating;
		}

		HandlePanInput();
		HandleZoomInput();
		HandleRotationInput( isRotating );
	}

	private void HandlePanInput()
	{
		var input = Input.AnalogMove;
		if ( !input.IsNearZeroLength || EnablePanMomentum )
		{
			var moveDir = GetWorldMoveDirection( input );
			float scale = ScalePanByDistance && PanDistanceReference > 0f
				? Camera.CameraDistance / PanDistanceReference
				: 1f;
			float panAmount = PanSpeed * scale * Time.Delta;

			if ( EnablePanMomentum )
			{
				if ( !moveDir.IsNearZeroLength )
					_panVelocity += moveDir * panAmount;

				Camera.Pan( _panVelocity.Normal, _panVelocity.Length * Time.Delta );
				_panVelocity *= MathF.Pow( PanDrag, Time.Delta * 60f );

				if ( _panVelocity.Length < 0.1f )
					_panVelocity = Vector3.Zero;
			}
			else if ( !moveDir.IsNearZeroLength )
			{
				Camera.Pan( moveDir, panAmount );
			}
		}
	}

	private void HandleZoomInput()
	{
		float scroll = Input.MouseWheel.y;
		if ( scroll != 0f )
		{
			float zoomFactor = MathF.Pow( 1f + ZoomStep, -scroll );
			Camera.Zoom( zoomFactor );
		}
	}

	private void HandleRotationInput( bool isRotating )
	{
		if ( isRotating )
		{
			var lookDelta = Input.AnalogLook * (Time.Delta * LookSensitivity);
			Camera.Rotate( new Angles( lookDelta.pitch, lookDelta.yaw, 0f ) );
		}
	}

	private Vector3 GetWorldMoveDirection( Vector3 input )
	{
		if ( input.IsNearZeroLength ) return Vector3.Zero;
		var worldMove = (Rotation.FromYaw( Camera.ViewRotation.Yaw() ) * input).WithZ( 0f );
		return worldMove.IsNearZeroLength ? Vector3.Zero : worldMove.Normal;
	}
}
