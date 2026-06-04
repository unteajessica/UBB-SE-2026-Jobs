using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.Auth;
using PussyCats.Web.Dtos;
using PussyCats.Web.Infrastructure;
using PussyCats.Web.Models;
using PussyCats.Web.Services;

namespace PussyCats.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService auth;
    private readonly ITiAuthService tiAuth;

    public AccountController(IAuthService auth, ITiAuthService tiAuth)
    {
        this.auth = auth;
        this.tiAuth = tiAuth;
    }

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
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        // Also call T&I API to get role claim + T&I JWT
        var tiResult = await tiAuth.LoginAsync(model.Email, model.Password);
        await SignInAsync(result.Response, tiResult);

        return LocalRedirect(returnUrl ?? Url.Action("Index", "Home")!);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Register()
    {
        var companies = await tiAuth.GetCompaniesAsync();
        ViewData["Companies"] = companies;
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Companies"] = await tiAuth.GetCompaniesAsync();
            return View(model);
        }

        var result = await auth.RegisterAsync(model.Email, model.Password, model.FirstName, model.LastName, cancellationToken);
        if (!result.Succeeded || result.Response is null)
        {
            // If Conflict for Recruiter: PussyCats user already exists but Recruiter record may be missing.
            // Fall through to T&I register instead of blocking.
            if (result.StatusCode == System.Net.HttpStatusCode.Conflict && model.Role == "Recruiter")
            {
                var loginResult = await auth.LoginAsync(model.Email, model.Password, cancellationToken);
                if (!loginResult.Succeeded || loginResult.Response is null)
                {
                    ModelState.AddModelError(string.Empty, "This email address is already registered.");
                    ViewData["Companies"] = await tiAuth.GetCompaniesAsync();
                    return View(model);
                }
                result = loginResult;
            }
            else
            {
                var message = result.StatusCode == System.Net.HttpStatusCode.Conflict
                    ? "This email address is already registered."
                    : "Registration failed. Try again";
                ModelState.AddModelError(string.Empty, message);
                ViewData["Companies"] = await tiAuth.GetCompaniesAsync();
                return View(model);
            }
        }

        // If registering as Recruiter, also register with T&I API to create Recruiter record
        if (!string.IsNullOrEmpty(model.Role) && model.Role == "Recruiter")
        {
            if (!model.CompanyId.HasValue)
            {
                ModelState.AddModelError(nameof(model.CompanyId), "Please select a company.");
                ViewData["Companies"] = await tiAuth.GetCompaniesAsync();
                return View(model);
            }

            var name = $"{model.FirstName} {model.LastName}".Trim();
            var tiRegister = await tiAuth.RegisterAsync(name, model.Email, model.Password, "Recruiter", model.CompanyId);
            if (tiRegister is null)
            {
                ModelState.AddModelError(string.Empty, "Recruiter setup failed: company not found or already registered. Please try again.");
                ViewData["Companies"] = await tiAuth.GetCompaniesAsync();
                return View(model);
            }
        }

        var tiResult = await tiAuth.LoginAsync(model.Email, model.Password);
        await SignInAsync(result.Response, tiResult);

        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied() => View();

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Remove(SessionKeys.JwtToken);
        return RedirectToAction(nameof(Login));
    }

    private async Task SignInAsync(AuthResponse info, AuthResponseModel? tiInfo)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, info.UserId.ToString()),
            new Claim(ClaimTypes.Email, info.Email),
            new Claim(ClaimTypes.Name, $"{info.FirstName} {info.LastName}".Trim()),
        };

        // Add T&I role + JWT if T&I login succeeded
        if (tiInfo != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, tiInfo.Role));
            claims.Add(new Claim("jwt", tiInfo.Token));

            // Add CompanyId claim if the user is a recruiter with an associated company
            if (tiInfo.CompanyId.HasValue)
            {
                claims.Add(new Claim("CompanyId", tiInfo.CompanyId.Value.ToString()));
            }
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        HttpContext.Session.SetString(SessionKeys.JwtToken, info.Token);
    }
}
