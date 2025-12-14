using System.Threading.Tasks;

/// <summary>
/// Component that loads and caches a single Magic: The Gathering card by ID or name.
/// Depends on LocalCardIndexSystem being ready in the scene.
/// </summary>
public sealed class GameCardData : Component
{
	/// <summary>
	/// Defines how to look up the card.
	/// </summary>
	public enum LookupMode
	{
		/// <summary>Look up by card ID (GUID)</summary>
		ById,
		/// <summary>Look up by exact card name</summary>
		ByName,
		/// <summary>Look up by Oracle ID (GUID) - returns first printing</summary>
		ByOracleId
	}

	[Property, Title( "Lookup Mode" )]
	public LookupMode Mode { get; set; } = LookupMode.ById;

	[Property, Title( "Card Identifier" )]
	[Description( "Card ID (GUID), exact card name, or Oracle ID depending on Lookup Mode" )]
	public string CardIdentifier { get; set; }

	/// <summary>
	/// The loaded card data. Null if loading failed or hasn't completed yet.
	/// </summary>
	public Card Card { get; private set; }

	/// <summary>
	/// If lookup by Oracle ID or Name returned multiple cards, they're stored here.
	/// Otherwise null.
	/// </summary>
	public IReadOnlyList<Card> AllPrintings { get; private set; }

	private LocalCardIndexSystem _index;
	private readonly TaskCompletionSource<bool> _readyTcs = new();

	/// <summary>
	/// Task that completes when the card has been loaded (or failed to load).
	/// Result is true if card was successfully loaded, false otherwise.
	/// </summary>
	public Task<bool> WhenReady => _readyTcs.Task;

	/// <summary>
	/// True if the card was successfully loaded and is available.
	/// </summary>
	public bool IsReady { get; private set; }

	protected override async Task OnLoad()
	{
		try
		{
			Log.Info( $"[GameCardData] Loading card data for '{CardIdentifier}' (mode: {Mode})..." );

			// Validate input
			if ( string.IsNullOrWhiteSpace( CardIdentifier ) )
			{
				Log.Error( "[GameCardData] CardIdentifier is empty. Cannot load card data." );
				IsReady = false;
				_readyTcs.TrySetResult( false );
				return;
			}

			// Get index system
			_index = Scene.GetSystem<LocalCardIndexSystem>();
			if ( _index == null )
			{
				Log.Error( "[GameCardData] LocalCardIndexSystem is missing from the Scene. Cannot load card data." );
				IsReady = false;
				_readyTcs.TrySetResult( false );
				return;
			}

			// Wait for indexes to be ready
			Log.Info( "[GameCardData] Waiting for LocalCardIndexSystem to be ready..." );
			await _index.WhenReady;

			if ( !_index.IsReady )
			{
				Log.Error( "[GameCardData] LocalCardIndexSystem finished but IsReady is false (no indexes available)." );
				IsReady = false;
				_readyTcs.TrySetResult( false );
				return;
			}

			// Look up card based on mode
			bool found = Mode switch
			{
				LookupMode.ById => LookupById(),
				LookupMode.ByName => LookupByName(),
				LookupMode.ByOracleId => LookupByOracleId(),
				_ => throw new InvalidOperationException( $"Unknown lookup mode: {Mode}" )
			};

			if ( !found )
			{
				Log.Error( $"[GameCardData] Card not found: '{CardIdentifier}' (mode: {Mode})" );
				IsReady = false;
				_readyTcs.TrySetResult( false );
				return;
			}

			// Validate we got a card
			if ( Card == null )
			{
				Log.Error( $"[GameCardData] Lookup succeeded but Card is null for '{CardIdentifier}'" );
				IsReady = false;
				_readyTcs.TrySetResult( false );
				return;
			}

			// Log success with card details
			LogCardDetails();

			IsReady = true;
			_readyTcs.TrySetResult( true );
			Log.Info( $"[GameCardData] Successfully loaded card: {Card.Name}" );
		}
		catch ( Exception ex )
		{
			Log.Error( ex, $"[GameCardData] Exception while loading card '{CardIdentifier}'" );
			IsReady = false;
			_readyTcs.TrySetResult( false );
		}
	}

	/// <summary>
	/// Look up card by ID (GUID). Searches both Oracle and Default indexes.
	/// </summary>
	private bool LookupById()
	{
		// Use the new helper method that searches both indexes
		if ( _index.TryFindCard( CardIdentifier, out var card ) )
		{
			Card = card;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Look up card by exact name. Returns first match if multiple printings exist.
	/// </summary>
	private bool LookupByName()
	{
		if ( _index.TryFindByName( CardIdentifier, out var cards ) )
		{
			if ( cards.Count == 0 )
			{
				Log.Warning( $"[GameCardData] Name lookup returned empty list for '{CardIdentifier}'" );
				return false;
			}

			// Store all printings
			AllPrintings = cards;

			// Use first printing as the primary card
			Card = cards[0];

			if ( cards.Count > 1 )
			{
				Log.Info( $"[GameCardData] Found {cards.Count} printings of '{CardIdentifier}'. Using first: {Card.SetName} ({Card.Set})" );
			}

			return true;
		}

		return false;
	}

	/// <summary>
	/// Look up card by Oracle ID. Returns first printing if multiple exist.
	/// </summary>
	private bool LookupByOracleId()
	{
		// Validate it's a valid GUID
		if ( !Guid.TryParse( CardIdentifier, out var oracleId ) )
		{
			Log.Error( $"[GameCardData] '{CardIdentifier}' is not a valid GUID for Oracle ID lookup" );
			return false;
		}

		if ( _index.TryFindByOracleId( oracleId, out var cards ) )
		{
			if ( cards.Count == 0 )
			{
				Log.Warning( $"[GameCardData] Oracle ID lookup returned empty list for '{CardIdentifier}'" );
				return false;
			}

			// Store all printings
			AllPrintings = cards;

			// Use first printing as the primary card
			Card = cards[0];

			if ( cards.Count > 1 )
			{
				Log.Info( $"[GameCardData] Found {cards.Count} printings for Oracle ID '{CardIdentifier}'. Using: {Card.SetName} ({Card.Set})" );
			}

			return true;
		}

		return false;
	}

	/// <summary>
	/// Log detailed information about the loaded card for debugging.
	/// </summary>
	private void LogCardDetails()
	{
		if ( Card == null ) return;

		Log.Info( $"[GameCardData] Card Details:" );
		Log.Info( $"  Name: {Card.Name}" );
		Log.Info( $"  ID: {Card.Id}" );

		if ( Card.OracleId != Guid.Empty )
		{
			Log.Info( $"  Oracle ID: {Card.OracleId}" );
		}

		if ( !string.IsNullOrWhiteSpace( Card.Set ) )
		{
			Log.Info( $"  Set: {Card.SetName} ({Card.Set})" );
		}

		if ( !string.IsNullOrWhiteSpace( Card.TypeLine ) )
		{
			Log.Info( $"  Type: {Card.TypeLine}" );
		}

		if ( AllPrintings != null && AllPrintings.Count > 1 )
		{
			Log.Info( $"  Available Printings: {AllPrintings.Count}" );
		}
	}

	/// <summary>
	/// Reload the card data. Useful if you've changed the CardIdentifier or Mode at runtime.
	/// </summary>
	public async Task<bool> ReloadAsync()
	{
		// Reset state
		Card = null;
		AllPrintings = null;
		IsReady = false;

		// Create new TCS for the reload
		var reloadTcs = new TaskCompletionSource<bool>();

		try
		{
			await OnLoad();
			return IsReady;
		}
		catch ( Exception ex )
		{
			Log.Error( ex, "[GameCardData] Exception during reload" );
			return false;
		}
	}

	/// <summary>
	/// Get a specific printing from AllPrintings by set code.
	/// Only works if the card was looked up by name or Oracle ID.
	/// </summary>
	public Card GetPrintingFromSet( string setCode )
	{
		if ( AllPrintings == null || AllPrintings.Count == 0 )
		{
			Log.Warning( "[GameCardData] No printings available. Card may have been looked up by ID." );
			return null;
		}

		if ( string.IsNullOrWhiteSpace( setCode ) )
		{
			Log.Warning( "[GameCardData] Set code is empty" );
			return null;
		}

		var printing = AllPrintings.FirstOrDefault( c =>
			string.Equals( c.Set, setCode, StringComparison.OrdinalIgnoreCase )
		);

		if ( printing == null )
		{
			Log.Warning( $"[GameCardData] No printing found in set '{setCode}' for {Card?.Name ?? CardIdentifier}" );
		}

		return printing;
	}
}
