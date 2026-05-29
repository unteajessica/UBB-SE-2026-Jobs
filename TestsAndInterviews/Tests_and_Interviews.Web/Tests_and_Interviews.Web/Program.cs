using Microsoft.AspNetCore.Authentication.Cookies;
using Tests_and_Interviews.Web.Services;

using Tests_and_Interviews.Web.Clients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
})
.ConfigureApplicationPartManager(manager =>
{
    var apiPart = manager.ApplicationParts
        .FirstOrDefault(p => p.Name == "Tests_and_Interviews_API");
    if (apiPart != null)
    {
        manager.ApplicationParts.Remove(apiPart);
    }
});

// Configure HttpClient for future API calls (not used yet with mock data)
builder.Services.AddHttpClient("BackendAPI", client =>
{
    // TODO: Update this URL when backend is deployed - maybe an env file or smth
    client.BaseAddress = new Uri("https://localhost:7000");
});

//builder.Services.AddScoped<Interface, Service>

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CandidateOnly", policy =>
        policy.RequireRole("Candidate"));

    options.AddPolicy("RecruiterOnly", policy =>
        policy.RequireRole("Recruiter"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("RecruiterOrAdmin", policy =>
        policy.RequireRole("Recruiter", "Admin"));

    options.AddPolicy("CandidateOrAdmin", policy =>
        policy.RequireRole("Candidate", "Admin"));

    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});
builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5179/");
});

builder.Services.AddHttpClient<TestsApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5179/");
});

builder.Services.AddHttpClient<JobsApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5179/");
});

builder.Services.AddHttpClient<ApplicantsApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5179/");
});


builder.Services.AddHttpClient<QuestionsApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5179");
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<UsersApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5179");
});

// Register AnswersApiClient for retrieving/saving answers
builder.Services.AddHttpClient<AnswersApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5179");
});

builder.Services.AddHttpClient<SlotsApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5179");
});
builder.Services.AddHttpClient<LeaderboardApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5179");
});

builder.Services.AddHttpClient<TestAttemptsApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5179");
});
builder.Services.AddHttpClient<EventsApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5179");
});

builder.Services.AddHttpClient<PaymentApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5179");
});

builder.Services.AddHttpClient<AnswersApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5179");
});

builder.Services.AddHttpClient<SlotsApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5179");
});

builder.Services.AddHttpClient<InterviewSessionsApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5179");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();