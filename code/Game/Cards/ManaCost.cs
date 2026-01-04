using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sandbox.Game.Enums;

namespace Sandbox.Game.Cards;

/// <summary>
/// Parsed mana cost from Scryfall-style strings like "{2}{W}{U}".
/// Single source of truth that can be interpreted in two ways:
/// - Casting requirements (hybrid is OR)
/// - Devotion / identity-style pip counts (hybrid contributes to both colors)
/// </summary>
public sealed class ManaCost
{
	public string RawValue { get; }
	public List<ManaSymbol> Symbols { get; }
	public bool IsAbsent { get; }

	private ManaCost( string rawValue, List<ManaSymbol> symbols, bool isAbsent )
	{
		RawValue = rawValue;
		Symbols = symbols;
		IsAbsent = isAbsent;
	}

	// {2}{W}{U} -> ["2","W","U"]
	private static readonly Regex SymbolRegex =
		new Regex( @"\{([^}]+)\}", RegexOptions.Compiled );

	public static ManaCost Parse( string manaCost )
	{
		// Empty string means absent mana cost (lands, suspend-only, etc.)
		if ( string.IsNullOrEmpty( manaCost ) )
		{
			return new ManaCost(
				"",
				new List<ManaSymbol>( 0 ),
				true
			);
		}

		var matches = SymbolRegex.Matches( manaCost );
		var symbols = new List<ManaSymbol>( matches.Count );

		foreach ( Match match in matches )
		{
			symbols.Add( ManaSymbol.Parse( match.Groups[1].Value ) );
		}

		return new ManaCost(
			manaCost,
			symbols,
			false
		);
	}

	/// <summary>Total mana value (formerly CMC). X/Y/Z count as 0.</summary>
	public int GetManaValue()
	{
		if ( IsAbsent )
			return 0;

		int total = 0;
		foreach ( var symbol in Symbols )
			total += symbol.GetNumericValue();

		return total;
	}

	public bool ContainsX()
	{
		foreach ( var symbol in Symbols )
		{
			if ( symbol.Type == ManaSymbolType.Variable )
				return true;
		}

		return false;
	}

	public bool ContainsColor( ColorIdentity color )
	{
		foreach ( var symbol in Symbols )
		{
			if ( (symbol.Colors & color) != 0 )
				return true;
		}

		return false;
	}

	/// <summary>
	/// Count of "true pips" like {W}{U}{B}{R}{G} only (not hybrid/phyrexian).
	/// Useful if you need strict colored symbol count.
	/// </summary>
	public int GetSingleColorPipCount()
	{
		int count = 0;

		foreach ( var symbol in Symbols )
		{
			if ( symbol.Type == ManaSymbolType.Colored )
				count++;
		}

		return count;
	}

	// ----------------------------
	// Casting requirements (payment)
	// ----------------------------

	/// <summary>
	/// Payment-focused requirements for casting.
	/// Each PipGroup is ONE pip, but represented as an OR-mask of acceptable colors:
	/// - {G}   => G
	/// - {W/U} => W|U
	/// - {W/P} => W (life option not modeled here)
	/// Generic mana is returned separately.
	/// </summary>
	public ManaRequirements GetCastingRequirements(
		bool includeHybrid = true,
		bool includePhyrexian = true )
	{
		if ( IsAbsent )
			return new ManaRequirements( 0, new List<ColorIdentity>( 0 ) );

		int generic = 0;
		var pipGroups = new List<ColorIdentity>();

		foreach ( var symbol in Symbols )
		{
			switch ( symbol.Type )
			{
				case ManaSymbolType.Generic:
					generic += symbol.GenericAmount;
					break;

				case ManaSymbolType.Colored:
					pipGroups.Add( symbol.Colors ); // exactly one color
					break;

				case ManaSymbolType.Hybrid:
					if ( includeHybrid )
						pipGroups.Add( symbol.Colors ); // OR group
					break;

				case ManaSymbolType.Phyrexian:
					if ( includePhyrexian )
						pipGroups.Add( symbol.Colors ); // OR group (color OR life; life not modeled)
					break;

				default:
					break;
			}
		}

		return new ManaRequirements( generic, pipGroups );
	}

	// ----------------------------
	// Devotion / identity pip counts
	// ----------------------------

	/// <summary>
	/// Devotion-focused pip counts.
	/// Hybrid contributes to each of its colors (W/U adds +1 to W and +1 to U).
	/// Phyrexian contributes to its color.
	/// Generic and variable symbols do not contribute.
	/// </summary>
	public Dictionary<ColorIdentity, int> GetDevotionPipCounts(
		bool includeHybrid = true,
		bool includePhyrexian = true )
	{
		var devotion = new Dictionary<ColorIdentity, int>();

		if ( IsAbsent )
			return devotion;

		foreach ( var symbol in Symbols )
		{
			bool countsForDevotion =
				symbol.Type == ManaSymbolType.Colored ||
				(includeHybrid && symbol.Type == ManaSymbolType.Hybrid) ||
				(includePhyrexian && symbol.Type == ManaSymbolType.Phyrexian);

			if ( !countsForDevotion )
				continue;

			AddDevotion( devotion, symbol.Colors, ColorIdentity.W );
			AddDevotion( devotion, symbol.Colors, ColorIdentity.U );
			AddDevotion( devotion, symbol.Colors, ColorIdentity.B );
			AddDevotion( devotion, symbol.Colors, ColorIdentity.R );
			AddDevotion( devotion, symbol.Colors, ColorIdentity.G );
		}

		return devotion;
	}

	private static void AddDevotion(
		Dictionary<ColorIdentity, int> devotion,
		ColorIdentity symbolColors,
		ColorIdentity color )
	{
		if ( (symbolColors & color) == 0 )
			return;

		devotion[color] = devotion.GetValueOrDefault( color ) + 1;
	}

	public override string ToString() => RawValue;
}

/// <summary>
/// Casting/payment representation:
/// Generic = total generic mana required.
/// PipGroups = list of OR-masks, one per non-generic pip.
/// </summary>
public sealed record ManaRequirements(
	int Generic,
	List<ColorIdentity> PipGroups
);
