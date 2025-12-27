/// <summary>
/// Dictates Camera Data
/// </summary>
public sealed class StrategyCamera2 : Component
{

	[RequireComponent, Hide]
	public CameraComponent Camera { get; set; }

	// Configuration (could be injected via interface)
	[Property, Group( "Limits" )] public float MinPitch { get; set; } = 10f;
	[Property, Group( "Limits" )] public float MaxPitch { get; set; } = 89f;
	[Property, Group( "Zoom" )] public float MinDistance { get; set; } = 100f;
	[Property, Group( "Zoom" )] public float MaxDistance { get; set; } = 3000f;
	[Property, Group( "Smoothing" )] public float PanSmoothTime { get; set; } = 0.05f;
	[Property, Group( "Smoothing" )] public float RotationSmoothTime { get; set; } = 0.05f;
	[Property, Group( "Smoothing" )] public float ZoomSmoothTime { get; set; } = 0.05f;

	// Public state
	public Rotation ViewRotation => Rotation.From( _viewAngles );
	public Rotation PivotRotation { get; private set; }
	public Vector3 PivotPosition => _pivotPosition;
	public float CameraDistance => _cameraDistance;

	// Internal state
	private Vector3 _pivotPosition, _targetPivotPosition;
	private Angles _viewAngles = new( 45, 0, 0 );
	private Angles _targetViewAngles = new( 45, 180, 0 );
	private float _cameraDistance = 350f, _targetDistance = 150f;

	// Set Camera
	public void SetPivotPosition( Vector3 position )
	{
		_pivotPosition = _targetPivotPosition = position;
	}

	public void SetRotation( Angles angles )
	{
		_targetViewAngles = angles;
		_targetViewAngles.pitch = Math.Clamp( _targetViewAngles.pitch, MinPitch, MaxPitch );
		_targetViewAngles.roll = 0f;
	}


	//Modify Camera
	public void Pan( Vector3 worldDirection, float amount )
	{
		if ( !worldDirection.IsNearZeroLength )
		{
			_targetPivotPosition += worldDirection.Normal * amount;
		}
	}

	public void Zoom( float zoomFactor )
	{
		_targetDistance = Math.Clamp( _targetDistance * zoomFactor, MinDistance, MaxDistance );
	}

	public void Rotate( Angles delta )
	{
		_targetViewAngles.pitch = Math.Clamp( _targetViewAngles.pitch + delta.pitch, MinPitch, MaxPitch );
		_targetViewAngles.yaw += delta.yaw;
		_targetViewAngles.roll = 0f;
	}

	// Hooks 
	protected override void OnStart()
	{
		if ( _pivotPosition == default )
			_pivotPosition = _targetPivotPosition = WorldPosition + WorldRotation.Forward * _cameraDistance;
	}

	protected override void OnUpdate()
	{
		ApplySmoothingAndTransform();
	}

	
	private void ApplySmoothingAndTransform()
	{
		_pivotPosition = Vector3.Lerp( _pivotPosition, _targetPivotPosition, SmoothFactor( PanSmoothTime ) );
		_viewAngles = _viewAngles.LerpTo( _targetViewAngles, SmoothFactor( RotationSmoothTime ) );
		_viewAngles.roll = 0f;
		_cameraDistance = MathX.LerpTo( _cameraDistance, _targetDistance, SmoothFactor( ZoomSmoothTime ) );

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
