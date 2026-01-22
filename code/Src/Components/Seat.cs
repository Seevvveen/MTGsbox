namespace Sandbox.GameNetworking;

/// <summary>
/// Represent a Seat Anchor in the scene
/// MatchManagers will look for these components on objects to know where to place players ingame
/// </summary>
public sealed class Seat : Component
{
	[Property] public int Order { get; set; }
	[Property, ReadOnly] public Guid? Occupent { get; private set; } = null;
	
	public bool IsOccupied
		=> Occupent is not null;

	public void SetOccupent(Guid id)
	{
		Occupent = id;
	}

	public void ClearOccupent()
	{
		Occupent = null;
	}
}
