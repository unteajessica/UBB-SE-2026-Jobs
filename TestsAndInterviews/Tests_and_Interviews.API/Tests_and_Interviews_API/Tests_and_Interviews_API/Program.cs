using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tests_and_Interviews.Services;
using Tests_and_Interviews_API.Data;
using Tests_and_Interviews_API.Helpers;
using Tests_and_Interviews_API.Mappers;
using Tests_and_Interviews_API.Repositories;
using Tests_and_Interviews_API.Repositories.Interfaces;
using Tests_and_Interviews_API.Services;
using Tests_and_Interviews_API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(Env.CONNECTION_STRING));

builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();
builder.Services.AddScoped<IApplicantRepository, ApplicantRepository>();
builder.Services.AddScoped<ICollaboratorsRepo, CollaboratorsRepo>();
builder.Services.AddScoped<ICompanyRepo, CompanyRepo>();
builder.Services.AddScoped<IEventsRepo, EventsRepo>();
builder.Services.AddScoped<IInterviewSessionRepository, InterviewSessionRepository>();
builder.Services.AddScoped<IJobsRepository, JobsRepository>();
builder.Services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<ISlotRepository, SlotRepository>();
builder.Services.AddScoped<ITestAttemptRepository, TestAttemptRepository>();
builder.Services.AddScoped<ITestRepository, TestRepository>();

builder.Services.AddScoped<IAnswerService, AnswerService>();
builder.Services.AddScoped<IApplicantService, ApplicantService>();
builder.Services.AddScoped<IAttemptValidationService, AttemptValidationService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ICollaboratorsService, CollaboratorsService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IDataProcessingService, DataProcessingService>();
builder.Services.AddScoped<IEventsService, EventsService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGradingService, GradingService>();
builder.Services.AddScoped<IInterviewSessionService, InterviewSessionService>();
builder.Services.AddScoped<IJobsService, JobsService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IProfileCompletionCalculator, ProfileCompletionCalculator>();
builder.Services.AddScoped<ISlotService, SlotService>();
builder.Services.AddScoped<ITestAttemptService, TestAttemptService>();
builder.Services.AddScoped<ITestService, TestService>();
builder.Services.AddScoped<ITimerService, TimerService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<ITestAttemptRepository, TestAttemptRepository>();
builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();
builder.Services.AddScoped<IGradingService, GradingService>();
builder.Services.AddScoped<ITimerService, TimerService>();
builder.Services.AddScoped<IAttemptValidationService, AttemptValidationService>();
builder.Services.AddScoped<IDataProcessingService, DataProcessingService>();
builder.Services.AddScoped<ITestService, TestService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ICompanyStatsService, CompanyStatsService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add CORS to allow Web project to access API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebProject", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5238",
            "https://localhost:7087",
            "http://localhost:3000",
            "http://127.0.0.1:5238"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "UBB-SE-2026", 
        ValidAudience = "UBB-SE-Client",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("O_CHEIE_SECRET_FOARTE_LUNGA_SI_SIGURA_AICI_12345!"))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tests and Interviews API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowWebProject");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
