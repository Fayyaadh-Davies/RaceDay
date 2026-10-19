using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Auth;

namespace RaceDay.Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected int CurrentUserId => HttpContext.Session.GetInt32(SessionKeys.UserId) ?? 0;

    protected string CurrentRole => HttpContext.Session.GetString(SessionKeys.Role) ?? string.Empty;

    protected bool IsAdmin => string.Equals(CurrentRole, Roles.Admin, StringComparison.OrdinalIgnoreCase);

    protected bool IsOrganiser => string.Equals(CurrentRole, Roles.Organiser, StringComparison.OrdinalIgnoreCase);

    protected bool IsParticipant => string.Equals(CurrentRole, Roles.Participant, StringComparison.OrdinalIgnoreCase);

    protected ObjectResult ForbiddenResult() =>
        StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not have permission to access this resource." });
}
