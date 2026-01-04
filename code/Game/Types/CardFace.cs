using Sandbox.Game.Cards;
using Sandbox.Game.Enums;
using Sandbox.Game.Enums.Cards;

namespace Sandbox.Game.Types;

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
	public Layout Layout;
	public Loyalty Loyalty;
	public ManaCost ManaCost;
	public string Name;
	public string Object;
	public Guid OracleId;
	public string OracleText;
	public Power Power;
	public string PrintedName;     //Localized
	public string PrintedText;     //Localized
	public string PrintedTypeLine; //Localized
	public Toughness Toughness;
	public string TypeLine; //TODO actual type system - Main Types Stable, SubTypes Dynamics
}
