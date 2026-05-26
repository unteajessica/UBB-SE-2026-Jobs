using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.Auth;
using PussyCats.Web.Infrastructure;
using PussyCats.Web.Models;

namespace PussyCats.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService auth;

    public AccountController(IAuthService auth) => this.auth = auth;

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await auth.LoginAsync(model.Email, model.Password, cancellationToken);
        if (!result.Succeeded || result.Response is null)
        {
            ModelState.AddModelError(string.Empty, "Email sau parola incorecta.");
            return View(model);
        }

        await SignInAsync(result.Response);

        return LocalRedirect(returnUrl ?? Url.Action("Index", "Home")!);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register() => View();

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await auth.RegisterAsync(model.Email, model.Password, model.FirstName, model.LastName, cancellationToken);
        if (!result.Succeeded || result.Response is null)
        {
            var msg = result.StatusCode == System.Net.HttpStatusCode.Conflict
                ? "Adresa de email este deja inregistrata."
                : "Inregistrare esuata. Incearca din nou.";
            ModelState.AddModelError(string.Empty, msg);
            return View(model);
        }

        await SignInAsync(result.Response);

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Remove(SessionKeys.JwtToken);
        return RedirectToAction(nameof(Login));
    }

    private async Task SignInAsync(AuthResponse info)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, info.UserId.ToString()),
            new Claim(ClaimTypes.Email, info.Email),
            new Claim(ClaimTypes.Name, $"{info.FirstName} {info.LastName}".Trim()),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        HttpContext.Session.SetString(SessionKeys.JwtToken, info.Token);
    }
}
