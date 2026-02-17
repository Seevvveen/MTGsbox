namespace Sandbox.Components.Card.Data;

// The universal handle for resolving any given card
public readonly record struct CardReference
{
	public Guid ObjectId {get; init;}
	public Guid DefinitionId {get; init;}
	
	public bool IsValid => ObjectId != Guid.Empty &&  DefinitionId != Guid.Empty;
}