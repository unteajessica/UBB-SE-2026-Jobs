#nullable disable

namespace Tests_and_Interviews.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <summary>
    /// Initial migration for creating the database schema.
    /// </summary>
    public partial class InitialCreate : Migration
    {
        /// <summary>
        /// Applies the migration by creating the necessary database tables and schema.
        /// </summary>
        /// <param name="migrationBuilder">The migration builder used to build the database schema.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    company_id = table.Column<int>(type: "int", nullable: false),
                    company_name = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    about_us = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    profile_picture_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    logo_picture_url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    location = table.Column<string>(type: "nvarchar(300)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    posted_jobs_count = table.Column<int>(type: "int", nullable: false),
                    collaborators_count = table.Column<int>(type: "int", nullable: false),
                    buddy_name = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    avatar_id = table.Column<int>(type: "int", nullable: true),
                    final_quote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    buddy_description = table.Column<string>(type: "nvarchar(255)", nullable: true),
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
                    scen2_reaction3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.company_id);
                });

            migrationBuilder.CreateTable(
                name: "skills",
                columns: table => new
                {
                    skill_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    skill_name = table.Column<string>(type: "nvarchar(255)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skills", x => x.skill_id);
                });

            migrationBuilder.CreateTable(
                name: "tests",
                columns: table => new
                {
                    test_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tests", x => x.test_id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    cv_xml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "int", nullable: false),
                    photo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    start_date = table.Column<DateTime>(type: "date", nullable: false),
                    end_date = table.Column<DateTime>(type: "date", nullable: false),
                    location = table.Column<string>(type: "nvarchar(300)", nullable: false),
                    host_company_id = table.Column<int>(type: "int", nullable: false),
                    posted_at = table.Column<DateTime>(type: "datetime", nullable: false),
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
                name: "jobs",
                columns: table => new
                {
                    job_id = table.Column<int>(type: "int", nullable: false),
                    company_id = table.Column<int>(type: "int", nullable: false),
                    photo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    job_title = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    industry_field = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    job_type = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    experience_level = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    start_date = table.Column<DateTime>(type: "date", nullable: true),
                    end_date = table.Column<DateTime>(type: "date", nullable: true),
                    job_description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    job_location = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    available_positions = table.Column<int>(type: "int", nullable: false),
                    posted_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    salary = table.Column<int>(type: "int", nullable: true),
                    amount_payed = table.Column<int>(type: "int", nullable: true),
                    deadline = table.Column<DateTime>(type: "date", nullable: true),
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
                name: "Recruiters",
                columns: table => new
                {
                    company_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recruiters", x => x.company_id);
                    table.ForeignKey(
                        name: "FK_Recruiters_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "company_id");
                });

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    question_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    position_id = table.Column<int>(type: "int", nullable: true),
                    test_id = table.Column<int>(type: "int", nullable: true),
                    question_text = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    question_type = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    question_score = table.Column<float>(type: "real", nullable: false),
                    question_answer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    options_json = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questions", x => x.question_id);
                    table.ForeignKey(
                        name: "FK_questions_tests_test_id",
                        column: x => x.test_id,
                        principalTable: "tests",
                        principalColumn: "test_id",
                        onDelete: ReferentialAction.SetNull);
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
                    score = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interview_sessions", x => x.session_id);
                    table.ForeignKey(
                        name: "FK_interview_sessions_Users_external_user_id",
                        column: x => x.external_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LeaderboardEntries",
                columns: table => new
                {
                    leaderboard_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    test_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    normalized_score = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    rank_position = table.Column<int>(type: "int", nullable: false),
                    tie_break_priority = table.Column<int>(type: "int", nullable: false),
                    last_recalculation_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderboardEntries", x => x.leaderboard_id);
                    table.ForeignKey(
                        name: "FK_LeaderboardEntries_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaderboardEntries_tests_test_id",
                        column: x => x.test_id,
                        principalTable: "tests",
                        principalColumn: "test_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "test_attempts",
                columns: table => new
                {
                    userTest_id = table.Column<int>(type: "int", nullable: false)
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
                    rejected_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_test_attempts", x => x.userTest_id);
                    table.ForeignKey(
                        name: "FK_test_attempts_Users_external_user_id",
                        column: x => x.external_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_test_attempts_tests_test_id",
                        column: x => x.test_id,
                        principalTable: "tests",
                        principalColumn: "test_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "collaborators",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "int", nullable: false),
                    company_id = table.Column<int>(type: "int", nullable: false),
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
                name: "applicants",
                columns: table => new
                {
                    applicant_id = table.Column<int>(type: "int", nullable: false),
                    job_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    app_test_grade = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    cv_grade = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    company_test_grade = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    interview_grade = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    application_status = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    applied_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    recommended_from_company_id = table.Column<int>(type: "int", nullable: true),
                    cv_file_url = table.Column<string>(type: "nvarchar(500)", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applicants", x => x.applicant_id);
                    table.ForeignKey(
                        name: "FK_applicants_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
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
                name: "job_skills",
                columns: table => new
                {
                    skill_id = table.Column<int>(type: "int", nullable: false),
                    job_id = table.Column<int>(type: "int", nullable: false),
                    required_percentage = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_skills", x => new { x.skill_id, x.job_id });
                    table.ForeignKey(
                        name: "FK_job_skills_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "jobs",
                        principalColumn: "job_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_job_skills_skills_skill_id",
                        column: x => x.skill_id,
                        principalTable: "skills",
                        principalColumn: "skill_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Slots",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    recruiter_id = table.Column<int>(type: "int", nullable: false),
                    candidate_id = table.Column<int>(type: "int", nullable: true),
                    start_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    duration = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    interview_type = table.Column<string>(type: "nvarchar(255)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slots", x => x.id);
                    table.ForeignKey(
                        name: "FK_Slots_Recruiters_recruiter_id",
                        column: x => x.recruiter_id,
                        principalTable: "Recruiters",
                        principalColumn: "company_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Slots_Users_candidate_id",
                        column: x => x.candidate_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "answers",
                columns: table => new
                {
                    answer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    question_id = table.Column<int>(type: "int", nullable: false),
                    attempt_id = table.Column<int>(type: "int", nullable: false),
                    value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_answers", x => x.answer_id);
                    table.ForeignKey(
                        name: "FK_answers_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "question_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_answers_test_attempts_attempt_id",
                        column: x => x.attempt_id,
                        principalTable: "test_attempts",
                        principalColumn: "userTest_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_answers_attempt_id",
                table: "answers",
                column: "attempt_id");

            migrationBuilder.CreateIndex(
                name: "IX_answers_question_id",
                table: "answers",
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
                name: "IX_job_skills_job_id",
                table: "job_skills",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_company_id",
                table: "jobs",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardEntries_test_id",
                table: "LeaderboardEntries",
                column: "test_id");

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardEntries_user_id",
                table: "LeaderboardEntries",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_questions_test_id",
                table: "questions",
                column: "test_id");

            migrationBuilder.CreateIndex(
                name: "IX_Slots_candidate_id",
                table: "Slots",
                column: "candidate_id");

            migrationBuilder.CreateIndex(
                name: "IX_Slots_recruiter_id",
                table: "Slots",
                column: "recruiter_id");

            migrationBuilder.CreateIndex(
                name: "IX_test_attempts_external_user_id",
                table: "test_attempts",
                column: "external_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_test_attempts_test_id",
                table: "test_attempts",
                column: "test_id");
        }

        /// <summmary>
        /// Reverts the migration by dropping the database tables created in the Up method.
        /// </summary>
        /// <param name="migrationBuilder">The migration builder used to build the database schema.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "answers");

            migrationBuilder.DropTable(
                name: "applicants");

            migrationBuilder.DropTable(
                name: "collaborators");

            migrationBuilder.DropTable(
                name: "interview_sessions");

            migrationBuilder.DropTable(
                name: "job_skills");

            migrationBuilder.DropTable(
                name: "LeaderboardEntries");

            migrationBuilder.DropTable(
                name: "Slots");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "test_attempts");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "jobs");

            migrationBuilder.DropTable(
                name: "skills");

            migrationBuilder.DropTable(
                name: "Recruiters");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "tests");

            migrationBuilder.DropTable(
                name: "companies");
        }
    }
}
