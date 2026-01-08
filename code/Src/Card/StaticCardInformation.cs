namespace Sandbox.Card;

public record StaticCardInformation
{
	public const float Height = 512f;
	public const float AspectRatio = 63f / 88f;
	public const float Width = Height    * AspectRatio;

	public static readonly Vector2 Size = new( Width, Height );
}
