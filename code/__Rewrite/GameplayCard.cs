using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Sandbox.__Rewrite.Gameplay;

/// <summary>
/// Gameplay-only card data. All fields are typed for domain correctness —
/// no raw Scryfall strings, no stringly-typed color lists or stat values.
/// Stored in gameplay_cards.json keyed by oracle_id.
/// </summary>
public sealed record GameplayCard
{
    // ---- Identity ----
    [JsonPropertyName( "id" )]        public string     Id       { get; init; }
    [JsonPropertyName( "oracle_id" )] public string     OracleId { get; init; }
    [JsonPropertyName( "lang" )]      public string     Lang     { get; init; }
    [JsonPropertyName( "layout" )]    public CardLayout Layout   { get; init; }

    // ---- Core Gameplay ----
    [JsonPropertyName( "name" )]        public string   Name       { get; init; }
    [JsonPropertyName( "mana_cost" )]   public ManaCost ManaCost   { get; init; }   // ManaCost.IsNull when stored per-face
    [JsonPropertyName( "cmc" )]         public int      Cmc        { get; init; }   // int — CMC is always a whole number
    [JsonPropertyName( "type_line" )]   public string   TypeLine   { get; init; }
    [JsonPropertyName( "oracle_text" )] public string   OracleText { get; init; }

    // ---- Colors (flags enum — O(1) ops, zero heap allocation) ----
    [JsonPropertyName( "colors" )]          public ManaColor Colors         { get; init; }   // ManaColor.None on multi-face cards
    [JsonPropertyName( "color_identity" )]  public ManaColor ColorIdentity  { get; init; }
    [JsonPropertyName( "color_indicator" )] public ManaColor ColorIndicator { get; init; }   // DFC back-face indicator
    [JsonPropertyName( "produced_mana" )]   public ManaColor ProducedMana   { get; init; }   // Lands, mana rocks

    // ---- Combat Stats ----
    [JsonPropertyName( "power" )]     public CombatValue? Power     { get; init; }
    [JsonPropertyName( "toughness" )] public CombatValue? Toughness { get; init; }

    // ---- Loyalty / Defense ----
    [JsonPropertyName( "loyalty" )] public StartingValue? Loyalty { get; init; }   // Planeswalker
    [JsonPropertyName( "defense" )] public StartingValue? Defense { get; init; }   // Battle

    // ---- Format Legality ----
    [JsonPropertyName( "legalities" )] public Legalities Legalities { get; init; }

    // ---- Keywords ----
    [JsonPropertyName( "keywords" )] public IReadOnlyList<string> Keywords { get; init; }

    // ---- Vanguard Modifiers ----
    [JsonPropertyName( "hand_modifier" )] public string HandModifier { get; init; }
    [JsonPropertyName( "life_modifier" )] public string LifeModifier { get; init; }

    // ---- Flags ----
    [JsonPropertyName( "reserved" )]     public bool  Reserved    { get; init; }
    [JsonPropertyName( "game_changer" )] public bool? GameChanger { get; init; }

    // ---- Multi-Face ----
    [JsonPropertyName( "card_faces" )] public IReadOnlyList<GameplayCardFace>    CardFaces { get; init; }
    [JsonPropertyName( "all_parts" )]  public IReadOnlyList<GameplayRelatedCard> AllParts  { get; init; }

    // ---- Convenience Queries ----

    /// <summary>True for any layout with card_faces populated.</summary>
    public bool IsMultiFace => CardFaces is { Count: > 0 };

    /// <summary>True if the card is legal or restricted in the given format.</summary>
    public bool IsPlayableIn( Format format ) => Legalities.IsPlayableIn( format );
}

/// <summary>
/// Per-face data for double-faced, split, and adventure cards.
/// Mirrors <see cref="GameplayCard"/> for all fields that can vary by face.
/// </summary>
public sealed record GameplayCardFace
{
    [JsonPropertyName( "name" )]            public string         Name           { get; init; }
    [JsonPropertyName( "mana_cost" )]       public ManaCost       ManaCost       { get; init; }
    [JsonPropertyName( "cmc" )]             public int?           Cmc            { get; init; }
    [JsonPropertyName( "type_line" )]       public string         TypeLine       { get; init; }
    [JsonPropertyName( "oracle_text" )]     public string         OracleText     { get; init; }
    [JsonPropertyName( "colors" )]          public ManaColor      Colors         { get; init; }
    [JsonPropertyName( "color_indicator" )] public ManaColor      ColorIndicator { get; init; }
    [JsonPropertyName( "power" )]           public CombatValue?   Power          { get; init; }
    [JsonPropertyName( "toughness" )]       public CombatValue?   Toughness      { get; init; }
    [JsonPropertyName( "loyalty" )]         public StartingValue? Loyalty        { get; init; }
    [JsonPropertyName( "defense" )]         public StartingValue? Defense        { get; init; }
    [JsonPropertyName( "oracle_id" )]       public string         OracleId       { get; init; }
    [JsonPropertyName( "layout" )]          public CardLayout     Layout         { get; init; }
}

/// <summary>
/// Minimal related-card stub (token, meld part, combo piece).
/// Only carries what's needed to resolve the relationship at game time.
/// </summary>
public sealed record GameplayRelatedCard
{
    [JsonPropertyName( "id" )]        public string Id        { get; init; }
    [JsonPropertyName( "component" )] public string Component { get; init; }
    [JsonPropertyName( "name" )]      public string Name      { get; init; }
    [JsonPropertyName( "type_line" )] public string TypeLine  { get; init; }
}