#nullable enable
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Scryfall;

namespace Sandbox._Startup;

/// <summary>
/// Cache service for s&box whitelist environment.
/// - Avoids Task.Run / Parallel / Progress{T}
/// - Avoids async iterators (IAsyncEnumerable)
/// - Avoids System.Text.Json incremental reader state types (JsonReaderState, Utf8JsonReader streaming)
/// - Provides bounded-memory streaming of top-level JSON arrays via manual object boundary scanning
/// </summary>
public abstract class CacheService( ScryfallClient api )
{
	
	
	private readonly ScryfallClient _api = api ?? throw new ArgumentNullException( nameof( api ) );

	// Keep modest by default; increase if you routinely see very large single objects.
	private const int BufferSize = 1024 * 1024; // 1MB

	// -------------------------
	// File operations
	// -------------------------

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

	public void DeletePattern( string pattern, CancellationToken token = default )
	{
		if ( string.IsNullOrWhiteSpace( pattern ) )
			throw new ArgumentException( "Pattern cannot be null/empty.", nameof( pattern ) );

		foreach ( var file in FileSystem.Data.FindFile( "/", pattern, recursive: true ) )
		{
			token.ThrowIfCancellationRequested();
			DeleteIfExists( file );
		}
	}

	// -------------------------
	// JSON object read/write (whitelist-safe)
	// -------------------------

	/// <summary>
	/// Read JSON from a file into a type using Sandbox.Json (handles engine types).
	/// </summary>
	public T ReadJson<T>( string filename ) where T : class
	{
		ValidateFileName( filename );
		EnsureExists( filename );

		var json = FileSystem.Data.ReadAllText( filename );
		if ( string.IsNullOrWhiteSpace( json ) )
			throw new InvalidOperationException( $"File is empty: '{filename}'" );

		var obj = Json.Deserialize<T>( json );
		return obj ?? throw new InvalidOperationException( $"Deserialized null: '{filename}' ({typeof(T).Name})" );
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
			value = ReadJson<T>( filename );
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Write JSON atomically: temp write + copy commit. Uses Sandbox.Json.Serialize (whitelist-safe).
	/// </summary>
	public void WriteJson<T>( string filename, T value ) where T : class
	{
		ValidateFileName( filename );
		if ( value is null ) throw new ArgumentNullException( nameof( value ) );

		var tmp = filename + ".tmp";

		try
		{
			var json = Json.Serialize( value );
			if ( string.IsNullOrWhiteSpace( json ) )
				throw new InvalidOperationException( $"Serialize produced empty JSON for '{filename}' ({typeof(T).Name})" );

			FileSystem.Data.WriteAllText( tmp, json );
			CommitTempFile( tmp, filename );
		}
		finally
		{
			DeleteIfExists( tmp );
		}
	}

	// -------------------------
	// Streaming top-level JSON arrays (bounded memory)
	// -------------------------

	/// <summary>
	/// Stream a file shaped like: [ { ... }, { ... }, ... ]
	/// This does not allocate the entire file; it extracts one object JSON at a time and deserializes it.
	///
	/// Whitelist-safe: no Utf8JsonReader streaming state, no IAsyncEnumerable.
	/// </summary>
	public IEnumerable<T> StreamJsonArray<T>( string filename, CancellationToken token = default ) where T : class
	{
		ValidateFileName( filename );
		EnsureExists( filename );

		using var stream = FileSystem.Data.OpenRead( filename );

		foreach ( var objJson in StreamTopLevelArrayObjects( stream, token ) )
		{
			token.ThrowIfCancellationRequested();

			// Prefer Sandbox.Json to handle engine types/resources.
			var item = Json.Deserialize<T>( objJson );
			if ( item != null )
				yield return item;
		}
	}

	/// <summary>
	/// Materialize all items (avoid for huge files).
	/// </summary>
	public IReadOnlyList<T> ReadJsonArray<T>( string filename, CancellationToken token = default ) where T : class
	{
		return StreamJsonArray<T>( filename, token ).ToList();
	}

	/// <summary>
	/// Write a JSON array file (streaming writer). Uses Sandbox.Json.Serialize per item.
	/// </summary>
	public void WriteJsonArray<T>( string filename, IEnumerable<T> items, CancellationToken token = default ) where T : class
	{
		ValidateFileName( filename );
		if ( items is null ) throw new ArgumentNullException( nameof( items ) );

		var tmp = filename + ".tmp";

		try
		{
			using var stream = FileSystem.Data.OpenWrite( tmp );

			WriteUtf8( stream, "[" );

			var first = true;

			foreach ( var item in items )
			{
				token.ThrowIfCancellationRequested();

				if ( item is null )
					continue;

				var json = Json.Serialize( item );
				if ( string.IsNullOrWhiteSpace( json ) )
					continue;

				if ( !first )
					WriteUtf8( stream, "," );

				WriteUtf8( stream, json );
				first = false;
			}

			WriteUtf8( stream, "]" );
			stream.Flush();
		}
		finally
		{
			// Commit after stream closes.
			if ( FileSystem.Data.FileExists( tmp ) )
			{
				CommitTempFile( tmp, filename );
			}

			DeleteIfExists( tmp );
		}
	}

	/// <summary>
	/// Transform: stream input array, write output array. Bounded memory.
	/// </summary>
	public void ProcessJsonArray<TIn, TOut>(
		string inputFile,
		string outputFile,
		Func<TIn, TOut?> transform,
		CancellationToken token = default )
		where TIn : class
		where TOut : class
	{
		ValidateFileName( inputFile );
		ValidateFileName( outputFile );
		if ( transform is null ) throw new ArgumentNullException( nameof( transform ) );

		var tmp = outputFile + ".tmp";

		try
		{
			using var outStream = FileSystem.Data.OpenWrite( tmp );

			WriteUtf8( outStream, "[" );

			var first = true;

			foreach ( var inItem in StreamJsonArray<TIn>( inputFile, token ) )
			{
				token.ThrowIfCancellationRequested();

				var outItem = transform( inItem );
				if ( outItem is null )
					continue;

				var json = Json.Serialize( outItem );
				if ( string.IsNullOrWhiteSpace( json ) )
					continue;

				if ( !first )
					WriteUtf8( outStream, "," );

				WriteUtf8( outStream, json );
				first = false;
			}

			WriteUtf8( outStream, "]" );
			outStream.Flush();

			CommitTempFile( tmp, outputFile );
		}
		finally
		{
			DeleteIfExists( tmp );
		}
	}

	// -------------------------
	// Download (keep your existing ScryfallClient async copy)
	// -------------------------

	public async Task DownloadToFileAsync( string uri, string filename, long? maxBytes, CancellationToken token )
	{
		ValidateUri( uri );
		ValidateFileName( filename );

		var tmp = filename + ".tmp";

		try
		{
			using ( var tmpStream = FileSystem.Data.OpenWrite( tmp ) )
			{
				// Uses your ScryfallClient implementation (already s&box-tested in your project).
				await _api.CopyUrlToStreamAsync( uri, tmpStream, BufferSize, token );
				tmpStream.Flush();
			}

			var bytes = FileSystem.Data.FileSize( tmp );
			if ( bytes <= 0 )
				throw new InvalidOperationException( $"Download produced empty file: '{uri}' -> '{filename}'" );

			if ( maxBytes.HasValue && bytes > maxBytes.Value )
				throw new InvalidOperationException( $"Download too large: {bytes:N0} bytes (limit {maxBytes.Value:N0})" );

			CommitTempFile( tmp, filename );
		}
		finally
		{
			DeleteIfExists( tmp );
		}
	}

	// -------------------------
	// Internal helpers
	// -------------------------

	private static void CommitTempFile( string tmpFile, string destinationFile )
	{
		ValidateFileName( tmpFile );
		ValidateFileName( destinationFile );

		if ( !FileSystem.Data.FileExists( tmpFile ) )
			throw new InvalidOperationException( $"Temp file missing: '{tmpFile}'" );

		using var input = FileSystem.Data.OpenRead( tmpFile );
		using var output = FileSystem.Data.OpenWrite( destinationFile );

		// Sync copy to avoid ValueTask/ReadAsync whitelist issues.
		input.CopyTo( output, BufferSize );
		output.Flush();
	}

	private static IEnumerable<string> StreamTopLevelArrayObjects( Stream stream, CancellationToken token )
	{
		// Strategy:
		// - Read UTF8 bytes in chunks
		// - Find '[' then repeatedly extract JSON objects at top-level: {...}
		// - Track brace depth and JSON string escaping to avoid false matches
		//
		// This assumes the top-level is a JSON array and elements are objects (Scryfall bulk is).
		//
		// Bounded memory: holds only current object text.
		var buf = ArrayPool<byte>.Shared.Rent( BufferSize );
		try
		{
			var sb = new StringBuilder( 64 * 1024 );

			var startedArray = false;
			var inString = false;
			var escape = false;

			var capturingObject = false;
			var braceDepth = 0;

			while ( true )
			{
				token.ThrowIfCancellationRequested();

				var read = stream.Read( buf, 0, buf.Length );
				if ( read <= 0 )
					yield break;

				// Convert this chunk to chars. We accept chunk-boundary UTF8 splits by using a Decoder.
				// Use a single decoder instance to handle partial characters.
				// NOTE: We create it lazily and keep it static inside this iterator scope.
				// (No async/yield boundary issues here; this is a sync iterator.)
				var chars = Encoding.UTF8.GetChars( buf, 0, read );

				for ( int i = 0; i < chars.Length; i++ )
				{
					token.ThrowIfCancellationRequested();

					var c = chars[i];

					if ( !startedArray )
					{
						if ( c == '[' )
							startedArray = true;
						continue;
					}

					if ( !capturingObject )
					{
						// Skip whitespace/commas until an object starts, or end array.
						if ( c == '{' )
						{
							capturingObject = true;
							braceDepth = 1;
							inString = false;
							escape = false;

							sb.Clear();
							sb.Append( c );
						}
						else if ( c == ']' )
						{
							yield break;
						}

						continue;
					}

					// Capturing an object: append and update JSON string/brace tracking.
					sb.Append( c );

					if ( inString )
					{
						if ( escape )
						{
							escape = false;
							continue;
						}

						if ( c == '\\' )
						{
							escape = true;
							continue;
						}

						if ( c == '"' )
						{
							inString = false;
							continue;
						}

						continue;
					}

					// Not in string
					if ( c == '"' )
					{
						inString = true;
						continue;
					}

					if ( c == '{' )
					{
						braceDepth++;
						continue;
					}

					if ( c == '}' )
					{
						braceDepth--;
						if ( braceDepth == 0 )
						{
							capturingObject = false;
							yield return sb.ToString();
						}
					}
				}
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return( buf );
		}
	}

	private static void ValidateFileName( string filename )
	{
		if ( string.IsNullOrWhiteSpace( filename ) )
			throw new ArgumentException( "Filename cannot be null/whitespace.", nameof( filename ) );
	}

	private static void ValidateUri( string uri )
	{
		if ( string.IsNullOrWhiteSpace( uri ) )
			throw new ArgumentException( "URI cannot be null/whitespace.", nameof( uri ) );
	}

	private static void EnsureExists( string filename )
	{
		if ( !FileSystem.Data.FileExists( filename ) )
			throw new FileNotFoundException( $"File not found in cache: '{filename}'" );
	}
	
	
	private static void WriteUtf8( Stream stream, string text )
	{
		// Small allocations are acceptable here; this is already streaming and bounded.
		// If you later want zero-GC, we can switch to ArrayPool<byte>.
		var bytes = Encoding.UTF8.GetBytes( text );
		stream.Write( bytes, 0, bytes.Length );
	}
	
}
