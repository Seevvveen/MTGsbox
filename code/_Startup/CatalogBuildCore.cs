#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;

namespace Sandbox._Startup;

/// <summary>
/// Generic collector interface for building results from a stream of definitions.
///
/// Build jobs own their own streaming/parsing strategy (whitelist-safe, bounded-memory).
/// The collector is a small accumulator + finalizer.
/// </summary>
public interface ICollector<in TDef, out TResult>
{
	/// <summary>Add a mapped definition into the build.</summary>
	void Add( TDef def );

	/// <summary>Finalize the collected state into the published result.</summary>
	TResult Build();
}

/// <summary>
/// Lightweight build counters. Prefer this over heavy progress/error frameworks.
/// </summary>
public readonly record struct BuildStats( int Processed, int Skipped, int Errors )
{
	public int Total => Processed + Skipped + Errors;
}

/// <summary>
/// Whitelist-safe helper for the common "map -> collect" loop.
///
/// Notes:
/// - No Task.Run, no Parallel, no IAsyncEnumerable.
/// - Cancellation is cooperative and checked per iteration.
/// - Progress is an optional callback (invoked every reportInterval iterations).
/// - Error handling is opt-in; you may let exceptions bubble if you prefer.
/// </summary>
public static class BuildCore
{
	public static TResult Build<TDto, TDef, TResult>(
		IEnumerable<TDto> source,
		Func<TDto, TDef?> map,
		ICollector<TDef, TResult> collector,
		out BuildStats stats,
		Action<int>? onProgress = null,
		int reportInterval = 1000,
		Action<Exception>? onError = null,
		int maxErrors = 100,
		CancellationToken token = default )
		where TDef : class
	{
		ArgumentNullException.ThrowIfNull( source );
		ArgumentNullException.ThrowIfNull( map );
		ArgumentNullException.ThrowIfNull( collector );

		var processed = 0;
		var skipped = 0;
		var errors = 0;
		var seen = 0;

		foreach ( var dto in source )
		{
			token.ThrowIfCancellationRequested();
			seen++;

			try
			{
				var def = map( dto );
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
				onError?.Invoke( ex );

				if ( errors >= maxErrors )
					throw new InvalidOperationException( $"Too many mapping errors ({errors}), aborting build.", ex );
			}

			if ( reportInterval > 0 && (seen % reportInterval) == 0 )
				onProgress?.Invoke( seen );
		}

		onProgress?.Invoke( seen );
		stats = new BuildStats( processed, skipped, errors );
		return collector.Build();
	}
}
