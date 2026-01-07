#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Engine;
using Sandbox.Scryfall.Types.Responses;
using Sandbox.Symbol;

namespace Sandbox.Catalog.Builders;

public sealed class BuildSymbolsCatalogJob( CacheService cache )
{
	private const string SymbolsFile = "ScryfallSymbology.json"; //Again prolly shouldn't hardcode this

	public Task<IReadOnlyDictionary<string, SymbolDefinition>> RunAsync( CancellationToken token )
	{
		// This assumes the symbols payload is reasonably small.
		var dto = cache.ReadJson<ScryfallList<ScryfallCardSymbol>>( SymbolsFile );

		var collector = new SymbolsCollector();
		var result = BuildCore.Build( dto.Data, SymbolMapper.Map, collector );

		return Task.FromResult<IReadOnlyDictionary<string, SymbolDefinition>>( result );
	}

	private sealed class SymbolsCollector : ICollector<SymbolDefinition, Dictionary<string, SymbolDefinition>>
	{
		private readonly Dictionary<string, SymbolDefinition> _bySymbol = new( StringComparer.Ordinal );

		public void Add( SymbolDefinition def )
		{
			_bySymbol.Add( def.Symbol, def ); // choose policy: Add vs overwrite
		}

		public Dictionary<string, SymbolDefinition> Build() => _bySymbol;
	}
}
