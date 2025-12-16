/// <summary>
/// Simple input provider: exposes movement + look vectors.
/// Drop this on a GameObject and reference it from other components.
/// </summary>
public sealed class PlayerInput : Component
{
	[Property] public bool UseGameInput { get; set; } = true;

	/// <summary>Movement input in local space (x = forward, y = left, z = up).</summary>
	public Vector3 Move =>
		UseGameInput ? Input.AnalogMove : Vector3.Zero;

	/// <summary>Look input angles (e.g., from mouse/controller).</summary>
	public Angles Look =>
		UseGameInput ? Input.AnalogLook : Angles.Zero;
}
