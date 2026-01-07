#nullable enable
using Sandbox.Card;

namespace Sandbox.Catalog;

public sealed class CardsCatalog
{
	public Catalog<Guid, CardDefinition> ById { get; } = new();
	public Catalog<Guid, CardDefinition> ByOracleId { get; } = new();
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
		IReadOnlyDictionary<Guid, CardDefinition> byOracleId,
		IReadOnlyDictionary<string, IReadOnlyList<Guid>> byExactName )
	{
		//build everything first, then set all.
		ById.Set( byId );
		ByOracleId.Set( byOracleId );
		ByExactName.Set( byExactName );
	}
}
