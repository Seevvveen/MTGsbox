using System.Threading.Tasks;
using Sandbox.Card;
using Sandbox.Catalog;
using Sandbox.Engine.Bootstrapper;
using Sandbox.UI;

namespace Sandbox.Components;


public class Card : Component
{
	private static readonly GameCatalogs Catalogs = ApplicationBootstrap.Catalogs;
	
	[Property, Change]
	public string CardId {get; set;} = Catalogs.Cards.ById.GetRandomOrThrow().Id.ToString();
	
	private CardDefinition _definition;

	[RequireComponent, Hide] private CardRenderer CardRenderer { get; set; }
	
	//Add Plane Collider Back
	//Add a CardInstance Class for Runtime Mutablility
	

	protected override async Task OnLoad()
	{
		try
		{
			await ApplicationBootstrap.EnsureStartedAsync();
			
			SetCard( Guid.Parse( CardId ) );
		}
		catch ( Exception e )
		{
			Log.Error( e, "Card: OnLoad failed" );
		}
	}
	
	public void SetCard( Guid id )
	{
		if ( !Catalogs.Cards.IsReady )
			return;

		if ( !Catalogs.Cards.ById.TryGet( id, out var def ) )
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
		if ( !Catalogs.Cards.IsReady )
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
