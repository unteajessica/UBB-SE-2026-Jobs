using System.Text;
using System.Web;
using Microsoft.Playwright;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.PdfExport;

namespace PussyCats.Api.Services.PdfExport;

public class PdfExportService : IPdfExportService
{
    private readonly string templateHtml;

    public PdfExportService(string templateHtml)
    {
        this.templateHtml = templateHtml;
    }

    public Task<string> RenderHtmlAsync(User user)
    {
        return Task.FromResult(BuildHtml(user));
    }

    public async Task<byte[]> GeneratePdfAsync(User user)
    {
        var html = BuildHtml(user);

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await page.SetContentAsync(html, new PageSetContentOptions { WaitUntil = WaitUntilState.NetworkIdle });
        return await page.PdfAsync(new PagePdfOptions { Format = "A4", PrintBackground = true });
    }

    private string BuildHtml(User user)
    {
        var skills = user.Skills is { Count: > 0 }
            ? string.Join(", ", user.Skills.Select(s => HttpUtility.HtmlEncode(s.Skill?.Name ?? string.Empty)))
            : "No skills listed";

        var workExperience = BuildSection(user.WorkExperiences, we =>
            $"<div class=\"entry\"><strong>{HttpUtility.HtmlEncode(we.Company)}</strong> — {HttpUtility.HtmlEncode(we.JobTitle)}<br/><em>{we.StartDate:yyyy-MM} – {(we.EndDate.HasValue ? we.EndDate.Value.ToString("yyyy-MM") : "Present")}</em><p>{HttpUtility.HtmlEncode(we.Description)}</p></div>");

        var projects = BuildSection(user.Projects, p =>
            $"<div class=\"entry\"><strong>{HttpUtility.HtmlEncode(p.Name)}</strong><p>{HttpUtility.HtmlEncode(p.Description)}</p></div>");

        var activities = BuildSection(user.ExtraCurricularActivities, a =>
            $"<div class=\"entry\"><strong>{HttpUtility.HtmlEncode(a.ActivityName)}</strong> — {HttpUtility.HtmlEncode(a.Organization)}<br/><em>{HttpUtility.HtmlEncode(a.Period)}</em><p>{HttpUtility.HtmlEncode(a.Description)}</p></div>");

        return templateHtml
            .Replace("{{FIRST_NAME}}", HttpUtility.HtmlEncode(user.FirstName))
            .Replace("{{LAST_NAME}}", HttpUtility.HtmlEncode(user.LastName))
            .Replace("{{EMAIL}}", HttpUtility.HtmlEncode(user.Email))
            .Replace("{{PHONE}}", HttpUtility.HtmlEncode(user.Phone))
            .Replace("{{COUNTRY}}", HttpUtility.HtmlEncode(user.Country))
            .Replace("{{CITY}}", HttpUtility.HtmlEncode(user.City))
            .Replace("{{UNIVERSITY}}", HttpUtility.HtmlEncode(user.University))
            .Replace("{{MOTIVATION}}", HttpUtility.HtmlEncode(user.Motivation))
            .Replace("{{LINKEDIN}}", HttpUtility.HtmlEncode(user.LinkedIn))
            .Replace("{{GITHUB}}", HttpUtility.HtmlEncode(user.GitHub))
            .Replace("{{SKILLS}}", skills)
            .Replace("{{WORK_EXPERIENCE}}", workExperience)
            .Replace("{{PROJECTS}}", projects)
            .Replace("{{ACTIVITIES}}", activities);
    }

    private static string BuildSection<T>(ICollection<T>? items, Func<T, string> render)
    {
        if (items is null || items.Count == 0)
            return "<p><em>None listed.</em></p>";
        var sb = new StringBuilder();
        foreach (var item in items)
            sb.Append(render(item));
        return sb.ToString();
    }
}
