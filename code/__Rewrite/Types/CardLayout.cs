namespace Sandbox.__Rewrite.Gameplay;

/// <summary>
/// All known Scryfall card layout codes.
/// Replaces the raw string field so layout checks are exhaustive and refactor-safe.
/// </summary>
public enum CardLayout
{
    Unknown = 0,

    // Single-face
    Normal,
    Leveler,
    Class,
    Case,
    Saga,
    Mutate,
    Prototype,
    Token,
    Emblem,

    // Two-face (card_faces[0] and [1])
    Split,          // Fire // Ice — both faces visible
    Flip,           // Kamigawa flip cards
    Transform,      // Double-faced, day/night
    ModalDfc,       // Modal double-faced (e.g. Pathway lands)
    Adventure,      // Creature + Adventure instant/sorcery
    Planar,         // Plane/Phenomenon (Planechase)
    Scheme,         // Scheme (Archenemy)
    Vanguard,       // Vanguard

    // Meld
    Meld,           // Two cards meld into a third

    // Other
    DoubleFacedToken,
    ArtSeries,
    ReversibleCard, // No oracle_id — treated specially
    Host,
    Augment,
}

public static class CardLayoutParser
{
    public static CardLayout Parse( string raw ) => raw switch
    {
        "normal"               => CardLayout.Normal,
        "split"                => CardLayout.Split,
        "flip"                 => CardLayout.Flip,
        "transform"            => CardLayout.Transform,
        "modal_dfc"            => CardLayout.ModalDfc,
        "meld"                 => CardLayout.Meld,
        "leveler"              => CardLayout.Leveler,
        "class"                => CardLayout.Class,
        "case"                 => CardLayout.Case,
        "saga"                 => CardLayout.Saga,
        "adventure"            => CardLayout.Adventure,
        "mutate"               => CardLayout.Mutate,
        "prototype"            => CardLayout.Prototype,
        "planar"               => CardLayout.Planar,
        "scheme"               => CardLayout.Scheme,
        "vanguard"             => CardLayout.Vanguard,
        "token"                => CardLayout.Token,
        "double_faced_token"   => CardLayout.DoubleFacedToken,
        "emblem"               => CardLayout.Emblem,
        "augment"              => CardLayout.Augment,
        "host"                 => CardLayout.Host,
        "art_series"           => CardLayout.ArtSeries,
        "reversible_card"      => CardLayout.ReversibleCard,
        _                      => CardLayout.Unknown,
    };
}