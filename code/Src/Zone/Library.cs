using Sandbox.Card;

namespace Sandbox.Zone;

public class Library(List<CardDefinition> cardList)
{
	public CardDefinition Draw()
	{
		var card = cardList.First();
		cardList.RemoveAt( 1 );
		return card;
	}

	public void Shuffle()
	{
		cardList.Shuffle();
	}
	
}
