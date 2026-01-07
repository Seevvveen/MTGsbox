using Editor;
using Sandbox.Card;
using Sandbox.UI;

namespace Sandbox.Components;


/// <summary>
/// Renders a CardImage within the scene
/// </summary>
public sealed class CardRenderer : PanelComponent
{
	[RequireComponent] private WorldPanel WorldPanel { get; set; }

	private Uri _uri;
	private Image _image;
	
	protected override void OnTreeFirstBuilt()
	{
		base.OnTreeFirstBuilt();
		
		_image = new Image
		{
			Parent = Panel
		};
		
		if ( _uri is not null)
			SetImage( _uri );
	}
	
	public void SetImage( Uri src )
	{
		if ( _image is null || !_image.IsValid )
			return;

		if ( src is null )
			return;
		
		var path = src.OriginalString;
		if ( string.IsNullOrWhiteSpace( path ) )
			return;

		_image.SetTexture( path );
		_uri = src;
	}
	
	protected override void OnDestroy()
	{
		WorldPanel?.Destroy();
	}

}
