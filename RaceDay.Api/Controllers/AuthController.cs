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
[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly RaceDayContext _db;

    public AuthController(RaceDayContext db)
    {
        _db = db;
    }

    /// <summary>Register a new organiser or participant account.</summary>
    /// <remarks>
    /// Public endpoint. Request: firstName, lastName, email, password, role (Organiser or Participant).
    /// Participants may include dateOfBirth, gender and emergency contact fields.
    /// Organisers should include organisationName and registrationNumber so an organisation record can be created.
    /// Response: 201 Created with user details. Password is hashed before storage.
    /// </remarks>
    [HttpPost("register")]
    [SwaggerOperation(Summary = "Register a new user", Description = "Creates an Organiser or Participant account. Public. Returns 201 with the new profile.")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var role = request.Role?.Trim();
        if (!string.Equals(role, Roles.Organiser, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(role, Roles.Participant, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Role must be Organiser or Participant." });
        }

        role = string.Equals(role, Roles.Organiser, StringComparison.OrdinalIgnoreCase) ? Roles.Organiser : Roles.Participant;

        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
        {
            return Conflict(new { message = "An account with this email already exists." });
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = PasswordHasher.Hash(request.Password),
            Role = role
        };

        if (role == Roles.Organiser)
        {
            if (string.IsNullOrWhiteSpace(request.OrganisationName) || string.IsNullOrWhiteSpace(request.RegistrationNumber))
            {
                return BadRequest(new { message = "Organisers must provide organisationName and registrationNumber." });
            }

            if (await _db.Organisations.AnyAsync(o => o.RegistrationNumber == request.RegistrationNumber))
            {
                return Conflict(new { message = "An organisation with this registration number already exists." });
            }

            var organisation = new Organisation
            {
                Name = request.OrganisationName.Trim(),
                RegistrationNumber = request.RegistrationNumber.Trim(),
                ContactEmail = string.IsNullOrWhiteSpace(request.ContactEmail) ? request.Email : request.ContactEmail.Trim(),
                ContactNumber = request.ContactNumber,
                Address = request.Address
            };
            _db.Organisations.Add(organisation);
            await _db.SaveChangesAsync();
            user.OrganisationID = organisation.OrganisationID;
        }

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        if (role == Roles.Participant)
        {
            _db.Participants.Add(new Participant
            {
                UserID = user.UserID,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                EmergencyContact = request.EmergencyContact,
                EmergencyContactNumber = request.EmergencyContactNumber
            });
            await _db.SaveChangesAsync();
        }

        var created = await _db.Users
            .Include(u => u.Organisation)
            .Include(u => u.Participant)
            .FirstAsync(u => u.UserID == user.UserID);

        return Created($"/api/users/{created.UserID}", DtoMapper.ToProfile(created));
    }

    /// <summary>Authenticate a user and start a role-based session.</summary>
    /// <remarks>
    /// Public endpoint. Request: email, password.
    /// Response: 200 with user details and role. A session cookie is issued so later requests stay authenticated.
    /// </remarks>
    [HttpPost("login")]
    [SwaggerOperation(Summary = "Log in", Description = "Validates credentials, stores user id and role in session, and returns the user profile.")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        await HttpContext.Session.LoadAsync();
        HttpContext.Session.SetInt32(SessionKeys.UserId, user.UserID);
        HttpContext.Session.SetString(SessionKeys.Role, user.Role);
        HttpContext.Session.SetString(SessionKeys.Email, user.Email);
        await HttpContext.Session.CommitAsync();

        return Ok(new AuthResponse
        {
            UserId = user.UserID,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            Message = "Session established. Use the session cookie for subsequent requests."
        });
    }

    /// <summary>End the current authenticated session.</summary>
    /// <remarks>Authenticated endpoint. Request: none. Response: 200 OK.</remarks>
    [HttpPost("logout")]
    [RequireAuth]
    [SwaggerOperation(Summary = "Log out", Description = "Clears the authenticated session. Requires an existing login.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.Session.CommitAsync();
        return Ok(new { message = "Logged out." });
    }
}
