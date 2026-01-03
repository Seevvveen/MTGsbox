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
	

}


