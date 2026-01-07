#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Card;
using Sandbox.Engine;
using Sandbox.Scryfall.Types.Responses;

namespace Sandbox.Catalog.Builders;

public sealed class BuildCardsCatalogJob( CacheService cache )
{
	private const string CardsFile = "oracle_cards.json"; //should prolly not hardcode this

	public Task<BuildResult> RunAsync( CancellationToken token )
	{

		// Replace this with a streaming enumerable if needed.
		var cards = cache.ReadJson<List<ScryfallCard>>( CardsFile );

		var collector = new CardsCollector();
		var result = BuildCore.Build( cards, CardMapper.Map, collector );

		return Task.FromResult( result );
	}

	public readonly record struct BuildResult(
		IReadOnlyDictionary<Guid, CardDefinition> ById,
		IReadOnlyDictionary<Guid, CardDefinition> ByOracleId,
		IReadOnlyDictionary<string, IReadOnlyList<Guid>> ByExactName );

	private sealed class CardsCollector : ICollector<CardDefinition, BuildResult>
	{
		private readonly Dictionary<Guid, CardDefinition> _byId = new();
		private readonly Dictionary<Guid, CardDefinition> _byOracleId = new();
		private readonly Dictionary<string, List<Guid>> _byExactName = new( StringComparer.Ordinal );

		public void Add( CardDefinition def )
		{
			_byId.Add( def.Id, def );

			if ( def.OracleId != Guid.Empty )
				_byOracleId[def.OracleId] = def;

			var name = def.Name; // consider normalizing here
			if ( !_byExactName.TryGetValue( name, out var list ) )
			{
				list = new List<Guid>( 1 );
				_byExactName[name] = list;
			}
			list.Add( def.Id );
		}

		public BuildResult Build()
		{
			var frozen = new Dictionary<string, IReadOnlyList<Guid>>( _byExactName.Count, _byExactName.Comparer );
			foreach ( var kv in _byExactName )
				frozen[kv.Key] = kv.Value;

			return new BuildResult( _byId, _byOracleId, frozen );
		}
	}
}
