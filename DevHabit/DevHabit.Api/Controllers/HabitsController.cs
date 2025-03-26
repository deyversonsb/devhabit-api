using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Common;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.DTOs.HabitTags;
using DevHabit.Api.Entities;
using DevHabit.Api.Services;
using DevHabit.Api.Services.Sorting;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DevHabit.Api.Controllers;

[ApiController]
[Route("habits")]
public sealed class HabitsController(
    ApplicationDbContext dbContext,
    LinkService linkService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHabits(
        [FromQuery] HabitsQueryParameters query,
        SortMappingProvider sortMappingProvider,
        DataShapingService dataShapingService)
    {
        if (!sortMappingProvider.ValidateMappings<HabitDto, Habit>(query.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort parameter isn't valid. '{query.Sort}'");
        }

        if (!dataShapingService.Validate<HabitDto>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields parameter isn't valid. '{query.Fields}'");
        }

        query.Search ??= query.Search?.Trim().ToLower();

        SortMapping[] sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();

        IQueryable<HabitDto> habitsQuery = dbContext
            .Habits
            .ApplySearchOrFilter(query)
            .ApplySort(query.Sort, sortMappings)
            .Select(HabitQueries.ProjectToDto());

        int totalCount = await habitsQuery.CountAsync();

        List<HabitDto> habits = await habitsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var paginationResult = new PaginationResult<ExpandoObject>
        {
            Items = dataShapingService.ShapeCollectionData(
                habits,
                query.Fields,
                h => CreateLinksForHabit(h.Id, query.Fields)),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };

        paginationResult.Links = CreateLinksForHabits(
            query,
            paginationResult.HasNextPage,
            paginationResult.HasPreviousPage);

        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetHabit(
        string id,
        string? fields,
        DataShapingService dataShapingService)
    {
        if (!dataShapingService.Validate<HabitWithTagsDto>(fields))
        {
            return Problem(
            statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shaping fields parameter isn't valid. '{fields}'");
        }

        HabitWithTagsDto? habit = await dbContext
            .Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToDtoWithTags())
            .FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound();
        }

        ExpandoObject shapedHabitDto = dataShapingService.ShapeData(habit, fields);

        List<LinkDto> links = CreateLinksForHabit(id, fields);

        shapedHabitDto.TryAdd("links", links);

        return Ok(shapedHabitDto);
    }

    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit(
        CreateHabitDto createHabitDto,
        IValidator<CreateHabitDto> validator)
    {
        await validator.ValidateAndThrowAsync(createHabitDto);

        Habit habit = createHabitDto.ToEntity();

        dbContext.Habits.Add(habit);

        await dbContext.SaveChangesAsync();

        HabitDto habitDto = habit.ToDto();
        habitDto.Links = CreateLinksForHabit(habitDto.Id, null);

        return CreatedAtAction(nameof(GetHabit), new { id = habit.Id }, habitDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, UpdateHabitDto updateHabitDto)
    {
        Habit? habit = await dbContext.Habits.Where(h => h.Id == id).FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound();
        }

        habit.UpdateFromDto(updateHabitDto);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit(string id, JsonPatchDocument<HabitDto> patchDocument)
    {
        Habit? habit = await dbContext.Habits.Where(h => h.Id == id).FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound();
        }

        HabitDto habitDto = habit.ToDto();

        patchDocument.ApplyTo(habitDto, ModelState);

        if (!TryValidateModel(habitDto))
        {
            return ValidationProblem(ModelState);
        }

        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id)
    {
        Habit? habit = await dbContext.Habits.Where(h => h.Id == id).FirstOrDefaultAsync();

        if (habit is null)
        {
            return NotFound();
        }

        dbContext.Habits.Remove(habit);

        await dbContext.SaveChangesAsync();

        return NoContent();
    }
    private List<LinkDto> CreateLinksForHabits(
        HabitsQueryParameters habitsQueryParameters,
        bool hasNextPage,
        bool hasPreviousPage)
    {
        List<LinkDto> links =
        [
            linkService.Create(nameof(GetHabits), "self", HttpMethods.Get, new
            {
                page = habitsQueryParameters.Page,
                pageSize = habitsQueryParameters.PageSize,
                fields = habitsQueryParameters.Fields,
                q = habitsQueryParameters.Search,
                sort = habitsQueryParameters.Sort,
                type = habitsQueryParameters.Type,
                status = habitsQueryParameters.Status
            }),
            linkService.Create(nameof(CreateHabit), "create", HttpMethods.Post)
        ];

        if (hasNextPage)
        {
            links.Add(linkService.Create(nameof(GetHabits), "next-page", HttpMethods.Get, new
            {
                page = habitsQueryParameters.Page + 1,
                pageSize = habitsQueryParameters.PageSize,
                fields = habitsQueryParameters.Fields,
                q = habitsQueryParameters.Search,
                sort = habitsQueryParameters.Sort,
                type = habitsQueryParameters.Type,
                status = habitsQueryParameters.Status
            }));
        }

        if (hasPreviousPage)
        {
            links.Add(linkService.Create(nameof(GetHabits), "previous-page", HttpMethods.Get, new
            {
                page = habitsQueryParameters.Page - 1,
                pageSize = habitsQueryParameters.PageSize,
                fields = habitsQueryParameters.Fields,
                q = habitsQueryParameters.Search,
                sort = habitsQueryParameters.Sort,
                type = habitsQueryParameters.Type,
                status = habitsQueryParameters.Status
            }));
        }

        return links;
    }
        
    private List<LinkDto> CreateLinksForHabit(string id, string? fields)
        =>
        [
            linkService.Create(nameof(GetHabit), "self", HttpMethods.Get, new { id, fields }),
            linkService.Create(nameof(UpdateHabit), "update", HttpMethods.Put, new { id }),
            linkService.Create(nameof(PatchHabit), "patch", HttpMethods.Patch, new { id }),
            linkService.Create(nameof(DeleteHabit), "delete", HttpMethods.Delete, new { id }),
            linkService.Create(
                nameof(HabitTagsController.UpsertHabitTags), 
                "upsert-tags", 
                HttpMethods.Put, 
                new { habitId = id },
                HabitTagsController.Name)
        ];

}
