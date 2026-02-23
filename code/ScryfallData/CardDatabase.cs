using System;
using Sandbox.ScryfallData.Types;

namespace Sandbox.ScryfallData;

public static class CardDatabase
{
    [SkipHotload] private static CardBlobReader     _cards;
    [SkipHotload] private static PrintingBlobReader _printings;

    public static bool IsReady        => _cards     != null;
    public static bool PrintingsReady => _printings != null;

    public static void Initialize( CardBlobReader cards )
    {
        _cards = cards;
        Log.Info( $"CardDatabase ready — {cards.CardCount} oracle cards indexed." );
    }

    public static void InitializePrintings( PrintingBlobReader printings )
    {
        _printings = printings;
        Log.Info( $"CardDatabase ready — {printings.PrintingCount} printings indexed." );
    }

    public static GameplayCard Fetch( Guid oracleId )
        => _cards?.Fetch( oracleId );

    public static List<GameplayCard> FetchBatch( IEnumerable<Guid> oracleIds )
        => _cards?.FetchBatch( oracleIds ) ?? new();

    public static bool Contains( Guid oracleId )
        => _cards?.Contains( oracleId ) ?? false;

    public static int CardCount
        => _cards?.CardCount ?? 0;

    public static GameplayPrinting FetchPrinting( Guid scryfallId )
        => _printings?.Fetch( scryfallId );

    public static GameplayPrinting FetchPreferredPrinting( Guid oracleId )
        => _printings?.FetchPreferred( oracleId );

    public static List<GameplayPrinting> FetchAllPrintings( Guid oracleId )
        => _printings?.FetchAllForOracle( oracleId ) ?? new();

    public static bool ContainsPrinting( Guid scryfallId )
        => _printings?.Contains( scryfallId ) ?? false;

    public static int PrintingCount
        => _printings?.PrintingCount ?? 0;

    public static (GameplayCard Card, GameplayPrinting Printing) FetchWithPreferredPrinting( Guid oracleId )
        => (_cards?.Fetch( oracleId ), _printings?.FetchPreferred( oracleId ));

    public static (GameplayCard Card, List<GameplayPrinting> Printings) FetchWithAllPrintings( Guid oracleId )
        => (_cards?.Fetch( oracleId ), _printings?.FetchAllForOracle( oracleId ) ?? new());
}