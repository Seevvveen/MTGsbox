using Sandbox.Symbol;

namespace Sandbox.Card;

public class CardMapper
{
	
	public static CardDefinition Map( ScryfallCard dto )
	{
		if ( dto is null )
			throw new CardMappingException( "Card DTO is null" );

		var id = Require( dto.Id, nameof(dto.Id) );

		return new CardDefinition()
		{
			Id = id, 
			Name = dto.Name, 
			ImageUris = ParseImageUris(dto.ImageUris)
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

		return Uri.TryCreate( value, UriKind.Absolute, out var uri )
			? uri
			: null;
	}

	
	private static CardImageUris ParseImageUris( ScryfallImageUris dto )
	{
		var images = new CardImageUris
		{
			Small      = TryParseUri( dto.Small ),
			Normal     = TryParseUri( dto.Normal ),
			Large      = TryParseUri( dto.Large ),
			Png        = TryParseUri( dto.Png ),
			ArtCrop    = TryParseUri( dto.ArtCrop ),
			BorderCrop = TryParseUri( dto.BorderCrop ),
		};
		
		return images;
	}



}
