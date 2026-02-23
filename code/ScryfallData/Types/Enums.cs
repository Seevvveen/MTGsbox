using System;

namespace Sandbox.ScryfallData.Types;

public enum MtgColor : byte
{
    White    = 0,
    Blue     = 1,
    Black    = 2,
    Red      = 3,
    Green    = 4,
    Colorless = 5
}

public enum MtgRarity : byte
{
    Common    = 0,
    Uncommon  = 1,
    Rare      = 2,
    Mythic    = 3,
    Special   = 4,
    Bonus     = 5
}

public enum MtgLayout : byte
{
    Normal         = 0,
    Split          = 1,
    Flip           = 2,
    Transform      = 3,
    ModalDfc       = 4,
    Meld           = 5,
    Leveler        = 6,
    Class          = 7,
    Case           = 8,
    Saga           = 9,
    Adventure      = 10,
    Mutate         = 11,
    Prototype       = 12,
    Battle         = 13,
    Planar         = 14,
    Scheme         = 15,
    Vanguard       = 16,
    Token          = 17,
    DoubleFacedToken = 18,
    Emblem         = 19,
    Augment        = 20,
    Host           = 21,
    ArtSeries      = 22,
    ReversibleCard = 23,
    Unknown        = 255
}

public enum MtgLegality : byte
{
    Legal      = 0,
    NotLegal   = 1,
    Restricted = 2,
    Banned     = 3
}

public enum MtgFormat : byte
{
    Standard       = 0,
    Pioneer        = 1,
    Modern         = 2,
    Legacy         = 3,
    Vintage        = 4,
    Commander      = 5,
    Oathbreaker    = 6,
    Brawl          = 7,
    HistoricBrawl  = 8,
    Alchemy        = 9,
    Historic       = 10,
    Explorer       = 11,
    Pauper         = 12,
    Penny          = 13,
    Gladiator      = 14,
    PauperCommander = 15,
    Predh          = 16,
    Premodern      = 17,
    Oldschool      = 18,
    Duel           = 19,
    Future         = 20
}

public enum MtgBorderColor : byte
{
    Black      = 0,
    White      = 1,
    Borderless = 2,
    Yellow     = 3,
    Silver     = 4,
    Gold       = 5
}

public enum MtgFinish : byte
{
    Nonfoil = 0,
    Foil    = 1,
    Etched  = 2
}

public enum MtgImageStatus : byte
{
    Missing      = 0,
    Placeholder  = 1,
    LowRes       = 2,
    HighResScan  = 3
}

public enum MtgSecurityStamp : byte
{
    None     = 0,
    Oval     = 1,
    Triangle = 2,
    Acorn    = 3,
    Circle   = 4,
    Arena    = 5,
    Heart    = 6
}

public enum MtgGame : byte
{
    Paper  = 0,
    Arena  = 1,
    Mtgo   = 2,
    Astral = 3,
    Sega   = 4
}

public enum RelatedCardComponent : byte
{
    Token      = 0,
    MeldPart   = 1,
    MeldResult = 2,
    ComboPiece = 3
}

// Stat represents power/toughness/loyalty/defense
// which can be numeric, variable (*), or modified (*+1, 1+*)
public enum StatType : byte
{
    Numeric   = 0,  // value is in StatValue
    Variable  = 1,  // *
    Combined  = 2,  // *+N or N+*
    X         = 3,  // X
    Any       = 4   // ∞ (reserved)
}

// Packed color identity / colors — up to 5 colors as bitflags
[Flags]
public enum ColorSet : byte
{
    None  = 0,
    White = 1 << 0,
    Blue  = 1 << 1,
    Black = 1 << 2,
    Red   = 1 << 3,
    Green = 1 << 4,
}