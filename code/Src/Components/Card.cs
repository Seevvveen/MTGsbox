using System.Threading.Tasks;
using Sandbox.Card;
using Sandbox.Catalog;
using Sandbox.Engine.Bootstrapper;

namespace Sandbox.Components;


public class Card : Component
{
	private readonly GameCatalogs _catalogs = ApplicationBootstrap.Catalogs;
	
	[Property, Change]
	public string CardId {get; set;} = string.Empty;
	
	private CardDefinition _definition;
	[RequireComponent] private CardRenderer CardRenderer { get; set; }


	//Sizing Code - unimplemented
	private const float CardHeight = 512f;
	private const float CardAspectRatio = 63f  / 88f;
	private const float CardWidth = CardHeight * CardAspectRatio;
	private static readonly Vector2 CardSize = new( CardWidth, CardHeight );
	//Add Plane Collider Back
	//Add a CardInstance Class for Runtime Mutablility
	
	
	

	protected override async Task OnLoad()
	{
		try
		{
			await ApplicationBootstrap.EnsureStartedAsync();

			if ( Guid.TryParse( CardId, out var id ) )
				SetCard( id );
		}
		catch ( Exception e )
		{
			Log.Error( e, "Card: OnLoad failed" );
		}
	}
	
	
	public void SetCard( Guid id )
	{
		if ( !_catalogs.Cards.IsReady )
			return;

		if ( !_catalogs.Cards.ById.TryGet( id, out var def ) )
		{
			Log.Error( $"Card: not found in catalog ById: {id}" );
			return;
		}


		_definition = def;

		var uri = def.ImageUris.Large;
		if ( uri is not null )
			CardRenderer.SetImage( uri );
	}
	
	private void OnCardIdChanged(string oldId, string newId)
	{
		if ( !_catalogs.Cards.IsReady )
			return;
		
		if ( Guid.TryParse( newId, out var id ) )
		{
			SetCard(id);
		}
		else
		{
			Log.Error( "Not Valid Id" );
		}
	}

	protected override void OnDestroy()
	{
		CardRenderer?.Destroy();
	}
	
	
}
