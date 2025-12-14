namespace Sandbox.Scryfall.Types;

public class ApiList<T> : APIBase
{
	[JsonPropertyName( "data" )] public List<T> Data { get; set; }
	[JsonPropertyName( "has_more" )] public bool HasMore { get; set; }
	[JsonPropertyName( "next_page" )] public Uri NextPage { get; set; }
	[JsonPropertyName( "total_cards" )] public int? TotalCards { get; set; }
	[JsonPropertyName( "warnings" )] public List<string> Warnings { get; set; }

	public bool HasData() => Data != null && Data.Count > 0;
	public int Count => Data?.Count ?? 0;

	public T GetFirstOrDefault()
	{
		if ( Data == null || Data.Count == 0 ) return default;
		return Data[0];
	}

	public T GetFirstOrDefault( Func<T, bool> predicate )
	{
		if ( Data == null ) return default;
		return Data.FirstOrDefault( predicate );
	}
}
