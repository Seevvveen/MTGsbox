using Sandbox.Enums;

namespace Sandbox.Types;

public class Loyalty
{
	public string Raw;
	public CardValueType Type;
	public int Value = 0;
	
	private Loyalty(string raw, CardValueType type, int value)
	{
		Raw = raw;
		Type = type;
		Value = value;
	}

	public static Loyalty Parse( string value )
	{
		if ( string.IsNullOrEmpty( value ) )
			return new Loyalty( value, CardValueType.Empty, 0 );

		if ( int.TryParse( value, out int numericValue ) )
			return new Loyalty( value, CardValueType.Numeric, numericValue );

		return value switch
		{
			"X" => new Loyalty( value, CardValueType.Variable, 0 ),
			"*" => new Loyalty( value, CardValueType.Special, 0 ),
			_ when value.Contains( '+' ) || value.Contains( 'd' ) =>
				new Loyalty( value, CardValueType.Formula, 0 ),
			_ => new Loyalty( value, CardValueType.Unknown, 0 )
		};
		
	}

	public override string ToString() => Raw ?? "null";
}
