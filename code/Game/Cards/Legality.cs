using Sandbox.Game.Enums;

namespace Sandbox.Game.Cards;

public class Legality
{
    private readonly Dictionary<Format, LegalStatus> _legalities;
    
    public Legality(Dictionary<Format, LegalStatus> legalities)
    {
        _legalities = legalities ?? new Dictionary<Format, LegalStatus>();
    }
    
    public static Legality Parse(Dictionary<string, string> legalitiesJson)
    {
        var legalities = new Dictionary<Format, LegalStatus>();
        
        if (legalitiesJson == null)
            return new Legality(legalities);
        
        foreach (var kvp in legalitiesJson)
        {
            if (TryParseFormat(kvp.Key, out var format) && 
                TryParseLegality(kvp.Value, out var legality))
            {
                legalities[format] = legality;
            }
        }
        
        return new Legality(legalities);
    }
    
    private static bool TryParseFormat(string formatString, out Format format)
    {
        // Handle special cases where JSON name differs from enum
        var normalized = formatString.ToLower() switch
        {
            "standardbrawl" => "StandardBrawl",
            "paupercommander" => "PauperCommander",
            "oldschool" => "OldSchool",
            "premodern" => "PreModern",
            "predh" => "PreDH",
            _ => formatString
        };
        
        return Enum.TryParse(normalized, true, out format);
    }
    
    private static bool TryParseLegality(string legalityString, out LegalStatus legality)
    {
        legality = legalityString?.ToLower() switch
        {
            "legal" => LegalStatus.Legal,
            "not_legal" => LegalStatus.NotLegal,
            "restricted" => LegalStatus.Restricted,
            "banned" => LegalStatus.Banned,
            _ => LegalStatus.NotLegal
        };
        return true;
    }
    
    // Core query methods
    public LegalStatus GetLegality(Format format)
    {
        return _legalities.TryGetValue(format, out var legality) 
            ? legality 
            : LegalStatus.NotLegal;
    }
    
    public bool IsLegal(Format format) => GetLegality(format)      == LegalStatus.Legal;
    public bool IsRestricted(Format format) => GetLegality(format) == LegalStatus.Restricted;
    public bool IsBanned(Format format) => GetLegality(format)     == LegalStatus.Banned;
    public bool IsNotLegal(Format format) => GetLegality(format)   == LegalStatus.NotLegal;
    
    // Playable check (legal or restricted, but not banned)
    public bool IsPlayable(Format format)
    {
        var legality = GetLegality(format);
        return legality == LegalStatus.Legal || legality == LegalStatus.Restricted;
    }
    
    // Get all formats where card is legal
    public List<Format> GetLegalFormats()
    {
        return _legalities
            .Where(kvp => kvp.Value == LegalStatus.Legal)
            .Select(kvp => kvp.Key)
            .ToList();
    }
    
    // Get all formats where card is playable (legal or restricted)
    public List<Format> GetPlayableFormats()
    {
        return _legalities
            .Where(kvp => kvp.Value == LegalStatus.Legal || kvp.Value == LegalStatus.Restricted)
            .Select(kvp => kvp.Key)
            .ToList();
    }
    
    // Get all formats where card is banned
    public List<Format> GetBannedFormats()
    {
        return _legalities
            .Where(kvp => kvp.Value == LegalStatus.Banned)
            .Select(kvp => kvp.Key)
            .ToList();
    }
    
    // Check multiple formats at once
    public bool IsLegalInAny(params Format[] formats)
    {
        return formats.Any(f => IsLegal(f));
    }
    
    public bool IsLegalInAll(params Format[] formats)
    {
        return formats.All(f => IsLegal(f));
    }
    
    // Format category checks
    public bool IsLegalInStandardFormats()
    {
        return IsLegal(Format.Standard) || 
               IsLegal(Format.StandardBrawl) || 
               IsLegal(Format.Alchemy);
    }
    
    public bool IsLegalInEternalFormats()
    {
        return IsLegal(Format.Vintage) || 
               IsLegal(Format.Legacy) || 
               IsLegal(Format.Commander);
    }
    
    // Get all legalities as dictionary
    public Dictionary<Format, LegalStatus> GetAllLegalities() => 
        new Dictionary<Format, LegalStatus>(_legalities);
}
