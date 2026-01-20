#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Card;
using Sandbox._Startup;
namespace Sandbox.Components;

[SelectionBase, Tag( "Card" )]
public sealed class Card : Component
{
	/// <summary>
	/// Runtime identity/state for this card in the game (zone, owner, counters, etc.).
	/// Keep this separate from CardDefinition (static data).
	/// </summary>
	[Property] public CardInstance? Instance { get; set; }

	/// <summary>
	/// Editor-friendly string form of the definition Guid.
	/// Uses [Property, Change] so edits and prefab overrides trigger refresh.
	/// </summary>
	[Property, Change( nameof( OnCardIdChanged ) )]
	public required string CardId { get; set; }

	private CardDefinition? _def;
	private CardRenderer? _renderer;

	// If catalogs aren't ready at load time, we keep the desired id here and retry.
	private Guid _pendingDefinitionId;
	private bool _hasPendingDefinition;
	private bool _warnedCatalogNotReady;
	
	protected override async Task OnLoad()
	{
		_renderer = Components.GetInChildren<CardRenderer>( includeDisabled: true );

		if ( Instance is not null && Instance.DefinitionId != Guid.Empty )
		{
			SetPendingDefinitionId( Instance.DefinitionId );
		}
		else if ( !string.IsNullOrWhiteSpace( CardId ) && Guid.TryParse( CardId, out var id ) )
		{
			SetPendingDefinitionId( id );
		}
		else
		{
			ClearVisuals();
			return;
		}

		// Hard gate: wait until catalogs are ready before resolving.
		await StaticDataManager.EnsureReadyAsync();

		TryResolvePendingDefinition();
	}


	protected override void OnUpdate()
	{
		if ( _hasPendingDefinition )
			TryResolvePendingDefinition();
	}

	/// <summary>
	/// Public API for gameplay systems: set the runtime instance and refresh visuals.
	/// </summary>
	public void SetInstance( CardInstance instance )
	{
		Instance = instance ?? throw new ArgumentNullException( nameof( instance ) );

		if ( instance.DefinitionId == Guid.Empty )
		{
			_def = null;
			_hasPendingDefinition = false;
			ClearVisuals();
			return;
		}

		// Keep editor property in sync for debugging/inspection.
		CardId = instance.DefinitionId.ToString();

		SetPendingDefinitionId( instance.DefinitionId );
		TryResolvePendingDefinition();
	}

	/// <summary>
	/// Public API for editor/debug usage: set by definition id.
	/// </summary>
	public void SetDefinitionId( Guid id )
	{
		CardId = id.ToString();
		SetPendingDefinitionId( id );
		TryResolvePendingDefinition();
	}

	private void OnCardIdChanged( string? oldId, string? newId )
	{
		if ( string.Equals( oldId, newId, StringComparison.Ordinal ) )
			return;

		if ( string.IsNullOrWhiteSpace( newId ) )
		{
			_def = null;
			_hasPendingDefinition = false;
			ClearVisuals();
			return;
		}

		if ( Guid.TryParse( newId, out var id ) )
		{
			SetPendingDefinitionId( id );
			TryResolvePendingDefinition();
		}
		else
		{
			// Invalid GUID entered in editor; do not spam errors at runtime.
			_def = null;
			_hasPendingDefinition = false;
			ClearVisuals();
		}
	}

	private void SetPendingDefinitionId( Guid id )
	{
		_pendingDefinitionId = id;
		_hasPendingDefinition = true;
		_warnedCatalogNotReady = false;
	}

	private void TryResolvePendingDefinition()
	{
		var cards = GlobalCatalogs.Cards;

		if ( !cards.IsReady )
		{
			if ( !_warnedCatalogNotReady )
			{
				_warnedCatalogNotReady = true;
				Log.Info( "Card: Cards catalog not ready yet; will retry." );
			}

			return;
		}


		if ( !cards.ById.TryGet( _pendingDefinitionId, out var def ) )
		{
			Log.Warning( $"Card: definition not found in catalog ById: {_pendingDefinitionId}" );
			_def = null;
			_hasPendingDefinition = false;
			ClearVisuals();
			return;
		}

		_def = def;
		_hasPendingDefinition = false;
		ApplyVisualsFromDefinition( def );
	}

	private void ApplyVisualsFromDefinition( CardDefinition def )
	{
		if ( _renderer is null )
			return;

		// Adjust this if your CardDefinition image shape differs.
		var uri = def.ImageUris?.Large;
		if ( uri is not null )
			_renderer.SetImage( uri );
		else
			ClearVisuals();
	}

	private void ClearVisuals()
	{
		// Implement a "Clear" method on CardRenderer if you want to blank the panel.
		// _renderer?.Clear();
	}

	[Button( "Set Random Card" )]
	private void SetRandomCard()
	{
		var cards = GlobalCatalogs.Cards;
		if ( !cards.IsReady || cards.Count <= 0 )
			return;


		var random = cards.ById.GetRandomOrThrow();
		SetDefinitionId( random.Id );
	}
}
