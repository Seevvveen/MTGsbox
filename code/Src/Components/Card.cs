#nullable enable
using System.Threading.Tasks;
using Sandbox.Card;
using Sandbox._Startup;

namespace Sandbox.Components;

[SelectionBase, Tag( "Card" )]
public sealed class Card : Component
{
	// Backing field that the [Change] hook observes.
	// Do NOT assign to the property inside CardChange; assign to the backing field only.
	[Change(nameof(OnDefinitionChanged))] private CardDefinition? _definition { get; set; }

	/// <summary>
	/// Immutable source of truth for what this card "is".
	/// Exposed read-only; use TrySetDefinition to change.
	/// </summary>
	public CardDefinition? Definition => _definition;

	[Property, ReadOnly] private string SourceCardName => _definition?.Name ?? "Null Card";
	[Property, ReadOnly] private string SourceCardId => _definition?.Id.ToString() ?? "Null Card";

	// Mutable gameplay state (counters, damage, etc.) derived from Definition
	private CardInstance? _instance;
	public CardInstance? Instance => _instance;

	public Sandbox.Zones.IZone? CurrentZone { get; set; }

	private CardRenderer? _renderer;

	protected override Task OnLoad( LoadingContext context )
	{
		context.Title = "Loading Cards";
		_renderer = GetOrAddComponent<CardRenderer>();
		return Task.CompletedTask;
	}

	/// <summary>
	/// Prefer calling this over assigning definition directly.
	/// Returns false if the definition is rejected.
	/// </summary>
	public bool TrySetDefinition( CardDefinition? def )
	{
		if ( def is null ) return false;
		if ( def.ImageUris.Large is null ) return false;

		_definition = def; // triggers OnDefinitionChanged
		return true;
	}

	private void OnDefinitionChanged( CardDefinition? oldValue, CardDefinition? newValue )
	{
		// If invalid, revert without re-triggering through the property.
		// Assigning the backing field is still "safe", but it will trigger again; guard by early-out.
		if ( newValue is null || newValue.ImageUris.Large is null )
		{
			Log.Error( "Card definition rejected (missing image uri)." );
			_definition = oldValue;
			return;
		}

		// Ensure renderer exists even if OnLoad didn't run yet for some reason
		_renderer ??= GetOrAddComponent<CardRenderer>();
		_renderer.Uri = newValue.ImageUris.Large;

		// (Optional) create/reset instance whenever definition changes
		// If your CardInstance constructor differs, adjust this line accordingly.
		//_instance = new CardInstance( newValue );
	}

	[Button( "Set Random Card" )]
	public void SetRandomCard()
	{
		var cards = GlobalCatalogs.Cards;
		var def = cards.ById.GetRandomOrThrow();

		if ( !TrySetDefinition( def ) )
			Log.Error( "Random card selection produced an invalid definition." );
	}
}
