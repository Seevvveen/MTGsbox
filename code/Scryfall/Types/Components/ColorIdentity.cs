#nullable enable

namespace Sandbox.Scryfall.Types.Components;

/// <summary>
/// Represents a color identity in Magic: The Gathering.
/// Colors are represented as single character strings: W, U, B, R, G, C
/// </summary>
public record ColorIdentity
{
	public bool White { get; init; }
	public bool Blue { get; init; }
	public bool Black { get; init; }
	public bool Red { get; init; }
	public bool Green { get; init; }
	public bool Colorless { get; init; }

	public static ColorIdentity None => new();

	public static ColorIdentity FromScryfall( List<string>? colors )
	{
		if ( colors == null || colors.Count == 0 )
			return None;

		return new ColorIdentity
		{
			White = colors.Contains( "W" ),
			Blue = colors.Contains( "U" ),
			Black = colors.Contains( "B" ),
			Red = colors.Contains( "R" ),
			Green = colors.Contains( "G" ),
			Colorless = colors.Contains( "C" )
		};
	}

	public List<string> ToScryfall()
	{
		var colors = new List<string>();
		if ( White ) colors.Add( "W" );
		if ( Blue ) colors.Add( "U" );
		if ( Black ) colors.Add( "B" );
		if ( Red ) colors.Add( "R" );
		if ( Green ) colors.Add( "G" );
		if ( Colorless ) colors.Add( "C" );
		return colors;
	}

	public bool IsMonocolor => ToScryfall().Count == 1;
	public bool IsMulticolor => ToScryfall().Count > 1;
	public bool IsColorless => ToScryfall().Count == 0 || (ToScryfall().Count == 1 && Colorless);
}
