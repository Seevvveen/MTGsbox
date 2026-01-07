namespace Sandbox.Card;

/// <summary>
/// Holds Scryfall Image uris
/// </summary>
public sealed record CardImageUris()
{
	public Uri Small { get; init; }
	public Uri Normal { get; init; }
	public Uri Large { get; init; } 
	public Uri Png { get; init; }
	public Uri ArtCrop { get; init; }
	public Uri BorderCrop { get; init; }
}

