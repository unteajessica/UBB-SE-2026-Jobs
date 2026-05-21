using PussyCats.Library.Services.CompanyService;
using PussyCats.Library.Services.CompletenessService;
using PussyCats.Library.Services.Documents;
using PussyCats.Library.Services.Jobs;
using PussyCats.Library.Services.Matches;
using PussyCats.Library.Services.PersonalityTestService;
using PussyCats.Library.Services.Recommendations;
using PussyCats.Library.Services.Skills;
using PussyCats.Library.Services.SkillTests;
using PussyCats.Library.Services.UserProfileService;
using PussyCats.Library.Services.Users;
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

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews()
    .AddJsonOptions(opt =>
        opt.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

static void RegisterServiceProxy<TService, TProxy>(
    IServiceCollection services,
    ApiConfiguration apiConfiguration)
    where TService : class
    where TProxy : class, TService
{
    services.AddHttpClient<TService, TProxy>(client =>
        client.BaseAddress = new Uri(apiConfiguration.BaseUrl));
}


RegisterServiceProxy<ICompanyService, CompanyServiceProxy>(builder.Services, apiConfig);


builder.Services.AddHttpClient<ISkillService, SkillServiceProxy>(client =>
{
    client.BaseAddress = new Uri(apiConfig.BaseUrl);
});

builder.Services.AddHttpClient<IUserService, UserServiceProxy>(client =>
{
    client.BaseAddress = new Uri(apiConfig.BaseUrl);
});

builder.Services.AddHttpClient<IJobService, JobServiceProxy>(client =>
{
    client.BaseAddress = new Uri(apiConfig.BaseUrl);
});

builder.Services.AddHttpClient<IRecommendationService, RecommendationServiceProxy>(client =>
{
    client.BaseAddress = new Uri(apiConfig.BaseUrl);
});
builder.Services.AddHttpClient<IMatchService, MatchServiceProxy>(client =>
{
    client.BaseAddress = new Uri(apiConfig.BaseUrl);
});
builder.Services.AddHttpClient<IPersonalityTestService, PersonalityTestServiceProxy>(client =>
{ 
    client.BaseAddress = new Uri(apiConfig.BaseUrl);
});

builder.Services.AddHttpClient<IDocumentService, DocumentServiceProxy>(client =>
{  
  client.BaseAddress = new Uri(apiConfig.BaseUrl);
});
builder.Services.AddHttpClient<ISkillTestService, SkillTestServiceProxy>(client =>
{ 
    client.BaseAddress = new Uri(apiConfig.BaseUrl);
});
builder.Services.AddHttpClient<IUserProfileService, UserProfileServiceProxy>(client =>
{
    client.BaseAddress = new Uri(apiConfig.BaseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
