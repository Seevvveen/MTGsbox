#nullable enable
using Sandbox.Symbol;

namespace Sandbox.Card;

public class CardMapper
{
	public static CardDefinition Map( ScryfallCard dto )
	{
		if ( dto is null )
			throw new CardMappingException( "Card DTO is null" );

		var id = Require( dto.Id, nameof(dto.Id) );

		// Prefer top-level image_uris; fallback to first face that has image_uris
		var imageDto =
			dto.ImageUris
			?? dto.CardFaces?.FirstOrDefault( f => f?.ImageUris is not null )?.ImageUris;

		return new CardDefinition
		{
			Id = id,
			Name = dto.Name,
			OracleId = dto.OracleId ?? Guid.Empty,

			// Never null
			ImageUris = ParseImageUris( imageDto )
		};
	}

	private static Guid Require( Guid? value, string name )
	{
		if ( !value.HasValue )
			throw new CardMappingException( $"Missing required field: {name}" );

		return value.Value;
	}

	private static Uri? TryParseUri( string? value )
	{
		if ( string.IsNullOrWhiteSpace( value ) )
			return null;

		return Uri.TryCreate( value, UriKind.Absolute, out var uri ) ? uri : null;
	}

	private static CardImageUris ParseImageUris( ScryfallImageUris? dto )
	{
		if ( dto is null )
			return CardImageUris.Empty;

		return new CardImageUris
		{
			Small      = TryParseUri( dto.Small ),
			Normal     = TryParseUri( dto.Normal ),
			Large      = TryParseUri( dto.Large ),
			Png        = TryParseUri( dto.Png ),
			ArtCrop    = TryParseUri( dto.ArtCrop ),
			BorderCrop = TryParseUri( dto.BorderCrop ),
		};
	}
}
