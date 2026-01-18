#nullable enable
using System;

namespace Sandbox.Card;

public sealed class CardInstance
{
	public Guid DefinitionId { get; private set; }
	public Guid InstanceId { get; private set; }

	// Editor / serializer constructor (must be public & parameterless)
	public CardInstance()
	{
		DefinitionId = Guid.Empty; // unresolved placeholder
		InstanceId   = Guid.Empty; // editor object, not a real runtime instance
	}

	// Runtime constructor (validated)
	public CardInstance( Guid definitionId )
	{
		if ( definitionId == Guid.Empty )
			throw new ArgumentException( "DefinitionId cannot be empty.", nameof( definitionId ) );

		DefinitionId = definitionId;
		InstanceId   = Guid.NewGuid();
	}

	public static CardInstance FromDefinition( CardDefinition def )
	{
		if ( def is null )
			throw new ArgumentNullException( nameof( def ) );

		return new CardInstance( def.Id );
	}

	/// <summary>
	/// Optional helper to validate that this is a real runtime instance.
	/// </summary>
	public bool IsValid => DefinitionId != Guid.Empty && InstanceId != Guid.Empty;
}
