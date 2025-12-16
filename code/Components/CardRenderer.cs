using Sandbox.UI;
/// <summary>
/// Hold a Card Image and render it to screen
/// </summary>
public class CardRenderer : PanelComponent
{
	[RequireComponent, Hide] public Sandbox.WorldPanel WorldPanel { get; set; }
	private ICardProvider _cardProvider;
	private Card _card;
	private Image Image;

	public float CardHeight = 512;

	protected override void OnAwake()
	{
		var S = 63f / 88f;
		WorldPanel.PanelSize = new Vector2( CardHeight * S, CardHeight );
	}


	protected override void OnStart()
	{
		Image = new Image()
			?? throw new Exception( "[CardRender] ImagePanel Failed" );
		Image.Parent = Panel;

		_cardProvider = Components.GetInParentOrSelf<ICardProvider>()
			?? throw new Exception( "[CardRender] No ICardProvider found" );
		_card = _cardProvider.Card
			?? throw new Exception( "[CardRender] Card is null" );

		Image.SetTexture( _card.ImageUris.Png.ToString() );
	}

	protected override void OnDestroy()
	{
		WorldPanel?.Destroy();
	}


}
