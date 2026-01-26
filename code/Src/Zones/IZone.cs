namespace Sandbox.Zones;
using System;

public interface IZone
{
	// Who owns this zone
	public Guid Owner { get; }

	public bool CanAdd( GameObject card );
	public bool TryAdd( GameObject card );

	public bool CanRemove( GameObject card );
	public bool TryRemove( GameObject card );
}