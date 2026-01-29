#nullable enable
using Sandbox.Diagnostics;
using Sandbox.Probe;
namespace Sandbox.Players.Probe;



/// <summary>
/// Players Movement
/// </summary>
[Title( "Probe  Movement")]
public sealed class ProbeMovement : Component
{
	[Property, RequireComponent, ReadOnly] public required Rigidbody RigidBody { get; set; }
	// Once Scene Camera that finds the local player
	[ReadOnly, Property] public required ProbeCamera? Camera { get; set; }
	[ReadOnly, Property] private bool HasCamera { get; set; } = false;
	[Property,Group("Tuning")] private float Speed { get; set; } = 400f;
	[Property,Group("Tuning")] private float AccelerationRate { get; set; } = 15f;
	[Property,Group("Tuning")] private float MaxVelocity { get; set; } = 1000f;
	
	private const float PerpFriction = 10f;
	
	private Vector3 _wishDir;

	private MoveSettings MoveCfg => new(
		Speed,
		AccelerationRate,
		MaxVelocity,
		PerpFriction
	);
	
	
	protected override void OnAwake()
	{
		base.OnAwake();
		RigidBody = GetComponent<Rigidbody>();
	}
	
	protected override void OnFixedUpdate()
	{
		if ( IsProxy ) return;
		Assert.NotNull( RigidBody );

		if (!HasCamera)
		{
			if ( Camera is not { IsValid: true } )
				Camera = ProbeCamera.Local;
			if ( Camera == null ) return;
			Camera.GameObject.Parent = GameObject;
			HasCamera = true;
		}
		
		_wishDir = ProbeMove.GetWishDirection( Camera!.WorldRotation, Input.AnalogMove );
		RigidBody.ApplyMovement(_wishDir, Time.Delta, MoveCfg );
	}
}
