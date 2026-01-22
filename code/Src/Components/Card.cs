#nullable enable
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Card;
using Sandbox._Startup;
namespace Sandbox.Components;

[SelectionBase, Tag( "Card" )]
public sealed class Card : Component
{
	[Property, ReadOnly] private string CardName => CardDefinition?.Name ?? "Null Card";
	[Property, ReadOnly] private string CardId => CardDefinition?.Id.ToString() ?? "Null Card";
	[Change("CardChange")] private CardDefinition? CardDefinition {get; set;} = null;
	
	private CardInstance? CardInstance { get; set; }
	private CardRenderer? _renderer;
	
	protected override Task OnLoad(LoadingContext context)
	{
		context.Title = "Loading Cards";
		_renderer = Components.GetInChildren<CardRenderer>( includeDisabled: true );
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
