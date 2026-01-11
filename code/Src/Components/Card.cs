using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Sandbox.Card;
using Sandbox.Catalog;
using Sandbox.Engine.Bootstrapper;
using Sandbox.UI;

namespace Sandbox.Components;

[SelectionBase]
public class Card : Component
{
	private static readonly GameCatalogs Catalogs = ApplicationBootstrap.Catalogs;
	
	public CardInstance CardInstance;
	
	[Property, Change] private string CardId {get; set;}
	private CardDefinition _def;

	private CardRenderer CardRenderer { get; set; }


	protected override async Task OnLoad()
	{
		try
		{
			await ApplicationBootstrap.EnsureStartedAsync();

			CardRenderer = GetComponentInChildren<CardRenderer>();
			
			if ( CardId is not null )
				SetCard( Guid.Parse( CardId ) );
		}
		catch ( Exception e )
		{
			Log.Error( e, "Card: OnLoad failed" );
		}
	}

	[Button( "SetRandomCard" )]
	private void SetRandomCard()
	{
		CardId = Catalogs.Cards.ById.GetRandomOrThrow().Id.ToString();
		//SetCard( Guid.Parse( CardId ) );
	}


	private void SetCard( Guid id )
	{
		if ( !Catalogs.Cards.IsReady )
			return;

		if ( !Catalogs.Cards.ById.TryGet( id, out var def ) )
		{
			Log.Error( $"Card: not found in catalog ById: {id}" );
			return;
		}
		
		_def = def;
		
		var uri = def.ImageUris.Large;
		if ( uri is not null )
			CardRenderer.SetImage( uri );
	}
	
	private void OnCardIdChanged(string oldId, string newId)
	{
		if ( newId == oldId )
			return;
		
		if ( !Catalogs.Cards.IsReady )
			return;
		
		if ( Guid.TryParse( newId, out var id ) )
		{
			SetCard(id);
		}
	}
}
