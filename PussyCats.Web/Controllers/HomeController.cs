using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Web.Infrastructure;
using PussyCats.Web.Models;
using System.Diagnostics;

namespace PussyCats.Web.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            var selected = HttpContext.Session.GetString(SessionKeys.Mode);
            if (string.IsNullOrWhiteSpace(selected))
            {
                return View();
            }

            return RedirectToModeHome(selected);
        }

        [AllowAnonymous]
        public IActionResult SwitchMode()
        {
            HttpContext.Session.Remove(SessionKeys.Mode);
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectMode(string mode)
        {
            if (!AppModes.IsValid(mode))
            {
                TempData["ModeError"] = "Select a valid mode to continue.";
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.SetString(SessionKeys.Mode, mode);
            return RedirectToModeHome(mode);
        }

        private IActionResult RedirectToModeHome(string mode)
        {
            if (string.Equals(mode, AppModes.User, StringComparison.OrdinalIgnoreCase))
            {
                var userLanding = Url.Action("Index", "UserProfile") ?? "/";
                return User.Identity?.IsAuthenticated == true
                    ? Redirect(userLanding)
                    : RedirectToAction("Login", "Account", new { returnUrl = userLanding });
            }

            if (string.Equals(mode, AppModes.Company, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Matches");
            }

            return RedirectToAction("Index", "Developer");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
