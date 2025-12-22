/// <summary>
/// RTS / Strategy-style camera with pan, orbit (hold RMB), and exponential zoom.
/// Zoom always uses the pivot point, not the cursor.
/// </summary>
public sealed class StrategyCamera : Component
{
	#region Components & Properties

	[RequireComponent, Hide]
	public CameraComponent Camera { get; set; }

	#endregion

	#region Configuration

	[Property, Group( "Look" )]
	public float LookSensitivity { get; set; } = 150f;

	[Property, Group( "Pan" )]
	public float PanSpeed { get; set; } = 125f;

	[Property, Group( "Pan" )]
	public bool ScalePanByDistance { get; set; } = true;

	[Property, Group( "Pan" )]
	public float PanDistanceReference { get; set; } = 150f;

	[Property, Group( "Pan" )]
	public bool EnablePanMomentum { get; set; } = false;

	[Property, Group( "Pan" ), ShowIf( nameof( EnablePanMomentum ), true )]
	public float PanDrag { get; set; } = 0.9f;

	[Property, Group( "Zoom" )]
	public float ZoomStep { get; set; } = 0.15f;

	[Property, Group( "Zoom" )]
	public float MinDistance { get; set; } = 100f;

	[Property, Group( "Zoom" )]
	public float MaxDistance { get; set; } = 3000f;

	[Property, Group( "Limits" )]
	public float MinPitch { get; set; } = 10f;

	[Property, Group( "Limits" )]
	public float MaxPitch { get; set; } = 89f;

	[Property, Group( "Smoothing" )]
	public float PanSmoothTime { get; set; } = 0.05f;

	[Property, Group( "Smoothing" )]
	public float RotationSmoothTime { get; set; } = 0.05f;

	[Property, Group( "Smoothing" )]
	public float ZoomSmoothTime { get; set; } = 0.05f;

	#endregion

	#region State

	// Current smoothed values
	private Vector3 _pivotPosition;
	private Angles _viewAngles = new( 45, 0, 0 );
	private float _cameraDistance = 350f;

	// Target values for smoothing
	private Vector3 _targetPivotPosition;
	private Angles _targetViewAngles = new( 45, 180, 0 );
	private float _targetDistance = 150f;

	// Pan momentum
	private Vector3 _panVelocity;

	// Mouse state
	private bool _wasRotating;

	#endregion

	#region Public Interface

	public Rotation ViewRotation => Rotation.From( _viewAngles );
	public Rotation PivotRotation { get; private set; }

	public Vector3 PivotPosition
	{
		get => _pivotPosition;
		set
		{
			_pivotPosition = value;
			_targetPivotPosition = value;
		}
	}

	#endregion

	#region Lifecycle

	protected override void OnStart()
	{
		InitializePivotIfNeeded();
	}

	protected override void OnUpdate()
	{
		bool isRotating = Input.Down( "attack2" );
		UpdateMouseVisibility( isRotating );

		ProcessInput( isRotating );
		SmoothToTargets();
		ApplyTransform();
	}

	#endregion

	#region Initialization

	private void InitializePivotIfNeeded()
	{
		if ( _pivotPosition == default )
		{
			_pivotPosition = WorldPosition + WorldRotation.Forward * _cameraDistance;
			_targetPivotPosition = _pivotPosition;
		}
	}

	#endregion

	#region Input Processing

	private void ProcessInput( bool isRotating )
	{
		HandlePanning();
		HandleZoom();

		if ( isRotating )
			HandleRotation();
	}

	private void HandlePanning()
	{
		var input = Input.AnalogMove;
		if ( input.IsNearZeroLength && !EnablePanMomentum ) return;

		if ( EnablePanMomentum )
			HandlePanningWithMomentum( input );
		else
			HandleDirectPanning( input );
	}

	private void HandlePanningWithMomentum( Vector3 input )
	{
		// Add velocity from input
		if ( !input.IsNearZeroLength )
		{
			var moveDirection = GetWorldSpaceMoveDirection( input );
			if ( !moveDirection.IsNearZeroLength )
			{
				float panAmount = PanSpeed * GetDistanceScale() * Time.Delta;
				_panVelocity += moveDirection * panAmount;
			}
		}

		// Apply velocity and drag
		_targetPivotPosition += _panVelocity * Time.Delta;
		_panVelocity *= MathF.Pow( PanDrag, Time.Delta * 60f );

		// Stop when nearly zero
		if ( _panVelocity.Length < 0.1f )
			_panVelocity = Vector3.Zero;
	}

	private void HandleDirectPanning( Vector3 input )
	{
		if ( input.IsNearZeroLength ) return;

		var moveDirection = GetWorldSpaceMoveDirection( input );
		if ( moveDirection.IsNearZeroLength ) return;

		float panAmount = PanSpeed * GetDistanceScale() * Time.Delta;
		_targetPivotPosition += moveDirection * panAmount;
	}

	private void HandleZoom()
	{
		float scroll = Input.MouseWheel.y;
		if ( scroll == 0f ) return;

		// Exponential zoom centered on pivot
		float zoomFactor = MathF.Pow( 1f + ZoomStep, -scroll );
		_targetDistance = Math.Clamp(
			_targetDistance * zoomFactor,
			MinDistance,
			MaxDistance
		);
	}

	private void HandleRotation()
	{
		var lookDelta = Input.AnalogLook * (Time.Delta * LookSensitivity);

		_targetViewAngles.pitch = Math.Clamp(
			_targetViewAngles.pitch + lookDelta.pitch,
			MinPitch,
			MaxPitch
		);

		_targetViewAngles.yaw += lookDelta.yaw;
		_targetViewAngles.roll = 0f;
	}

	#endregion

	#region Helper Methods

	private Vector3 GetWorldSpaceMoveDirection( Vector3 input )
	{
		var flatRotation = Rotation.FromYaw( _targetViewAngles.yaw );
		var worldMove = (flatRotation * input).WithZ( 0f );
		return worldMove.IsNearZeroLength ? Vector3.Zero : worldMove.Normal;
	}

	private float GetDistanceScale()
	{
		if ( !ScalePanByDistance || PanDistanceReference <= 0f )
			return 1f;

		return _targetDistance / PanDistanceReference;
	}

	private void UpdateMouseVisibility( bool isRotating )
	{
		if ( isRotating != _wasRotating )
		{
			Mouse.Visibility = isRotating ? MouseVisibility.Hidden : MouseVisibility.Visible;
			_wasRotating = isRotating;
		}
	}

	#endregion

	#region Cursor Raycasting

	/// <summary>
	/// Traces from the camera through the cursor position into the world.
	/// </summary>
	/// <param name="maxDistance">Maximum trace distance (default: 10000)</param>
	/// <returns>Trace result containing hit information</returns>
	public SceneTraceResult TraceCursor( float maxDistance = 10000f )
	{
		var ray = Camera.ScreenPixelToRay( Mouse.Position );

		return Scene.Trace
			.Ray( ray, maxDistance )
			.Run();
	}

	/// <summary>
	/// Traces from the camera through the cursor position with custom trace options.
	/// Useful when you need to filter by tags, ignore specific objects, etc.
	/// </summary>
	/// <param name="maxDistance">Maximum trace distance</param>
	/// <param name="setupTrace">Action to configure the trace (add tags, ignore objects, etc.)</param>
	/// <returns>Trace result containing hit information</returns>
	public SceneTraceResult TraceCursor( float maxDistance, Action<SceneTrace> setupTrace )
	{
		var ray = Camera.ScreenPixelToRay( Mouse.Position );
		var trace = Scene.Trace.Ray( ray, maxDistance );

		setupTrace?.Invoke( trace );

		return trace.Run();
	}

	/// <summary>
	/// Gets the world position under the cursor, or null if nothing was hit.
	/// </summary>
	/// <param name="maxDistance">Maximum trace distance (default: 10000)</param>
	/// <returns>World position if hit, null otherwise</returns>
	public Vector3? GetCursorWorldPosition( float maxDistance = 10000f )
	{
		var trace = TraceCursor( maxDistance );
		return trace.Hit ? trace.HitPosition : null;
	}

	#endregion

	#region Smoothing & Transform

	private void SmoothToTargets()
	{
		float panT = GetSmoothingFactor( PanSmoothTime );
		float rotT = GetSmoothingFactor( RotationSmoothTime );
		float zoomT = GetSmoothingFactor( ZoomSmoothTime );

		_pivotPosition = Vector3.Lerp( _pivotPosition, _targetPivotPosition, panT );
		_viewAngles = _viewAngles.LerpTo( _targetViewAngles, rotT );
		_viewAngles.roll = 0f;
		_cameraDistance = MathX.LerpTo( _cameraDistance, _targetDistance, zoomT );
	}

	private void ApplyTransform()
	{
		PivotRotation = ViewRotation;
		WorldPosition = _pivotPosition + PivotRotation.Backward * _cameraDistance;
		WorldRotation = PivotRotation;
	}

	private static float GetSmoothingFactor( float smoothTime )
	{
		if ( smoothTime <= 0f ) return 1f;
		if ( Time.Delta <= 0f ) return 0f;

		return Math.Clamp( 1f - MathF.Exp( -Time.Delta / smoothTime ), 0f, 1f );
	}

	#endregion
}
