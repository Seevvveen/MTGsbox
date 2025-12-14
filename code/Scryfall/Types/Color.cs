namespace Sandbox.Scryfall.Types;

public enum Color
{
	White,
	Blue,
	Black,
	Red,
	Green
}

public readonly struct ColorIdentity : IEquatable<ColorIdentity>
{
	private readonly byte _mask; // WUBRG

	private ColorIdentity( byte mask )
		=> _mask = mask;

	public static readonly ColorIdentity None = new ColorIdentity( 0 );
	public static readonly ColorIdentity White = new ColorIdentity( 1 << 0 );
	public static readonly ColorIdentity Blue = new ColorIdentity( 1 << 1 );
	public static readonly ColorIdentity Black = new ColorIdentity( 1 << 2 );
	public static readonly ColorIdentity Red = new ColorIdentity( 1 << 3 );
	public static readonly ColorIdentity Green = new ColorIdentity( 1 << 4 );

	public bool IsColorless => _mask == 0;

	public bool Has( Color color )
	{
		switch ( color )
		{
			case Color.White: return (_mask & (1 << 0)) != 0;
			case Color.Blue: return (_mask & (1 << 1)) != 0;
			case Color.Black: return (_mask & (1 << 2)) != 0;
			case Color.Red: return (_mask & (1 << 3)) != 0;
			case Color.Green: return (_mask & (1 << 4)) != 0;
			default: return false;
		}
	}

	public IEnumerable<Color> AsEnumerable()
	{
		if ( Has( Color.White ) ) yield return Color.White;
		if ( Has( Color.Blue ) ) yield return Color.Blue;
		if ( Has( Color.Black ) ) yield return Color.Black;
		if ( Has( Color.Red ) ) yield return Color.Red;
		if ( Has( Color.Green ) ) yield return Color.Green;
	}

	public static ColorIdentity FromScryfall( IEnumerable<string> codes )
	{
		if ( codes == null )
			return None;

		byte mask = 0;

		foreach ( var code in codes )
		{
			if ( string.IsNullOrEmpty( code ) ) continue;

			switch ( code[0] )
			{
				case 'W': mask |= 1 << 0; break;
				case 'U': mask |= 1 << 1; break;
				case 'B': mask |= 1 << 2; break;
				case 'R': mask |= 1 << 3; break;
				case 'G': mask |= 1 << 4; break;
			}
		}
		return new ColorIdentity( mask );
	}

	public override string ToString()
	{
		var list = new List<char>();

		if ( Has( Color.White ) ) list.Add( 'W' );
		if ( Has( Color.Blue ) ) list.Add( 'U' );
		if ( Has( Color.Black ) ) list.Add( 'B' );
		if ( Has( Color.Red ) ) list.Add( 'R' );
		if ( Has( Color.Green ) ) list.Add( 'G' );

		return new string( list.ToArray() );
	}

	public bool Equals( ColorIdentity other ) => _mask == other._mask;
	public override bool Equals( object obj ) => obj is ColorIdentity other && Equals( other );
	public override int GetHashCode() => _mask.GetHashCode();

	public static bool operator ==( ColorIdentity left, ColorIdentity right ) => left.Equals( right );
	public static bool operator !=( ColorIdentity left, ColorIdentity right ) => !left.Equals( right );
}
