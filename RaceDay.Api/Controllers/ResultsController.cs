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
[Route("api/results")]
public class ResultsController : ApiControllerBase
{
    private readonly RaceDayContext _db;

    public ResultsController(RaceDayContext db)
    {
        _db = db;
    }

    /// <summary>Capture a participant's finish time and position.</summary>
    /// <remarks>Organiser/Admin. Request: enrolmentId, finishTime, position. Response: 201 Created.</remarks>
    [HttpPost]
    [RequireRole(Roles.Admin, Roles.Organiser)]
    [SwaggerOperation(Summary = "Capture participant results", Description = "Organisers record finish time and finishing position after an event.")]
    [ProducesResponseType(typeof(ResultDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] ResultRequest request)
    {
        var enrolment = await _db.Enrolments.Include(e => e.Event).FirstOrDefaultAsync(e => e.EnrolmentID == request.EnrolmentId);
        if (enrolment is null)
        {
            return NotFound(new { message = "Enrolment not found." });
        }

        if (!await CanManageEvent(enrolment.Event))
        {
            return ForbiddenResult();
        }

        if (await _db.Results.AnyAsync(r => r.EnrolmentID == request.EnrolmentId))
        {
            return Conflict(new { message = "A result already exists for this enrolment. Use PUT to update it." });
        }

        if (request.Position is <= 0)
        {
            return BadRequest(new { message = "Position must be greater than 0." });
        }

        var result = new Result
        {
            EnrolmentID = request.EnrolmentId,
            FinishTime = request.FinishTime,
            Position = request.Position,
            Pace = request.Pace,
            ResultStatus = string.IsNullOrWhiteSpace(request.ResultStatus) ? "Finished" : request.ResultStatus
        };
        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        var created = await Query().FirstAsync(r => r.ResultID == result.ResultID);
        return Created($"/api/results/{created.ResultID}", DtoMapper.ToResult(created));
    }

    /// <summary>Update a captured result.</summary>
    /// <remarks>Organiser/Admin. Request: result details. Response: 200 OK.</remarks>
    [HttpPut("{id:int}")]
    [RequireRole(Roles.Admin, Roles.Organiser)]
    [SwaggerOperation(Summary = "Update result", Description = "Updates finish time, position or pace.")]
    [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateResultRequest request)
    {
        var result = await Query().FirstOrDefaultAsync(r => r.ResultID == id);
        if (result is null)
        {
            return NotFound(new { message = "Result not found." });
        }

        if (!await CanManageEvent(result.Enrolment.Event))
        {
            return ForbiddenResult();
        }

        if (request.Position is <= 0)
        {
            return BadRequest(new { message = "Position must be greater than 0." });
        }

        result.FinishTime = request.FinishTime;
        result.Position = request.Position;
        result.Pace = request.Pace;
        result.ResultStatus = string.IsNullOrWhiteSpace(request.ResultStatus) ? result.ResultStatus : request.ResultStatus;
        await _db.SaveChangesAsync();
        return Ok(DtoMapper.ToResult(result));
    }

    /// <summary>View a single result.</summary>
    /// <remarks>Participant or Organiser. Participants may only view their own result.</remarks>
    [HttpGet("{id:int}")]
    [RequireRole(Roles.Participant, Roles.Organiser, Roles.Admin)]
    [SwaggerOperation(Summary = "View result", Description = "Returns one result if the caller is the participant or the event organiser.")]
    [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await Query().FirstOrDefaultAsync(r => r.ResultID == id);
        if (result is null)
        {
            return NotFound(new { message = "Result not found." });
        }

        if (!await CanView(result))
        {
            return ForbiddenResult();
        }

        return Ok(DtoMapper.ToResult(result));
    }

    private IQueryable<Result> Query() =>
        _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e.Event)
            .Include(r => r.Enrolment).ThenInclude(e => e.Participant).ThenInclude(p => p.User);

    private async Task<bool> CanManageEvent(Event ev)
    {
        if (IsAdmin)
        {
            return true;
        }

        var user = await _db.Users.FirstAsync(u => u.UserID == CurrentUserId);
        return user.OrganisationID == ev.OrganisationID;
    }

    private async Task<bool> CanView(Result result)
    {
        if (IsAdmin)
        {
            return true;
        }

        if (IsParticipant)
        {
            var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserID == CurrentUserId);
            return participant is not null && result.Enrolment.ParticipantID == participant.ParticipantID;
        }

        return await CanManageEvent(result.Enrolment.Event);
    }
}
