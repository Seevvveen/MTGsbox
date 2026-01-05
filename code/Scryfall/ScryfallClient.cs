#nullable enable
global using System;
global using System.Text.Json.Serialization;
global using Sandbox.Scryfall.Types.DTOs;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Scryfall.Types.Responses;

namespace Sandbox.Scryfall;

/// <summary>
/// Interact with the scryfall API
/// </summary>
public class ScryfallClient
{
	private const string BaseUrl = "https://api.scryfall.com";

	// Static Singleton
	public static ScryfallClient Instance { get; } = new ScryfallClient();
	private ScryfallClient() {} //No External Construction
	
	
	private readonly SemaphoreSlim _gate = new( 1, 1 );
	private TimeSince _sinceLast;
	
	/// <summary> Delay Api Calls by 100ms</summary>
	private async Task ApiDelay()
	{
		await _gate.WaitAsync();
		try
		{
			const float interval = 0.1f;
			if ( _sinceLast < interval )
				await Task.Delay( (int)((interval - _sinceLast) * 1000f) );
			_sinceLast = 0;
		}
		finally
		{
			_gate.Release();
		}
	}

	/// <summary>
	/// Foundational Api Request
	/// Make request to provided Endpoint Expecting T in Return
	/// </summary>
	public async Task<T> RequestAsync<T>( string endpoint ) where T : class
	{
		await ApiDelay();
		var response = await Http.RequestJsonAsync<T>( $"{BaseUrl}/{endpoint}" )
			?? throw new InvalidOperationException( $"[Scryfall] Failed to deserialize response from {BaseUrl}/{endpoint}" );
		return response;
	}
	
	/// <summary>
	/// Alternative to RequestAsync that uses full URL for debug/ easy testing
	/// </summary>
	public async Task<T> RequestUrlAsync<T>( string url ) where T : class
	{
		await ApiDelay();
		var response = await Http.RequestJsonAsync<T>( url )
			?? throw new InvalidOperationException( $"[Scryfall] Failed to deserialize {url}" );
		return response;
	}
	
	// Helper
	public Task<ScryfallCard> GetCardAsync( string id )
		=> RequestAsync<ScryfallCard>( $"cards/{id}" );

	// Helper
	public Task<ScryfallList<ScryfallCard>> SearchCardsAsync( string query )
		=> RequestAsync<ScryfallList<ScryfallCard>>( $"cards/search?q={Uri.EscapeDataString( query )}" );

	
	/// <summary>
	/// Stream an HTTP response directly into a destination stream while keeping the HTTP response alive.
	/// Avoids Http.RequestStreamAsync (it disposes the response before you can use the stream).
	/// </summary>
	public async Task CopyUrlToStreamAsync(
		string url,
		Stream destination,
		int bufferSize = 1024 * 1024,
		CancellationToken token = default )
	{
		ArgumentNullException.ThrowIfNull( destination );
		if ( !destination.CanWrite )
			throw new InvalidOperationException( "[Scryfall] Destination stream is not writable." );

		token.ThrowIfCancellationRequested();
		await ApiDelay();

		// Keep response alive while reading its content stream
		using var response = await Http.RequestAsync( url, cancellationToken: token );

		// If EnsureSuccessStatusCode isn't available/allowed, do manual check
		if ( !response.IsSuccessStatusCode )
			throw new InvalidOperationException( $"[Scryfall] HTTP {(int)response.StatusCode} downloading {url}" );

		using var src = await response.Content.ReadAsStreamAsync( token );
		await src.CopyToAsync( destination, bufferSize, token );
	}
}


