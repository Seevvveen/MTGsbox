#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Scryfall;

namespace Sandbox.Engine;

/// <summary>
/// Simple wrapper around FileSystem.Data for strict cache reads and safe writes/downloads.
/// </summary>
public sealed class CacheService( ScryfallClient api )
{
	private readonly ScryfallClient _api = api ?? throw new ArgumentNullException( nameof(api) );
	private const int BufferSize = 1024 * 1024;

	// ---- Basic helpers ----

	public bool Exists( string filename )
	{
		ValidateFileName( filename );
		return FileSystem.Data.FileExists( filename );
	}

	public long Size( string filename )
	{
		ValidateFileName( filename );
		EnsureExists( filename );
		return FileSystem.Data.FileSize( filename );
	}

	public void DeleteFile( string filename )
	{
		ValidateFileName( filename );
		EnsureExists( filename );
		FileSystem.Data.DeleteFile( filename );
	}

	public void DeleteIfExists( string filename )
	{
		ValidateFileName( filename );
		if ( FileSystem.Data.FileExists( filename ) )
			FileSystem.Data.DeleteFile( filename );
	}

	// ---- JSON ----

	/// <summary>Strict read: throws if missing, invalid, or deserializes to null.</summary>
	public T ReadJson<T>( string filename ) where T : class
	{
		ValidateFileName( filename );
		EnsureExists( filename );

		return FileSystem.Data.ReadJson<T>( filename )
			?? throw new InvalidOperationException( $"CacheService.ReadJson<{typeof(T).Name}> returned null for '{filename}'." );
	}

	public bool TryReadJson<T>( string filename, out T? value ) where T : class
	{
		value = null;

		if ( string.IsNullOrWhiteSpace( filename ) )
			return false;

		if ( !FileSystem.Data.FileExists( filename ) )
			return false;

		try
		{
			value = FileSystem.Data.ReadJson<T>( filename );
			return value is not null;
		}
		catch
		{
			return false;
		}
	}

	public void WriteJson<T>( string filename, T value )
	{
		ValidateFileName( filename );
		ArgumentNullException.ThrowIfNull( value );

		FileSystem.Data.WriteJson( filename, value );
	}

	// ---- Download ----

	/// <summary>
	/// Downloads a URI to a temp file, validates, then commits by copying to destination.
	/// Note: async runs on main thread by default in sbox; caller may want to run this on a worker thread.
	/// </summary>
	public async Task DownloadToFileAsync(
		string uri,
		string filename,
		long? maxBytes = null,
		CancellationToken token = default )
	{
		token.ThrowIfCancellationRequested();
		ValidateUri( uri );
		ValidateFileName( filename );

		var tmpFile = filename + ".tmp";

		try
		{
			await using ( var netStream = await _api.FetchStreamAsync( uri, token ) )
				await using ( var tmpStream = FileSystem.Data.OpenWrite( tmpFile ) )
				{
					await netStream.CopyToAsync( tmpStream, BufferSize, token );
					await tmpStream.FlushAsync( token );
				}

			var bytes = FileSystem.Data.FileSize( tmpFile );
			if ( bytes <= 0 )
				throw new InvalidOperationException( $"Download produced empty file '{tmpFile}' from '{uri}'." );

			if ( maxBytes.HasValue && bytes > maxBytes.Value )
				throw new InvalidOperationException( $"Download too large ({bytes} bytes) for '{filename}' (limit {maxBytes.Value})." );

			await CommitTempByCopyAsync( tmpFile, filename, token );
		}
		catch ( Exception e )
		{
			throw new InvalidOperationException(
				$"CacheService.DownloadToFileAsync failed (uri='{uri}', file='{filename}').",
				e
			);
		}
		finally
		{
			// Best-effort cleanup
			try
			{
				if ( FileSystem.Data.FileExists( tmpFile ) )
					FileSystem.Data.DeleteFile( tmpFile );
			}
			catch
			{
				// ignored
			}
		}
	}

	private static async Task CommitTempByCopyAsync( string tmpFile, string destinationFile, CancellationToken token )
	{
		token.ThrowIfCancellationRequested();
		ValidateFileName( tmpFile );
		ValidateFileName( destinationFile );

		if ( !FileSystem.Data.FileExists( tmpFile ) )
			throw new InvalidOperationException( $"Temp file missing: '{tmpFile}'." );

		await using var input = FileSystem.Data.OpenRead( tmpFile );
		await using var output = FileSystem.Data.OpenWrite( destinationFile );
		await input.CopyToAsync( output, BufferSize, token );
		await output.FlushAsync( token );
	}

	// ---- Guards ----

	private static void ValidateFileName( string filename )
	{
		if ( string.IsNullOrWhiteSpace( filename ) )
			throw new InvalidOperationException( "CacheService: filename is null/empty." );
	}

	private static void ValidateUri( string uri )
	{
		if ( string.IsNullOrWhiteSpace( uri ) )
			throw new InvalidOperationException( "CacheService: uri is null/empty." );
	}

	private static void EnsureExists( string filename )
	{
		if ( !FileSystem.Data.FileExists( filename ) )
			throw new InvalidOperationException( $"CacheService: file does not exist: '{filename}'." );
	}
}
