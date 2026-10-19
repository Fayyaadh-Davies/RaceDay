using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Mapping;
using RaceDay.Api.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ApiControllerBase
{
    private static readonly HashSet<string> Types = new(StringComparer.OrdinalIgnoreCase)
    {
        "Running", "Walking", "Cycling"
    };

    private readonly RaceDayContext _db;

    public CategoriesController(RaceDayContext db)
    {
        _db = db;
    }

    /// <summary>View available categories.</summary>
    /// <remarks>Public. Optional query eventId returns categories offered for that event. Response: category list.</remarks>
    [HttpGet]
    [SwaggerOperation(Summary = "View categories", Description = "Public list of age or distance categories. Filter with ?eventId=.")]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int? eventId)
    {
        if (eventId is not null)
        {
            var offered = await _db.EventCategories
                .Include(ec => ec.Category)
                .Where(ec => ec.EventID == eventId)
                .Select(ec => new EventCategoryDto
                {
                    EventCategoryId = ec.EventCategoryID,
                    EventId = ec.EventID,
                    CategoryId = ec.CategoryID,
                    CategoryName = ec.Category.Name,
                    Type = ec.Category.Type,
                    DistanceKM = ec.Category.DistanceKM,
                    AgeGroup = ec.Category.AgeGroup,
                    EntryFee = ec.EntryFee,
                    MaximumParticipants = ec.MaximumParticipants
                })
                .ToListAsync();
            return Ok(offered);
        }

        var categories = await _db.Categories.OrderBy(c => c.DistanceKM).ToListAsync();
        return Ok(categories.Select(DtoMapper.ToCategory));
    }

    /// <summary>View a single category.</summary>
    /// <remarks>Public. Request: none. Response: category.</remarks>
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "View category", Description = "Public category details such as Under 20, Senior, 10km or 21km.")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        return category is null ? NotFound(new { message = "Category not found." }) : Ok(DtoMapper.ToCategory(category));
    }

    /// <summary>Create a category and optionally attach it to an event.</summary>
    /// <remarks>Organiser/Admin. Request: category details. Response: 201 Created.</remarks>
    [HttpPost]
    [RequireRole(Roles.Admin, Roles.Organiser)]
    [SwaggerOperation(Summary = "Create category", Description = "Organisers define age or distance categories. Supply eventId to offer it on an event.")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CategoryRequest request)
    {
        if (!Types.Contains(request.Type))
        {
            return BadRequest(new { message = "Type must be Running, Walking or Cycling." });
        }

        var category = new Category
        {
            Name = request.Name.Trim(),
            Type = request.Type,
            DistanceKM = request.DistanceKM,
            AgeGroup = request.AgeGroup
        };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        if (request.EventId is not null)
        {
            var ev = await _db.Events.FindAsync(request.EventId.Value);
            if (ev is null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (!await CanManageEvent(ev))
            {
                return ForbiddenResult();
            }

            _db.EventCategories.Add(new EventCategory
            {
                EventID = ev.EventID,
                CategoryID = category.CategoryID,
                EntryFee = request.EntryFee,
                MaximumParticipants = request.MaximumParticipants
            });
            await _db.SaveChangesAsync();
        }

        return Created($"/api/categories/{category.CategoryID}", DtoMapper.ToCategory(category));
    }

    /// <summary>Edit a category.</summary>
    /// <remarks>Organiser/Admin. Request: category details. Response: 200 OK.</remarks>
    [HttpPut("{id:int}")]
    [RequireRole(Roles.Admin, Roles.Organiser)]
    [SwaggerOperation(Summary = "Edit category", Description = "Updates a category definition.")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryRequest request)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null)
        {
            return NotFound(new { message = "Category not found." });
        }

        if (!Types.Contains(request.Type))
        {
            return BadRequest(new { message = "Type must be Running, Walking or Cycling." });
        }

        category.Name = request.Name.Trim();
        category.Type = request.Type;
        category.DistanceKM = request.DistanceKM;
        category.AgeGroup = request.AgeGroup;
        await _db.SaveChangesAsync();
        return Ok(DtoMapper.ToCategory(category));
    }

    /// <summary>Delete a category.</summary>
    /// <remarks>Organiser/Admin. Request: none. Response: 204 No Content.</remarks>
    [HttpDelete("{id:int}")]
    [RequireRole(Roles.Admin, Roles.Organiser)]
    [SwaggerOperation(Summary = "Delete category", Description = "Deletes a category that is not linked to enrolments.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null)
        {
            return NotFound(new { message = "Category not found." });
        }

        if (await _db.Enrolments.AnyAsync(e => e.EventCategory.CategoryID == id))
        {
            return Conflict(new { message = "Cannot delete a category that is used by enrolments." });
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> CanManageEvent(Event ev)
    {
        if (IsAdmin)
        {
            return true;
        }

        var user = await _db.Users.FirstAsync(u => u.UserID == CurrentUserId);
        return user.OrganisationID == ev.OrganisationID;
    }
}
