using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Tests_and_Interviews.Web.Models;
using Microsoft.AspNetCore.Authorization;

namespace Tests_and_Interviews.Web.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}