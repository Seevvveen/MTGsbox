#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Card;
using Sandbox.Engine;
using Sandbox.Scryfall.Types.Responses;

namespace Sandbox.Catalog.Builders;

public sealed class BuildCardsCatalogJob( CacheService cache )
{
	private const string CardsFile = "default_cards.json"; //should prolly not hardcode this

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
		IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> ByOracleId,
		IReadOnlyDictionary<string, IReadOnlyList<Guid>> ByExactName );

	private sealed class CardsCollector : ICollector<CardDefinition, BuildResult>
	{
		private readonly Dictionary<Guid, CardDefinition> _byId = new();
		private readonly Dictionary<Guid, List<Guid>> _byOracleId = new();
		private readonly Dictionary<string, List<Guid>> _byExactName = new( StringComparer.Ordinal );

		public void Add( CardDefinition def )
		{
			// In all-cards, duplicates can happen across weird data; prefer overwrite-safe
			_byId[def.Id] = def;

			if ( def.OracleId != Guid.Empty )
			{
				if ( !_byOracleId.TryGetValue( def.OracleId, out var list ) )
				{
					list = new List<Guid>( 1 );
					_byOracleId[def.OracleId] = list;
				}
				list.Add( def.Id );
			}

			var name = def.Name;
			if ( !_byExactName.TryGetValue( name, out var nameList ) )
			{
				nameList = new List<Guid>( 1 );
				_byExactName[name] = nameList;
			}
			nameList.Add( def.Id );
		}

		public BuildResult Build()
		{
			static Dictionary<TKey, IReadOnlyList<Guid>> Freeze<TKey>( Dictionary<TKey, List<Guid>> src )
				where TKey : notnull
			{
				var frozen = new Dictionary<TKey, IReadOnlyList<Guid>>( src.Count, src.Comparer );
				foreach ( var kv in src )
					frozen[kv.Key] = kv.Value;
				return frozen;
			}

			return new BuildResult(
				_byId,
				Freeze( _byOracleId ),
				Freeze( _byExactName )
			);
		}
	}
}
