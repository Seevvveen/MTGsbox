namespace Sandbox.__Rewrite.Types;

public readonly struct CardStat
{
	public readonly StatType Type;
	public readonly int      Value;    // Only meaningful when Type == Numeric or Combined
	public readonly int      Modifier; // The N in *+N when Type == Combined

	public static readonly CardStat Variable = new( StatType.Variable, 0, 0 );
	public static CardStat X = new( StatType.X, 0, 0 );
	
	// Reusing the StatType enum reference explicitly
	public enum StatType { Numeric, Variable, Combined, X, Any }

	private CardStat( StatType type, int value, int modifier )
	{
		Type     = (StatType)(byte)type;
		Value    = value;
		Modifier = modifier;
	}

	public static CardStat FromNumeric( int value )      => new( StatType.Numeric, value, 0 );
	public static CardStat FromCombined( int modifier )  => new( StatType.Combined, 0, modifier );

	public static CardStat Parse( string raw )
	{
		if ( string.IsNullOrEmpty( raw ) )                return Variable;
		if ( raw == "*" )                                  return Variable;
		if ( raw == "X" )                                  return X;
		if ( int.TryParse( raw, out int n ) )              return FromNumeric( n );

		// *+1 or 1+* patterns
		if ( raw.Contains( '+' ) )
		{
			var parts = raw.Split( '+' );
			foreach ( var part in parts )
				if ( int.TryParse( part.Trim(), out int mod ) )
					return FromCombined( mod );
		}

		return Variable; // fallback for anything exotic
	}

	public override string ToString() => Type switch
	{
		StatType.Numeric  => Value.ToString(),
		StatType.Variable => "*",
		StatType.Combined => $"*+{Modifier}",
		StatType.X        => "X",
		_                 => "?"
	};
}