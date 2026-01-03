using Sandbox.Game.Enums;

namespace Sandbox.Game.Cards;

public record CardFace
{
	public string Artist;
	public Guid ArtistId;
	public float Cmc; //Make Property
	public float ConvertedManaCost => Cmc; //Alias
	public ColorIdentity ColorIndicator;
	public ColorIdentity Colors;
	public int Defense;
	public string FlavorText;
	public Guid IllustrationId;
	public ImageUris ImageUris;
	public CardLayout Layout;
	public CardLoyalty Loyalty;
	


}
