public sealed class SimpleCardDrag : Component, IDragNDrop
{
	// Required by the interface — store simple state.
	public Vector3 DragStartPos { get; set; }
	public Vector3 DragStopPos { get; set; }

	public bool IsDragging { get; private set; }
	public bool IsHovering { get; private set; }

	//
	// DRAG EVENTS
	//

	public void OnDragStart( GameObject dragger, Vector3 pointerWorldPos )
	{
		IsDragging = true;
		DragStartPos = WorldPosition;
	}

	public void OnDragUpdate( GameObject dragger, Vector3 pointerWorldPos )
	{
		// Move card to pointer (keep original Z so it stays on the virtual table)
		var pos = WorldPosition;
		WorldPosition = new Vector3( pointerWorldPos.x, pointerWorldPos.y, pos.z );

		DragStopPos = pointerWorldPos;
	}

	public void OnDragStop( GameObject dragger, Vector3 pointerWorldPos )
	{
		IsDragging = false;
		DragStopPos = pointerWorldPos;
	}

	//
	// OPTIONAL — we let default interface methods handle these
	// but you can override later if you want
	//

	// Hover:
	// void OnHoverStart(...) {}
	// void OnHoverUpdate(...) {}
	// void OnHoverStop(...) {}

	// Drop:
	// void OnDropSuccess(...) {}
	// void OnDropFailed(...) {}

	// Rules:
	// bool CanDrop(...) => true;
	// bool CanDrag(...) => true;
	// bool CanHover(...) => true;
}
