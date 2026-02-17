#nullable enable
using System.Threading.Tasks;
using Sandbox.Card;
using Sandbox._Startup;

namespace Sandbox.Components;

[SelectionBase, Tag( "Card" )]
public sealed class CardOLD : Component
{
	// Networked stable key (host-authoritative)
	[Sync( SyncFlags.FromHost ), Change( nameof( OnCardIdChanged ) )]
	public required Guid CardId { get; set; }
	
	[Property,InlineEditor] public CardDefinition? Definition { get; set; }

	private CardRenderer? _renderer;

	protected override Task OnLoad( LoadingContext context )
	{
		context.Title = "Loading Cards";
		_renderer = GetComponentInChildren<CardRenderer>(true);
		return Task.CompletedTask;
	}

	public bool TrySetDefinition( CardDefinition? def )
	{
		if ( def is null ) return false;
		if ( def.ImageUris.Large is null ) return false;
		if ( !Networking.IsHost ) return false;

		Definition = def;
		CardId = def.Id;
		return true;
	}

	private void OnCardIdChanged( Guid oldValue, Guid newValue )
	{
		_renderer ??= GetComponentInChildren<CardRenderer>(true);

		if ( newValue == Guid.Empty )
		{
			Definition = null;
			_renderer.Uri = null;
			return;
		}

		// Resolve locally from your catalog (keyed by Guid)
		if ( GlobalCatalogs.Cards.ById.TryGet( newValue, out var def )
		     && def.ImageUris.Large is { } uri )
		{
			Definition = def;
			_renderer.Uri = uri;
			return;
		}

		Definition = null;
		_renderer.Uri = null;
	}

	[Button( "Set Random Card" )]
	public void SetRandomCard()
	{
		if ( !Networking.IsHost ) return;

		var cards = GlobalCatalogs.Cards;
		var def = cards.ById.GetRandomOrThrow();

		if ( !TrySetDefinition( def ) )
			Log.Error( "Random card selection produced an invalid definition." );
	}
}
