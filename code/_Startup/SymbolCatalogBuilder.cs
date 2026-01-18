#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Symbol;
using Sandbox.Scryfall;
using Sandbox.Scryfall.Types.Responses;

namespace Sandbox._Startup;

public sealed class BuildSymbolsCatalogJob
{
	private readonly CacheService _cache;

	private const string SymbologyFile = "ScryfallSymbology.json";
	private const string CachedIndicesFile = "symbol_indices.cache.json";

	private const int ReportInterval = 250;
	private const int MaxErrors = 50;

	public BuildSymbolsCatalogJob( CacheService cache )
	{
		_cache = cache ?? throw new ArgumentNullException( nameof( cache ) );
	}

	/// <summary>
	/// Primary entrypoint. Loads cached indices if available, otherwise reads the symbology file and builds indices.
	/// </summary>
	public Task<BuildResult> RunAsync( Action<BuildProgress>? progress = null, CancellationToken token = default )
	{
		// 1) Fast path: cached indices
		var cached = TryLoadCachedIndices();
		if ( cached.HasValue )
		{
			progress?.Invoke( new BuildProgress
			{
				Phase = "Loaded from cache",
				Processed = cached.Value.Count,
				Total = cached.Value.Count,
				Errors = 0
			} );

			return Task.FromResult( cached.Value );
		}

		progress?.Invoke( new BuildProgress { Phase = "Reading symbology", Processed = 0, Errors = 0 } );

		if ( !_cache.Exists( SymbologyFile ) )
			throw new InvalidOperationException( $"Missing symbology file: {SymbologyFile}. Run ScryfallSymbologySyncJob first." );

		// { object: "list", data: [...] }
		var response = _cache.ReadJson<ScryfallList<ScryfallCardSymbol>>( SymbologyFile );
		if ( response.Data is null || response.Data.Count == 0 )
			throw new InvalidOperationException( $"Symbology response contained no data: {SymbologyFile}" );

		var collector = new SymbolsCollector();
		var processed = 0;
		var skipped = 0;
		var errors = 0;

		for ( var i = 0; i < response.Data.Count; i++ )
		{
			token.ThrowIfCancellationRequested();

			var dto = response.Data[i];

			try
			{
				var def = SymbolMapper.Map( dto );
				if ( def is null )
				{
					skipped++;
				}
				else
				{
					collector.Add( def );
					processed++;
				}
			}
			catch ( Exception ex )
			{
				errors++;
				Log.Warning( $"Failed to map symbol: {ex.Message}" );

				if ( errors >= MaxErrors )
					throw new InvalidOperationException( $"Too many mapping errors ({errors}), aborting build.", ex );
			}

			if ( (processed + skipped + errors) % ReportInterval == 0 )
			{
				progress?.Invoke( new BuildProgress
				{
					Phase = "Reading symbology",
					Processed = processed,
					Total = response.Data.Count,
					Errors = errors
				} );
			}
		}

		var result = collector.Build();

		progress?.Invoke( new BuildProgress
		{
			Phase = "Complete",
			Processed = processed,
			Total = processed + skipped + errors,
			Errors = errors
		} );

		// 3) Best-effort cache write
		TryCacheIndices( result );

		return Task.FromResult( result );
	}

	public void ClearCache()
	{
		_cache.DeleteIfExists( CachedIndicesFile );
	}

	#region Caching

	private BuildResult? TryLoadCachedIndices()
	{
		if ( !_cache.TryReadJson<CachedIndices>( CachedIndicesFile, out var cached ) || cached is null )
			return null;

		if ( cached.Version != CachedIndices.CurrentVersion )
			return null;

		if ( cached.BySymbol is null || cached.BySymbol.Count == 0 )
			return null;

		return new BuildResult( cached.BySymbol );
	}

	private void TryCacheIndices( BuildResult result )
	{
		var cached = new CachedIndices
		{
			Version = CachedIndices.CurrentVersion,
			BySymbol = result.BySymbol,
			CachedAtUtc = DateTime.UtcNow
		};

		try
		{
			_cache.WriteJson( CachedIndicesFile, cached );
		}
		catch ( Exception ex )
		{
			Log.Warning( $"Failed to cache symbol indices: {ex.Message}" );
		}
	}

	#endregion

	#region Data Structures

	public readonly record struct BuildResult( IReadOnlyDictionary<string, SymbolDefinition> BySymbol )
	{
		public int Count => BySymbol.Count;
	}

	public sealed class BuildProgress
	{
		public string Phase { get; init; } = string.Empty;
		public int Processed { get; init; }
		public int Total { get; init; } = -1;
		public int Errors { get; init; }

		public double? PercentComplete => Total > 0 ? (double)Processed / Total : null;
	}

	private sealed class CachedIndices
	{
		public const int CurrentVersion = 1;

		public int Version { get; init; }
		public required IReadOnlyDictionary<string, SymbolDefinition> BySymbol { get; init; }
		public DateTime CachedAtUtc { get; init; }
	}

	#endregion

	#region Collector

	private sealed class SymbolsCollector : ICollector<SymbolDefinition, BuildResult>
	{
		private readonly Dictionary<string, SymbolDefinition> _bySymbol = new( StringComparer.Ordinal );

		public void Add( SymbolDefinition def )
		{
			var key = def.Symbol ?? string.Empty;
			if ( key.Length == 0 )
				return;

			_bySymbol[key] = def;
		}

		public BuildResult Build()
		{
			return new BuildResult( new Dictionary<string, SymbolDefinition>( _bySymbol, _bySymbol.Comparer ) );
		}
	}

	#endregion
}
