using System.Threading.Tasks;

/// <summary>
/// Exposes card data to dependent components.
/// </summary>
public interface ICardProvider
{
	Card Card { get; }
	Task WhenReady { get; }
	bool IsReady { get; }
}
