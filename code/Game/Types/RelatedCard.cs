using Sandbox.Game.Enums;

namespace Sandbox.Game.Cards;

/// <summary>
/// Cards that are closely related to other cards (because they call them by name, or generate a token, or meld, etc)
/// Found in a cards all_parts array
/// </summary>
public record RelatedCard
{
	public Guid Id;
	public string Object;
	public RelatedCardComponent Component;
	public string Name;
	public string Type;
	public Uri Uri;
}
