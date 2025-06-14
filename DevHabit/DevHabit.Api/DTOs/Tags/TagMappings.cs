﻿using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Tags;

internal static class TagMappings
{
    public static TagDto ToDto(this Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            CreatedAtUtc = tag.CreatedAtUtc,
            UpdatedAtUtc = tag.UpdatedAtUtc
        };
    }
    public static Tag ToEntity(this CreateTagDto dto, string userId)
    {
        return new Tag
        {
            Id = $"t_{Guid.CreateVersion7():N}", // Interpolation to remove hyphens is the same {Guid.CreateVersion7().ToString("N")}
            UserId = userId,
            Name = dto.Name,
            Description = dto.Description,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
    public static void UpdateFromDto(this Tag tag, UpdateTagDto dto)
    {
        tag.Name = dto.Name;
        tag.Description = dto.Description;
        tag.UpdatedAtUtc = DateTime.UtcNow;
    }
}
