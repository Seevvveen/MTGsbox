using Sandbox.Game.Enums;

namespace Sandbox.Game.Cards;

public class FullCardDefinition
{
	//Core Card Fields
	public int ArenaId;
	public Guid Id;
	public string Language;
	public int MtgoId;
	public int MtgoFoilId;
	public List<int> MultiverseIds;
	public string ResourceId;
	public int TcgplayerId;
	public int TcgplayerEtchedId;
	public int CardMarketId;
	public string Object;
	public string Layout;
	public Guid OracleId;
	public Uri PrintsSearchUri;
	public Uri RulingsUri;
	public Uri ScryfallUri;
	public Uri Uri;
	
	//Gameplay Fields
	public List<RelatedCard> AllParts;
	public List<CardFace> CardFaces;
	public float Cmc;
	public float ConvertedManaCost => Cmc; //Alias
	public ColorIdentity ColorIdentity;
	public ColorIdentity ColorIndicator; //TODO slightly differnt to ColorIdentity, Maybe Refactor?
	public ColorIdentity Colors; //TODO again may need more specific color types
	public int Defense;
	public int EdhRecRank;
	public bool GameChanger;
	public string HandModifer; //TODO Need Types?
	public List<string> Keywords; //TODO Keyword System
	public Legality Legality; //TODO inspect
	public string LifeModifer; //TODO need types?
	public Loyalty Loyalty;
	public ManaCost ManaCost;
	public string Name;
	public string OracleText;
	public int PennyRank;
	public Power Power;
	public ColorIdentity ProducedMana; //TODO Validate Type
	public bool ReservedList; //Reserverd
	public Toughness Toughness;
	public string TypeLine; //TODO

	//PrintFields
	public string Artist;
	public List<Guid> ArtistIds;
	public List<int> AttractionLights;
	public bool Booster;
	public BorderColor BorderColor;
	public Guid CardBackId;
	public string CollectorNumber;
	public bool ContentWarning;
	public bool Digital;
	public List<string> CardFinish; //TODO Enum Made with bit flags just needs parser
	public string FlavorName;
	public string FlavorText;
	public FrameEffect FrameEffects; //TODO Type inspection
	public string Frame; //TODO Type
	public bool FullArt;
	public List<string> Games; //TODO
	public bool HighResImage;
	public Guid IllustrationId;
	public string ImageStatus; //TODO Type
	public ImageUris ImageUris;
	public bool Oversized;
	public List<string> Prices; //TODO TYPE
	public string PrintedName;
	public string PrintedText;
	public string PrintedTypeLine;
	public bool Promo;
	public List<string> PromoTypes;   //TODO Type?
	public List<string> PurchaseUris; //TODO Type
	public string Rarity;             //TODO Type
	public List<string> RelatedUris; //TODO TYPE
	public string ReleasedAt; //TODO Type?
	public bool Reprint;
	public string ScryfallSetUri;
	public string SetName;
	public string SetSearchUri;
	public string SetType;
	public string SetUri;
	public string Set;
	public Guid SetId;
	public bool StorySpotlight;
	public bool Textless;
	public bool Variation;
	public Guid VariationOf;
	public string SecurityStamp; //TODO Enum
	public string Watermark;
	public string PreviewAtDate; //TODO Type?
	public string PreviewSourceUri;
	public string PreviewSource;
}


