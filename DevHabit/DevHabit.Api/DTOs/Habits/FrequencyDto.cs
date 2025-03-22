using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Habits;

public sealed record FrequencyDto
{
    public required FrequencyType Type { get; set; }
    public required int TimesPerPeriod { get; set; }
}
