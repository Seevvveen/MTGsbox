namespace Sandbox.Game.Enums;

/// <summary>
/// TODO Cleanup
/// </summary>
public sealed class ManaCost
{
	public string Raw { get; }
	public IReadOnlyList<CardSymbol> Symbols { get; }
    
	// Cached derived values
	private readonly Lazy<double> _manaValue;
	private readonly Lazy<ColorIdentity> _colorIdentity;
    
	public ManaCost(string raw, IReadOnlyList<CardSymbol> symbols)
	{
		Raw = raw;
		Symbols = symbols;
        
		_manaValue = new Lazy<double>(() => 
			Symbols.Sum(s => s.ManaValue));
            
		_colorIdentity = new Lazy<ColorIdentity>(() =>
		{
			var identity = ColorIdentity.None;
			foreach (var symbol in Symbols)
			{
				foreach (var color in symbol.Colors)
				{
					identity |= color switch
					{
						"W" => ColorIdentity.White,
						"U" => ColorIdentity.Blue,
						"B" => ColorIdentity.Black,
						"R" => ColorIdentity.Red,
						"G" => ColorIdentity.Green,
						_ => ColorIdentity.None
					};
				}
			}
			return identity;
		});
	}
    
	public double ManaValue => _manaValue.Value;
	public ColorIdentity ColorIdentity => _colorIdentity.Value;
    
	public int GenericManaCost => Symbols
	                              .Where(s => !s.Hybrid && s.Colors.Count == 0 && s.RepresentsMana)
	                              .Sum(s => (int)s.ManaValue);
    
	public bool ContainsX => Symbols.Any(s => s.Symbol == "{X}");
	public bool HasPhyrexianMana => Symbols.Any(s => s.Phyrexian);
	public bool HasHybridMana => Symbols.Any(s => s.Hybrid);
    
	public override string ToString() => Raw;
}
