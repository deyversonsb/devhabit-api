using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.DTOs.Habits;

public static class HabitExtensions
{
    public static IQueryable<Habit> ApplySearchOrFilter(this IQueryable<Habit> habits, HabitQueryParameters query)
        => habits
            .Where(h => query.Search == null ||
                        EF.Functions.Like(h.Name.ToLower(), $"%{query.Search}%") ||
                        h.Description != null && EF.Functions.Like(h.Description.ToLower(), $"%{query.Search}%"))
            .Where(h => query.Type == null || h.Type == query.Type)
            .Where(h => query.Status == null || h.Status == query.Status);
}
