#nullable enable
namespace Sandbox.Symbol;

/// <summary>
/// Maps ScryfallCardSymbol.cs into CardSymbolDefinition.cs
/// Ensure validity and GameReady
/// </summary>
public static class SymbolMapper
{
	public static SymbolDefinition Map( ScryfallCardSymbol dto )
	{
		if ( dto is null ) throw new ArgumentNullException( nameof(dto) );

		// Required fields (pick what “required” means for your game)
		var symbol = Require( dto.Symbol, nameof(dto.Symbol) );
		var english = dto.English ?? string.Empty;

		// Null-safe
		var colors = dto.Colors                     ?? [];
		var gathererAlts = dto.GathererAlternatives ?? [];
		var manaValue = dto.ManaValue                 ?? 0f;
		var convertedManaCost = dto.ConvertedManaCost ?? 0f;
		
		// Parsing
		var colorMask = ParseColors( colors );
		var svgUri = TryParseUri( dto.SvgUri );

		return new SymbolDefinition()
		{
			Symbol = symbol,
			English = english,

			Transposable = dto.Transposable,
			RepresentsMana = dto.RepresentsMana,
			AppearsInManaCosts = dto.AppearsInManaCosts,
			Hybrid = dto.Hybrid,
			Phyrexian = dto.Phyrexian,
			Funny = dto.Funny,

			ManaValue = manaValue,
			ConvertedManaCost = convertedManaCost,

			SvgUri = svgUri,
			LooseVariant = string.IsNullOrWhiteSpace( dto.LooseVariant ) ? null : dto.LooseVariant,

			Colors = colorMask,
			GathererAlternatives = gathererAlts.Count == 0 ? [] : gathererAlts.ToArray(),
		};
	}
	
	/// <summary>
	/// Forces a value to exist on card definitions or throw error
	/// </summary>
	private static string Require( string? value, string name )
	{
		if ( string.IsNullOrWhiteSpace( value ) )
			throw new CardMappingException( $"Missing required field: {name}" );

		return value;
	}

	/// <summary>
	/// Convert String property to Uri
	/// </summary>
	private static Uri? TryParseUri( string? uri )
	{
		if ( string.IsNullOrWhiteSpace( uri ) )
			return null;

		return Uri.TryCreate( uri, UriKind.Absolute, out var parsed ) ? parsed : null;
	}

	/// <summary>
	/// Turn string of colors into color Array
	/// </summary>
	private static ColorMask ParseColors( IEnumerable<string> colors )
	{
		var mask = ColorMask.None;

		foreach ( var c in colors )
		{
			switch ( c )
			{
				case "W": mask |= ColorMask.White; break;
				case "U": mask |= ColorMask.Blue; break;
				case "B": mask |= ColorMask.Black; break;
				case "R": mask |= ColorMask.Red; break;
				case "G": mask |= ColorMask.Green; break;
			}
		}

		return mask;
	}
}

/// <summary>
/// Throw this when the DTO is present but doesn't meet your domain rules.
/// </summary>
public sealed class CardMappingException( string message ) : Exception( message );
