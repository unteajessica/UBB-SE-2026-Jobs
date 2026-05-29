namespace Tests_and_Interviews.Web.Controllers
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Tests_and_Interviews.Web.Models;
    using Tests_and_Interviews.Web.Services;
    using Tests_and_Interviews.Web.ViewModels;

    /// <summary>
    /// Handles login, registration and logout for the MVC web app.
    /// </summary>
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IAuthService authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="authService">The authentication service.</param>
        public AccountController(IAuthService authService)
        {
            this.authService = authService;
        }

        /// <summary>
        /// Displays the login page.
        /// </summary>
        /// <returns>The login view.</returns>
        [HttpGet]
        public IActionResult Login() => this.View();

        /// <summary>
        /// Handles login form submission.
        /// </summary>
        /// <param name="model">The login view model.</param>
        /// <returns>Redirects to home on success, returns login view on failure.</returns>
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            AuthResponseModel? response =
                await this.authService.LoginAsync(model.Email, model.Password);

            if (response == null)
            {
                this.ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return this.View(model);
            }

            await this.SignInUserAsync(response);
            return this.RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Displays the registration page.
        /// </summary>
        /// <returns>The register view.</returns>
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            // Load companies from API and pass to view
            var companies = await this.authService.GetCompaniesAsync();
            this.ViewData["Companies"] = companies;
            return this.View();
        }

        /// <summary>
        /// Handles registration form submission.
        /// </summary>
        /// <param name="model">The register view model.</param>
        /// <returns>Redirects to home on success, returns register view on failure.</returns>
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            AuthResponseModel? response = await this.authService.RegisterAsync(
                model.Name, model.Email, model.Password, model.Role, model.CompanyId);

            if (response == null)
            {
                this.ModelState.AddModelError(string.Empty, "Email already in use.");
                return this.View(model);
            }

            await this.SignInUserAsync(response);
            return this.RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Logs the current user out.
        /// </summary>
        /// <returns>Redirects to login page.</returns>
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await this.HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return this.RedirectToAction("Login");
        }

        private async Task SignInUserAsync(AuthResponseModel response)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, response.Name),
                new Claim(ClaimTypes.Email, response.Name),
                new Claim(ClaimTypes.Role, response.Role),
                new Claim(ClaimTypes.NameIdentifier, response.UserId.ToString()),
                new Claim("jwt", response.Token),
            };

            ClaimsIdentity identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            await this.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        /// <summary>
        /// Displays the access denied page.
        /// </summary>
        /// <returns>The access denied view.</returns>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return this.View();
        }
    }
}