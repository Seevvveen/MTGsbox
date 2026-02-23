using System;

namespace Sandbox.__Rewrite.Types;

public sealed class GameplayFace
{
	public Guid?   OracleId       { get; init; }
	public string  Name           { get; init; }
	public string  ManaCostRaw    { get; init; }
	public List<ManaCostSymbol> ManaCost { get; init; }
	public float   Cmc            { get; init; }
	public string  TypeLine       { get; init; }
	public List<string> Supertypes { get; init; }
	public List<string> CardTypes  { get; init; }
	public List<string> Subtypes   { get; init; }
	public string  OracleText     { get; init; }
	public ColorSet Colors        { get; init; }
	public ColorSet ColorIndicator { get; init; }
	public CardStat? Power        { get; init; }
	public CardStat? Toughness    { get; init; }
	public CardStat? Loyalty      { get; init; }
	public CardStat? Defense      { get; init; }
	public MtgLayout? Layout      { get; init; }
	// Artist, ImageUris, FlavorText, Watermark, IllustrationId — all moved to GameplayPrinting
}