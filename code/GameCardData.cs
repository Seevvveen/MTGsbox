using System.Threading.Tasks;

public sealed class GameCardData : Component
{
	[Property, Title( "CardID" )] public string CardId;


	public Card Card { get; private set; }

	private LocalCardIndexSystem _index;
	private readonly TaskCompletionSource<bool> _readyTcs = new();
	public Task WhenReady => _readyTcs.Task;



	protected override async Task OnLoad()
	{
		try
		{
			_index = Scene.GetSystem<LocalCardIndexSystem>()
				?? throw new Exception( "Missing Index System" );

			await _index.WhenReady;

			var Cards = _index.DefaultCards
				?? throw new Exception( "Default Cards Index Missing" );

			Card = Cards.TryGetById( CardId, out var result )
				? result
				: throw new Exception( "Card Not Found in Index" );

			_readyTcs.TrySetResult( true );
		}
		catch ( Exception ex )
		{
			Log.Error( ex.Message );
			_readyTcs.TrySetResult( false );
		}

	}
}
