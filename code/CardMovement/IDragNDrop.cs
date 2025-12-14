// Functionality in DragNDropSystem

public interface IDragNDrop
{
	Vector3 DragStartPos { get; set; }
	Vector3 DragStopPos { get; set; }

	bool IsDragging { get; }
	bool IsHovering { get; }

	void OnHoverStart( GameObject dragger, Vector3 pointerWorldPos ) { }
	void OnHoverUpdate( GameObject dragger, Vector3 pointerWorldPos ) { }
	void OnHoverStop( GameObject dragger ) { }

	void OnDragStart( GameObject dragger, Vector3 pointerWorldPos ) { }
	void OnDragUpdate( GameObject dragger, Vector3 pointerWorldPos ) { }
	void OnDragStop( GameObject dragger, Vector3 pointerWorldPos ) { }

	void OnDropFailed( GameObject dragger, Vector3 pointerWorldPos ) { }
	void OnDropSuccess( GameObject dragger, Vector3 pointerWorldPos ) { }

	bool CanDrop( GameObject dragger, Vector3 pointerWorldPos ) => true;
	bool CanDrag( GameObject dragger ) => true;
	bool CanHover( GameObject dragger ) => true;
}
