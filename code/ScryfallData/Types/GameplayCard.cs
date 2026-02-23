using System;

namespace Sandbox.ScryfallData.Types;

public sealed class GameplayCard
{
    // Identity
    public Guid       ScryfallId    { get; init; }
    public Guid?      OracleId      { get; init; }
    public string     Name          { get; init; }
    public MtgLayout  Layout        { get; init; }

    // Faces — oracle text only, no art/flavor
    public List<GameplayFace> Faces { get; init; }

    // Colors
    public ColorSet ColorIdentity   { get; init; }
    public ColorSet ProducedMana    { get; init; }

    // Mana / CMC
    public string              ManaCostRaw { get; init; }
    public List<ManaCostSymbol> ManaCost   { get; init; }
    public float               Cmc         { get; init; }

    // Types
    public string       TypeLine   { get; init; }
    public List<string> Supertypes { get; init; }
    public List<string> CardTypes  { get; init; }
    public List<string> Subtypes   { get; init; }

    // Rules
    public string       OracleText { get; init; }
    public List<string> Keywords   { get; init; }

    // Stats
    public CardStat? Power     { get; init; }
    public CardStat? Toughness { get; init; }
    public CardStat? Loyalty   { get; init; }
    public CardStat? Defense   { get; init; }

    // Vanguard
    public string HandModifier { get; init; }
    public string LifeModifier { get; init; }

    // Legality
    public LegalityMap Legalities { get; init; }

    // Relationships
    public List<RelatedCard> RelatedCards { get; init; }

    // Flags that are oracle-level (not per-printing)
    public bool IsReserved    { get; init; }
    public bool IsGameChanger { get; init; }

    // Ranks
    public int? EdhrecRank { get; init; }
    public int? PennyRank  { get; init; }
}