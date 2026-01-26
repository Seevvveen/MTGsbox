using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Sandbox;

namespace Sandbox.Zones.ZoneComponents;

public class Hand : Zone
{
	[Property] public float Radius { get; set; } = 10f;
	[Property, ReadOnly, Required] private SphereCollider? Collider { get; set; }
	
	// Layout properties
	[Property] public float CardSpacing { get; set; } = 4f;
	[Property] public float CurveAmount { get; set; } = 2f; // Height variation for curve
	[Property] public float FanDegrees { get; set; } = 10f;
	[Property] public float Depth { get; set; } = 0.5f;
	
	protected override Task OnLoad()
	{
		Collider?.Center = WorldPosition;
		Collider?.Radius = Radius;
		Collider?.IsTrigger = true; // Zones should probably be triggers?
		return Task.CompletedTask;
	}

	protected override void OnUpdate()
	{
		UpdateHandLayout();
	}

	private void UpdateHandLayout()
	{
		if ( Cards.Count == 0 ) return;

		// Simple automatic layout
		// Center the cards around the hand's origin
		
		var totalWidth = (Cards.Count - 1) * CardSpacing;
		var startX = -totalWidth           / 2f;

		for ( int i = 0; i < Cards.Count; i++ )
		{
			var card = Cards[i];
			if ( !card.IsValid() ) continue;

			float t = (Cards.Count > 1) ? (float)i / (Cards.Count - 1) : 0.5f;
			float n = (t * 2f) - 1f; // -1..1

			float x = startX + (i * CardSpacing);

			// Arc on Z (height)
			float zCurve = (1f - (n * n)) * CurveAmount;

			// Depth separation on Y (so it doesn't alter the arc)
			float yDepth = i * Depth;                   // flip sign if needed: -i * Depth
			yDepth -= (Cards.Count - 1) * Depth * 0.5f; // optional: keep stack centered

			var targetLocalPos = new Vector3( x, yDepth, zCurve );

			var baseRot = Rotation.FromYaw( 90f );
			var fanRot  = Rotation.FromYaw( -n * FanDegrees );
			var targetLocalRot = baseRot * fanRot;

			card.LocalPosition = Vector3.Lerp( card.LocalPosition, targetLocalPos, Time.Delta   * 10f );
			card.LocalRotation = Rotation.Slerp( card.LocalRotation, targetLocalRot, Time.Delta * 10f );
		}

	}

	protected override void OnCardAdded( GameObject card )
	{
		// Optional: Trigger sound or effect
	}

	protected override void OnCardRemoved( GameObject card )
	{
		// Optional: Trigger sound or effect
	}
}