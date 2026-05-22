using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PussyCats.Web.Infrastructure;

public class ModeAuthorizeFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() is not null)
        {
            return;
        }

        var mode = context.HttpContext.Session.GetString(SessionKeys.Mode);
        if (string.IsNullOrWhiteSpace(mode))
        {
            context.Result = new RedirectToActionResult("Index", "Home", null);
            return;
        }

        if (string.Equals(mode, AppModes.User, StringComparison.OrdinalIgnoreCase)
            && context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
        }
    }
}
