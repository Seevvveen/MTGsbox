using Sandbox.Probe;

namespace Sandbox.Players.Probe;

/// <summary>
/// Leave One in the scene and have it resolve to the local player
/// One One of these should ever exist in the scene, meaning we dont need to check for proxies since each client only has the one
/// </summary>
public class ProbeCamera : Component
{
	public static ProbeCamera? Local { get; private set; }


	[RequireComponent] private CameraComponent Camera { get; set; }

	
	[Property] private float SensX { get; set; } = 100f;
	[Property] private float SensY { get; set; } = 100f;
	
	private LookSettings LookCfg => new(
		SensX,
		SensY,
		MinPitch: -90f,
		MaxPitch:  90f
	);

	private Angles _angles;
	private float _pitch;
	private bool _paired;


	protected override void OnAwake()
	{
		// Claim Local on the non-proxy instance. In network games each client should have exactly
		// one non-proxy copy of their pawn hierarchy.
		if ( !IsProxy )
			Local = this;

		_angles = WorldRotation.Angles();
		_pitch = _angles.pitch;
	}

	protected override void OnStart()
	{
		base.OnStart();

		// Only the local (non-proxy) instance should drive an active camera.
		// This prevents multiple cameras fighting when proxies exist.
		if ( Camera is { IsValid: true } )
		{
			Camera.Enabled = !IsProxy;
		}

		// Pairing/parenting can be load-order dependent, so we do it lazily in Update.
		_paired = false;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		// Load-order safe pairing: if the movement root didn't exist at our OnStart,
		// keep trying until we can attach once.
		if ( !_paired && !IsProxy )
		{
			var movement = ProbeMovement.Local;
			movement ??= GetComponentInParent<ProbeMovement>();

			if ( movement is { IsValid: true } && GameObject.Parent != movement.GameObject )
			{
				GameObject.Parent = movement.GameObject;
			}

			if ( movement is { IsValid: true } )
				_paired = true;
		}

		(_angles, _pitch, var rot) = ProbeLook.ApplyAnalogLook( _angles, _pitch, LookCfg, Time.Delta );
		WorldRotation = rot;
	}
	
	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ( Local == this )
			Local = null;
	}
}