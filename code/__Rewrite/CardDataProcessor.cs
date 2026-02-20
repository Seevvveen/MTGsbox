using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Sandbox.__Rewrite.Gameplay;
using Sandbox.__Rewrite.Scryfall;

namespace Sandbox.__Rewrite;

public static class CardDataProcessor
{
	private const string OracleJsonPath   = "scryfall/oracle_cards.json";
	private const string GameplayBlobPath = "scryfall/gameplay_cards.blob";
	private const string GameplayMetaPath = "scryfall/gameplay_cards.meta.json";

	private static readonly SemaphoreSlim ProcessLock = new( 1, 1 );

	public static async Task ProcessAsync( string oracleUpdatedAt, CancellationToken ct = default )
	{
		await ProcessLock.WaitAsync( ct );
		try
		{
			var blobPath = FileSystem.NormalizeFilename( GameplayBlobPath );
			var metaPath = FileSystem.NormalizeFilename( GameplayMetaPath );

			if ( !ShouldProcess( blobPath, metaPath, oracleUpdatedAt ) )
			{
				Log.Info( "Gameplay blob is already current; skipping processing." );
				return;
			}

			Log.Info( "Building gameplay blob from oracle_cards.json…" );
			var sw = System.Diagnostics.Stopwatch.StartNew();

			GameplayCardsBlob blob;
			try
			{
				blob = BuildGameplayBlob( ct );
			}
			catch ( Exception e )
			{
				Log.Error( $"Failed to build gameplay blob: {e}" );
				return;
			}

			blob.RebuildOracleIndex();

			if ( !WriteBlob( blobPath, blob ) )
				return;

			WriteGameplayMeta( metaPath, oracleUpdatedAt );

			sw.Stop();
			Log.Info( $"Gameplay blob written ({blob.Cards.Count} cards) in {sw.ElapsedMilliseconds} ms." );
		}
		finally
		{
			ProcessLock.Release();
		}
	}

	private static bool ShouldProcess( string blobPath, string metaPath, string oracleUpdatedAt )
	{
		if ( !FileSystem.Data.FileExists( blobPath ) )
			return true;

		var meta = FileSystem.Data.ReadJsonOrDefault<GameplayMeta>( metaPath );
		if ( meta == null || string.IsNullOrWhiteSpace( meta.ProcessedFromOracleUpdatedAt ) )
			return true;

		return !string.Equals( meta.ProcessedFromOracleUpdatedAt, oracleUpdatedAt, StringComparison.Ordinal );
	}

	private static GameplayCardsBlob BuildGameplayBlob( CancellationToken ct )
	{
		ct.ThrowIfCancellationRequested();

		string oraclePath = FileSystem.NormalizeFilename( OracleJsonPath );

		if ( !FileSystem.Data.FileExists( oraclePath ) )
			throw new FileNotFoundException( $"oracle_cards.json not found at {oraclePath}." );

		using var stream = FileSystem.Data.OpenRead( oraclePath );

		var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = false };

		// Whitelist-safe: one-time parse when rebuilding blob.
		var cards = JsonSerializer.Deserialize<List<ScryfallCard>>( stream, options );
		if ( cards == null )
			throw new Exception( "Failed to deserialize oracle_cards.json (null result)." );

		var blob = new GameplayCardsBlob();

		// Fix #2: overwrite in-place
		var oracleSidToIndex = new Dictionary<int, int>();
		var oracleSidToLangSid = new Dictionary<int, int>();

		int skipped = 0;
		int replacedForEnglish = 0;

		for ( int i = 0; i < cards.Count; i++ )
		{
			ct.ThrowIfCancellationRequested();

			var card = cards[i];
			if ( card == null ) continue;

			var layout = CardLayoutParser.Parse( card.Layout );

			if ( layout == CardLayout.ReversibleCard )
			{
				if ( card.CardFaces == null || card.CardFaces.Count == 0 )
				{
					skipped++;
					continue;
				}

				foreach ( var face in card.CardFaces )
				{
					if ( face == null || string.IsNullOrWhiteSpace( face.OracleId ) )
					{
						skipped++;
						continue;
					}

					var rec = BuildFaceRecord( blob, card, face, layout );
					AddOrReplaceByLang_InPlace( blob, oracleSidToIndex, oracleSidToLangSid, rec, ref replacedForEnglish );
				}

				continue;
			}

			if ( string.IsNullOrWhiteSpace( card.OracleId ) )
			{
				skipped++;
				continue;
			}

			var r = BuildCardRecord( blob, card, layout );
			AddOrReplaceByLang_InPlace( blob, oracleSidToIndex, oracleSidToLangSid, r, ref replacedForEnglish );
		}

		if ( replacedForEnglish > 0 )
			Log.Info( $"Replaced {replacedForEnglish} entries to prefer English versions (lang=\"en\")." );

		if ( skipped > 0 )
			Log.Info( $"Skipped {skipped} entries (missing oracle_id, malformed reversible_card, etc.)." );
		
		Log.Info($"BuildGameplayBlob: cards={blob.Cards.Count}");
		return blob;
	}

	private static void AddOrReplaceByLang_InPlace(
		GameplayCardsBlob blob,
		Dictionary<int, int> oracleSidToIndex,
		Dictionary<int, int> oracleSidToLangSid,
		in GameplayCardsBlob.CardRecord rec,
		ref int replacedForEnglish )
	{
		if ( rec.OracleSid < 0 )
			return;

		bool candidateEn = string.Equals( blob.GetString( rec.LangSid ), "en", StringComparison.OrdinalIgnoreCase );

		if ( oracleSidToIndex.TryGetValue( rec.OracleSid, out var existingIndex ) )
		{
			var existingLangSid = oracleSidToLangSid[rec.OracleSid];
			bool existingEn = string.Equals( blob.GetString( existingLangSid ), "en", StringComparison.OrdinalIgnoreCase );

			// Prefer en if candidate is en and existing isn't.
			if ( candidateEn && !existingEn )
			{
				blob.Cards[existingIndex] = rec;
				oracleSidToLangSid[rec.OracleSid] = rec.LangSid;
				replacedForEnglish++;
			}

			return;
		}

		int newIndex = blob.Cards.Count;
		blob.Cards.Add( rec );
		oracleSidToIndex[rec.OracleSid] = newIndex;
		oracleSidToLangSid[rec.OracleSid] = rec.LangSid;
	}

	private static GameplayCardsBlob.CardRecord BuildCardRecord( GameplayCardsBlob blob, ScryfallCard src, CardLayout layout )
	{
		blob.AppendKeywords( src.Keywords, out var kwStart, out var kwCount );

		return new GameplayCardsBlob.CardRecord
		{
			Id = src.Id.ToGuid(),

			OracleSid = blob.Intern( src.OracleId ),
			LangSid   = blob.Intern( src.Lang ),

			Layout = (int)layout,

			NameSid       = blob.Intern( src.Name ),
			ManaCostSid   = blob.Intern( src.ManaCost ),
			TypeLineSid   = blob.Intern( src.TypeLine ),
			OracleTextSid = blob.Intern( src.OracleText ),

			Cmc = (int)MathF.Round( (float)src.Cmc ),

			Colors         = (int)ManaColorExtensions.ParseList( src.Colors ),
			ColorIdentity  = (int)ManaColorExtensions.ParseList( src.ColorIdentity ),
			ColorIndicator = (int)ManaColorExtensions.ParseList( src.ColorIndicator ),
			ProducedMana   = (int)ManaColorExtensions.ParseList( src.ProducedMana ),

			PowerSid     = blob.Intern( src.Power ),
			ToughnessSid = blob.Intern( src.Toughness ),
			LoyaltySid   = blob.Intern( src.Loyalty ),
			DefenseSid   = blob.Intern( src.Defense ),

			KeywordsStart = kwStart,
			KeywordsCount = kwCount
		};
	}

	private static GameplayCardsBlob.CardRecord BuildFaceRecord( GameplayCardsBlob blob, ScryfallCard parent, ScryfallCardFace face, CardLayout parentLayout )
	{
		var faceColors = ManaColorExtensions.ParseList( face.Colors );
		if ( faceColors == ManaColor.None )
			faceColors = ManaColorExtensions.ParseList( parent.Colors );

		blob.AppendKeywords( parent.Keywords, out var kwStart, out var kwCount );

		var faceLayout = CardLayoutParser.Parse( face.Layout );
		if ( faceLayout == CardLayout.Unknown )
			faceLayout = parentLayout;

		return new GameplayCardsBlob.CardRecord
		{
			Id = parent.Id.ToGuid(),

			OracleSid = blob.Intern( face.OracleId ),
			LangSid   = blob.Intern( parent.Lang ),

			Layout = (int)faceLayout,

			NameSid       = blob.Intern( face.Name ),
			ManaCostSid   = blob.Intern( face.ManaCost ),
			TypeLineSid   = blob.Intern( face.TypeLine ?? parent.TypeLine ),
			OracleTextSid = blob.Intern( face.OracleText ),

			Cmc = face.Cmc.HasValue ? (int)MathF.Round( (float)face.Cmc.Value ) : (int)MathF.Round( (float)parent.Cmc ),

			Colors         = (int)faceColors,
			ColorIdentity  = (int)ManaColorExtensions.ParseList( parent.ColorIdentity ),
			ColorIndicator = (int)ManaColorExtensions.ParseList( face.ColorIndicator ),
			ProducedMana   = (int)ManaColorExtensions.ParseList( parent.ProducedMana ),

			PowerSid     = blob.Intern( face.Power ),
			ToughnessSid = blob.Intern( face.Toughness ),
			LoyaltySid   = blob.Intern( face.Loyalty ),
			DefenseSid   = blob.Intern( face.Defense ),

			KeywordsStart = kwStart,
			KeywordsCount = kwCount
		};
	}

	private static bool WriteBlob( string blobPath, GameplayCardsBlob blob )
	{
		string tmpPath = blobPath + ".tmp";

		try
		{
			var bs = ByteStream.Create( 1024 * 1024 );
			try
			{
				bs.Write<int>( blob.Version ); // header
				var writer = new BlobData.Writer { Stream = bs };
				blob.Serialize( ref writer );

				var bytes = writer.Stream.ToArray();

				if ( bytes.Length <= 4 )
					throw new Exception( $"Gameplay blob serialized to {bytes.Length} bytes (expected > 4)." );

				FileSystem.Data.WriteAllBytes( tmpPath, bytes );
			}
			finally
			{
				bs.Dispose();
			}

			// Promote tmp -> final
			if ( FileSystem.Data.FileExists( blobPath ) )
				FileSystem.Data.DeleteFile( blobPath );

			// If you don’t want StreamCopyFileAsync, you can just ReadAllBytes+WriteAllBytes,
			// but since you already have StreamCopyFileAsync in DiskDataSystem, reuse it.
			using ( var input = FileSystem.Data.OpenRead( tmpPath ) )
				using ( var output = FileSystem.Data.OpenWrite( blobPath, FileMode.Create ) )
				{
					input.CopyTo( output, 1024 * 1024 );
					output.Flush();
				}

			FileSystem.Data.DeleteFile( tmpPath );
			return true;
		}
		catch ( Exception e )
		{
			Log.Error( $"Failed to write gameplay blob: {e}" );
			try { if ( FileSystem.Data.FileExists( tmpPath ) ) FileSystem.Data.DeleteFile( tmpPath ); } catch {}
			return false;
		}
	}


	private static void WriteGameplayMeta( string metaPath, string oracleUpdatedAt )
	{
		try
		{
			var meta = new GameplayMeta
			{
				ProcessedFromOracleUpdatedAt = oracleUpdatedAt,
				ProcessedAt                  = DateTimeOffset.UtcNow.ToString( "O" ),
			};

			FileSystem.Data.WriteJson( metaPath, meta );
		}
		catch ( Exception e )
		{
			Log.Warning( $"Failed to write gameplay meta: {e.Message}" );
		}
	}

	private sealed class GameplayMeta
	{
		public string ProcessedFromOracleUpdatedAt { get; init; }
		public string ProcessedAt { get; init; }
	}
}