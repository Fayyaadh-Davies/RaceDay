using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireAuthAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await context.HttpContext.Session.LoadAsync();
        if (context.HttpContext.Session.GetInt32(SessionKeys.UserId) is null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Authentication required. Please log in." });
            return;
        }

        await next();
    }
}
