namespace DevHabit.Api.DTOs.Habits;


public sealed record TargetDto
{
    public required int Value { get; set; }
    public required string Unit { get; set; }
}
