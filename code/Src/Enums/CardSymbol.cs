#nullable enable
namespace Sandbox.Enums;

public sealed class CardSymbol
{
	public string Symbol { get; init; } = "";
	public string? SvgUri { get; init; }
	public string English { get; init; } = "";
	public double ManaValue { get; init; }
	public bool RepresentsMana { get; init; }
	public bool AppearsInManaCosts { get; init; }
	public bool Hybrid { get; init; }
	public bool Phyrexian { get; init; }
	public bool Funny { get; init; }
	public IReadOnlyList<string> Colors { get; init; } = Array.Empty<string>();
    
	// Convenience properties
	public bool IsGeneric => RepresentsMana && !Hybrid && Colors.Count == 0 && !Phyrexian;
	public bool IsColored => Colors.Count > 0;
	public bool IsVariable => Symbol is "{X}" or "{Y}" or "{Z}";
}
