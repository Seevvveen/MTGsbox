namespace Sandbox._Startup;

public static class GlobalCatalogs
{
	// Global, static lookup state.
	// These are populated by your build jobs and then used by gameplay/UI.
	public static CardsCatalog Cards { get; } = new();
	public static SymbolsCatalog Symbols { get; } = new();
}
