public class WorldCard : Component, ICardProvider
{
	[Property] string CardID { get; set; } = "56ebc372-aabd-4174-a943-c7bf59e5028d";

	public LocalCardIndexSystem IndexSystem { get; set; }
	public Card Card { get; set; }
	[RequireComponent] public CardRenderer CardRenderer { get; set; }

	protected override void OnStart()
	{
		IndexSystem = Scene.GetSystem<LocalCardIndexSystem>();
		IndexSystem.DefaultCards.TryGetById( CardID, out Card returnedCard );
		Card = returnedCard;
	}


	protected override void OnDestroy()
	{
		CardRenderer?.Destroy();
	}

}
