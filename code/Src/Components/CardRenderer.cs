using System.Runtime.CompilerServices;
using Editor;
using Sandbox.Card;
using Sandbox.Rendering;
using Sandbox.UI;

namespace Sandbox.Components;


/// <summary>
/// Renders a CardImage within the scene
/// </summary>
public sealed class CardRenderer : PanelComponent
{
	[RequireComponent]
	private WorldPanel WorldPanel { get; set; }

	[Change("UriChanged"), Property, ReadOnly]
	public Uri Uri { get; set; } =
		new("https://cards.scryfall.io/large/front/8/6/8625b50d-474d-46dd-af84-0b267ed5fab3.jpg?1616041637");
	private Image _image;

	protected override void OnAwake()
	{
		WorldPanel.PanelSize = StaticCardInformation.Size;
	}

	protected override void OnTreeFirstBuilt()
	{
		base.OnTreeFirstBuilt();

		_image = new Image { Parent = Panel };
		
		_image.SetTexture( Uri.ToString() );
	}
	
	void UriChanged(Uri old, Uri @new)
	{
		_image?.SetTexture(@new.OriginalString.IsWhiteSpace() ? old.OriginalString : @new.OriginalString);
	}
	
	protected override void OnDestroy()
	{
		WorldPanel?.Destroy();
	}
}
