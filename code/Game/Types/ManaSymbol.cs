using Sandbox.Game.Enums;

namespace Sandbox.Game.Cards;

public sealed class ManaSymbol
{
	public string RawValue { get; }
	public ManaSymbolType Type { get; }
	public ColorIdentity Colors { get; }
	public int GenericAmount { get; }

	private ManaSymbol(
		string raw,
		ManaSymbolType type,
		ColorIdentity colors,
		int genericAmount )
	{
		RawValue = raw;
		Type = type;
		Colors = colors;
		GenericAmount = genericAmount;
	}

	public static ManaSymbol Parse( string symbol )
	{
		// Variable cost: {X}, {Y}, {Z}
		if ( symbol is "X" or "Y" or "Z" )
		{
			return new ManaSymbol(
				symbol,
				ManaSymbolType.Variable,
				ColorIdentity.None,
				0
			);
		}

		// Generic mana: {2}, {10}
		if ( int.TryParse( symbol, out int amount ) )
		{
			return new ManaSymbol(
				symbol,
				ManaSymbolType.Generic,
				ColorIdentity.None,
				amount
			);
		}

		// Phyrexian mana: {W/P}
		if ( symbol.EndsWith( "/P" ) )
		{
			var colorPart = symbol[..^2]; // strip "/P"
			TryParseColor( colorPart, out var color );

			return new ManaSymbol(
				symbol,
				ManaSymbolType.Phyrexian,
				color,
				0
			);
		}

		// Hybrid mana: {W/U}, {2/B}
		if ( symbol.Contains( '/' ) )
		{
			var parts = symbol.Split( '/' );
			ColorIdentity colors = ColorIdentity.None;

			foreach ( var part in parts )
			{
				if ( TryParseColor( part, out var color ) )
					colors |= color;
			}

			return new ManaSymbol(
				symbol,
				ManaSymbolType.Hybrid,
				colors,
				0
			);
		}

		// Single colored mana: {W}, {U}, {B}, {R}, {G}
		if ( TryParseColor( symbol, out var singleColor ) )
		{
			return new ManaSymbol(
				symbol,
				ManaSymbolType.Colored,
				singleColor,
				0
			);
		}

		// Colorless or special symbols
		return new ManaSymbol(
			symbol,
			ManaSymbolType.Special,
			ColorIdentity.None,
			0
		);
	}

	private static bool TryParseColor( string symbol, out ColorIdentity color )
	{
		color = symbol.ToUpperInvariant() switch
		{
			"W" => ColorIdentity.W,
			"U" => ColorIdentity.U,
			"B" => ColorIdentity.B,
			"R" => ColorIdentity.R,
			"G" => ColorIdentity.G,
			"C" => ColorIdentity.Colorless,
			_   => ColorIdentity.None
		};

		return color != ColorIdentity.None;
	}

	public int GetNumericValue()
	{
		return Type switch
		{
			ManaSymbolType.Generic    => GenericAmount,
			ManaSymbolType.Colored    => 1,
			ManaSymbolType.Hybrid     => 1,
			ManaSymbolType.Phyrexian  => 1,
			ManaSymbolType.Variable   => 0,
			_                         => 0
		};
	}
}
