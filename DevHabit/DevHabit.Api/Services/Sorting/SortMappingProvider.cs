namespace DevHabit.Api.Services.Sorting;

public sealed class SortMappingProvider(
    IEnumerable<ISortMappingDefinition> sortMappingDefinitions)
{
    public SortMapping[] GetMappings<TSource, TDefinition>()
    {
        SortMappingDefinition<TSource, TDefinition>? sortMappingDefinition = sortMappingDefinitions
            .OfType<SortMappingDefinition<TSource, TDefinition>>()
            .FirstOrDefault();

        if (sortMappingDefinition is null)
        {
            throw new InvalidOperationException($"No sort mapping definition found for {typeof(TSource).Name} and {typeof(TDefinition).Name}");
        }

        return sortMappingDefinition.Mappings;
    }

    public bool ValidateMappings<TSource, TDefinition>(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
		{
			return true;
		}

        var sortFields = sort
            .Split(',')
			.Select(s => s.Trim().Split(' ')[0])
			.Where(s => !string.IsNullOrWhiteSpace(s))
			.ToList();

        SortMapping[] mapping = GetMappings<TSource, TDefinition>();

        return sortFields.All(f => mapping.Any(m => m.SortField.Equals(f, StringComparison.OrdinalIgnoreCase)));
	}
}
