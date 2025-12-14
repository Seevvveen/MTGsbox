using Sandbox.UI;
using System.Threading.Tasks;

public sealed class SimpleCardDisplayWorld : PanelComponent
{
	private GameCardData CardData;
	private Card _card;

	private Image ImagePanel;

	protected override void OnTreeFirstBuilt()
	{
		base.OnTreeFirstBuilt();

		// make sure the image exists
		ImagePanel = new Image
		{
			Parent = Panel
		};
	}

	protected override async Task OnLoad()
	{
		// 1. Find GameCardData on a parent
		CardData = GetComponentInParent<GameCardData>();

		if ( CardData == null )
		{
			Log.Error( "[SimpleCardDisplayWorld] No GameCardData found in parent hierarchy." );
			return;
		}

		// 2. Wait until GameCardData has finished loading its card
		await CardData.WhenReady;

		if ( CardData.Card == null )
		{
			Log.Error( "[SimpleCardDisplayWorld] GameCardData finished but Card is null." );
			return;
		}

		_card = CardData.Card;

		// 3. Ensure the ImagePanel exists (in case OnTreeFirstBuilt hasn’t run yet for some reason)
		if ( ImagePanel == null )
		{
			ImagePanel = new Image
			{
				Parent = Panel
			};
		}

		// 4. Make sure the card actually has an image URI
		//if ( _card.ImageUris == null || _card.ImageUris.Png == null )
		//{
		//	Log.Warning( $"[SimpleCardDisplayWorld] Card '{_card.Name}' has no Png image URI." );
		//	return;
		//}

		// 5. Finally set the texture
		ImagePanel.SetTexture( _card.ImageUris.Png.ToString() );
	}
}
