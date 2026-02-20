using System;
using System.Collections.Generic;
using System.Numerics;

namespace Sandbox.__Rewrite.Gameplay;

[Flags]
public enum ManaColor : byte
{
	None      = 0,
	White     = 1 << 0, // W
	Blue      = 1 << 1, // U
	Black     = 1 << 2, // B
	Red       = 1 << 3, // R
	Green     = 1 << 4, // G
	Colorless = 1 << 5, // C
}

public static class ManaColorExtensions
{
	public static int ColorCount( this ManaColor color )
	{
		var chromatic = (byte)( (byte)color & ~(byte)ManaColor.Colorless );
		// Fast popcount; if BitOperations unavailable in your env, swap to your loop.
		return BitOperations.PopCount( chromatic );
	}

	public static bool IsMonoColor( this ManaColor color ) => color.ColorCount()  == 1;
	public static bool IsMultiColor( this ManaColor color ) => color.ColorCount() > 1;
	public static bool IsColorless( this ManaColor color )  => color == ManaColor.None || color == ManaColor.Colorless;

	public static bool Contains( this ManaColor identity, ManaColor subset ) => ( identity & subset ) == subset;

	public static ManaColor ParseSymbol( string symbol )
	{
		if ( string.IsNullOrEmpty( symbol ) ) return ManaColor.None;
		if ( symbol.Length != 1 ) return ManaColor.None;

		return symbol[0] switch
		{
			'W' or 'w' => ManaColor.White,
			'U' or 'u' => ManaColor.Blue,
			'B' or 'b' => ManaColor.Black,
			'R' or 'r' => ManaColor.Red,
			'G' or 'g' => ManaColor.Green,
			'C' or 'c' => ManaColor.Colorless,
			_ => ManaColor.None
		};
	}

	public static ManaColor ParseList( IEnumerable<string> symbols )
	{
		if ( symbols == null ) return ManaColor.None;
		var result = ManaColor.None;
		foreach ( var s in symbols )
			result |= ParseSymbol( s );
		return result;
	}
}