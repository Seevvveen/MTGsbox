using System.Threading.Tasks;
using Sandbox;
using Sandbox.Engine;
using Sandbox.Game.Instances;

namespace Sandbox.Game.Cards;

/// <summary>
/// Debug card entity that waits for GameStartupSystem to finish (bulk sync + index build)
/// before trying to fetch/render cards.
/// </summary>
public sealed class CardEntity : Component, Component.ExecuteInEditor
{
	[Property, RequireComponent] public CardRenderer CardRenderer { get; private set; } = null!;
	[Property, RequireComponent] public PlaneCollider PlaneCollider { get; private set; } = null!;

	// Pick a specific Scryfall card id (printing id) or leave empty for random.
	[Property, Change( nameof( OnIdChanged ) )]
	public string Id { get; set; } = "";

	[Property, ReadOnly] public CardInstance? Instance { get; private set; }

	private GameStartupSystem Startup => Scene.GetSystem<GameStartupSystem>();

	public const float CardHeight = 512f;
	public const float CardAspectRatio = 63f / 88f;
	public static readonly float CardWidth = CardHeight * CardAspectRatio;
	public static readonly Vector2 CardSize = new( CardWidth, CardHeight );

	protected override void OnAwake()
	{
		PlaneCollider.Normal = Vector3.Forward;
		PlaneCollider.Scale = CardSize * Sandbox.UI.WorldPanel.ScreenToWorldScale;
		CardRenderer.WorldPanel.PanelSize = CardSize;
	}

	protected override async Task OnLoad()
	{
		await WaitForStartupAsync();

		if ( !string.IsNullOrWhiteSpace( Id ) )
			SetById( Id );
		else
			SetRandomCard();
	}

	private async Task WaitForStartupAsync()
	{
		// StartupTask is set during OnHostPreInitialize; OnLoad might run before that.
		while ( Startup.StartupTask == null )
			await Task.Yield();

		await Startup.StartupTask; // If startup failed, this will throw (good for debugging).
	}

	private void OnIdChanged( string oldValue, string newValue )
	{
		// Change callbacks can fire in editor/early init; don't do anything unless startup finished.
		if ( Startup.StartupTask == null || !Startup.StartupTask.IsCompletedSuccessfully )
			return;

		if ( string.IsNullOrWhiteSpace( newValue ) )
			return;

		SetById( newValue );
	}

	private void SetById( string id )
	{
		// TODO: Replace these with your real CardCatalog API names.
		// The important change is: we now read from Startup.Catalog (which is populated after startup).
		var card = Startup.Catalog.TryGetById( Guid.Parse(id), out ScryfallCard scryfallCard ); // throws if invalid (same as old)

		CardRenderer.Card = scryfallCard;

		// TODO: when you split rules/print definitions, build a real CardInstance here.
		Instance = null;

		Log.Info( $"[CardEntity] Set card to {scryfallCard.Name} ({id})" );
	}

	[Button( "Random Card" )]
	public void SetRandomCard()
	{
		if ( Startup.StartupTask == null || !Startup.StartupTask.IsCompletedSuccessfully )
			return;

		var card = Startup.Catalog.GetRandomCard();

		Id = card.Id.ToString();
		CardRenderer.Card = card;

		Instance = null;

		Log.Info( $"[CardEntity] Random card: {card.Name} ({Id})" );
	}
}
