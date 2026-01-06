using System.Threading.Tasks;
using Sandbox.Engine;

namespace Sandbox.Components;

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
	public new string Id { get; set; } = "";

	private ApplicationStartupSystem Startup => Scene.GetSystem<ApplicationStartupSystem>();

	private const float CardHeight = 512f;
	private const float CardAspectRatio = 63f  / 88f;
	private const float CardWidth = CardHeight * CardAspectRatio;
	private static readonly Vector2 CardSize = new( CardWidth, CardHeight );

	protected override void OnAwake()
	{
		PlaneCollider.Normal = Vector3.Forward;
		PlaneCollider.Scale = CardSize * UI.WorldPanel.ScreenToWorldScale;
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

	private void OnIdChanged( string _, string newValue )
	{
		// Change callbacks can fire in editor/early init; don't do anything unless startup finished.
		if ( Startup.StartupTask is not { IsCompletedSuccessfully: true } )
			return;

		if ( string.IsNullOrWhiteSpace( newValue ) )
			return;

		SetById( newValue );
	}

	private void SetById( string id )
	{
		var _ = Startup.Catalog.TryGetById( Guid.Parse(id), out ScryfallCard scryfallCard ); // throws if invalid (same as old)

		CardRenderer.Card = scryfallCard;

		Log.Info( $"[CardEntity] Set card to {scryfallCard!.Name} ({id})" );
	}

	[Button( "Random Card" )]
	public void SetRandomCard()
	{
		if ( Startup.StartupTask is not { IsCompletedSuccessfully: true } )
			return;

		var card = Startup.Catalog.GetRandomCard();

		Id = card.Id.ToString();
		CardRenderer.Card = card;

		Log.Info( $"[CardEntity] Random card: {card.Name} ({Id})" );
	}
}
