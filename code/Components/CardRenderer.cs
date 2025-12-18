using Sandbox.UI;
using System.Threading.Tasks;

/// <summary>
/// Hold a Card Image and render it to screen
/// </summary>
public class CardRenderer : PanelComponent
{
	[RequireComponent, Hide] public Sandbox.WorldPanel WorldPanel { get; set; }

	private ICardProvider _cardProvider;
	private Card _card;
	private Image _image;

	public float CardHeight = 512;


	protected override void OnTreeFirstBuilt()
	{
		base.OnTreeFirstBuilt();

		// Build UI here (Panel root is guaranteed to exist)
		_image = new Image
		{
			Parent = Panel
		};

		// ?? maybe
		//_image.Style.Width = Length.Percent( 100 );
		//_image.Style.Height = Length.Percent( 100 );
	}


	protected override void OnAwake()
	{
		try
		{
			var aspectRatio = 63f / 88f;
			var CardWidth = CardHeight * aspectRatio;
			WorldPanel.PanelSize = new Vector2( CardHeight, CardWidth );
		}
		catch ( Exception ex )
		{
			Log.Error( $"[CardRenderer] Failed to set WorldPanel size: {ex}" );
		}
	}

	protected override async Task OnLoad()
	{
		try
		{

			// Initialize UI
			_image = new Image();
			_image.Parent = Panel;

			_cardProvider = Components.GetInParentOrSelf<ICardProvider>();
			if ( _cardProvider == null )
			{
				Log.Error( "[CardRenderer] No ICardProvider found" );
				return;
			}

			// Wait for provider to be ready
			Log.Info( "[CardRenderer] Waiting on Card Provider" );
			await _cardProvider.WhenReady;

			if ( !_cardProvider.IsReady )
			{
				Log.Error( "[CardRenderer] Card provider failed to initialize" );
				return;
			}

			// Get card from provider
			_card = _cardProvider.Card;
			if ( _card == null )
			{
				Log.Warning( "[CardRenderer] Card is null from provider" );
				return;
			}

			// Validate image URI
			if ( _card.ImageUris.Png == null )
			{
				Log.Error( $"[CardRenderer] Card {_card} has no PNG image URI" );
				return;
			}

			// Load the texture
			var imageUrl = _card.ImageUris.Png.ToString();
			if ( string.IsNullOrWhiteSpace( imageUrl ) )
			{
				Log.Error( $"[CardRenderer] Card {_card} has empty image URL" );
				return;
			}

			_image.SetTexture( imageUrl );

			Log.Info( $"[CardRenderer] Successfully loaded card {_card.Name}" );
		}
		catch ( Exception ex )
		{
			Log.Error( $"[CardRenderer] Failed to load: {ex}" );
		}
	}

	protected override void OnDestroy()
	{
		try
		{
			_image?.Delete();
			_image = null;

			WorldPanel?.Destroy();
		}
		catch ( Exception ex )
		{
			Log.Error( $"[CardRenderer] Error during cleanup: {ex}" );
		}
	}

	// Public method to update card if needed
	public void SetCard( Card card )
	{
		try
		{
			if ( card == null )
			{
				Log.Warning( "[CardRenderer] Attempted to set null card" );
				return;
			}

			if ( card.ImageUris.Png == null )
			{
				Log.Error( $"[CardRenderer] Card {card.Id} has no PNG image URI" );
				return;
			}

			_card = card;
			_image?.SetTexture( _card.ImageUris.Png.ToString() );
		}
		catch ( Exception ex )
		{
			Log.Error( $"[CardRenderer] Failed to set card: {ex}" );
		}
	}
}
