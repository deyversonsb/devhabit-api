namespace DevHabit.Api.Services.Sorting;

public sealed class SortMappingDefinition<TSource, TDefinition> : ISortMappingDefinition
{
    public required SortMapping[] Mappings { get; init; }
}
