using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Mapping;
using Swashbuckle.AspNetCore.Annotations;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ApiControllerBase
{
    private readonly RaceDayContext _db;

    public UsersController(RaceDayContext db)
    {
        _db = db;
    }

    /// <summary>View the currently authenticated user's profile.</summary>
    /// <remarks>Authenticated. Request: none. Response: user profile including role-specific fields.</remarks>
    [HttpGet("me")]
    [RequireAuth]
    [SwaggerOperation(Summary = "View own profile", Description = "Returns the logged-in user's profile.")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe()
    {
        var user = await LoadUser(CurrentUserId);
        return user is null ? Unauthorized() : Ok(DtoMapper.ToProfile(user));
    }

    /// <summary>Update the currently authenticated user's profile.</summary>
    /// <remarks>Authenticated. Request: updated profile fields. Response: updated profile.</remarks>
    [HttpPut("me")]
    [RequireAuth]
    [SwaggerOperation(Summary = "Edit own profile", Description = "Updates name, email and participant details for the logged-in user.")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
    {
        var user = await LoadUser(CurrentUserId);
        if (user is null)
        {
            return Unauthorized();
        }

        if (await _db.Users.AnyAsync(u => u.Email == request.Email && u.UserID != user.UserID))
        {
            return Conflict(new { message = "Email is already in use." });
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = request.Email.Trim();

        if (user.Participant is not null)
        {
            user.Participant.DateOfBirth = request.DateOfBirth;
            user.Participant.Gender = request.Gender;
            user.Participant.EmergencyContact = request.EmergencyContact;
            user.Participant.EmergencyContactNumber = request.EmergencyContactNumber;
        }

        if (user.Organisation is not null)
        {
            user.Organisation.ContactNumber = request.ContactNumber ?? user.Organisation.ContactNumber;
            user.Organisation.Address = request.Address ?? user.Organisation.Address;
        }

        await _db.SaveChangesAsync();
        return Ok(DtoMapper.ToProfile(user));
    }

    /// <summary>Delete the currently authenticated user's account.</summary>
    /// <remarks>Authenticated. Request: none. Response: 204 No Content.</remarks>
    [HttpDelete("me")]
    [HttpDelete("/api/user/me")]
    [RequireAuth]
    [SwaggerOperation(Summary = "Delete own account", Description = "Deletes the logged-in account when it has no blocking enrolments or events.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteMe()
    {
        var user = await LoadUser(CurrentUserId);
        if (user is null)
        {
            return Unauthorized();
        }

        if (user.Participant is not null &&
            await _db.Enrolments.AnyAsync(e => e.ParticipantID == user.Participant.ParticipantID && e.Status != "Cancelled"))
        {
            return Conflict(new { message = "Cancel all active enrolments before deleting the account." });
        }

        if (user.OrganisationID is not null &&
            await _db.Events.AnyAsync(e => e.OrganisationID == user.OrganisationID))
        {
            return Conflict(new { message = "Organisers with existing events cannot delete their account." });
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        HttpContext.Session.Clear();
        return NoContent();
    }

    /// <summary>View another user's details (organisers and admins only).</summary>
    /// <remarks>Organiser/Admin. Request: none. Response: user details.</remarks>
    [HttpGet("{id:int}")]
    [HttpGet("/api/user/{id:int}")]
    [RequireRole(Roles.Admin, Roles.Organiser)]
    [SwaggerOperation(Summary = "View user", Description = "Returns user details. Restricted to Organiser and Admin.")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await LoadUser(id);
        return user is null ? NotFound(new { message = "User not found." }) : Ok(DtoMapper.ToProfile(user));
    }

    private Task<Models.User?> LoadUser(int id) =>
        _db.Users
            .Include(u => u.Organisation)
            .Include(u => u.Participant)
            .FirstOrDefaultAsync(u => u.UserID == id);
}
