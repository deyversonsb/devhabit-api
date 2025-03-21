using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Habits;

public sealed record HabitCollectionDto
{
    public List<HabitDto> Data { get; init; }
}
public sealed record HabitDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required HabityType Type { get; init; }
    public required FrequencyDto Frequency { get; init; }
    public required TargetDto Target { get; init; }
    public required HabitStatus Status { get; init; }
    public required bool IsArchived { get; init; }
    public DateOnly? EndDate { get; init; }
    public MilestoneDto? Milestone { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public DateTime? LastCompletedAtUtc { get; init; }
}

public sealed record FrequencyDto
{
    public required FrequencyType Type { get; set; }
    public required int TimesPerPeriod { get; set; }
}

public sealed record TargetDto
{
    public required int Value { get; set; }
    public required string Unit { get; set; }
}

public sealed class MilestoneDto
{
    public required int Target { get; set; }
    public required int Current { get; set; }
}
