﻿using FluentValidation;
using Microsoft.AspNetCore.Rewrite;

namespace DevHabit.Api.DTOs.Tags;

public sealed record CreateTagDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}
