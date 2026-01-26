namespace Sandbox.Zones;

public enum ZoneType
{
	Null, // Outside of game
	Library,
	Hand,
	Battlefield,
	Graveyard,
	Stack,
	Exile,
	Command,
	Ante, // Legacy
}

public enum ZoneVisibility
{
	Public,
	Private
}
