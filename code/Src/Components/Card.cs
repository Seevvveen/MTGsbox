#nullable enable
using System.Threading.Tasks;
using Sandbox.Card;
using Sandbox._Startup;

namespace Sandbox.Components;

[SelectionBase, Tag( "Card" )]
public sealed class Card : Component
{
	// Networked stable key (host-authoritative)
	[Sync( SyncFlags.FromHost ), Change( nameof( OnCardIdChanged ) )]
	public required Guid CardId { get; set; } = Guid.Empty;

	
	// Local-only resolved definition (do not Sync this)
	private CardDefinition? _definition;
	public CardDefinition? Definition => _definition;

	[Property, ReadOnly] private string SourceCardName => _definition?.Name ?? "Null Card";
	[Property, ReadOnly] private string SourceCardId => _definition?.Id.ToString() ?? "Null Card";

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

		// FromHost => only host can write the synced value
		if ( !Networking.IsHost ) return false;

		CardId = def.Id; // <-- the synced assignment
		return true;
	}

	private void OnCardIdChanged( Guid oldValue, Guid newValue )
	{
		_renderer ??= GetComponentInChildren<CardRenderer>(true);

		if ( newValue == Guid.Empty )
		{
			_definition = null;
			_renderer.Uri = null;
			return;
		}

		// Resolve locally from your catalog (keyed by Guid)
		if ( GlobalCatalogs.Cards.ById.TryGet( newValue, out var def )
		     && def.ImageUris.Large is { } uri )
		{
			_definition = def;
			_renderer.Uri = uri;
			return;
		}

		// Catalog not ready / missing entry: blank (or set a card-back uri if you want)
		_definition = null;
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
