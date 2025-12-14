public sealed class DragNDropSystem : GameObjectSystem<DragNDropSystem>
{
	private IDragNDrop _hover;
	private IDragNDrop _dragging;

	public DragNDropSystem( Scene scene ) : base( scene )
	{
		// Run once per frame in a known spot in the pipeline.
		// Adjust Stage / order to fit your scene.
		Listen( Stage.StartUpdate, 0, OnUpdate, "DragNDrop.Update" );
	}

	private void OnUpdate()
	{
		var camera = Scene.Camera;
		if ( camera == null )
			return;

		// 1. Compute pointer position on your virtual table plane
		var ray = camera.ScreenPixelToRay( Mouse.Position );
		var pointerWorldPos = ProjectOntoTablePlane( ray );

		// Use some GameObject as "dragger" – usually the player
		var dragger = camera.GameObject; // or Scene.Get<YourPlayer>()

		if ( _dragging == null )
		{
			UpdateHover( dragger, pointerWorldPos );

			// Start drag
			if ( Input.Pressed( "attack1" ) && _hover != null && _hover.CanDrag( dragger ) )
			{
				_dragging = _hover;
				_dragging.OnDragStart( dragger, pointerWorldPos );
			}

			return;
		}

		// While dragging, send updates
		_dragging.OnDragUpdate( dragger, pointerWorldPos );

		// Stop drag and resolve drop
		if ( Input.Released( "attack1" ) )
		{
			_dragging.OnDragStop( dragger, pointerWorldPos );

			if ( _dragging.CanDrop( dragger, pointerWorldPos ) )
				_dragging.OnDropSuccess( dragger, pointerWorldPos );
			else
				_dragging.OnDropFailed( dragger, pointerWorldPos );

			_dragging = null;
		}
	}

	private void UpdateHover( GameObject dragger, Vector3 pointerWorldPos )
	{
		// However you want to pick the card under the pointer.
		// This example assumes your cards sit on a plane and have some collider.
		var tr = Scene.Trace
			.Ray( pointerWorldPos + Vector3.Up * 10f, pointerWorldPos + Vector3.Down * 10f )
			.Run();

		IDragNDrop newHover = null;

		if ( tr.Hit && tr.GameObject != null &&
			 tr.GameObject.Components.TryGet<IDragNDrop>( out var dnd ) &&
			 dnd.CanHover( dragger ) )
		{
			newHover = dnd;
		}

		if ( newHover == _hover )
		{
			_hover?.OnHoverUpdate( dragger, pointerWorldPos );
			return;
		}

		// Hover changed
		_hover?.OnHoverStop( dragger );
		_hover = newHover;
		_hover?.OnHoverStart( dragger, pointerWorldPos );
	}

	private Vector3 ProjectOntoTablePlane( Ray ray )
	{
		// Example: card table is a plane at Z = 0
		if ( ray.Forward.z == 0 )
			return ray.Position;

		//var t = -ray.Position.z / ray.Forward.z;
		return ray.Position + ray.Forward * 200;
	}
}
