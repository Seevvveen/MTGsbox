#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Card;
using Sandbox.Scryfall;

namespace Sandbox._Startup;

public sealed class BuildCardsCatalogJob
{
	private readonly CacheService _cache;

	private const string CardsFile = "default_cards.json";
	private const string CachedIndicesFile = "card_indices.cache.json";

	private const int ReportInterval = 1000;
	private const int MaxErrors = 100;

	public BuildCardsCatalogJob( CacheService cache )
	{
		_cache = cache ?? throw new ArgumentNullException( nameof( cache ) );
	}

	/// <summary>
	/// Primary entrypoint. Loads cached indices if available, otherwise streams the bulk cards file and builds indices.
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

		// 2) Build from source (streaming)
		progress?.Invoke( new BuildProgress { Phase = "Streaming cards", Processed = 0, Errors = 0 } );

		var collector = new CardsCollector();

		var processed = 0;
		var skipped = 0;
		var errors = 0;

		foreach ( var dto in _cache.StreamJsonArray<ScryfallCard>( CardsFile, token ) )
		{
			token.ThrowIfCancellationRequested();

			try
			{
				var def = CardMapper.Map( dto );
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
				Log.Warning( $"Failed to map card: {ex.Message}" );

				if ( errors >= MaxErrors )
					throw new InvalidOperationException( $"Too many mapping errors ({errors}), aborting build.", ex );
			}

			var totalSoFar = processed + skipped + errors;
			if ( totalSoFar % ReportInterval == 0 )
			{
				progress?.Invoke( new BuildProgress
				{
					Phase = "Streaming cards",
					Processed = processed,
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

		if ( cached.ById is null || cached.ByOracleId is null || cached.ByExactName is null )
			return null;

		return new BuildResult(
			cached.ById,
			cached.ByOracleId,
			cached.ByExactName
		);
	}

	private void TryCacheIndices( BuildResult result )
	{
		var cached = new CachedIndices
		{
			Version = CachedIndices.CurrentVersion,
			ById = result.ById,
			ByOracleId = result.ByOracleId,
			ByExactName = result.ByExactName,
			CachedAtUtc = DateTime.UtcNow
		};

		try
		{
			_cache.WriteJson( CachedIndicesFile, cached );
		}
		catch ( Exception ex )
		{
			Log.Warning( $"Failed to cache card indices: {ex.Message}" );
		}
	}

	#endregion

	#region Data Structures

	public readonly record struct BuildResult(
		IReadOnlyDictionary<Guid, CardDefinition> ById,
		IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> ByOracleId,
		IReadOnlyDictionary<string, IReadOnlyList<Guid>> ByExactName )
	{
		public int Count => ById.Count;
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
		public required IReadOnlyDictionary<Guid, CardDefinition> ById { get; init; }
		public required IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> ByOracleId { get; init; }
		public required IReadOnlyDictionary<string, IReadOnlyList<Guid>> ByExactName { get; init; }
		public DateTime CachedAtUtc { get; init; }
	}

	#endregion

	#region Collector

	private sealed class CardsCollector : ICollector<CardDefinition, BuildResult>
	{
		private readonly Dictionary<Guid, CardDefinition> _byId = new();
		private readonly Dictionary<Guid, List<Guid>> _byOracleId = new();
		private readonly Dictionary<string, List<Guid>> _byExactName = new( StringComparer.Ordinal );

		public void Add( CardDefinition def )
		{
			_byId[def.Id] = def;

			if ( def.OracleId != Guid.Empty )
			{
				if ( !_byOracleId.TryGetValue( def.OracleId, out var list ) )
				{
					list = new List<Guid>( 1 );
					_byOracleId[def.OracleId] = list;
				}

				if ( !list.Contains( def.Id ) )
					list.Add( def.Id );
			}

			var name = def.Name ?? string.Empty;
			if ( !_byExactName.TryGetValue( name, out var nameList ) )
			{
				nameList = new List<Guid>( 1 );
				_byExactName[name] = nameList;
			}

			if ( !nameList.Contains( def.Id ) )
				nameList.Add( def.Id );
		}

		public BuildResult Build()
		{
			static Dictionary<TKey, IReadOnlyList<Guid>> Freeze<TKey>( Dictionary<TKey, List<Guid>> src )
				where TKey : notnull
			{
				var frozen = new Dictionary<TKey, IReadOnlyList<Guid>>( src.Count, src.Comparer );
				foreach ( var kv in src )
					frozen[kv.Key] = kv.Value.AsReadOnly();
				return frozen;
			}

			return new BuildResult(
				_byId,
				Freeze( _byOracleId ),
				Freeze( _byExactName )
			);
		}
	}

	#endregion
}
