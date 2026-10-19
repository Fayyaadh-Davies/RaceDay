using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireRoleAttribute : Attribute, IAsyncActionFilter
{
    private readonly HashSet<string> _roles;

    public RequireRoleAttribute(params string[] roles)
    {
        _roles = new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase);
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await context.HttpContext.Session.LoadAsync();
        if (context.HttpContext.Session.GetInt32(SessionKeys.UserId) is null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Authentication required. Please log in." });
            return;
        }

        var role = context.HttpContext.Session.GetString(SessionKeys.Role);
        if (string.IsNullOrWhiteSpace(role) || !_roles.Contains(role))
        {
            context.Result = new ObjectResult(new { message = "You do not have permission to access this resource." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        await next();
    }
}
