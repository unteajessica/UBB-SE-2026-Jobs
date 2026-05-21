using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Web.Models;
using PussyCats.Web.ServiceProxies;

namespace PussyCats.Web.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly AuthServiceProxy auth;

    public AccountController(AuthServiceProxy auth) => this.auth = auth;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return View(model);

        var response = await auth.LoginAsync(model.Email, model.Password, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Email sau parolă incorectă.");
            return View(model);
        }

        var info = await response.Content.ReadFromJsonAsync<UserInfo>(cancellationToken: cancellationToken);
        await SignInAsync(info!);

        return LocalRedirect(returnUrl ?? Url.Action("Index", "Home")!);
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return View(model);

        var response = await auth.RegisterAsync(model.Email, model.Password, model.FirstName, model.LastName, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var msg = response.StatusCode == System.Net.HttpStatusCode.Conflict
                ? "Adresa de email este deja înregistrată."
                : "Înregistrare eșuată. Încearcă din nou.";
            ModelState.AddModelError(string.Empty, msg);
            return View(model);
        }

        var info = await response.Content.ReadFromJsonAsync<UserInfo>(cancellationToken: cancellationToken);
        await SignInAsync(info!);

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private async Task SignInAsync(UserInfo info)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, info.UserId.ToString()),
            new Claim(ClaimTypes.Email, info.Email),
            new Claim(ClaimTypes.Name, $"{info.FirstName} {info.LastName}".Trim()),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }

    private record UserInfo(int UserId, string Email, string FirstName, string LastName);
}
