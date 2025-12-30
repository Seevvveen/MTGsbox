using Sandbox.Scryfall.Types.Dtos;
using Sandbox.Scryfall.Types.DTOs;

/// <summary>
/// Immutable, session-scoped catalog providing O(1) card lookups.
/// This is application infrastructure, not a gameplay object.
/// 
/// Lifetime:
/// - Controlled by explicit TaskSource
/// - Becomes invalid when TaskSource is cancelled
/// - Not a Component - lives outside scene graph
/// </summary>
public sealed class CardCatalog : IDisposable
{
    public enum BuildState
    {
        NotStarted,
        Building,
        Ready,
        Failed,
        Disposed
    }
    
    private readonly TaskSource _lifetime;
    private IReadOnlyDictionary<Guid, ScryfallCard> _cards;
    private BuildState _state = BuildState.NotStarted;
    private Exception _buildError;
    
    /// <summary>
    /// Creates a new catalog with explicit lifetime scope.
    /// </summary>
    /// <param name="lifetime">TaskSource that controls this catalog's lifetime</param>
    public CardCatalog(TaskSource lifetime)
    {
        _lifetime = lifetime;
    }
    
    /// <summary>
    /// Current state of the catalog.
    /// Thread-safe to read.
    /// </summary>
    public BuildState State => _state;
    
    /// <summary>
    /// Whether the catalog is ready for queries.
    /// </summary>
    public bool IsReady => _state == BuildState.Ready;
    
    /// <summary>
    /// Number of cards in the catalog.
    /// Only valid when State == Ready.
    /// </summary>
    public int Count => _cards?.Count ?? 0;
    
    /// <summary>
    /// Error from build process, if any.
    /// </summary>
    public Exception BuildError => _buildError;
    
    /// <summary>
    /// The TaskSource controlling this catalog's lifetime.
    /// Use this to scope async operations to the catalog's lifetime.
    /// </summary>
    public TaskSource Lifetime => _lifetime;
    
    // Internal: Only builder can populate
    internal void SetCards(IReadOnlyDictionary<Guid, ScryfallCard> cards)
    {
        EnsureNotDisposed();
        
        if (_state == BuildState.Ready)
            throw new InvalidOperationException("Catalog already built");
        
        _cards = cards ?? throw new ArgumentNullException(nameof(cards));
        _state = BuildState.Ready;
        
        Log.Info($"CardCatalog ready: {cards.Count} cards loaded");
    }
    
    internal void SetBuilding()
    {
        EnsureNotDisposed();
        _state = BuildState.Building;
    }
    
    internal void SetFailed(Exception error)
    {
        if (_state == BuildState.Disposed)
            return; // Already disposed, ignore
        
        _buildError = error;
        _state = BuildState.Failed;
        Log.Error($"CardCatalog build failed: {error.Message}");
    }
    
    /// <summary>
    /// Get a card by ID.
    /// Throws if card not found or catalog not ready.
    /// </summary>
    public ScryfallCard Get(Guid cardId)
    {
        EnsureReady();
        
        if (!_cards.TryGetValue(cardId, out var card))
            throw new KeyNotFoundException($"Card not found: {cardId}");
        
        return card;
    }
    
    /// <summary>
    /// Try to get a card by ID.
    /// Returns false if not found or catalog not ready.
    /// </summary>
    public bool TryGet(Guid cardId, out ScryfallCard card)
    {
        card = null;
        
        if (_state != BuildState.Ready)
            return false;
        
        return _cards.TryGetValue(cardId, out card);
    }
    
    /// <summary>
    /// Check if a card exists in the catalog.
    /// </summary>
    public bool Contains(Guid cardId)
    {
        return _state == BuildState.Ready && _cards.ContainsKey(cardId);
    }
    
    /// <summary>
    /// Get all cards in the catalog.
    /// Throws if catalog not ready.
    /// </summary>
    public IEnumerable<ScryfallCard> GetAll()
    {
        EnsureReady();
        return _cards.Values;
    }
    
    /// <summary>
    /// Query cards matching a predicate.
    /// </summary>
    public IEnumerable<ScryfallCard> Where(Func<ScryfallCard, bool> predicate)
    {
        EnsureReady();
        return _cards.Values.Where(predicate);
    }
    
    private void EnsureReady()
    {
        EnsureNotDisposed();
        
        if (_state == BuildState.NotStarted)
            throw new InvalidOperationException("Catalog build never started");
        
        if (_state == BuildState.Building)
            throw new InvalidOperationException("Catalog still building");
        
        if (_state == BuildState.Failed)
            throw new InvalidOperationException($"Catalog build failed: {_buildError?.Message}");
    }
    
    private void EnsureNotDisposed()
    {
        if (_state == BuildState.Disposed)
            throw new ObjectDisposedException(nameof(CardCatalog));
    }
    
    public void Dispose()
    {
        if (_state == BuildState.Disposed)
            return;
        
        _state = BuildState.Disposed;
        _cards = null;
        _buildError = null;
        
        Log.Info("CardCatalog disposed");
    }
}
