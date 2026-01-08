using System.Runtime.CompilerServices;
using Editor;
using Sandbox.Card;
using Sandbox.Rendering;
using Sandbox.UI;

namespace Sandbox.Components;


/// <summary>
/// Renders a CardImage within the scene
/// </summary>
public sealed class CardRenderer() : PanelComponent, Component.ExecuteInEditor
{
	[RequireComponent] private WorldPanel WorldPanel { get; set; }

	private Uri _uri;
	private Image _image;


	protected override void OnAwake()
	{
		WorldPanel.PanelSize = StaticCardInformation.Size;
	}
	

	protected override void OnTreeFirstBuilt()
	{
		base.OnTreeFirstBuilt();

		_image = new Image
		{
			Parent = Panel
		};

		
		
		if ( _uri is not null )
			ApplyImage();
	}
	
	public void SetImage( Uri src )
	{
		if ( src is null )
		{
			Log.Error( "Card: SetImage: src is null" );
			return;
		}

		_uri = src;

		if ( _image is null || !_image.IsValid )
			return; // not built yet, will apply in OnTreeFirstBuilt

		ApplyImage();
	}
	
	private void ApplyImage()
	{
		var path = _uri.OriginalString;
		if ( string.IsNullOrWhiteSpace( path ) )
			return;

		_image.SetTexture( path );
	}
	
	
	protected override void OnDestroy()
	{
		WorldPanel?.Destroy();
	}

}
