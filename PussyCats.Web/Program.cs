using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Authorization;
using PussyCats.Library.Services.CompatibilityService;
using PussyCats.Library.Services.Developers;
using PussyCats.Library.Services.CompanyService;
using PussyCats.Library.Services.CooldownService;
using PussyCats.Library.Services.CompanyRecommendationService;
using PussyCats.Library.Services.CompanyStatusService;
using PussyCats.Library.Services.CompletenessService;
using PussyCats.Library.Services.Documents;
using PussyCats.Library.Services.Jobs;
using PussyCats.Library.Services.JobSkills;
using PussyCats.Library.Services.Matches;
using PussyCats.Library.Services.PersonalityTestService;
using PussyCats.Library.Services.Preferences;
using PussyCats.Library.Services.Recommendations;
using PussyCats.Library.Services.Skills;
using PussyCats.Library.Services.SkillTests;
using PussyCats.Library.Services.UserProfileService;
using PussyCats.Library.Services.UserRecommendationService;
using PussyCats.Library.Services.Users;
using PussyCats.Library.Services.UserSkillService;
using PussyCats.Library.Services.UserStatusService;
using PussyCats.Web.Configuration;
using PussyCats.Web.ServiceProxies;

var builder = WebApplication.CreateBuilder(args);

var apiConfig = builder.Configuration
        .GetSection("Api")
        .Get<ApiConfiguration>()
        ?? throw new InvalidOperationException("Missing 'Api' configuration section in appsettings.json.");

builder.Services.AddSingleton(apiConfig);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ICompletenessService, CompletenessService>();

builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    options.Filters.Add(new AuthorizeFilter());
})
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });
builder.Services.AddAuthorization();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

RegisterServiceProxy<IDeveloperService, DeveloperServiceProxy>(builder.Services, apiConfig);
RegisterServiceProxy<ICompanyService, CompanyServiceProxy>(builder.Services, apiConfig);
RegisterServiceProxy<ICompatibilityService, CompatibilityServiceProxy>(builder.Services, apiConfig);
RegisterServiceProxy<ICooldownService, CooldownServiceProxy>(builder.Services, apiConfig);
RegisterServiceProxy<ICompanyRecommendationService, CompanyRecommendationServiceProxy>(builder.Services, apiConfig);
RegisterServiceProxy<ICompanyStatusService, CompanyStatusServiceProxy>(builder.Services, apiConfig);
RegisterServiceProxy<IDocumentService, DocumentServiceProxy>(builder.Services, apiConfig);
RegisterServiceProxy<IJobService, JobServiceProxy>(builder.Services, apiConfig);
RegisterServiceProxy<IJobSkillService, JobSkillServiceProxy>(builder.Services, apiConfig);
RegisterServiceProxy<IMatchService, MatchServiceProxy>(builder.Services, apiConfig);
RegisterServiceProxy<IPersonalityTestService, PersonalityTestServiceProxy>(builder.Services, apiConfig);
RegisterServiceProxy<IPreferenceService, PreferenceServiceProxy>(builder.Services, apiConfig);
RegisterServiceProxy<IRecommendationService, RecommendationServiceProxy>(builder.Services, apiConfig);
RegisterServiceProxy<ISkillService, SkillServiceProxy>(builder.Services, apiConfig);
RegisterServiceProxy<ISkillTestService, SkillTestServiceProxy>(builder.Services, apiConfig);
RegisterServiceProxy<IUserProfileService, UserProfileServiceProxy>(builder.Services, apiConfig);
RegisterServiceProxy<IUserRecommendationService, UserRecommendationServiceProxy>(builder.Services, apiConfig);
RegisterServiceProxy<IUserService, UserServiceProxy>(builder.Services, apiConfig);
RegisterServiceProxy<IUserStatusService, UserStatusServiceProxy>(builder.Services, apiConfig);

builder.Services.AddHttpClient<IUserSkillService, UserSkillServiceProxy>(client =>
{
    client.BaseAddress = new Uri(apiConfig.BaseUrl);
});
builder.Services.AddHttpClient<AuthServiceProxy>(client =>
    client.BaseAddress = new Uri(apiConfig.BaseUrl));
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static void RegisterServiceProxy<TService, TProxy>(
    IServiceCollection services,
    ApiConfiguration apiConfiguration)
    where TService : class
    where TProxy : class, TService
{
    services.AddHttpClient<TService, TProxy>(client =>
        client.BaseAddress = new Uri(apiConfiguration.BaseUrl));
}
