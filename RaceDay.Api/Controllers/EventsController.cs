using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Mapping;
using RaceDay.Api.Models;
using Route = RaceDay.Api.Models.Route;
using Environment = RaceDay.Api.Models.Environment;
using Swashbuckle.AspNetCore.Annotations;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ApiControllerBase
{
    private static readonly HashSet<string> EventTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Running", "Walking", "Cycling", "Run", "Walk", "Cycle"
    };

    private readonly RaceDayContext _db;

    public EventsController(RaceDayContext db)
    {
        _db = db;
    }

    /// <summary>View all upcoming and open events.</summary>
    /// <remarks>Public. Request: none. Response: event list with name, description, date, location, distance and type.</remarks>
    [HttpGet]
    [SwaggerOperation(Summary = "View all events", Description = "Public list of events. Both roles and anonymous users can call this.")]
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var events = await _db.Events.Include(e => e.Organisation).OrderBy(e => e.EventDate).ToListAsync();
        return Ok(events.Select(DtoMapper.ToEvent));
    }

    /// <summary>View a single event.</summary>
    /// <remarks>Public. Request: none. Response: event details.</remarks>
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "View event", Description = "Public event details including distance and event type.")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _db.Events.Include(e => e.Organisation).FirstOrDefaultAsync(e => e.EventID == id);
        return ev is null ? NotFound(new { message = "Event not found." }) : Ok(DtoMapper.ToEvent(ev));
    }

    /// <summary>Create a new event.</summary>
    /// <remarks>Organiser/Admin. Request: event details (name, description, date, location, distance, type). Response: 201 Created.</remarks>
    [HttpPost]
    [RequireRole(Roles.Admin, Roles.Organiser)]
    [SwaggerOperation(Summary = "Create event", Description = "Organisers create events for their organisation. Participants are forbidden.")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] EventRequest request)
    {
        if (!EventTypes.Contains(request.EventType))
        {
            return BadRequest(new { message = "EventType must be Running, Walking or Cycling." });
        }

        var user = await _db.Users.FirstAsync(u => u.UserID == CurrentUserId);
        var organisationId = user.OrganisationID;
        if (organisationId is null)
        {
            return BadRequest(new { message = "Organiser is not linked to an organisation." });
        }

        var ev = new Event
        {
            OrganisationID = organisationId.Value,
            Name = request.Name.Trim(),
            Description = request.Description,
            EventDate = request.EventDate,
            StartTime = request.StartTime,
            Location = request.Location.Trim(),
            DistanceKM = request.DistanceKM,
            EventType = NormaliseType(request.EventType),
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Upcoming" : request.Status
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        await _db.Entry(ev).Reference(e => e.Organisation).LoadAsync();
        return Created($"/api/events/{ev.EventID}", DtoMapper.ToEvent(ev));
    }

    /// <summary>Edit an existing event.</summary>
    /// <remarks>Organiser/Admin. Request: event details. Response: 200 OK with updated event.</remarks>
    [HttpPut("{id:int}")]
    [RequireRole(Roles.Admin, Roles.Organiser)]
    [SwaggerOperation(Summary = "Edit event", Description = "Updates an event owned by the organiser's organisation.")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] EventRequest request)
    {
        var ev = await _db.Events.Include(e => e.Organisation).FirstOrDefaultAsync(e => e.EventID == id);
        if (ev is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (!await CanManageEvent(ev))
        {
            return ForbiddenResult();
        }

        if (!EventTypes.Contains(request.EventType))
        {
            return BadRequest(new { message = "EventType must be Running, Walking or Cycling." });
        }

        ev.Name = request.Name.Trim();
        ev.Description = request.Description;
        ev.EventDate = request.EventDate;
        ev.StartTime = request.StartTime;
        ev.Location = request.Location.Trim();
        ev.DistanceKM = request.DistanceKM;
        ev.EventType = NormaliseType(request.EventType);
        ev.Status = string.IsNullOrWhiteSpace(request.Status) ? ev.Status : request.Status;
        await _db.SaveChangesAsync();
        return Ok(DtoMapper.ToEvent(ev));
    }

    /// <summary>Delete an event.</summary>
    /// <remarks>Organiser/Admin. Request: none. Response: 204 No Content.</remarks>
    [HttpDelete("{id:int}")]
    [RequireRole(Roles.Admin, Roles.Organiser)]
    [SwaggerOperation(Summary = "Delete event", Description = "Deletes an event owned by the organiser's organisation.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == id);
        if (ev is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (!await CanManageEvent(ev))
        {
            return ForbiddenResult();
        }

        if (await _db.Enrolments.AnyAsync(e => e.EventID == id && e.Status != "Cancelled"))
        {
            return Conflict(new { message = "Cannot delete an event that still has active enrolments." });
        }

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>View all enrolments for an event.</summary>
    /// <remarks>Organiser/Admin. Request: none. Response: participant enrolment list.</remarks>
    [HttpGet("{id:int}/enrolments")]
    [RequireRole(Roles.Admin, Roles.Organiser)]
    [SwaggerOperation(Summary = "View event enrolments", Description = "Organisers view every participant entered in their event.")]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnrolments(int id)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == id);
        if (ev is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (!await CanManageEvent(ev))
        {
            return ForbiddenResult();
        }

        var enrolments = await QueryEnrolments().Where(e => e.EventID == id).ToListAsync();
        return Ok(enrolments.Select(DtoMapper.ToEnrolment));
    }

    /// <summary>View published results for an event.</summary>
    /// <remarks>Public. Request: none. Response: result list.</remarks>
    [HttpGet("{id:int}/results")]
    [SwaggerOperation(Summary = "View event results", Description = "Public/organiser result list for a completed or in-progress event.")]
    [ProducesResponseType(typeof(IEnumerable<ResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResults(int id)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == id))
        {
            return NotFound(new { message = "Event not found." });
        }

        var results = await QueryResults().Where(r => r.Enrolment.EventID == id).OrderBy(r => r.Position).ToListAsync();
        return Ok(results.Select(DtoMapper.ToResult));
    }

    /// <summary>View race route information.</summary>
    /// <remarks>Public. Request: none. Response: route information.</remarks>
    [HttpGet("{id:int}/route")]
    [SwaggerOperation(Summary = "View race route", Description = "Public route details for race-day preparation.")]
    [ProducesResponseType(typeof(RouteDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoute(int id)
    {
        var route = await _db.Routes.FirstOrDefaultAsync(r => r.EventID == id);
        return route is null ? NotFound(new { message = "Route not found for this event." }) : Ok(DtoMapper.ToRoute(route));
    }

    /// <summary>Add a route to an event.</summary>
    /// <remarks>Organiser/Admin. Request: route details. Response: 201 Created.</remarks>
    [HttpPost("{id:int}/route")]
    [RequireRole(Roles.Admin, Roles.Organiser)]
    [SwaggerOperation(Summary = "Add route", Description = "Creates the race route for an event. One route per event.")]
    [ProducesResponseType(typeof(RouteDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddRoute(int id, [FromBody] RouteRequest request)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == id);
        if (ev is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (!await CanManageEvent(ev))
        {
            return ForbiddenResult();
        }

        if (await _db.Routes.AnyAsync(r => r.EventID == id))
        {
            return Conflict(new { message = "A route already exists. Use PUT to update it." });
        }

        var route = new Route
        {
            EventID = id,
            Name = request.Name.Trim(),
            DistanceKM = request.DistanceKM,
            ElevationM = request.ElevationM,
            MapURL = request.MapURL
        };
        _db.Routes.Add(route);
        await _db.SaveChangesAsync();
        return Created($"/api/events/{id}/route", DtoMapper.ToRoute(route));
    }

    /// <summary>Update the route for an event.</summary>
    /// <remarks>Organiser/Admin. Request: route details. Response: 200 OK.</remarks>
    [HttpPut("{id:int}/route")]
    [RequireRole(Roles.Admin, Roles.Organiser)]
    [SwaggerOperation(Summary = "Update route", Description = "Updates the existing race route.")]
    [ProducesResponseType(typeof(RouteDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateRoute(int id, [FromBody] RouteRequest request)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == id);
        if (ev is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (!await CanManageEvent(ev))
        {
            return ForbiddenResult();
        }

        var route = await _db.Routes.FirstOrDefaultAsync(r => r.EventID == id);
        if (route is null)
        {
            return NotFound(new { message = "Route not found for this event." });
        }

        route.Name = request.Name.Trim();
        route.DistanceKM = request.DistanceKM;
        route.ElevationM = request.ElevationM;
        route.MapURL = request.MapURL;
        await _db.SaveChangesAsync();
        return Ok(DtoMapper.ToRoute(route));
    }

    /// <summary>View the latest weather reading for an event.</summary>
    /// <remarks>Public. Request: none. Response: weather information.</remarks>
    [HttpGet("{id:int}/weather")]
    [SwaggerOperation(Summary = "View current weather", Description = "Public latest stored weather for the event.")]
    [ProducesResponseType(typeof(WeatherDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWeather(int id)
    {
        var weather = await _db.WeatherRecords
            .Where(w => w.EventID == id)
            .OrderByDescending(w => w.RecordedAt)
            .FirstOrDefaultAsync();
        return weather is null ? NotFound(new { message = "Weather data not found for this event." }) : Ok(DtoMapper.ToWeather(weather));
    }

    /// <summary>View surface and environment conditions for an event.</summary>
    /// <remarks>Public. Request: none. Response: environment information.</remarks>
    [HttpGet("{id:int}/environment")]
    [SwaggerOperation(Summary = "View current environment", Description = "Public environment conditions such as temperature, humidity and surface.")]
    [ProducesResponseType(typeof(EnvironmentDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnvironment(int id)
    {
        var environment = await _db.Environments.FirstOrDefaultAsync(e => e.EventID == id);
        return environment is null
            ? NotFound(new { message = "Environment data not found for this event." })
            : Ok(DtoMapper.ToEnvironment(environment));
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

    private IQueryable<Enrolment> QueryEnrolments() =>
        _db.Enrolments
            .Include(e => e.Event)
            .Include(e => e.Participant).ThenInclude(p => p.User)
            .Include(e => e.EventCategory).ThenInclude(ec => ec.Category);

    private IQueryable<Result> QueryResults() =>
        _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e.Event)
            .Include(r => r.Enrolment).ThenInclude(e => e.Participant).ThenInclude(p => p.User);

    private static string NormaliseType(string type) => type.ToLowerInvariant() switch
    {
        "run" or "running" => "Running",
        "walk" or "walking" => "Walking",
        "cycle" or "cycling" => "Cycling",
        _ => type
    };
}
