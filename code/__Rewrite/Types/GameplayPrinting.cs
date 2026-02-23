using System;

namespace Sandbox.__Rewrite.Types;

public sealed class GameplayPrinting
{
    // Links
    public Guid  ScryfallId { get; init; }
    public Guid? OracleId   { get; init; }

    // Set info
    public string Set             { get; init; }
    public Guid   SetId           { get; init; }
    public string SetName         { get; init; }
    public string CollectorNumber { get; init; }
    public string ReleasedAt      { get; init; }

    // Per-printing rules variance
    public MtgRarity       Rarity        { get; init; }
    public MtgBorderColor  BorderColor   { get; init; }
    public MtgImageStatus  ImageStatus   { get; init; }
    public MtgSecurityStamp SecurityStamp { get; init; }
    public List<MtgFinish> Finishes      { get; init; }
    public List<MtgGame>   Games         { get; init; }

    // Art at card level (single-faced)
    public Dictionary<string, string> ImageUris { get; init; }

    // Art per face (DFCs, split, etc.) — index matches GameplayCard.Faces
    public List<GameplayFaceArt> FaceArt { get; init; }

    // Flags
    public bool IsPromo           { get; init; }
    public bool IsReprint         { get; init; }
    public bool IsFullArt         { get; init; }
    public bool IsOversized       { get; init; }
    public bool IsTextless        { get; init; }
    public bool IsStorySpotlight  { get; init; }
    public bool IsBooster         { get; init; }
    public bool HasContentWarning { get; init; }
    public bool HasHighResImage   { get; init; }
    public bool IsDigital         { get; init; }
    public bool IsVariation       { get; init; }
    public List<string> PromoTypes { get; init; }

    // External IDs
    public int?  ArenaId           { get; init; }
    public int?  MtgoId            { get; init; }
    public int?  TcgPlayerId       { get; init; }
    public int?  CardmarketId      { get; init; }
    public Guid  CardBackId        { get; init; }
    public Guid? VariationOf       { get; init; }

    // Prices — string because Scryfall returns nullable decimal strings
    public string PriceUsd        { get; init; }
    public string PriceUsdFoil    { get; init; }
    public string PriceUsdEtched  { get; init; }
    public string PriceEur        { get; init; }
    public string PriceEurFoil    { get; init; }
    public string PriceTix        { get; init; }
}

public sealed class GameplayFaceArt
{
    public string Artist         { get; init; }
    public Guid?  ArtistId       { get; init; }
    public Guid?  IllustrationId { get; init; }
    public string FlavorName     { get; init; }
    public string FlavorText     { get; init; }
    public string Watermark      { get; init; }
    public Dictionary<string, string> ImageUris { get; init; }
}