using System;

namespace Sandbox.__Rewrite.Types;

public sealed class RelatedCard
{
	public Guid                  ScryfallId  { get; init; }
	public RelatedCardComponent  Component   { get; init; }
	public string                Name        { get; init; }
	public string                TypeLine    { get; init; }
}