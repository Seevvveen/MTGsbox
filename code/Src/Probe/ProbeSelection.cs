#nullable enable

using Sandbox.Zones;

namespace Sandbox.Probe;

public sealed class ProbeSelection : Component
{
	[Property] public GameObject? Probe { get; set; }

	[Property] public float MaxDistance { get; set; } = 1000f;
	[Property] public float RayStartOffset { get; set; } = 50f;
	[Property] public float DragHeightOffset { get; set; } = 1f;

	[Property] public GameObject? Hovered { get; private set; }
	[Property] public GameObject? Held { get; private set; }

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;
		if ( Probe is null ) return;

		var ray = BuildRay( Probe );

		// 1) Update hover (only matters when not holding)
		var hitSelectable = Trace( ray, withTag: "selectable" );
		Hovered = (Held is null && hitSelectable.Hit) ? hitSelectable.GameObject : null;


		// 2) Begin hold
		if ( Held is null && Input.Pressed( "attack1" ) && Hovered is not null )
		{
			// Check if we can pick it up from its current zone (if any)
			var cardComp = Hovered.Components.Get<Sandbox.Components.Card>();
			if ( cardComp != null && cardComp.CurrentZone != null )
			{
				if ( !cardComp.CurrentZone.TryRemove( Hovered ) )
				{
					// Zone refused to release the card
					return;
				}
			}

			Held = Hovered;
		}

		// 3) Drag
		if ( Held is not null && Input.Down( "attack1" ) )
		{
			var hitWorld = Trace( ray, withTag: null, withoutTag: "selectable", ignore: Held );

			if ( hitWorld.Hit )
				Held.WorldPosition = hitWorld.HitPosition + Vector3.Up * DragHeightOffset;
		}

		// 4) Drop / place
		if ( Held is not null && Input.Released( "attack1" ) )
		{
			var hitPlaceable = Trace( ray, withTag: "placeable", ignore: Held );

			if ( hitPlaceable.Hit )
			{
				// Attempt to add to zone
				// We assume any "placeable" might be a zone or have a zone component
				var zone = hitPlaceable.GameObject.Components.Get<IZone>( FindMode.EverythingInSelfAndAncestors );
				
				if ( zone != null )
				{
					if ( zone.TryAdd( Held ) )
					{
						// Success!
					}
					else 
					{
						// Zone refused to accept it
						// Maybe return to old zone? Or just drop in world?
					}
				}
			}
			else
			{
				// Dropped in emptiness
				// If it has a previous zone, maybe we should re-add it?
				// For now, let's assume dropping in world is valid (e.g. creating a new pile or just placing on table)
				// If we want to enforce zones, we'd check cardComp.CurrentZone and re-add.
			}

			Held = null;
		}
	}

	private Ray BuildRay( GameObject probe )
	{
		var start = probe.WorldPosition + probe.WorldRotation.Forward * RayStartOffset;
		var dir = probe.WorldRotation.Forward;
		return new Ray( start, dir );
	}
	
	
	private SceneTraceResult Trace( Ray ray, string? withTag = null, string? withoutTag = null, GameObject? ignore = null )
	{
		var builder = Scene.Trace.Ray( ray, MaxDistance );

		if ( !string.IsNullOrEmpty( withTag ) )
			builder = builder.WithTag( withTag );

		if ( !string.IsNullOrEmpty( withoutTag ) )
			builder = builder.WithoutTags( withoutTag );

		if ( ignore is not null )
			builder = builder.IgnoreGameObjectHierarchy( ignore ); // use IgnoreGameObject(...) if you only want the root

		var tr = builder.Run();
		DebugOverlay.Trace( tr );
		return tr;
	}
	

	
}
