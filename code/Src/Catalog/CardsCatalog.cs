#nullable enable
using Sandbox.Card;

namespace Sandbox.Catalog;

public sealed class CardsCatalog
{
	public Catalog<Guid, CardDefinition> ById { get; } = new();
	public Catalog<Guid, IReadOnlyList<Guid>> ByOracleId { get; } = new();
	public Catalog<string, IReadOnlyList<Guid>> ByExactName { get; } = new();

	public void Clear()
	{
		ById.Clear();
		ByOracleId.Clear();
		ByExactName.Clear();
	}

	public bool IsReady => ById.IsReady && ByOracleId.IsReady && ByExactName.IsReady;
	public int Count => ById.Count;

	public void Publish(
		IReadOnlyDictionary<Guid, CardDefinition> byId,
		IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> byOracleId,
		IReadOnlyDictionary<string, IReadOnlyList<Guid>> byExactName )
	{
		ById.Set( byId );
		ByOracleId.Set( byOracleId );
		ByExactName.Set( byExactName );
	}
}

