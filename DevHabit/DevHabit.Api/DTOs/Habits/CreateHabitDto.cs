﻿using DevHabit.Api.Entities;
using FluentValidation;

namespace DevHabit.Api.DTOs.Habits;

public sealed record CreateHabitDto
{
	public required string Name { get; init; }
	public string? Description { get; init; }
	public required HabityType Type { get; init; }
	public required FrequencyDto Frequency { get; init; }
	public required TargetDto Target { get; init; }
	public DateOnly? EndDate { get; init; }
	public MilestoneDto? Milestone { get; init; }
}
