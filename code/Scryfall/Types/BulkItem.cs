namespace Sandbox.Scryfall.Types;

public class BulkItem : ItemBase
{
	/// <summary>The Scryfall API URI for this file.</summary>
	[JsonPropertyName( "uri" )] public string Uri { get; set; }
	/// <summary>A computer-readable string for the kind of bulk item.</summary>
	[JsonPropertyName( "type" )] public string Type { get; set; }
	/// <summary>A human-readable name for this file.</summary>
	[JsonPropertyName( "name" )] public string Name { get; set; }
	/// <summary>A human-readable description for this file.</summary>
	[JsonPropertyName( "description" )] public string Description { get; set; }
	/// <summary>The URI that hosts this bulk file for fetching.</summary>
	[JsonPropertyName( "download_uri" )] public string DownloadUri { get; set; }
	/// <summary>The time when this file was last updated.</summary>
	[JsonPropertyName( "updated_at" )] public DateTimeOffset UpdatedAt { get; set; }
	/// <summary>The size of this file in integer bytes.</summary>
	[JsonPropertyName( "size" )] public long? Size { get; set; }
	/// <summary>The MIME type of this file.</summary>
	[JsonPropertyName( "content_type" )] public string ContentType { get; set; }

	/// <summary>The Content-Encoding encoding that will be used to transmit this file when you download it.</summary>
	[JsonPropertyName( "content_encoding" )] public string ContentEncoding { get; set; }

	/// <summary> </summary>
	public bool IsNewerThan( BulkItem other )
		=> other == null || UpdatedAt > other.UpdatedAt;

	/// <summary> </summary>
	public bool IsUpdateNeeded()
		=> UpdatedAt < DateTimeOffset.UtcNow.AddHours( -24 );
}
