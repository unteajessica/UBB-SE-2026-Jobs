using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Services.CompanyService;

namespace PussyCats.Web.Controllers;

public class CompanyController : Controller
{
    private readonly ICompanyService companyService;

    public CompanyController(ICompanyService companyService)
    {
        this.companyService = companyService;
    }

    public async Task<IActionResult> Profile(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            ViewBag.ErrorMessage = "No company ID provided.";
            return View();
        }

        try
        {
            var company = await companyService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

            if (company is null)
            {
                ViewBag.ErrorMessage = "Company not found.";
                return View();
            }

            return View(company);
        }
        catch (Exception exception)
        {
            ViewBag.ErrorMessage = exception.Message;
            return View();
        }
    }
}