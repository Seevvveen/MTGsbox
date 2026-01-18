#nullable enable
using Sandbox.Card;

namespace Sandbox._Startup;

/// <summary>
/// Multi-indexed catalog for card lookups.
/// Provides fast access to cards by ID, oracle ID, and exact name.
/// </summary>
public sealed class CardsCatalog
{
	public Catalog<Guid, CardDefinition> ById { get; } = new();
    public Catalog<Guid, IReadOnlyList<Guid>> ByOracleId { get; } = new();
    public Catalog<string, IReadOnlyList<Guid>> ByExactName { get; } = new();

    public bool IsReady => ById.IsReady && ByOracleId.IsReady && ByExactName.IsReady;
    public int Count => ById.Count;

    /// <summary>
    /// Publishes all catalog indices atomically.
    /// </summary>
    public void Publish(
        IReadOnlyDictionary<Guid, CardDefinition> byId,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> byOracleId,
        IReadOnlyDictionary<string, IReadOnlyList<Guid>> byExactName)
    {
        ArgumentNullException.ThrowIfNull(byId);
        ArgumentNullException.ThrowIfNull(byOracleId);
        ArgumentNullException.ThrowIfNull(byExactName);

        ById.Set(byId);
        ByOracleId.Set(byOracleId);
        ByExactName.Set(byExactName);
    }

    /// <summary>
    /// Clears all catalog indices.
    /// </summary>
    public void Clear()
    {
        ById.Clear();
        ByOracleId.Clear();
        ByExactName.Clear();
    }

    // ===== Convenience Lookup Methods =====

    /// <summary>
    /// Gets a card by its unique ID.
    /// </summary>
    public CardDefinition? GetCard(Guid id)
    {
        return ById.TryGet(id, out var card) ? card : null;
    }

    /// <summary>
    /// Gets a card by ID, throwing if not found.
    /// </summary>
    public CardDefinition GetCardOrThrow(Guid id)
    {
        return ById.GetOrThrow(id);
    }

    /// <summary>
    /// Tries to get a card by ID.
    /// </summary>
    public bool TryGetCard(Guid id, out CardDefinition card)
    {
        return ById.TryGet(id, out card);
    }

    /// <summary>
    /// Gets all cards with the same oracle ID (different printings of the same card).
    /// </summary>
    public IReadOnlyList<CardDefinition> GetCardsByOracleId(Guid oracleId)
    {
        if (!ByOracleId.TryGet(oracleId, out var cardIds))
            return Array.Empty<CardDefinition>();

        return cardIds
            .Select(id => ById.TryGet(id, out var card) ? card : null)
            .Where(card => card is not null)
            .ToArray()!;
    }

    /// <summary>
    /// Gets all cards with an exact name match (case-sensitive).
    /// </summary>
    public IReadOnlyList<CardDefinition> GetCardsByExactName(string name)
    {
        if (!ByExactName.TryGet(name, out var cardIds))
            return Array.Empty<CardDefinition>();

        return cardIds
            .Select(id => ById.TryGet(id, out var card) ? card : null)
            .Where(card => card is not null)
            .ToArray()!;
    }

    /// <summary>
    /// Gets all cards with an exact name match (case-insensitive).
    /// </summary>
    public IReadOnlyList<CardDefinition> GetCardsByNameIgnoreCase(string name)
    {
        var normalizedName = name.Trim();
        
        var matchingKey = ByExactName.GetKeys()
            .FirstOrDefault(key => string.Equals(key, normalizedName, StringComparison.OrdinalIgnoreCase));

        return matchingKey is not null 
            ? GetCardsByExactName(matchingKey) 
            : Array.Empty<CardDefinition>();
    }

    /// <summary>
    /// Gets the first card matching the exact name, or null if not found.
    /// Useful when you expect only one card with that name.
    /// </summary>
    public CardDefinition? GetFirstCardByName(string name)
    {
        if (!ByExactName.TryGet(name, out var cardIds) || cardIds.Count == 0)
            return null;

        return ById.TryGet(cardIds[0], out var card) ? card : null;
    }

    /// <summary>
    /// Gets a random card from the catalog.
    /// </summary>
    public CardDefinition GetRandomCard()
    {
        return ById.GetRandomOrThrow();
    }

    /// <summary>
    /// Gets all cards in the catalog.
    /// </summary>
    public IReadOnlyList<CardDefinition> GetAllCards()
    {
        return ById.GetAll();
    }

    /// <summary>
    /// Searches for cards by partial name match (case-insensitive).
    /// Warning: This iterates all cards and should be used sparingly.
    /// </summary>
    public IEnumerable<CardDefinition> SearchByPartialName(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<CardDefinition>();

        var normalized = searchTerm.Trim().ToLowerInvariant();
        
        return ByExactName.GetKeys()
            .Where(name => name.ToLowerInvariant().Contains(normalized))
            .SelectMany(GetCardsByExactName);
    }

    /// <summary>
    /// Checks if a card with the given ID exists.
    /// </summary>
    public bool ContainsCard(Guid id)
    {
        return ById.ContainsKey(id);
    }

    /// <summary>
    /// Gets the number of unique printings for a given oracle ID.
    /// </summary>
    public int GetPrintingCount(Guid oracleId)
    {
        return ByOracleId.TryGet(oracleId, out var cardIds) ? cardIds.Count : 0;
    }

    /// <summary>
    /// Gets all unique card names in the catalog.
    /// </summary>
    public IEnumerable<string> GetAllCardNames()
    {
        return ByExactName.GetKeys();
    }
}
