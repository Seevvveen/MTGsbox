#nullable enable

using Sandbox.Components;
using CardComp = Sandbox.Components.Card;




namespace Sandbox.Zones;

public abstract class Zone : Component, IZone
{
	[Property] public Guid Owner { get; set; }

	// Keep track of cards in this zone
	public List<GameObject> Cards { get; protected set; } = new();

	// Optional: if this gets hot, keep a HashSet for O(1) membership checks.
	// private readonly HashSet<GameObject> _cardSet = new();

	//
	// IZone Implementation
	//

	public virtual bool CanAdd( GameObject card )
	{
		if ( !card.IsValid() ) return false;

		// Prevent duplicates
		if ( Cards.Contains( card ) ) return false;

		return true;
	}

	public virtual bool TryAdd( GameObject card )
	{
		if ( !CanAdd( card ) ) return false;

		// Use ONE Card type consistently
		var cardComp = card.GetComponent<CardComp>();
		if ( cardComp is null ) return false;

		// If card is in another zone, remove it first (transactional transfer)
		var from = cardComp.CurrentZone;
		if ( from is not null && from != this )
		{
			// If removal fails, do not partially add here
			if ( !from.TryRemove( card ) )
				return false;
		}

		// Now add locally
		Cards.Add( card );
		// _cardSet.Add(card);

		cardComp.CurrentZone = this;

		// Parent for organization and (if you lay out in local-space) correctness
		card.SetParent( GameObject );

		OnCardAdded( card );
		return true;
	}

	public virtual bool CanRemove( GameObject card )
	{
		if ( !card.IsValid() ) return false;
		return Cards.Contains( card );
	}

	public virtual bool TryRemove( GameObject card )
	{
		if ( !CanRemove( card ) ) return false;

		// Remove locally first
		Cards.Remove( card );
		// _cardSet.Remove(card);

		var cardComp = card.GetComponent<CardComp>();
		if ( cardComp is not null && cardComp.CurrentZone == this )
			cardComp.CurrentZone = null;

		// Optional: detach so it doesn't keep inheriting zone transforms.
		// If you have a known "table root", parent there instead.
		card.SetParent( null );

		OnCardRemoved( card );
		return true;
	}

	//
	// Virtual Hooks for subclasses (Layout, Effects, etc.)
	//
	protected virtual void OnCardAdded( GameObject card ) { }
	protected virtual void OnCardRemoved( GameObject card ) { }
}
