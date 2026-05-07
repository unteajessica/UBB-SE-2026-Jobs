using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Persistence;
using Scalar.AspNetCore;
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
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

builder.Services.AddControllers(options =>
    {
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

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
builder.Services.AddScoped<IJobSkillRepository, JobSkillRepository>();
builder.Services.AddScoped<IUserSkillRepository, UserSkillRepository>();
builder.Services.AddScoped<ISkillGroupRepository, SkillGroupRepository>();
builder.Services.AddScoped<ISkillTestRepository, SkillTestRepository>();
builder.Services.AddScoped<IPersonalityTestRepository, PersonalityTestRepository>();
builder.Services.AddScoped<IRecommendationRepository, RecommendationRepository>();

var app = builder.Build();

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
    
