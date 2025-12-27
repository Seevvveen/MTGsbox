/// <summary>
/// RTS/Strategy camera with pan, orbit (RMB), and exponential zoom around pivot point.
/// </summary>
public sealed class StrategyCamera : Component
{
	[RequireComponent, Hide]
	public CameraComponent Camera { get; set; }

	// Look & Rotation
	[Property, Group( "Look" )] public float LookSensitivity { get; set; } = 150f;
	[Property, Group( "Limits" )] public float MinPitch { get; set; } = 10f;
	[Property, Group( "Limits" )] public float MaxPitch { get; set; } = 89f;

	// Pan
	[Property, Group( "Pan" )] public float PanSpeed { get; set; } = 125f;
	[Property, Group( "Pan" )] public bool ScalePanByDistance { get; set; } = true;
	[Property, Group( "Pan" )] public float PanDistanceReference { get; set; } = 150f;
	[Property, Group( "Pan" )] public bool EnablePanMomentum { get; set; } = false;
	[Property, Group( "Pan" ), ShowIf( nameof( EnablePanMomentum ), true )]
	public float PanDrag { get; set; } = 0.9f;

	// Zoom
	[Property, Group( "Zoom" )] public float ZoomStep { get; set; } = 0.15f;
	[Property, Group( "Zoom" )] public float MinDistance { get; set; } = 100f;
	[Property, Group( "Zoom" )] public float MaxDistance { get; set; } = 3000f;

	// Smoothing
	[Property, Group( "Smoothing" )] public float PanSmoothTime { get; set; } = 0.05f;
	[Property, Group( "Smoothing" )] public float RotationSmoothTime { get; set; } = 0.05f;
	[Property, Group( "Smoothing" )] public float ZoomSmoothTime { get; set; } = 0.05f;

	// Public state
	public Rotation ViewRotation => Rotation.From( _viewAngles );
	public Rotation PivotRotation { get; private set; }
	public Vector3 PivotPosition
	{
		get => _pivotPosition;
		set => _pivotPosition = _targetPivotPosition = value;
	}

	// Internal state
	private Vector3 _pivotPosition, _targetPivotPosition;
	private Angles _viewAngles = new( 45, 0, 0 );
	private Angles _targetViewAngles = new( 45, 180, 0 );
	private float _cameraDistance = 350f, _targetDistance = 150f;
	private Vector3 _panVelocity;
	private bool _wasRotating;

	protected override void OnStart()
	{
		// Initialize pivot if not set
		if ( _pivotPosition == default )
			_pivotPosition = _targetPivotPosition = WorldPosition + WorldRotation.Forward * _cameraDistance;
	}

	protected override void OnUpdate()
	{
		bool isRotating = Input.Down( "attack2" );

		// Toggle mouse visibility when rotation state changes
		if ( isRotating != _wasRotating )
		{
			Mouse.Visibility = isRotating ? MouseVisibility.Hidden : MouseVisibility.Visible;
			_wasRotating = isRotating;
		}

		HandleInput( isRotating );
		ApplySmoothingAndTransform();
	}

	private void HandleInput( bool isRotating )
	{
		// Pan
		var input = Input.AnalogMove;
		if ( !input.IsNearZeroLength || EnablePanMomentum )
		{
			var moveDir = GetWorldMoveDirection( input );
			float scale = ScalePanByDistance && PanDistanceReference > 0f
				? _targetDistance / PanDistanceReference
				: 1f;
			float panAmount = PanSpeed * scale * Time.Delta;

			if ( EnablePanMomentum )
			{
				if ( !moveDir.IsNearZeroLength )
					_panVelocity += moveDir * panAmount;

				_targetPivotPosition += _panVelocity * Time.Delta;
				_panVelocity *= MathF.Pow( PanDrag, Time.Delta * 60f );

				if ( _panVelocity.Length < 0.1f )
					_panVelocity = Vector3.Zero;
			}
			else if ( !moveDir.IsNearZeroLength )
			{
				_targetPivotPosition += moveDir * panAmount;
			}
		}

		// Zoom
		float scroll = Input.MouseWheel.y;
		if ( scroll != 0f )
		{
			float zoomFactor = MathF.Pow( 1f + ZoomStep, -scroll );
			_targetDistance = Math.Clamp( _targetDistance * zoomFactor, MinDistance, MaxDistance );
		}

		// Rotate
		if ( isRotating )
		{
			var lookDelta = Input.AnalogLook * (Time.Delta * LookSensitivity);
			_targetViewAngles.pitch = Math.Clamp( _targetViewAngles.pitch + lookDelta.pitch, MinPitch, MaxPitch );
			_targetViewAngles.yaw += lookDelta.yaw;
			_targetViewAngles.roll = 0f;
		}
	}

	private Vector3 GetWorldMoveDirection( Vector3 input )
	{
		if ( input.IsNearZeroLength ) return Vector3.Zero;
		var worldMove = (Rotation.FromYaw( _targetViewAngles.yaw ) * input).WithZ( 0f );
		return worldMove.IsNearZeroLength ? Vector3.Zero : worldMove.Normal;
	}

	private void ApplySmoothingAndTransform()
	{
		// Smooth interpolation
		_pivotPosition = Vector3.Lerp( _pivotPosition, _targetPivotPosition, SmoothFactor( PanSmoothTime ) );
		_viewAngles = _viewAngles.LerpTo( _targetViewAngles, SmoothFactor( RotationSmoothTime ) );
		_viewAngles.roll = 0f;
		_cameraDistance = MathX.LerpTo( _cameraDistance, _targetDistance, SmoothFactor( ZoomSmoothTime ) );

		// Apply transform
		PivotRotation = ViewRotation;
		WorldPosition = _pivotPosition + PivotRotation.Backward * _cameraDistance;
		WorldRotation = PivotRotation;
	}

	private static float SmoothFactor( float smoothTime )
	{
		if ( smoothTime <= 0f ) return 1f;
		if ( Time.Delta <= 0f ) return 0f;
		return Math.Clamp( 1f - MathF.Exp( -Time.Delta / smoothTime ), 0f, 1f );
	}

	// Cursor utilities
	public SceneTraceResult TraceCursor( float maxDistance = 10000f )
	{
		var ray = Camera.ScreenPixelToRay( Mouse.Position );
		return Scene.Trace.Ray( ray, maxDistance ).Run();
	}

	public SceneTraceResult TraceCursor( float maxDistance, Action<SceneTrace> setupTrace )
	{
		var ray = Camera.ScreenPixelToRay( Mouse.Position );
		var trace = Scene.Trace.Ray( ray, maxDistance );
		setupTrace?.Invoke( trace );
		return trace.Run();
	}

	public Vector3? GetCursorWorldPosition( float maxDistance = 10000f )
	{
		var trace = TraceCursor( maxDistance );
		return trace.Hit ? trace.HitPosition : null;
	}
}
