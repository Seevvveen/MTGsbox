#nullable enable
using System.Threading.Tasks;
using Sandbox.Card;
using Sandbox._Startup;
namespace Sandbox.Components;

[SelectionBase, Tag( "Card" )]
public sealed class Card : Component
{
	// We store our immutable source of truth within our definition
	[Change("CardChange")] private CardDefinition? CardDefinition {get; set;} = null;
	// Derive some basics from our source
	[Property, ReadOnly] private string SourceCardName => CardDefinition?.Name ?? "Null Card";
	[Property, ReadOnly] private string SourceCardId => CardDefinition?.Id.ToString() ?? "Null Card";
	
	// Create a Instance of the card that initalizes with card definition then allows use to mutate it 
	private CardInstance? CardInstance { get; set; }
	
	private CardRenderer? _renderer;
	
	protected override Task OnLoad(LoadingContext context)
	{
		context.Title = "Loading Cards";
		//_renderer = Components.GetInChildren<CardRenderer>( includeDisabled: true );
		_renderer = GetOrAddComponent<CardRenderer>();
		return Task.CompletedTask;
	}
	
	private void CardChange( CardDefinition? old, CardDefinition? @new )
	{
		if (@new?.ImageUris.Large is null)
		{
			CardDefinition = old;
			Log.Error("Card Failed To Change");
		}
		else
		{
			CardDefinition = @new;
			_renderer?.Uri = @new.ImageUris.Large;
		}
	}
	
	[Button( "Set Random Card" )]
	public void SetRandomCard()
	{
		var cards = GlobalCatalogs.Cards;
		CardDefinition = cards.ById.GetRandomOrThrow();
		
	}
}
