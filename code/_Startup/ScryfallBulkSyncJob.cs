#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Diagnostics;
using Sandbox.Scryfall.Types.Responses;

namespace Sandbox._Startup;

public sealed class ScryfallBulkSyncJob
{
	private const string MetaFilename = "BulkMeta.json";
	private const long MaxFileSizeBytes = 1_000_000_000; // 1 GB
	private const string BulkIndexUri = "https://api.scryfall.com/bulk-data";

	private readonly CacheService _cache;
	private readonly Logger _logger = new( "ScryfallBulkSyncJob" );

	public ScryfallBulkSyncJob( CacheService cache )
	{
		_cache = cache ?? throw new ArgumentNullException( nameof( cache ) );
	}

	public async Task RunAsync( Action<BulkSyncProgress>? progress = null, CancellationToken token = default )
	{
		progress?.Invoke( BulkSyncProgress.PhaseOnly( "Loading bulk meta" ) );

		var meta = await LoadOrDownloadMetaAsync( progress, token );

		var plan = BuildDownloadPlan( meta );
		if ( plan.Count == 0 )
		{
			_logger.Warning( "Bulk meta returned no downloadable items." );
			return;
		}

		progress?.Invoke( new BulkSyncProgress
		{
			Phase = "Downloading bulk files",
			TotalFiles = plan.Count
		} );

		await DownloadPlanAsync( plan, progress, token );

		progress?.Invoke( BulkSyncProgress.PhaseOnly( "Bulk sync complete" ) );
	}

	private async Task<ScryfallList<ScryfallBulkData>> LoadOrDownloadMetaAsync(
		Action<BulkSyncProgress>? progress,
		CancellationToken token )
	{
		// 1) Try cached meta first (treat invalid json as cache-miss)
		if ( _cache.Exists( MetaFilename ) )
		{
			if ( _cache.TryReadJson<ScryfallList<ScryfallBulkData>>( MetaFilename, out var cached ) &&
			     cached is not null &&
			     cached.Data is not null &&
			     cached.Data.Count > 0 )
			{
				_logger.Info( $"Found: {MetaFilename}" );
				return cached;
			}

			_logger.Warning( $"Cached meta invalid: {MetaFilename}. Re-downloading." );
			_cache.DeleteIfExists( MetaFilename );
		}

		// 2) Download meta
		progress?.Invoke( BulkSyncProgress.PhaseOnly( $"Downloading meta: {BulkIndexUri}" ) );

		await _cache.DownloadToFileAsync(
			uri: BulkIndexUri,
			filename: MetaFilename,
			maxBytes: MaxFileSizeBytes,
			token: token
		);

		// 3) Read meta
		var meta = _cache.ReadJson<ScryfallList<ScryfallBulkData>>( MetaFilename );

		if ( meta.Data is null || meta.Data.Count == 0 )
			throw new InvalidOperationException( "Bulk meta contained no data after download." );

		return meta;
	}

	private static IReadOnlyList<BulkItem> BuildDownloadPlan( ScryfallList<ScryfallBulkData> meta )
	{
		if ( meta?.Data is null || meta.Data.Count == 0 )
			return Array.Empty<BulkItem>();

		return meta.Data
			.Where( item =>
				!string.IsNullOrWhiteSpace( item.Type ) &&
				!string.IsNullOrWhiteSpace( item.DownloadUri ) &&
				item.Size > 0 &&
				item.Size <= MaxFileSizeBytes
			)
			.Select( item =>
			{
				var type = item.Type!;
				return new BulkItem(
					Type: type,
					DownloadUri: item.DownloadUri!,
					LocalFileName: $"{type}.json",
					ExpectedBytes: item.Size
				);
			} )
			.ToList();
	}

	private async Task DownloadPlanAsync(
		IReadOnlyList<BulkItem> plan,
		Action<BulkSyncProgress>? progress,
		CancellationToken token )
	{
		var completed = 0;

		foreach ( var item in plan )
		{
			token.ThrowIfCancellationRequested();

			progress?.Invoke( new BulkSyncProgress
			{
				Phase = "Checking bulk file",
				CurrentType = item.Type,
				CurrentFile = item.LocalFileName,
				CompletedFiles = completed,
				TotalFiles = plan.Count
			} );

			if ( IsValidExistingFile( item ) )
			{
				completed++;
				continue;
			}

			_logger.Info( $"Downloading: {item.Type} -> {item.LocalFileName}" );

			progress?.Invoke( new BulkSyncProgress
			{
				Phase = "Downloading bulk file",
				CurrentType = item.Type,
				CurrentFile = item.LocalFileName,
				CompletedFiles = completed,
				TotalFiles = plan.Count,
				CurrentFileExpectedBytes = item.ExpectedBytes
			} );

			try
			{
				await _cache.DownloadToFileAsync(
					uri: item.DownloadUri,
					filename: item.LocalFileName,
					maxBytes: MaxFileSizeBytes,
					token: token
				);

				if ( !IsValidExistingFile( item ) )
				{
					var have = _cache.Exists( item.LocalFileName ) ? _cache.Size( item.LocalFileName ) : 0;
					throw new InvalidOperationException(
						$"Downloaded file failed validation: {item.LocalFileName} ({have:N0} bytes)" );
				}
			}
			catch ( Exception ex )
			{
				_logger.Error( $"Error downloading {item.LocalFileName} from {item.DownloadUri}: {ex}" );
				throw;
			}

			completed++;

			progress?.Invoke( new BulkSyncProgress
			{
				Phase = "Downloaded bulk file",
				CurrentType = item.Type,
				CurrentFile = item.LocalFileName,
				CompletedFiles = completed,
				TotalFiles = plan.Count
			} );
		}
	}

	private bool IsValidExistingFile( BulkItem item )
	{
		if ( !_cache.Exists( item.LocalFileName ) )
			return false;

		var size = _cache.Size( item.LocalFileName );
		if ( size <= 0 )
			return false;

		if ( item.ExpectedBytes > 0 && size != item.ExpectedBytes )
		{
			_logger.Warning(
				$"Size mismatch for {item.LocalFileName}: have {size:N0}, expected {item.ExpectedBytes:N0}. Re-downloading." );

			_cache.DeleteIfExists( item.LocalFileName );
			return false;
		}

		return true;
	}

	private readonly record struct BulkItem(
		string Type,
		string DownloadUri,
		string LocalFileName,
		long ExpectedBytes
	);

	public sealed class BulkSyncProgress
	{
		public string Phase { get; init; } = string.Empty;

		public int CompletedFiles { get; init; }
		public int TotalFiles { get; init; }

		public string? CurrentType { get; init; }
		public string? CurrentFile { get; init; }

		public long? CurrentFileExpectedBytes { get; init; }

		public static BulkSyncProgress PhaseOnly( string phase ) => new() { Phase = phase };
	}
}
