namespace DevHabit.Api.DTOs.Tags;

public sealed record TagCollectionDto
{
    public List<TagDto> Data { get; init; }
}
