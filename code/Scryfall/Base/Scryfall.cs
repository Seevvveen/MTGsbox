global using Sandbox.Scryfall.Types;
global using System;
global using System.Text.Json.Serialization;

namespace Sandbox.Scryfall;

public static class Endpoints
{
	public const string Base = "https://api.scryfall.com";
	public const string Sets = Base + "/sets/";
	public const string Cards = Base + "/cards/";
	//Symbols
	//Catalogs
	public const string BulkData = Base + "/bulk-data/";
}



