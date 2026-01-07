#nullable enable
namespace Sandbox.Catalog.Builders;


public interface ICollector<TDef, TResult>
{
	void Add( TDef def );
	TResult Build();
}

/// <summary>
/// provides global build method
/// </summary>
public static class BuildCore
{
	public static TResult Build<TDto, TDef, TResult>(
		IEnumerable<TDto> source,
		Func<TDto, TDef> map,
		ICollector<TDef, TResult> collector )
	{
		foreach ( var dto in source )
		{
			var def = map( dto );
			collector.Add( def );
		}

		return collector.Build();
	}
}
