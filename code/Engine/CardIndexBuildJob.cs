#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Sandbox.Diagnostics;

namespace Sandbox.Engine.StartUp;

/// <summary>
/// Startup-only job:
/// - Read bulk json file from FileSystem.Data
/// - Deserialize via Sandbox.Json (whitelist-safe)
/// - Build indexes in one pass
/// </summary>
public sealed class CardIndexBuildJob( TaskSource lifetime, string bulkFilePath )
{
	private readonly Logger _logger = new( "CardIndexBuildJob" );
	private TaskSource _lifetime = lifetime;
	private readonly string _bulkFilePath = bulkFilePath;

	private const int DefaultCapacity = 100_000;

	public readonly record struct Progress( int CardsProcessed, int CardsIndexed );
	public Action<Progress>? OnProgress { get; set; }

	public sealed class BuildResult
	{
		public required IReadOnlyDictionary<Guid, ScryfallCard> ById { get; init; }
		public required IReadOnlyDictionary<Guid, ScryfallCard> ByOracleId { get; init; }
		public required IReadOnlyDictionary<string, IReadOnlyList<Guid>> ByExactName { get; init; }
		public required int Processed { get; init; }
	}

	public Task<BuildResult> RunAsync( CancellationToken token )
	{
		_logger.Info( $"Building indices from: {_bulkFilePath}" );
		return _lifetime.RunInThreadAsync( () => BuildOnWorker( token ) );
	}

	private BuildResult BuildOnWorker( CancellationToken ct )
	{
		ct.ThrowIfCancellationRequested();

		if ( !FileSystem.Data.FileExists( _bulkFilePath ) )
			throw new Exception( $"Bulk file not found: {_bulkFilePath}" );

		// Whitelist-safe: read the json text then use Sandbox.Json to deserialize.
		string json = FileSystem.Data.ReadAllText( _bulkFilePath );
		ct.ThrowIfCancellationRequested();

		// Bulk files are a top-level array.
		var cards = Json.Deserialize<ScryfallCard[]>( json );
		ct.ThrowIfCancellationRequested();

		var byId = new Dictionary<Guid, ScryfallCard>( Math.Max( DefaultCapacity, cards.Length ) );
		var byOracleId = new Dictionary<Guid, ScryfallCard>( Math.Max( DefaultCapacity, cards.Length ) );
		var byExactNameMutable = new Dictionary<string, List<Guid>>( StringComparer.OrdinalIgnoreCase );

		int processed = 0;
		int indexed = 0;

		foreach ( var card in cards )
		{
			ct.ThrowIfCancellationRequested();
			processed++;

			if ( card is null )
				continue;

			if ( !TryGetValidGuid( card.Id, out var id ) )
				continue;

			byId[id] = card;
			indexed++;

			if ( TryGetValidGuid( card.OracleId, out var oracleId ) )
				byOracleId[oracleId] = card;

			var name = card.Name;
			if ( !string.IsNullOrWhiteSpace( name ) )
			{
				if ( !byExactNameMutable.TryGetValue( name, out var list ) )
				{
					list = new List<Guid>( 1 );
					byExactNameMutable[name] = list;
				}
				list.Add( id );
			}

			if ( (processed % 25_000) == 0 )
				OnProgress?.Invoke( new Progress( processed, indexed ) );
		}

		var byExactName = new Dictionary<string, IReadOnlyList<Guid>>( byExactNameMutable.Count, StringComparer.OrdinalIgnoreCase );
		foreach ( var kvp in byExactNameMutable )
			byExactName[kvp.Key] = kvp.Value.ToArray();

		return new BuildResult
		{
			ById = byId,
			ByOracleId = byOracleId,
			ByExactName = byExactName,
			Processed = processed
		};
	}

	private static bool TryGetValidGuid( Guid? value, out Guid guid )
	{
		if ( value.HasValue && value.Value != Guid.Empty )
		{
			guid = value.Value;
			return true;
		}

		guid = Guid.Empty;
		return false;
	}
}
