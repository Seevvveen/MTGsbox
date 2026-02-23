using System;

namespace Sandbox.ScryfallData.Types;

// Packed legality across all formats — 2 bits per format
// Stored as a ulong (supports up to 32 formats)
public readonly struct LegalityMap(ulong packed)
{
    public ulong Packed => packed;

    private MtgLegality Get( MtgFormat format )
    {
        int shift = (int)format * 2;
        return (MtgLegality)(( packed >> shift ) & 0b11);
    }

    public bool IsLegal( MtgFormat format ) => Get( format ) == MtgLegality.Legal;

    public static LegalityMap Build( Dictionary<string, string> raw )
    {
        ulong packed = 0;
        foreach ( MtgFormat format in Enum.GetValues<MtgFormat>() )
        {
            string key = FormatToKey( format );
            if ( raw.TryGetValue( key, out string val ) )
            {
                MtgLegality legality = val switch
                {
                    "legal"      => MtgLegality.Legal,
                    "restricted" => MtgLegality.Restricted,
                    "banned"     => MtgLegality.Banned,
                    _            => MtgLegality.NotLegal
                };
                int shift = (int)format * 2;
                packed |= ( (ulong)legality << shift );
            }
        }
        return new LegalityMap( packed );
    }

    private static string FormatToKey( MtgFormat f ) => f switch
    {
        MtgFormat.Standard        => "standard",
        MtgFormat.Pioneer         => "pioneer",
        MtgFormat.Modern          => "modern",
        MtgFormat.Legacy          => "legacy",
        MtgFormat.Vintage         => "vintage",
        MtgFormat.Commander       => "commander",
        MtgFormat.Oathbreaker     => "oathbreaker",
        MtgFormat.Brawl           => "brawl",
        MtgFormat.HistoricBrawl   => "historicbrawl",
        MtgFormat.Alchemy         => "alchemy",
        MtgFormat.Historic        => "historic",
        MtgFormat.Explorer        => "explorer",
        MtgFormat.Pauper          => "pauper",
        MtgFormat.Penny           => "penny",
        MtgFormat.Gladiator       => "gladiator",
        MtgFormat.PauperCommander => "paupercommander",
        MtgFormat.Predh           => "predh",
        MtgFormat.Premodern       => "premodern",
        MtgFormat.Oldschool       => "oldschool",
        MtgFormat.Duel            => "duel",
        MtgFormat.Future          => "future",
        _                         => ""
    };
}