using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PussyCats.Library.Migrations
{
    /// <inheritdoc />
    public partial class InitialMergedSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    company_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    company_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    about_us = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    profile_picture_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    logo_picture_url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    location = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    posted_jobs_count = table.Column<int>(type: "int", nullable: false),
                    collaborators_count = table.Column<int>(type: "int", nullable: false),
                    buddy_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    avatar_id = table.Column<int>(type: "int", nullable: true),
                    final_quote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    buddy_description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    scen_1_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scen1_answer1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scen1_answer2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scen1_answer3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scen1_reaction1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scen1_reaction2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scen1_reaction3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scen2_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scen2_answer1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scen2_answer2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scen2_answer3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scen2_reaction1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scen2_reaction2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scen2_reaction3 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.company_id);
                });

            migrationBuilder.CreateTable(
                name: "SkillGroups",
                columns: table => new
                {
                    SkillGroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: false),
                    JobRole = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillGroups", x => x.SkillGroupId);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    SkillId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.SkillId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    University = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Degree = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UniversityStartYear = table.Column<int>(type: "int", nullable: false),
                    ExpectedGraduationYear = table.Column<int>(type: "int", nullable: false),
                    GitHub = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LinkedIn = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Motivation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    HasDisabilities = table.Column<bool>(type: "bit", nullable: false),
                    ProfilePicturePath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ParsedCv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    PreferredEmploymentType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WorkModePreference = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    LocationPreference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    YearsOfExperience = table.Column<int>(type: "int", nullable: false),
                    TotalExperiencePoints = table.Column<int>(type: "int", nullable: false),
                    CurrentLevel = table.Column<int>(type: "int", nullable: false),
                    ActiveAccount = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "jobs",
                columns: table => new
                {
                    job_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    company_id = table.Column<int>(type: "int", nullable: false),
                    photo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    job_title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    industry_field = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    job_type = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    experience_level = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    start_date = table.Column<DateTime>(type: "date", nullable: true),
                    end_date = table.Column<DateTime>(type: "date", nullable: true),
                    job_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    job_location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    available_positions = table.Column<int>(type: "int", nullable: false),
                    posted_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    salary = table.Column<int>(type: "int", nullable: true),
                    amount_payed = table.Column<int>(type: "int", nullable: true),
                    deadline = table.Column<DateTime>(type: "date", nullable: true),
                    promotion_level = table.Column<int>(type: "int", nullable: true),
                    job_role = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jobs", x => x.job_id);
                    table.ForeignKey(
                        name: "FK_jobs_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "company_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkillGroupSkills",
                columns: table => new
                {
                    SkillGroupId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillGroupSkills", x => new { x.SkillGroupId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_SkillGroupSkills_SkillGroups_SkillGroupId",
                        column: x => x.SkillGroupId,
                        principalTable: "SkillGroups",
                        principalColumn: "SkillGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SkillGroupSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    DocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DocumentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_Documents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExtraCurricularActivities",
                columns: table => new
                {
                    ExtraCurricularActivityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ActivityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Organization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Period = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraCurricularActivities", x => x.ExtraCurricularActivityId);
                    table.ForeignKey(
                        name: "FK_ExtraCurricularActivities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalityTestResults",
                columns: table => new
                {
                    PersonalityTestResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SelectedRole = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalityTestResults", x => x.PersonalityTestResultId);
                    table.ForeignKey(
                        name: "FK_PersonalityTestResults_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Technologies = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectId);
                    table.ForeignKey(
                        name: "FK_Projects_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkillTests",
                columns: table => new
                {
                    SkillTestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    AchievedDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillTests", x => x.SkillTestId);
                    table.ForeignKey(
                        name: "FK_SkillTests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSkills",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    AchievedDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSkills", x => new { x.UserId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_UserSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSkills_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkExperiences",
                columns: table => new
                {
                    WorkExperienceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Company = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CurrentlyWorking = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkExperiences", x => x.WorkExperienceId);
                    table.ForeignKey(
                        name: "FK_WorkExperiences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    ChatId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    SecondUserId = table.Column<int>(type: "int", nullable: true),
                    JobId = table.Column<int>(type: "int", nullable: true),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    BlockedByUserId = table.Column<int>(type: "int", nullable: true),
                    DeletedAtByUser = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAtBySecondParty = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.ChatId);
                    table.ForeignKey(
                        name: "FK_Chats_Users_BlockedByUserId",
                        column: x => x.BlockedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Chats_Users_SecondUserId",
                        column: x => x.SecondUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Chats_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Chats_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "company_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Chats_jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "jobs",
                        principalColumn: "job_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobSkills",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    RequiredLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSkills", x => new { x.JobId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_JobSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobSkills_jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "jobs",
                        principalColumn: "job_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    MatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FeedbackMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.MatchId);
                    table.ForeignKey(
                        name: "FK_Matches_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "jobs",
                        principalColumn: "job_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Recommendations",
                columns: table => new
                {
                    RecommendationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.RecommendationId);
                    table.ForeignKey(
                        name: "FK_Recommendations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recommendations_jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "jobs",
                        principalColumn: "job_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonalityTraitScores",
                columns: table => new
                {
                    PersonalityTraitScoreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonalityTestResultId = table.Column<int>(type: "int", nullable: false),
                    Trait = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalityTraitScores", x => x.PersonalityTraitScoreId);
                    table.ForeignKey(
                        name: "FK_PersonalityTraitScores_PersonalityTestResults_PersonalityTestResultId",
                        column: x => x.PersonalityTestResultId,
                        principalTable: "PersonalityTestResults",
                        principalColumn: "PersonalityTestResultId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    MessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChatId = table.Column<int>(type: "int", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_Messages_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "ChatId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SkillGroups",
                columns: new[] { "SkillGroupId", "GroupName", "JobRole", "Weight" },
                values: new object[,]
                {
                    { 1, "UI Markup", "FrontendDeveloper", 20 },
                    { 2, "JavaScript", "FrontendDeveloper", 20 },
                    { 3, "Frontend Framework", "FrontendDeveloper", 25 },
                    { 4, "Version Control", "FrontendDeveloper", 10 },
                    { 5, "Testing", "FrontendDeveloper", 10 },
                    { 6, "Build Tools", "FrontendDeveloper", 8 },
                    { 7, "Design Collaboration", "FrontendDeveloper", 7 },
                    { 8, "Backend Language", "BackendDeveloper", 25 },
                    { 9, "Web Framework", "BackendDeveloper", 20 },
                    { 10, "Database Management", "BackendDeveloper", 20 },
                    { 11, "API Design", "BackendDeveloper", 15 },
                    { 12, "Version Control", "BackendDeveloper", 10 },
                    { 13, "Testing", "BackendDeveloper", 10 },
                    { 14, "Design Tools", "UiUxDesigner", 30 },
                    { 15, "Prototyping", "UiUxDesigner", 20 },
                    { 16, "User Research", "UiUxDesigner", 20 },
                    { 17, "Visual Design", "UiUxDesigner", 15 },
                    { 18, "Handoff", "UiUxDesigner", 10 },
                    { 19, "Analytics", "UiUxDesigner", 5 },
                    { 20, "Containerization", "DevOpsEngineer", 20 },
                    { 21, "Orchestration", "DevOpsEngineer", 20 },
                    { 22, "CI/CD", "DevOpsEngineer", 20 },
                    { 23, "Cloud Platform", "DevOpsEngineer", 15 },
                    { 24, "Infrastructure as Code", "DevOpsEngineer", 15 },
                    { 25, "Monitoring", "DevOpsEngineer", 10 },
                    { 26, "Methodologies", "ProjectManager", 25 },
                    { 27, "Project Tools", "ProjectManager", 20 },
                    { 28, "Risk Management", "ProjectManager", 20 },
                    { 29, "Communication", "ProjectManager", 20 },
                    { 30, "Budgeting", "ProjectManager", 15 },
                    { 31, "Query Language", "DataAnalyst", 25 },
                    { 32, "Data Visualization", "DataAnalyst", 25 },
                    { 33, "Programming", "DataAnalyst", 20 },
                    { 34, "Statistical Analysis", "DataAnalyst", 15 },
                    { 35, "Spreadsheets", "DataAnalyst", 10 },
                    { 36, "Data Cleaning", "DataAnalyst", 5 },
                    { 37, "Network Security", "CybersecuritySpecialist", 20 },
                    { 38, "Penetration Testing", "CybersecuritySpecialist", 20 },
                    { 39, "SIEM & Monitoring", "CybersecuritySpecialist", 15 },
                    { 40, "Cryptography", "CybersecuritySpecialist", 15 },
                    { 41, "Compliance & Standards", "CybersecuritySpecialist", 15 },
                    { 42, "Incident Response", "CybersecuritySpecialist", 15 },
                    { 43, "ML Frameworks", "AiMlEngineer", 25 },
                    { 44, "Programming", "AiMlEngineer", 20 },
                    { 45, "Mathematics", "AiMlEngineer", 20 },
                    { 46, "Data Engineering", "AiMlEngineer", 15 },
                    { 47, "Model Deployment", "AiMlEngineer", 10 },
                    { 48, "NLP / Computer Vision", "AiMlEngineer", 10 }
                });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "SkillId", "Category", "Name" },
                values: new object[,]
                {
                    { 1, "Backend Language", "C#" },
                    { 2, "Frontend Framework", "React" },
                    { 3, "Database Management", "SQL" },
                    { 4, "Testing", "Testing" },
                    { 5, "Testing", "Selenium" },
                    { 6, "Containerization", "Docker" },
                    { 7, "Orchestration", "Kubernetes" },
                    { 8, "Backend Language", "Python" },
                    { 9, "Data Cleaning", "Pandas" },
                    { 10, "ML Frameworks", "Machine Learning" },
                    { 11, "NLP / Computer Vision", "NLP" },
                    { 12, "Design Tools", "Figma" },
                    { 13, "Visual Design", "UI Design" },
                    { 14, "Leadership", "Architecture" },
                    { 15, "Leadership", "Leadership" },
                    { 16, "Cloud Platform", "Cloud" },
                    { 17, "Mobile", "Flutter" },
                    { 18, "Mobile", "Kotlin" },
                    { 19, "Penetration Testing", "Penetration Testing" },
                    { 20, "SIEM & Monitoring", "SIEM" },
                    { 21, "Backend Language", "Java" },
                    { 22, "Web Framework", "Spring Boot" },
                    { 23, "Methodologies", "Agile" },
                    { 24, "Data Engineering", "Spark" },
                    { 25, "Backend Language", "Go" },
                    { 26, "Database Management", "PostgreSQL" },
                    { 27, "NLP / Computer Vision", "Computer Vision" },
                    { 28, "ML Frameworks", "PyTorch" },
                    { 29, "Frontend Framework", "Angular" },
                    { 30, "Frontend Framework", "Vue.js" },
                    { 31, "JavaScript", "TypeScript" },
                    { 32, "Cloud Platform", "AWS" },
                    { 33, "UI Markup", "HTML" },
                    { 34, "UI Markup", "CSS" },
                    { 35, "UI Markup", "SCSS" },
                    { 36, "UI Markup", "Tailwind" },
                    { 37, "JavaScript", "JavaScript" },
                    { 38, "Frontend Framework", "Svelte" },
                    { 39, "Version Control", "Git" },
                    { 40, "Version Control", "GitHub" },
                    { 41, "Testing", "Jest" },
                    { 42, "Testing", "Cypress" },
                    { 43, "Build Tools", "Webpack" },
                    { 44, "Build Tools", "Vite" },
                    { 45, "Build Tools", "Parcel" },
                    { 46, "Design Tools", "Adobe XD" },
                    { 47, "Design Collaboration", "Zeplin" },
                    { 48, "Backend Language", "Node.js" },
                    { 49, "Web Framework", "ASP.NET" },
                    { 50, "Web Framework", "Django" },
                    { 51, "Database Management", "MySQL" },
                    { 52, "Database Management", "MongoDB" },
                    { 53, "Database Management", "Redis" },
                    { 54, "API Design", "REST" },
                    { 55, "API Design", "GraphQL" },
                    { 56, "API Design", "gRPC" },
                    { 57, "Testing", "JUnit" },
                    { 58, "Testing", "NUnit" },
                    { 59, "Testing", "pytest" },
                    { 60, "Testing", "Postman" },
                    { 61, "Design Tools", "Sketch" },
                    { 62, "Design Tools", "InVision" },
                    { 63, "Prototyping", "Figma Prototyping" },
                    { 64, "Prototyping", "Marvel" },
                    { 65, "Prototyping", "Axure" },
                    { 66, "User Research", "Interviews" },
                    { 67, "User Research", "Surveys" },
                    { 68, "User Research", "Usability Testing" },
                    { 69, "Visual Design", "Typography" },
                    { 70, "Visual Design", "Color Theory" },
                    { 71, "Visual Design", "Grid Systems" },
                    { 72, "Handoff", "Storybook" },
                    { 73, "Analytics", "Google Analytics" },
                    { 74, "Analytics", "Hotjar" },
                    { 75, "Analytics", "Mixpanel" },
                    { 76, "Containerization", "Podman" },
                    { 77, "Orchestration", "Docker Swarm" },
                    { 78, "Orchestration", "OpenShift" },
                    { 79, "CI/CD", "Jenkins" },
                    { 80, "CI/CD", "GitHub Actions" },
                    { 81, "CI/CD", "GitLab CI" },
                    { 82, "CI/CD", "CircleCI" },
                    { 83, "Cloud Platform", "Azure" },
                    { 84, "Cloud Platform", "Google Cloud" },
                    { 85, "Infrastructure as Code", "Terraform" },
                    { 86, "Infrastructure as Code", "Ansible" },
                    { 87, "Infrastructure as Code", "Pulumi" },
                    { 88, "Monitoring", "Prometheus" },
                    { 89, "Monitoring", "Grafana" },
                    { 90, "Monitoring", "Datadog" },
                    { 91, "Methodologies", "Scrum" },
                    { 92, "Methodologies", "Kanban" },
                    { 93, "Methodologies", "Waterfall" },
                    { 94, "Project Tools", "Jira" },
                    { 95, "Project Tools", "Trello" },
                    { 96, "Project Tools", "Asana" },
                    { 97, "Risk Management", "Risk Assessment" },
                    { 98, "Risk Management", "Mitigation Planning" },
                    { 99, "Communication", "Stakeholder Management" },
                    { 100, "Communication", "Reporting" },
                    { 101, "Communication", "Presentations" },
                    { 102, "Budgeting", "Cost Estimation" },
                    { 103, "Budgeting", "Budget Tracking" },
                    { 104, "Budgeting", "MS Project" },
                    { 105, "Query Language", "BigQuery" },
                    { 106, "Data Visualization", "Power BI" },
                    { 107, "Data Visualization", "Tableau" },
                    { 108, "Data Visualization", "Looker" },
                    { 109, "Programming", "R" },
                    { 110, "Statistical Analysis", "Descriptive Statistics" },
                    { 111, "Statistical Analysis", "Regression" },
                    { 112, "Statistical Analysis", "Hypothesis Testing" },
                    { 113, "Spreadsheets", "Excel" },
                    { 114, "Spreadsheets", "Google Sheets" },
                    { 115, "Data Cleaning", "OpenRefine" },
                    { 116, "Network Security", "Firewalls" },
                    { 117, "Network Security", "VPN" },
                    { 118, "Network Security", "IDS/IPS" },
                    { 119, "Network Security", "TCP/IP" },
                    { 120, "Penetration Testing", "Metasploit" },
                    { 121, "Penetration Testing", "Burp Suite" },
                    { 122, "Penetration Testing", "Nmap" },
                    { 123, "SIEM & Monitoring", "Splunk" },
                    { 124, "SIEM & Monitoring", "IBM QRadar" },
                    { 125, "SIEM & Monitoring", "Microsoft Sentinel" },
                    { 126, "Cryptography", "AES" },
                    { 127, "Cryptography", "RSA" },
                    { 128, "Cryptography", "PKI" },
                    { 129, "Cryptography", "TLS/SSL" },
                    { 130, "Compliance & Standards", "ISO 27001" },
                    { 131, "Compliance & Standards", "GDPR" },
                    { 132, "Compliance & Standards", "NIST" },
                    { 133, "Compliance & Standards", "SOC 2" },
                    { 134, "Incident Response", "Forensics" },
                    { 135, "Incident Response", "Malware Analysis" },
                    { 136, "Incident Response", "DFIR" },
                    { 137, "ML Frameworks", "TensorFlow" },
                    { 138, "ML Frameworks", "scikit-learn" },
                    { 139, "ML Frameworks", "Keras" },
                    { 140, "Programming", "Julia" },
                    { 141, "Mathematics", "Linear Algebra" },
                    { 142, "Mathematics", "Calculus" },
                    { 143, "Mathematics", "Probability" },
                    { 144, "Mathematics", "Statistics" },
                    { 145, "Data Engineering", "NumPy" },
                    { 146, "Data Engineering", "Apache Spark" },
                    { 147, "Model Deployment", "FastAPI" },
                    { 148, "Model Deployment", "MLflow" },
                    { 149, "NLP / Computer Vision", "Hugging Face" },
                    { 150, "NLP / Computer Vision", "OpenCV" },
                    { 151, "NLP / Computer Vision", "NLTK" },
                    { 152, "NLP / Computer Vision", "spaCy" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "ActiveAccount", "Address", "Age", "City", "Country", "CreatedAt", "CurrentLevel", "Degree", "Email", "ExpectedGraduationYear", "FirstName", "Gender", "GitHub", "HasDisabilities", "LastName", "LastUpdated", "LinkedIn", "LocationPreference", "Motivation", "ParsedCv", "PasswordHash", "Phone", "PreferredEmploymentType", "ProfilePicturePath", "TotalExperiencePoints", "University", "UniversityStartYear", "WorkModePreference", "YearsOfExperience" },
                values: new object[] { 1, true, "123 Main St", 25, "Bucharest", "Romania", new DateTime(2025, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "Computer Science", "alice.smith@example.com", 2022, "Alice", "", "", false, "Smith", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "", "", "", "", "+40123456789", "", "", 0, "University of Bucharest", 2018, "", 0 });

            migrationBuilder.InsertData(
                table: "companies",
                columns: new[] { "company_id", "about_us", "avatar_id", "buddy_description", "buddy_name", "collaborators_count", "email", "final_quote", "location", "logo_picture_url", "company_name", "posted_jobs_count", "profile_picture_url", "scen1_answer1", "scen1_answer2", "scen1_answer3", "scen1_reaction1", "scen1_reaction2", "scen1_reaction3", "scen_1_text", "scen2_answer1", "scen2_answer2", "scen2_answer3", "scen2_reaction1", "scen2_reaction2", "scen2_reaction3", "scen2_text" },
                values: new object[,]
                {
                    { 1, null, null, null, null, 1, "hr@technova.com", null, "San Francisco, CA", "technova_logo.png", "TechNova", 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
                    { 2, null, null, null, null, 1, "careers@dataflow.com", null, "New York, NY", "dataflow_logo.png", "DataFlow Inc", 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
                    { 3, null, null, null, null, 2, "hello@ecocode.com", null, "Seattle, WA", "ecocode_logo.png", "EcoCode", 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
                    { 4, null, null, null, null, 1, "hr@finedge.com", null, "London, UK", "finedge_logo.png", "FinEdge", 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "SkillGroupSkills",
                columns: new[] { "SkillGroupId", "SkillId" },
                values: new object[,]
                {
                    { 1, 33 },
                    { 1, 34 },
                    { 1, 35 },
                    { 1, 36 },
                    { 2, 31 },
                    { 2, 37 },
                    { 3, 2 },
                    { 3, 29 },
                    { 3, 30 },
                    { 3, 38 },
                    { 4, 39 },
                    { 4, 40 },
                    { 5, 5 },
                    { 5, 41 },
                    { 5, 42 },
                    { 6, 43 },
                    { 6, 44 },
                    { 6, 45 },
                    { 7, 12 },
                    { 7, 46 },
                    { 7, 47 },
                    { 8, 1 },
                    { 8, 8 },
                    { 8, 21 },
                    { 8, 25 },
                    { 8, 48 },
                    { 9, 22 },
                    { 9, 49 },
                    { 9, 50 },
                    { 10, 3 },
                    { 10, 26 },
                    { 10, 51 },
                    { 10, 52 },
                    { 10, 53 },
                    { 11, 54 },
                    { 11, 55 },
                    { 11, 56 },
                    { 12, 39 },
                    { 12, 40 },
                    { 13, 57 },
                    { 13, 58 },
                    { 13, 59 },
                    { 13, 60 },
                    { 14, 12 },
                    { 14, 46 },
                    { 14, 61 },
                    { 14, 62 },
                    { 15, 63 },
                    { 15, 64 },
                    { 15, 65 },
                    { 16, 66 },
                    { 16, 67 },
                    { 16, 68 },
                    { 17, 69 },
                    { 17, 70 },
                    { 17, 71 },
                    { 18, 12 },
                    { 18, 47 },
                    { 18, 72 },
                    { 19, 73 },
                    { 19, 74 },
                    { 19, 75 },
                    { 20, 6 },
                    { 20, 76 },
                    { 21, 7 },
                    { 21, 77 },
                    { 21, 78 },
                    { 22, 79 },
                    { 22, 80 },
                    { 22, 81 },
                    { 22, 82 },
                    { 23, 32 },
                    { 23, 83 },
                    { 23, 84 },
                    { 24, 85 },
                    { 24, 86 },
                    { 24, 87 },
                    { 25, 88 },
                    { 25, 89 },
                    { 25, 90 },
                    { 26, 23 },
                    { 26, 91 },
                    { 26, 92 },
                    { 26, 93 },
                    { 27, 94 },
                    { 27, 95 },
                    { 27, 96 },
                    { 28, 97 },
                    { 28, 98 },
                    { 29, 99 },
                    { 29, 100 },
                    { 29, 101 },
                    { 30, 102 },
                    { 30, 103 },
                    { 30, 104 },
                    { 31, 3 },
                    { 31, 26 },
                    { 31, 105 },
                    { 32, 106 },
                    { 32, 107 },
                    { 32, 108 },
                    { 33, 8 },
                    { 33, 109 },
                    { 34, 110 },
                    { 34, 111 },
                    { 34, 112 },
                    { 35, 113 },
                    { 35, 114 },
                    { 36, 9 },
                    { 36, 115 },
                    { 37, 116 },
                    { 37, 117 },
                    { 37, 118 },
                    { 37, 119 },
                    { 38, 120 },
                    { 38, 121 },
                    { 38, 122 },
                    { 39, 123 },
                    { 39, 124 },
                    { 39, 125 },
                    { 40, 126 },
                    { 40, 127 },
                    { 40, 128 },
                    { 40, 129 },
                    { 41, 130 },
                    { 41, 131 },
                    { 41, 132 },
                    { 41, 133 },
                    { 42, 134 },
                    { 42, 135 },
                    { 42, 136 },
                    { 43, 28 },
                    { 43, 137 },
                    { 43, 138 },
                    { 43, 139 },
                    { 44, 8 },
                    { 44, 109 },
                    { 44, 140 },
                    { 45, 141 },
                    { 45, 142 },
                    { 45, 143 },
                    { 45, 144 },
                    { 46, 3 },
                    { 46, 9 },
                    { 46, 145 },
                    { 46, 146 },
                    { 47, 6 },
                    { 47, 147 },
                    { 47, 148 },
                    { 48, 149 },
                    { 48, 150 },
                    { 48, 151 },
                    { 48, 152 }
                });

            migrationBuilder.InsertData(
                table: "SkillTests",
                columns: new[] { "SkillTestId", "AchievedDate", "Name", "Score", "UserId" },
                values: new object[,]
                {
                    { 1, new DateOnly(2026, 1, 7), "C# Fundamentals", 82, 1 },
                    { 2, new DateOnly(2026, 1, 7), "SQL Server", 76, 1 },
                    { 3, new DateOnly(2026, 1, 7), "Software Design", 88, 1 }
                });

            migrationBuilder.InsertData(
                table: "jobs",
                columns: new[] { "job_id", "amount_payed", "available_positions", "company_id", "deadline", "end_date", "experience_level", "industry_field", "job_description", "job_location", "job_role", "job_title", "job_type", "photo", "posted_at", "promotion_level", "salary", "start_date" },
                values: new object[,]
                {
                    { 101, 0, 3, 1, new DateTime(2026, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Mid-Level", "IT", "Develop robust REST APIs using .NET Core.", "Remote", 1, "Backend C# Developer", "Full-time", null, new DateTime(2026, 4, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), 2, 95000, null },
                    { 102, 0, 1, 2, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Senior", "Data Science", "Maintain cloud data pipelines and warehouses.", "New York, NY", 5, "Data Engineer", "Contract", null, new DateTime(2026, 4, 18, 10, 30, 0, 0, DateTimeKind.Unspecified), 1, 120000, null }
                });

            migrationBuilder.InsertData(
                table: "JobSkills",
                columns: new[] { "JobId", "SkillId", "RequiredLevel" },
                values: new object[,]
                {
                    { 101, 1, 80 },
                    { 101, 3, 75 },
                    { 102, 8, 68 },
                    { 102, 9, 62 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chats_BlockedByUserId",
                table: "Chats",
                column: "BlockedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_CompanyId",
                table: "Chats",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_JobId",
                table: "Chats",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_SecondUserId",
                table: "Chats",
                column: "SecondUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_UserId",
                table: "Chats",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UserId",
                table: "Documents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraCurricularActivities_UserId",
                table: "ExtraCurricularActivities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_company_id",
                table: "jobs",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_job_location",
                table: "jobs",
                column: "job_location");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_job_type",
                table: "jobs",
                column: "job_type");

            migrationBuilder.CreateIndex(
                name: "IX_JobSkills_SkillId",
                table: "JobSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_JobId",
                table: "Matches",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_Status",
                table: "Matches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_UserId_JobId",
                table: "Matches",
                columns: new[] { "UserId", "JobId" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatId",
                table: "Messages",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalityTestResults_UserId",
                table: "PersonalityTestResults",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalityTraitScores_PersonalityTestResultId",
                table: "PersonalityTraitScores",
                column: "PersonalityTestResultId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UserId",
                table: "Projects",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_JobId",
                table: "Recommendations",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_UserId",
                table: "Recommendations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_UserId_JobId_Timestamp",
                table: "Recommendations",
                columns: new[] { "UserId", "JobId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SkillGroups_JobRole",
                table: "SkillGroups",
                column: "JobRole");

            migrationBuilder.CreateIndex(
                name: "IX_SkillGroupSkills_SkillId",
                table: "SkillGroupSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Name",
                table: "Skills",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkillTests_UserId",
                table: "SkillTests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSkills_SkillId",
                table: "UserSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkExperiences_UserId",
                table: "WorkExperiences",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "ExtraCurricularActivities");

            migrationBuilder.DropTable(
                name: "JobSkills");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "PersonalityTraitScores");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "SkillGroupSkills");

            migrationBuilder.DropTable(
                name: "SkillTests");

            migrationBuilder.DropTable(
                name: "UserSkills");

            migrationBuilder.DropTable(
                name: "WorkExperiences");

            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropTable(
                name: "PersonalityTestResults");

            migrationBuilder.DropTable(
                name: "SkillGroups");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "jobs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "companies");
        }
    }
}
