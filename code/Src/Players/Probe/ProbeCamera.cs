using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
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


	protected override void OnAwake()
	{
		// Only claim Local if this is the correct instance
		if ( !Networking.IsActive )
		{
			if ( !IsProxy )
				Local = this;
		}
		else
		{
			if ( Network.IsOwner )
				Local = this;
		}

		_angles = WorldRotation.Angles();
		_pitch = _angles.pitch;
	}
	
	protected override void OnUpdate()
	{
		base.OnUpdate();
		(_angles, _pitch, var rot) = ProbeLook.ApplyAnalogLook( _angles, _pitch, LookCfg, Time.Delta );
		WorldRotation = rot;
	}
	
	protected override void OnDestroy()
	{
		if ( Local == this )
			Local = null;
	}
}