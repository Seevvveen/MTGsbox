using System;
using System.Collections.Generic;
using Sandbox;

namespace Sandbox.__Rewrite.Gameplay;

/// <summary>
/// Binary gameplay dataset.
/// Whitelist-safe: uses Sandbox.ByteStream via BlobData.
/// Stores:
/// - string table
/// - keyword string-id pool
/// - card records (string ids + packed ints)
/// Builds oracleSid->cardIndex map on demand (not persisted).
/// </summary>
public sealed class GameplayCardsBlob : BlobData
{
	public override int Version => 1;

	// -------------------------
	// String table
	// -------------------------
	private readonly List<string> _strings = new();
	private readonly Dictionary<string, int> _stringToId = new( StringComparer.Ordinal );

	public int Intern( string s )
	{
		if ( string.IsNullOrEmpty( s ) ) return -1;
		if ( _stringToId.TryGetValue( s, out var id ) ) return id;

		id = _strings.Count;
		_strings.Add( s );
		_stringToId.Add( s, id );
		return id;
	}

	public string GetString( int sid ) => (sid >= 0 && sid < _strings.Count) ? _strings[sid] : null;

	// -------------------------
	// Keywords pool (FIXED)
	// -------------------------
	private readonly List<int> _keywordSids = new();

	public void AppendKeywords( List<string> keywords, out int start, out int count )
	{
		if ( keywords == null || keywords.Count == 0 )
		{
			start = 0;
			count = 0;
			return;
		}

		start = _keywordSids.Count;
		count = keywords.Count;

		for ( int i = 0; i < keywords.Count; i++ )
			_keywordSids.Add( Intern( keywords[i] ) );
	}

	// -------------------------
	// Cards
	// -------------------------
	public readonly List<CardRecord> Cards = new();

	// oracle string-id -> card index (rebuilt after load/build)
	private readonly Dictionary<int, int> _oracleSidToIndex = new();

	public struct CardRecord
	{
		public Guid Id;

		public int OracleSid;
		public int LangSid;

		public int Layout; // int for stability

		public int NameSid;
		public int ManaCostSid;
		public int TypeLineSid;
		public int OracleTextSid;

		public int Cmc;

		public int Colors;
		public int ColorIdentity;
		public int ColorIndicator;
		public int ProducedMana;

		// Raw combat-ish fields stored as string sids (avoid CombatValue serialize dependencies)
		public int PowerSid;
		public int ToughnessSid;
		public int LoyaltySid;
		public int DefenseSid;

		// Keywords slice into _keywordSids
		public int KeywordsStart;
		public int KeywordsCount;
	}

	public void Clear()
	{
		_strings.Clear();
		_stringToId.Clear();
		_keywordSids.Clear();
		Cards.Clear();
		_oracleSidToIndex.Clear();
	}

	public void RebuildOracleIndex()
	{
		_oracleSidToIndex.Clear();
		for ( int i = 0; i < Cards.Count; i++ )
		{
			var sid = Cards[i].OracleSid;
			if ( sid >= 0 )
				_oracleSidToIndex[sid] = i;
		}
	}

	public bool TryGetIndexByOracleId( string oracleId, out int index )
	{
		index = -1;
		if ( string.IsNullOrEmpty( oracleId ) ) return false;
		if ( !_stringToId.TryGetValue( oracleId, out var sid ) ) return false;
		return _oracleSidToIndex.TryGetValue( sid, out index );
	}

	// -------------------------
	// BlobData
	// -------------------------
	public override void Serialize( ref Writer writer )
	{
		var s = writer.Stream;

		// Strings
		s.Write<int>( _strings.Count );
		for ( int i = 0; i < _strings.Count; i++ )
			s.Write( _strings[i] );

		// Keyword pool (FIXED: includeCount=true so ReadArray<int>() works)
		s.WriteArray<int>( _keywordSids.ToArray() ); // writes count then data

		// Cards
		s.Write<int>( Cards.Count );
		for ( int i = 0; i < Cards.Count; i++ )
		{
			var c = Cards[i];

			s.Write<Guid>( c.Id );

			s.Write<int>( c.OracleSid );
			s.Write<int>( c.LangSid );

			s.Write<int>( c.Layout );

			s.Write<int>( c.NameSid );
			s.Write<int>( c.ManaCostSid );
			s.Write<int>( c.TypeLineSid );
			s.Write<int>( c.OracleTextSid );

			s.Write<int>( c.Cmc );

			s.Write<int>( c.Colors );
			s.Write<int>( c.ColorIdentity );
			s.Write<int>( c.ColorIndicator );
			s.Write<int>( c.ProducedMana );

			s.Write<int>( c.PowerSid );
			s.Write<int>( c.ToughnessSid );
			s.Write<int>( c.LoyaltySid );
			s.Write<int>( c.DefenseSid );

			s.Write<int>( c.KeywordsStart );
			s.Write<int>( c.KeywordsCount );
		}

		// ByteStream is a struct — assign the mutated copy back so the caller sees the writes.
		writer.Stream = s;
	}

	public override void Deserialize( ref Reader reader )
	{
		var s = reader.Stream;

		Clear();

		// Strings
		int stringCount = s.Read<int>();
		for ( int i = 0; i < stringCount; i++ )
		{
			var str = s.Read<string>( "" );
			_strings.Add( str );
			if ( !_stringToId.ContainsKey( str ) )
				_stringToId.Add( str, i );
		}

		// Keyword pool (FIXED: paired with WriteArray<int>())
		var kw = s.ReadArray<int>();
		if ( kw != null && kw.Length > 0 )
			_keywordSids.AddRange( kw );

		// Cards
		int cardCount = s.Read<int>();
		if ( cardCount > 0 )
			Cards.Capacity = cardCount;

		for ( int i = 0; i < cardCount; i++ )
		{
			CardRecord c = default;

			c.Id = s.Read<Guid>();

			c.OracleSid = s.Read<int>();
			c.LangSid = s.Read<int>();

			c.Layout = s.Read<int>();

			c.NameSid = s.Read<int>();
			c.ManaCostSid = s.Read<int>();
			c.TypeLineSid = s.Read<int>();
			c.OracleTextSid = s.Read<int>();

			c.Cmc = s.Read<int>();

			c.Colors = s.Read<int>();
			c.ColorIdentity = s.Read<int>();
			c.ColorIndicator = s.Read<int>();
			c.ProducedMana = s.Read<int>();

			c.PowerSid = s.Read<int>();
			c.ToughnessSid = s.Read<int>();
			c.LoyaltySid = s.Read<int>();
			c.DefenseSid = s.Read<int>();

			c.KeywordsStart = s.Read<int>();
			c.KeywordsCount = s.Read<int>();

			Cards.Add( c );
		}

		RebuildOracleIndex();

		// ByteStream is a struct — assign the mutated copy back so the caller's stream position is current.
		reader.Stream = s;
	}
}