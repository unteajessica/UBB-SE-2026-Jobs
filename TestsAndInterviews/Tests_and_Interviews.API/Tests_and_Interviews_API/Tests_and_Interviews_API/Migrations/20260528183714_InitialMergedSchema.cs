using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tests_and_Interviews_API.Migrations
{
    /// <inheritdoc />
    public partial class InitialMergedSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "applicants",
                columns: table => new
                {
                    applicant_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    job_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    app_test_grade = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    cv_grade = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    company_test_grade = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    interview_grade = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    application_status = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    applied_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    recommended_from_company_id = table.Column<int>(type: "int", nullable: true),
                    cv_file_url = table.Column<string>(type: "nvarchar(500)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applicants", x => x.applicant_id);
                    table.ForeignKey(
                        name: "FK_applicants_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_applicants_companies_recommended_from_company_id",
                        column: x => x.recommended_from_company_id,
                        principalTable: "companies",
                        principalColumn: "company_id");
                    table.ForeignKey(
                        name: "FK_applicants_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "jobs",
                        principalColumn: "job_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    photo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    start_date = table.Column<DateTime>(type: "date", nullable: false),
                    end_date = table.Column<DateTime>(type: "date", nullable: false),
                    location = table.Column<string>(type: "nvarchar(300)", nullable: false),
                    host_company_id = table.Column<int>(type: "int", nullable: false),
                    posted_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.event_id);
                    table.ForeignKey(
                        name: "FK_events_companies_host_company_id",
                        column: x => x.host_company_id,
                        principalTable: "companies",
                        principalColumn: "company_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "interview_sessions",
                columns: table => new
                {
                    session_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    position_id = table.Column<int>(type: "int", nullable: false),
                    external_user_id = table.Column<int>(type: "int", nullable: true),
                    interviewer_id = table.Column<int>(type: "int", nullable: false),
                    date_start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    video = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    score = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interview_sessions", x => x.session_id);
                    table.ForeignKey(
                        name: "FK_interview_sessions_Users_external_user_id",
                        column: x => x.external_user_id,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Recruiters",
                columns: table => new
                {
                    company_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recruiters", x => new { x.company_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_Recruiters_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_Recruiters_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "company_id");
                });

            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "collaborators",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "int", nullable: false),
                    company_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collaborators", x => new { x.event_id, x.company_id });
                    table.ForeignKey(
                        name: "FK_collaborators_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "company_id");
                    table.ForeignKey(
                        name: "FK_collaborators_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "event_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Slots",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    recruiter_id = table.Column<int>(type: "int", nullable: false),
                    RecruiterCompanyId = table.Column<int>(type: "int", nullable: false),
                    RecruiterUserId = table.Column<int>(type: "int", nullable: false),
                    candidate_id = table.Column<int>(type: "int", nullable: true),
                    start_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    duration = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    interview_type = table.Column<string>(type: "nvarchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slots", x => x.id);
                    table.ForeignKey(
                        name: "FK_Slots_Recruiters_RecruiterCompanyId_RecruiterUserId",
                        columns: x => new { x.RecruiterCompanyId, x.RecruiterUserId },
                        principalTable: "Recruiters",
                        principalColumns: new[] { "company_id", "user_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Slots_Users_candidate_id",
                        column: x => x.candidate_id,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LeaderboardEntries",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    test_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    normalized_score = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    rank_position = table.Column<int>(type: "int", nullable: false),
                    tie_break_priority = table.Column<int>(type: "int", nullable: false),
                    last_recalculation_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderboardEntries", x => x.id);
                    table.ForeignKey(
                        name: "FK_LeaderboardEntries_Tests_test_id",
                        column: x => x.test_id,
                        principalTable: "Tests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaderboardEntries_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    position_id = table.Column<int>(type: "int", nullable: true),
                    test_id = table.Column<int>(type: "int", nullable: true),
                    question_text = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    question_type_string = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    question_score = table.Column<float>(type: "real", nullable: false),
                    question_answer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    options_json = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.id);
                    table.ForeignKey(
                        name: "FK_Questions_Tests_test_id",
                        column: x => x.test_id,
                        principalTable: "Tests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TestAttempts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    test_id = table.Column<int>(type: "int", nullable: false),
                    external_user_id = table.Column<int>(type: "int", nullable: true),
                    score = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    started_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    answers_file_path = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    is_validated = table.Column<bool>(type: "bit", nullable: false),
                    percentage_score = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    rejection_reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    rejected_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestAttempts", x => x.id);
                    table.ForeignKey(
                        name: "FK_TestAttempts_Tests_test_id",
                        column: x => x.test_id,
                        principalTable: "Tests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestAttempts_Users_external_user_id",
                        column: x => x.external_user_id,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    question_id = table.Column<int>(type: "int", nullable: false),
                    attempt_id = table.Column<int>(type: "int", nullable: false),
                    value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_Answers_Questions_question_id",
                        column: x => x.question_id,
                        principalTable: "Questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Answers_TestAttempts_attempt_id",
                        column: x => x.attempt_id,
                        principalTable: "TestAttempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_attempt_id",
                table: "Answers",
                column: "attempt_id");

            migrationBuilder.CreateIndex(
                name: "IX_Answers_question_id",
                table: "Answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_applicants_job_id",
                table: "applicants",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "IX_applicants_recommended_from_company_id",
                table: "applicants",
                column: "recommended_from_company_id");

            migrationBuilder.CreateIndex(
                name: "IX_applicants_user_id",
                table: "applicants",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_collaborators_company_id",
                table: "collaborators",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_events_host_company_id",
                table: "events",
                column: "host_company_id");

            migrationBuilder.CreateIndex(
                name: "IX_interview_sessions_external_user_id",
                table: "interview_sessions",
                column: "external_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardEntries_test_id",
                table: "LeaderboardEntries",
                column: "test_id");

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardEntries_user_id",
                table: "LeaderboardEntries",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_test_id",
                table: "Questions",
                column: "test_id");

            migrationBuilder.CreateIndex(
                name: "IX_Recruiters_user_id",
                table: "Recruiters",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Slots_candidate_id",
                table: "Slots",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "IX_Slots_RecruiterCompanyId_RecruiterUserId",
                table: "Slots",
                columns: new[] { "RecruiterCompanyId", "RecruiterUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_TestAttempts_external_user_id",
                table: "TestAttempts",
                column: "external_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_TestAttempts_test_id",
                table: "TestAttempts",
                column: "test_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "applicants");

            migrationBuilder.DropTable(
                name: "collaborators");

            migrationBuilder.DropTable(
                name: "interview_sessions");

            migrationBuilder.DropTable(
                name: "LeaderboardEntries");

            migrationBuilder.DropTable(
                name: "Slots");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "TestAttempts");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "Recruiters");

            migrationBuilder.DropTable(
                name: "Tests");
        }
    }
}
