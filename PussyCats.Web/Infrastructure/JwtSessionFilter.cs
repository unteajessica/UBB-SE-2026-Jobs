using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PussyCats.Web.Infrastructure;

public class JwtSessionFilter : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
            return;

        var token = context.HttpContext.Session.GetString(SessionKeys.JwtToken);
        if (!string.IsNullOrWhiteSpace(token))
            return;

        // Cookie says logged in but session lost the JWT (server restart / expiry mismatch).
        // Sign out so the user gets a clean login page instead of a 401 crash.
        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        context.HttpContext.Session.Clear();

        var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
        context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
    }
}
