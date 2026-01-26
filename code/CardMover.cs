using Sandbox;
using Sandbox.Utility.Svg;

public sealed class CardMover : Component
{
	[Property] public GameObject Spot1 { get; set; }
	[Property] public GameObject Spot2 { get; set; }

	public void MoveTo(GameObject card, GameObject location)
	{
		card.WorldPosition = location.WorldPosition;
	}
	
}
