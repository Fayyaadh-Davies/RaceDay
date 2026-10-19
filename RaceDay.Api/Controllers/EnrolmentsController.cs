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
[Route("api/enrolments")]
public class EnrolmentsController : ApiControllerBase
{
    private readonly RaceDayContext _db;

    public EnrolmentsController(RaceDayContext db)
    {
        _db = db;
    }

    /// <summary>Enter an event by selecting a category.</summary>
    /// <remarks>Participant. Request: eventId, categoryId. Response: 201 Created enrolment linking participant, event and category.</remarks>
    [HttpPost]
    [RequireRole(Roles.Participant)]
    [SwaggerOperation(Summary = "Enter an event", Description = "Participants enrol in an event category. Organisers cannot use this endpoint.")]
    [ProducesResponseType(typeof(EnrolmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Enrol([FromBody] EnrolmentRequest request)
    {
        var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserID == CurrentUserId);
        if (participant is null)
        {
            return BadRequest(new { message = "Participant profile not found." });
        }

        var eventCategory = await _db.EventCategories
            .Include(ec => ec.Event)
            .Include(ec => ec.Category)
            .FirstOrDefaultAsync(ec => ec.EventID == request.EventId && ec.CategoryID == request.CategoryId);

        if (eventCategory is null)
        {
            return NotFound(new { message = "That category is not offered for this event." });
        }

        if (await _db.Enrolments.AnyAsync(e => e.ParticipantID == participant.ParticipantID && e.EventID == request.EventId && e.Status != "Cancelled"))
        {
            return Conflict(new { message = "You are already enrolled in this event." });
        }

        if (eventCategory.MaximumParticipants is not null)
        {
            var count = await _db.Enrolments.CountAsync(e => e.EventCategoryID == eventCategory.EventCategoryID && e.Status != "Cancelled");
            if (count >= eventCategory.MaximumParticipants)
            {
                return Conflict(new { message = "This category is full." });
            }
        }

        var enrolment = new Enrolment
        {
            ParticipantID = participant.ParticipantID,
            EventID = request.EventId,
            EventCategoryID = eventCategory.EventCategoryID,
            EnrolmentDate = DateTime.UtcNow,
            Status = "Confirmed"
        };
        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        var created = await Query().FirstAsync(e => e.EnrolmentID == enrolment.EnrolmentID);
        return Created($"/api/enrolments/{created.EnrolmentID}", DtoMapper.ToEnrolment(created));
    }

    /// <summary>View the logged-in participant's enrolments.</summary>
    /// <remarks>Participant. Request: none. Response: enrolment list.</remarks>
    [HttpGet("me")]
    [RequireRole(Roles.Participant)]
    [SwaggerOperation(Summary = "View own enrolments", Description = "Returns the participant's event entries.")]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine()
    {
        var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserID == CurrentUserId);
        if (participant is null)
        {
            return Ok(Array.Empty<EnrolmentDto>());
        }

        var enrolments = await Query().Where(e => e.ParticipantID == participant.ParticipantID).ToListAsync();
        return Ok(enrolments.Select(DtoMapper.ToEnrolment));
    }

    /// <summary>View a single enrolment.</summary>
    /// <remarks>Participant or Organiser. Participants may only see their own record.</remarks>
    [HttpGet("{id:int}")]
    [RequireRole(Roles.Participant, Roles.Organiser, Roles.Admin)]
    [SwaggerOperation(Summary = "View enrolment", Description = "Participants see their own enrolment; organisers see enrolments for their events.")]
    [ProducesResponseType(typeof(EnrolmentDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var enrolment = await Query().FirstOrDefaultAsync(e => e.EnrolmentID == id);
        if (enrolment is null)
        {
            return NotFound(new { message = "Enrolment not found." });
        }

        if (!await CanView(enrolment))
        {
            return ForbiddenResult();
        }

        return Ok(DtoMapper.ToEnrolment(enrolment));
    }

    /// <summary>Cancel an enrolment.</summary>
    /// <remarks>Participant. Request: none. Response: 204 No Content.</remarks>
    [HttpDelete("{id:int}")]
    [RequireRole(Roles.Participant)]
    [SwaggerOperation(Summary = "Cancel enrolment", Description = "Participants cancel their own event entry.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(int id)
    {
        var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserID == CurrentUserId);
        var enrolment = await _db.Enrolments.FirstOrDefaultAsync(e => e.EnrolmentID == id);
        if (enrolment is null)
        {
            return NotFound(new { message = "Enrolment not found." });
        }

        if (participant is null || enrolment.ParticipantID != participant.ParticipantID)
        {
            return ForbiddenResult();
        }

        if (enrolment.Result is not null || await _db.Results.AnyAsync(r => r.EnrolmentID == id))
        {
            return Conflict(new { message = "Cannot cancel an enrolment that already has a result." });
        }

        enrolment.Status = "Cancelled";
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private IQueryable<Enrolment> Query() =>
        _db.Enrolments
            .Include(e => e.Event)
            .Include(e => e.Participant).ThenInclude(p => p.User)
            .Include(e => e.EventCategory).ThenInclude(ec => ec.Category);

    private async Task<bool> CanView(Enrolment enrolment)
    {
        if (IsAdmin)
        {
            return true;
        }

        if (IsParticipant)
        {
            var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserID == CurrentUserId);
            return participant is not null && enrolment.ParticipantID == participant.ParticipantID;
        }

        var user = await _db.Users.FirstAsync(u => u.UserID == CurrentUserId);
        return user.OrganisationID == enrolment.Event.OrganisationID;
    }
}
