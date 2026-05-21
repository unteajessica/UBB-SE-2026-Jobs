using Microsoft.EntityFrameworkCore;
using PussyCats.Api.Configuration;
using PussyCats.Library.Persistence;
using PussyCats.Library.Repositories.Chats;
using PussyCats.Library.Repositories.Companies;
using PussyCats.Library.Repositories.Documents;
using PussyCats.Library.Repositories.Jobs;
using PussyCats.Library.Repositories.Matches;
using PussyCats.Library.Repositories.Messages;
using PussyCats.Library.Repositories.PersonalityTests;
using PussyCats.Library.Repositories.Recommendations;
using PussyCats.Library.Repositories.Skills;
using PussyCats.Library.Repositories.SkillTests;
using PussyCats.Library.Repositories.Users;
using PussyCats.Library.Services.CompatibilityService;
using PussyCats.Library.Services.CooldownService;
using PussyCats.Library.Services.CompanyService;
using PussyCats.Library.Services.CompanyStatusService;
using PussyCats.Library.Services.Documents;
using PussyCats.Library.Services.CvParsing;
using PussyCats.Library.Services.FileStorage;
using PussyCats.Library.Services.Jobs;
using PussyCats.Library.Services.JobSkills;
using PussyCats.Library.Services.Matches;
using PussyCats.Library.Services.PersonalityTestService;
using PussyCats.Library.Services.Preferences;
using PussyCats.Library.Services.RecommendationAlgorithm;
using PussyCats.Library.Services.Recommendations;
using PussyCats.Library.Services.Skills;
using PussyCats.Library.Services.SkillTests;
using PussyCats.Library.Services.UserProfileService;
using PussyCats.Library.Services.UserRecommendationService;
using PussyCats.Library.Services.Users;
using PussyCats.Library.Services.UserSkillService;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
builder.Services.AddControllers(options =>
    {
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
/*
builder.Services.AddControllers(options =>
    {
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });*/

builder.Services.AddOpenApi();

builder.Services.AddDbContext<PussyCatsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PussyCatsDb")));

builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IJobSkillRepository, JobSkillRepository>();
builder.Services.AddScoped<IJobSkillService, JobSkillService>();
builder.Services.AddScoped<IUserSkillRepository, UserSkillRepository>();
builder.Services.AddScoped<ISkillGroupRepository, SkillGroupRepository>();
builder.Services.AddScoped<ISkillTestRepository, SkillTestRepository>();
builder.Services.AddScoped<IPersonalityTestRepository, PersonalityTestRepository>();
builder.Services.AddScoped<IRecommendationRepository, RecommendationRepository>();

// Library/Services seam — see docs/ServiceApiRefactor.md
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<IDocumentService>(provider => provider.GetRequiredService<DocumentService>());
builder.Services.AddScoped<ILocalDocumentFileService>(provider => provider.GetRequiredService<DocumentService>());
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IPersonalityTestService, PersonalityTestService>();
builder.Services.AddScoped<ICvParsingService, CvParsingService>();
builder.Services.AddSingleton<ILocalFileStorageService, ApiLocalFileStorageService>();
builder.Services.AddScoped<ISkillTestService, SkillTestService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IRecommendationAlgorithm, RecommendationAlgorithm>();
builder.Services.AddScoped<IUserRecommendationService, UserRecommendationService>();
builder.Services.AddScoped<ICooldownService>(provider =>
    new CooldownService(
        provider.GetRequiredService<IRecommendationRepository>(),
        TimeSpan.FromHours(24) 
    ));


builder.Services.AddScoped<IUserSkillService, UserSkillService>();

builder.Services.AddScoped<ICompatibilityService, CompatibilityService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<ICompanyStatusService, CompanyStatusService>();
builder.Services.AddScoped<IPreferenceService, PreferenceService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PussyCats.Library.Persistence.PussyCatsDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
    
