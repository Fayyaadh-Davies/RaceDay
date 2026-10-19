using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Mapping;
using Swashbuckle.AspNetCore.Annotations;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/participants")]
public class ParticipantsController : ApiControllerBase
{
    private readonly RaceDayContext _db;

    public ParticipantsController(RaceDayContext db)
    {
        _db = db;
    }

    /// <summary>View the logged-in participant's personal performance history.</summary>
    /// <remarks>Participant. Request: none. Response: result history.</remarks>
    [HttpGet("me/results")]
    [RequireRole(Roles.Participant)]
    [SwaggerOperation(Summary = "View personal performance history", Description = "Participants view their own results only.")]
    [ProducesResponseType(typeof(IEnumerable<ResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyResults()
    {
        var participant = await _db.Participants.FirstOrDefaultAsync(p => p.UserID == CurrentUserId);
        if (participant is null)
        {
            return Ok(Array.Empty<ResultDto>());
        }

        var results = await _db.Results
            .Include(r => r.Enrolment).ThenInclude(e => e.Event)
            .Include(r => r.Enrolment).ThenInclude(e => e.Participant).ThenInclude(p => p.User)
            .Where(r => r.Enrolment.ParticipantID == participant.ParticipantID)
            .OrderByDescending(r => r.Enrolment.Event.EventDate)
            .ToListAsync();

        return Ok(results.Select(DtoMapper.ToResult));
    }
}
