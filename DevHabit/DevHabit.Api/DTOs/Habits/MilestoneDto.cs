namespace DevHabit.Api.DTOs.Habits;

public sealed class MilestoneDto
{
    public required int Target { get; set; }
    public required int Current { get; set; }
}
