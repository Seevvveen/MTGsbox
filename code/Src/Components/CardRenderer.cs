using Sandbox.Card;
using Sandbox.UI;

namespace Sandbox.Components;

/// <summary>
/// Renders a CardImage within the scene
/// </summary>
public sealed class CardRenderer : PanelComponent
{
	private readonly Card _card = null;
	private Image _image = null;
	
	public CardRenderer(Card card)
	{
		_card = card;
	}
	
	protected override void OnTreeFirstBuilt()
	{
		base.OnTreeFirstBuilt();
		
		_image = new Image
		{
			Parent = Panel
		};

		_image.SetTexture( _card.Test.Large.ToString() );
	}
	
	public void SetImage( string src )
	{
		if ( _image is null || !_image.IsValid )
			return;

		_image.SetTexture( src );
	}

	
	
	/*
	protected override void BuildRenderTree( RenderTreeBuilder builder )
	{
		builder.OpenElement( 0, "image" );
		builder.AddAttribute( 1, "src", Card?.ImageUris.Normal.ToString() );
		builder.CloseElement();
	}

	protected override int BuildHash()
	{
		return HashCode.Combine( Card?.Id, Card?.ImageUris.Normal );
	}
	*/
}
