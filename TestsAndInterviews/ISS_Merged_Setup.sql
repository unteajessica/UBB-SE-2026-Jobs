-- ============================================================
-- ISS_Merged Database Setup Script
-- Server:   ASUS\SQLEXPRESS
-- Database: ISS_Merged
--
-- Instructions:
--   1. Open SSMS and connect to ASUS\SQLEXPRESS
--   2. Open this file and click Execute (F5)
-- ============================================================

USE [master];
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'ISS_Merged')
BEGIN
    CREATE DATABASE [ISS_Merged];
END
GO

USE [ISS_Merged];
GO

-- ===========================================================
-- Part 1: PussyCats tables
-- (companies, jobs, Users, Skills, JobSkills, Matches, Chats, ...)
-- ===========================================================

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO


BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [companies] (
        [company_id] int NOT NULL IDENTITY,
        [company_name] nvarchar(255) NOT NULL,
        [about_us] nvarchar(max) NULL,
        [profile_picture_url] nvarchar(max) NULL,
        [logo_picture_url] nvarchar(max) NOT NULL,
        [location] nvarchar(300) NULL,
        [email] nvarchar(100) NULL,
        [posted_jobs_count] int NOT NULL,
        [collaborators_count] int NOT NULL,
        [buddy_name] nvarchar(255) NULL,
        [avatar_id] int NULL,
        [final_quote] nvarchar(max) NULL,
        [buddy_description] nvarchar(255) NULL,
        [scen_1_text] nvarchar(max) NULL,
        [scen1_answer1] nvarchar(max) NULL,
        [scen1_answer2] nvarchar(max) NULL,
        [scen1_answer3] nvarchar(max) NULL,
        [scen1_reaction1] nvarchar(max) NULL,
        [scen1_reaction2] nvarchar(max) NULL,
        [scen1_reaction3] nvarchar(max) NULL,
        [scen2_text] nvarchar(max) NULL,
        [scen2_answer1] nvarchar(max) NULL,
        [scen2_answer2] nvarchar(max) NULL,
        [scen2_answer3] nvarchar(max) NULL,
        [scen2_reaction1] nvarchar(max) NULL,
        [scen2_reaction2] nvarchar(max) NULL,
        [scen2_reaction3] nvarchar(max) NULL,
        CONSTRAINT [PK_companies] PRIMARY KEY ([company_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [SkillGroups] (
        [SkillGroupId] int NOT NULL IDENTITY,
        [GroupName] nvarchar(100) NOT NULL,
        [Weight] int NOT NULL,
        [JobRole] nvarchar(40) NOT NULL,
        CONSTRAINT [PK_SkillGroups] PRIMARY KEY ([SkillGroupId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [Skills] (
        [SkillId] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Category] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Skills] PRIMARY KEY ([SkillId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [Users] (
        [UserId] int NOT NULL IDENTITY,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Age] int NOT NULL,
        [Gender] nvarchar(20) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [Phone] nvarchar(40) NOT NULL,
        [Country] nvarchar(100) NOT NULL,
        [City] nvarchar(100) NOT NULL,
        [Address] nvarchar(256) NOT NULL,
        [University] nvarchar(200) NOT NULL,
        [Degree] nvarchar(200) NOT NULL,
        [UniversityStartYear] int NOT NULL,
        [ExpectedGraduationYear] int NOT NULL,
        [GitHub] nvarchar(256) NOT NULL,
        [LinkedIn] nvarchar(256) NOT NULL,
        [Motivation] nvarchar(2000) NOT NULL,
        [HasDisabilities] bit NOT NULL,
        [ProfilePicturePath] nvarchar(512) NOT NULL,
        [ParsedCv] nvarchar(max) NOT NULL,
        [PasswordHash] nvarchar(512) NOT NULL,
        [PreferredEmploymentType] nvarchar(200) NOT NULL,
        [WorkModePreference] nvarchar(40) NOT NULL,
        [LocationPreference] nvarchar(100) NOT NULL,
        [YearsOfExperience] int NOT NULL,
        [TotalExperiencePoints] int NOT NULL,
        [CurrentLevel] int NOT NULL,
        [ActiveAccount] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastUpdated] datetime2 NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [jobs] (
        [job_id] int NOT NULL IDENTITY,
        [company_id] int NOT NULL,
        [photo] nvarchar(max) NULL,
        [job_title] nvarchar(255) NOT NULL,
        [industry_field] nvarchar(255) NULL,
        [job_type] nvarchar(255) NULL,
        [experience_level] nvarchar(255) NULL,
        [start_date] date NULL,
        [end_date] date NULL,
        [job_description] nvarchar(max) NULL,
        [job_location] nvarchar(255) NULL,
        [available_positions] int NOT NULL,
        [posted_at] datetime NULL,
        [salary] int NULL,
        [amount_payed] int NULL,
        [deadline] date NULL,
        [promotion_level] int NULL,
        [job_role] int NULL,
        CONSTRAINT [PK_jobs] PRIMARY KEY ([job_id]),
        CONSTRAINT [FK_jobs_companies_company_id] FOREIGN KEY ([company_id]) REFERENCES [companies] ([company_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [SkillGroupSkills] (
        [SkillGroupId] int NOT NULL,
        [SkillId] int NOT NULL,
        CONSTRAINT [PK_SkillGroupSkills] PRIMARY KEY ([SkillGroupId], [SkillId]),
        CONSTRAINT [FK_SkillGroupSkills_SkillGroups_SkillGroupId] FOREIGN KEY ([SkillGroupId]) REFERENCES [SkillGroups] ([SkillGroupId]) ON DELETE CASCADE,
        CONSTRAINT [FK_SkillGroupSkills_Skills_SkillId] FOREIGN KEY ([SkillId]) REFERENCES [Skills] ([SkillId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [Documents] (
        [DocumentId] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [DocumentName] nvarchar(200) NOT NULL,
        [FilePath] nvarchar(512) NOT NULL,
        [UploadDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Documents] PRIMARY KEY ([DocumentId]),
        CONSTRAINT [FK_Documents_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [ExtraCurricularActivities] (
        [ExtraCurricularActivityId] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [ActivityName] nvarchar(200) NOT NULL,
        [Organization] nvarchar(200) NOT NULL,
        [Role] nvarchar(200) NOT NULL,
        [Period] nvarchar(100) NOT NULL,
        [Description] nvarchar(2000) NOT NULL,
        CONSTRAINT [PK_ExtraCurricularActivities] PRIMARY KEY ([ExtraCurricularActivityId]),
        CONSTRAINT [FK_ExtraCurricularActivities_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [PersonalityTestResults] (
        [PersonalityTestResultId] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [CompletedAt] datetime2 NOT NULL,
        [SelectedRole] int NULL,
        CONSTRAINT [PK_PersonalityTestResults] PRIMARY KEY ([PersonalityTestResultId]),
        CONSTRAINT [FK_PersonalityTestResults_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [Projects] (
        [ProjectId] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(2000) NOT NULL,
        [Technologies] nvarchar(max) NOT NULL,
        [Url] nvarchar(512) NOT NULL,
        CONSTRAINT [PK_Projects] PRIMARY KEY ([ProjectId]),
        CONSTRAINT [FK_Projects_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [SkillTests] (
        [SkillTestId] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Score] int NOT NULL,
        [AchievedDate] date NOT NULL,
        CONSTRAINT [PK_SkillTests] PRIMARY KEY ([SkillTestId]),
        CONSTRAINT [FK_SkillTests_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [UserSkills] (
        [UserId] int NOT NULL,
        [SkillId] int NOT NULL,
        [Score] int NOT NULL,
        [IsVerified] bit NOT NULL,
        [AchievedDate] date NULL,
        CONSTRAINT [PK_UserSkills] PRIMARY KEY ([UserId], [SkillId]),
        CONSTRAINT [FK_UserSkills_Skills_SkillId] FOREIGN KEY ([SkillId]) REFERENCES [Skills] ([SkillId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserSkills_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [WorkExperiences] (
        [WorkExperienceId] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Company] nvarchar(200) NOT NULL,
        [JobTitle] nvarchar(200) NOT NULL,
        [StartDate] datetimeoffset NOT NULL,
        [EndDate] datetimeoffset NULL,
        [Description] nvarchar(2000) NOT NULL,
        [CurrentlyWorking] bit NOT NULL,
        CONSTRAINT [PK_WorkExperiences] PRIMARY KEY ([WorkExperienceId]),
        CONSTRAINT [FK_WorkExperiences_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [Chats] (
        [ChatId] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [CompanyId] int NULL,
        [SecondUserId] int NULL,
        [JobId] int NULL,
        [IsBlocked] bit NOT NULL,
        [BlockedByUserId] int NULL,
        [DeletedAtByUser] datetime2 NULL,
        [DeletedAtBySecondParty] datetime2 NULL,
        CONSTRAINT [PK_Chats] PRIMARY KEY ([ChatId]),
        CONSTRAINT [FK_Chats_Users_BlockedByUserId] FOREIGN KEY ([BlockedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Chats_Users_SecondUserId] FOREIGN KEY ([SecondUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Chats_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Chats_companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [companies] ([company_id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Chats_jobs_JobId] FOREIGN KEY ([JobId]) REFERENCES [jobs] ([job_id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [JobSkills] (
        [JobId] int NOT NULL,
        [SkillId] int NOT NULL,
        [RequiredLevel] int NOT NULL,
        CONSTRAINT [PK_JobSkills] PRIMARY KEY ([JobId], [SkillId]),
        CONSTRAINT [FK_JobSkills_Skills_SkillId] FOREIGN KEY ([SkillId]) REFERENCES [Skills] ([SkillId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_JobSkills_jobs_JobId] FOREIGN KEY ([JobId]) REFERENCES [jobs] ([job_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [Matches] (
        [MatchId] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [JobId] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        [FeedbackMessage] nvarchar(2000) NOT NULL,
        CONSTRAINT [PK_Matches] PRIMARY KEY ([MatchId]),
        CONSTRAINT [FK_Matches_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Matches_jobs_JobId] FOREIGN KEY ([JobId]) REFERENCES [jobs] ([job_id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [Recommendations] (
        [RecommendationId] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [JobId] int NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        CONSTRAINT [PK_Recommendations] PRIMARY KEY ([RecommendationId]),
        CONSTRAINT [FK_Recommendations_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Recommendations_jobs_JobId] FOREIGN KEY ([JobId]) REFERENCES [jobs] ([job_id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [PersonalityTraitScores] (
        [PersonalityTraitScoreId] int NOT NULL IDENTITY,
        [PersonalityTestResultId] int NOT NULL,
        [Trait] nvarchar(40) NOT NULL,
        [Score] int NOT NULL,
        CONSTRAINT [PK_PersonalityTraitScores] PRIMARY KEY ([PersonalityTraitScoreId]),
        CONSTRAINT [FK_PersonalityTraitScores_PersonalityTestResults_PersonalityTestResultId] FOREIGN KEY ([PersonalityTestResultId]) REFERENCES [PersonalityTestResults] ([PersonalityTestResultId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE TABLE [Messages] (
        [MessageId] int NOT NULL IDENTITY,
        [ChatId] int NOT NULL,
        [SenderId] int NOT NULL,
        [Content] nvarchar(4000) NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        [Type] int NOT NULL,
        [IsRead] bit NOT NULL,
        [OriginalFileName] nvarchar(500) NOT NULL,
        CONSTRAINT [PK_Messages] PRIMARY KEY ([MessageId]),
        CONSTRAINT [FK_Messages_Chats_ChatId] FOREIGN KEY ([ChatId]) REFERENCES [Chats] ([ChatId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SkillGroupId', N'GroupName', N'JobRole', N'Weight') AND [object_id] = OBJECT_ID(N'[SkillGroups]'))
        SET IDENTITY_INSERT [SkillGroups] ON;
    EXEC(N'INSERT INTO [SkillGroups] ([SkillGroupId], [GroupName], [JobRole], [Weight])
    VALUES (1, N''UI Markup'', N''FrontendDeveloper'', 20),
    (2, N''JavaScript'', N''FrontendDeveloper'', 20),
    (3, N''Frontend Framework'', N''FrontendDeveloper'', 25),
    (4, N''Version Control'', N''FrontendDeveloper'', 10),
    (5, N''Testing'', N''FrontendDeveloper'', 10),
    (6, N''Build Tools'', N''FrontendDeveloper'', 8),
    (7, N''Design Collaboration'', N''FrontendDeveloper'', 7),
    (8, N''Backend Language'', N''BackendDeveloper'', 25),
    (9, N''Web Framework'', N''BackendDeveloper'', 20),
    (10, N''Database Management'', N''BackendDeveloper'', 20),
    (11, N''API Design'', N''BackendDeveloper'', 15),
    (12, N''Version Control'', N''BackendDeveloper'', 10),
    (13, N''Testing'', N''BackendDeveloper'', 10),
    (14, N''Design Tools'', N''UiUxDesigner'', 30),
    (15, N''Prototyping'', N''UiUxDesigner'', 20),
    (16, N''User Research'', N''UiUxDesigner'', 20),
    (17, N''Visual Design'', N''UiUxDesigner'', 15),
    (18, N''Handoff'', N''UiUxDesigner'', 10),
    (19, N''Analytics'', N''UiUxDesigner'', 5),
    (20, N''Containerization'', N''DevOpsEngineer'', 20),
    (21, N''Orchestration'', N''DevOpsEngineer'', 20),
    (22, N''CI/CD'', N''DevOpsEngineer'', 20),
    (23, N''Cloud Platform'', N''DevOpsEngineer'', 15),
    (24, N''Infrastructure as Code'', N''DevOpsEngineer'', 15),
    (25, N''Monitoring'', N''DevOpsEngineer'', 10),
    (26, N''Methodologies'', N''ProjectManager'', 25),
    (27, N''Project Tools'', N''ProjectManager'', 20),
    (28, N''Risk Management'', N''ProjectManager'', 20),
    (29, N''Communication'', N''ProjectManager'', 20),
    (30, N''Budgeting'', N''ProjectManager'', 15),
    (31, N''Query Language'', N''DataAnalyst'', 25),
    (32, N''Data Visualization'', N''DataAnalyst'', 25),
    (33, N''Programming'', N''DataAnalyst'', 20),
    (34, N''Statistical Analysis'', N''DataAnalyst'', 15),
    (35, N''Spreadsheets'', N''DataAnalyst'', 10),
    (36, N''Data Cleaning'', N''DataAnalyst'', 5),
    (37, N''Network Security'', N''CybersecuritySpecialist'', 20),
    (38, N''Penetration Testing'', N''CybersecuritySpecialist'', 20),
    (39, N''SIEM & Monitoring'', N''CybersecuritySpecialist'', 15),
    (40, N''Cryptography'', N''CybersecuritySpecialist'', 15),
    (41, N''Compliance & Standards'', N''CybersecuritySpecialist'', 15),
    (42, N''Incident Response'', N''CybersecuritySpecialist'', 15);
    INSERT INTO [SkillGroups] ([SkillGroupId], [GroupName], [JobRole], [Weight])
    VALUES (43, N''ML Frameworks'', N''AiMlEngineer'', 25),
    (44, N''Programming'', N''AiMlEngineer'', 20),
    (45, N''Mathematics'', N''AiMlEngineer'', 20),
    (46, N''Data Engineering'', N''AiMlEngineer'', 15),
    (47, N''Model Deployment'', N''AiMlEngineer'', 10),
    (48, N''NLP / Computer Vision'', N''AiMlEngineer'', 10)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SkillGroupId', N'GroupName', N'JobRole', N'Weight') AND [object_id] = OBJECT_ID(N'[SkillGroups]'))
        SET IDENTITY_INSERT [SkillGroups] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SkillId', N'Category', N'Name') AND [object_id] = OBJECT_ID(N'[Skills]'))
        SET IDENTITY_INSERT [Skills] ON;
    EXEC(N'INSERT INTO [Skills] ([SkillId], [Category], [Name])
    VALUES (1, N''Backend Language'', N''C#''),
    (2, N''Frontend Framework'', N''React''),
    (3, N''Database Management'', N''SQL''),
    (4, N''Testing'', N''Testing''),
    (5, N''Testing'', N''Selenium''),
    (6, N''Containerization'', N''Docker''),
    (7, N''Orchestration'', N''Kubernetes''),
    (8, N''Backend Language'', N''Python''),
    (9, N''Data Cleaning'', N''Pandas''),
    (10, N''ML Frameworks'', N''Machine Learning''),
    (11, N''NLP / Computer Vision'', N''NLP''),
    (12, N''Design Tools'', N''Figma''),
    (13, N''Visual Design'', N''UI Design''),
    (14, N''Leadership'', N''Architecture''),
    (15, N''Leadership'', N''Leadership''),
    (16, N''Cloud Platform'', N''Cloud''),
    (17, N''Mobile'', N''Flutter''),
    (18, N''Mobile'', N''Kotlin''),
    (19, N''Penetration Testing'', N''Penetration Testing''),
    (20, N''SIEM & Monitoring'', N''SIEM''),
    (21, N''Backend Language'', N''Java''),
    (22, N''Web Framework'', N''Spring Boot''),
    (23, N''Methodologies'', N''Agile''),
    (24, N''Data Engineering'', N''Spark''),
    (25, N''Backend Language'', N''Go''),
    (26, N''Database Management'', N''PostgreSQL''),
    (27, N''NLP / Computer Vision'', N''Computer Vision''),
    (28, N''ML Frameworks'', N''PyTorch''),
    (29, N''Frontend Framework'', N''Angular''),
    (30, N''Frontend Framework'', N''Vue.js''),
    (31, N''JavaScript'', N''TypeScript''),
    (32, N''Cloud Platform'', N''AWS''),
    (33, N''UI Markup'', N''HTML''),
    (34, N''UI Markup'', N''CSS''),
    (35, N''UI Markup'', N''SCSS''),
    (36, N''UI Markup'', N''Tailwind''),
    (37, N''JavaScript'', N''JavaScript''),
    (38, N''Frontend Framework'', N''Svelte''),
    (39, N''Version Control'', N''Git''),
    (40, N''Version Control'', N''GitHub''),
    (41, N''Testing'', N''Jest''),
    (42, N''Testing'', N''Cypress'');
    INSERT INTO [Skills] ([SkillId], [Category], [Name])
    VALUES (43, N''Build Tools'', N''Webpack''),
    (44, N''Build Tools'', N''Vite''),
    (45, N''Build Tools'', N''Parcel''),
    (46, N''Design Tools'', N''Adobe XD''),
    (47, N''Design Collaboration'', N''Zeplin''),
    (48, N''Backend Language'', N''Node.js''),
    (49, N''Web Framework'', N''ASP.NET''),
    (50, N''Web Framework'', N''Django''),
    (51, N''Database Management'', N''MySQL''),
    (52, N''Database Management'', N''MongoDB''),
    (53, N''Database Management'', N''Redis''),
    (54, N''API Design'', N''REST''),
    (55, N''API Design'', N''GraphQL''),
    (56, N''API Design'', N''gRPC''),
    (57, N''Testing'', N''JUnit''),
    (58, N''Testing'', N''NUnit''),
    (59, N''Testing'', N''pytest''),
    (60, N''Testing'', N''Postman''),
    (61, N''Design Tools'', N''Sketch''),
    (62, N''Design Tools'', N''InVision''),
    (63, N''Prototyping'', N''Figma Prototyping''),
    (64, N''Prototyping'', N''Marvel''),
    (65, N''Prototyping'', N''Axure''),
    (66, N''User Research'', N''Interviews''),
    (67, N''User Research'', N''Surveys''),
    (68, N''User Research'', N''Usability Testing''),
    (69, N''Visual Design'', N''Typography''),
    (70, N''Visual Design'', N''Color Theory''),
    (71, N''Visual Design'', N''Grid Systems''),
    (72, N''Handoff'', N''Storybook''),
    (73, N''Analytics'', N''Google Analytics''),
    (74, N''Analytics'', N''Hotjar''),
    (75, N''Analytics'', N''Mixpanel''),
    (76, N''Containerization'', N''Podman''),
    (77, N''Orchestration'', N''Docker Swarm''),
    (78, N''Orchestration'', N''OpenShift''),
    (79, N''CI/CD'', N''Jenkins''),
    (80, N''CI/CD'', N''GitHub Actions''),
    (81, N''CI/CD'', N''GitLab CI''),
    (82, N''CI/CD'', N''CircleCI''),
    (83, N''Cloud Platform'', N''Azure''),
    (84, N''Cloud Platform'', N''Google Cloud'');
    INSERT INTO [Skills] ([SkillId], [Category], [Name])
    VALUES (85, N''Infrastructure as Code'', N''Terraform''),
    (86, N''Infrastructure as Code'', N''Ansible''),
    (87, N''Infrastructure as Code'', N''Pulumi''),
    (88, N''Monitoring'', N''Prometheus''),
    (89, N''Monitoring'', N''Grafana''),
    (90, N''Monitoring'', N''Datadog''),
    (91, N''Methodologies'', N''Scrum''),
    (92, N''Methodologies'', N''Kanban''),
    (93, N''Methodologies'', N''Waterfall''),
    (94, N''Project Tools'', N''Jira''),
    (95, N''Project Tools'', N''Trello''),
    (96, N''Project Tools'', N''Asana''),
    (97, N''Risk Management'', N''Risk Assessment''),
    (98, N''Risk Management'', N''Mitigation Planning''),
    (99, N''Communication'', N''Stakeholder Management''),
    (100, N''Communication'', N''Reporting''),
    (101, N''Communication'', N''Presentations''),
    (102, N''Budgeting'', N''Cost Estimation''),
    (103, N''Budgeting'', N''Budget Tracking''),
    (104, N''Budgeting'', N''MS Project''),
    (105, N''Query Language'', N''BigQuery''),
    (106, N''Data Visualization'', N''Power BI''),
    (107, N''Data Visualization'', N''Tableau''),
    (108, N''Data Visualization'', N''Looker''),
    (109, N''Programming'', N''R''),
    (110, N''Statistical Analysis'', N''Descriptive Statistics''),
    (111, N''Statistical Analysis'', N''Regression''),
    (112, N''Statistical Analysis'', N''Hypothesis Testing''),
    (113, N''Spreadsheets'', N''Excel''),
    (114, N''Spreadsheets'', N''Google Sheets''),
    (115, N''Data Cleaning'', N''OpenRefine''),
    (116, N''Network Security'', N''Firewalls''),
    (117, N''Network Security'', N''VPN''),
    (118, N''Network Security'', N''IDS/IPS''),
    (119, N''Network Security'', N''TCP/IP''),
    (120, N''Penetration Testing'', N''Metasploit''),
    (121, N''Penetration Testing'', N''Burp Suite''),
    (122, N''Penetration Testing'', N''Nmap''),
    (123, N''SIEM & Monitoring'', N''Splunk''),
    (124, N''SIEM & Monitoring'', N''IBM QRadar''),
    (125, N''SIEM & Monitoring'', N''Microsoft Sentinel''),
    (126, N''Cryptography'', N''AES'');
    INSERT INTO [Skills] ([SkillId], [Category], [Name])
    VALUES (127, N''Cryptography'', N''RSA''),
    (128, N''Cryptography'', N''PKI''),
    (129, N''Cryptography'', N''TLS/SSL''),
    (130, N''Compliance & Standards'', N''ISO 27001''),
    (131, N''Compliance & Standards'', N''GDPR''),
    (132, N''Compliance & Standards'', N''NIST''),
    (133, N''Compliance & Standards'', N''SOC 2''),
    (134, N''Incident Response'', N''Forensics''),
    (135, N''Incident Response'', N''Malware Analysis''),
    (136, N''Incident Response'', N''DFIR''),
    (137, N''ML Frameworks'', N''TensorFlow''),
    (138, N''ML Frameworks'', N''scikit-learn''),
    (139, N''ML Frameworks'', N''Keras''),
    (140, N''Programming'', N''Julia''),
    (141, N''Mathematics'', N''Linear Algebra''),
    (142, N''Mathematics'', N''Calculus''),
    (143, N''Mathematics'', N''Probability''),
    (144, N''Mathematics'', N''Statistics''),
    (145, N''Data Engineering'', N''NumPy''),
    (146, N''Data Engineering'', N''Apache Spark''),
    (147, N''Model Deployment'', N''FastAPI''),
    (148, N''Model Deployment'', N''MLflow''),
    (149, N''NLP / Computer Vision'', N''Hugging Face''),
    (150, N''NLP / Computer Vision'', N''OpenCV''),
    (151, N''NLP / Computer Vision'', N''NLTK''),
    (152, N''NLP / Computer Vision'', N''spaCy'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SkillId', N'Category', N'Name') AND [object_id] = OBJECT_ID(N'[Skills]'))
        SET IDENTITY_INSERT [Skills] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'ActiveAccount', N'Address', N'Age', N'City', N'Country', N'CreatedAt', N'CurrentLevel', N'Degree', N'Email', N'ExpectedGraduationYear', N'FirstName', N'Gender', N'GitHub', N'HasDisabilities', N'LastName', N'LastUpdated', N'LinkedIn', N'LocationPreference', N'Motivation', N'ParsedCv', N'PasswordHash', N'Phone', N'PreferredEmploymentType', N'ProfilePicturePath', N'TotalExperiencePoints', N'University', N'UniversityStartYear', N'WorkModePreference', N'YearsOfExperience') AND [object_id] = OBJECT_ID(N'[Users]'))
        SET IDENTITY_INSERT [Users] ON;
    EXEC(N'INSERT INTO [Users] ([UserId], [ActiveAccount], [Address], [Age], [City], [Country], [CreatedAt], [CurrentLevel], [Degree], [Email], [ExpectedGraduationYear], [FirstName], [Gender], [GitHub], [HasDisabilities], [LastName], [LastUpdated], [LinkedIn], [LocationPreference], [Motivation], [ParsedCv], [PasswordHash], [Phone], [PreferredEmploymentType], [ProfilePicturePath], [TotalExperiencePoints], [University], [UniversityStartYear], [WorkModePreference], [YearsOfExperience])
    VALUES (1, CAST(1 AS bit), N''123 Main St'', 25, N''Bucharest'', N''Romania'', ''2025-05-07T00:00:00.0000000'', 1, N''Computer Science'', N''alice.smith@example.com'', 2022, N''Alice'', N'''', N'''', CAST(0 AS bit), N''Smith'', ''0001-01-01T00:00:00.0000000'', N'''', N'''', N'''', N'''', N'''', N''+40123456789'', N'''', N'''', 0, N''University of Bucharest'', 2018, N'''', 0)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'ActiveAccount', N'Address', N'Age', N'City', N'Country', N'CreatedAt', N'CurrentLevel', N'Degree', N'Email', N'ExpectedGraduationYear', N'FirstName', N'Gender', N'GitHub', N'HasDisabilities', N'LastName', N'LastUpdated', N'LinkedIn', N'LocationPreference', N'Motivation', N'ParsedCv', N'PasswordHash', N'Phone', N'PreferredEmploymentType', N'ProfilePicturePath', N'TotalExperiencePoints', N'University', N'UniversityStartYear', N'WorkModePreference', N'YearsOfExperience') AND [object_id] = OBJECT_ID(N'[Users]'))
        SET IDENTITY_INSERT [Users] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'company_id', N'about_us', N'avatar_id', N'buddy_description', N'buddy_name', N'collaborators_count', N'email', N'final_quote', N'location', N'logo_picture_url', N'company_name', N'posted_jobs_count', N'profile_picture_url', N'scen1_answer1', N'scen1_answer2', N'scen1_answer3', N'scen1_reaction1', N'scen1_reaction2', N'scen1_reaction3', N'scen_1_text', N'scen2_answer1', N'scen2_answer2', N'scen2_answer3', N'scen2_reaction1', N'scen2_reaction2', N'scen2_reaction3', N'scen2_text') AND [object_id] = OBJECT_ID(N'[companies]'))
        SET IDENTITY_INSERT [companies] ON;
    EXEC(N'INSERT INTO [companies] ([company_id], [about_us], [avatar_id], [buddy_description], [buddy_name], [collaborators_count], [email], [final_quote], [location], [logo_picture_url], [company_name], [posted_jobs_count], [profile_picture_url], [scen1_answer1], [scen1_answer2], [scen1_answer3], [scen1_reaction1], [scen1_reaction2], [scen1_reaction3], [scen_1_text], [scen2_answer1], [scen2_answer2], [scen2_answer3], [scen2_reaction1], [scen2_reaction2], [scen2_reaction3], [scen2_text])
    VALUES (1, NULL, NULL, NULL, NULL, 1, N''hr@technova.com'', NULL, N''San Francisco, CA'', N''technova_logo.png'', N''TechNova'', 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (2, NULL, NULL, NULL, NULL, 1, N''careers@dataflow.com'', NULL, N''New York, NY'', N''dataflow_logo.png'', N''DataFlow Inc'', 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (3, NULL, NULL, NULL, NULL, 2, N''hello@ecocode.com'', NULL, N''Seattle, WA'', N''ecocode_logo.png'', N''EcoCode'', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
    (4, NULL, NULL, NULL, NULL, 1, N''hr@finedge.com'', NULL, N''London, UK'', N''finedge_logo.png'', N''FinEdge'', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'company_id', N'about_us', N'avatar_id', N'buddy_description', N'buddy_name', N'collaborators_count', N'email', N'final_quote', N'location', N'logo_picture_url', N'company_name', N'posted_jobs_count', N'profile_picture_url', N'scen1_answer1', N'scen1_answer2', N'scen1_answer3', N'scen1_reaction1', N'scen1_reaction2', N'scen1_reaction3', N'scen_1_text', N'scen2_answer1', N'scen2_answer2', N'scen2_answer3', N'scen2_reaction1', N'scen2_reaction2', N'scen2_reaction3', N'scen2_text') AND [object_id] = OBJECT_ID(N'[companies]'))
        SET IDENTITY_INSERT [companies] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SkillGroupId', N'SkillId') AND [object_id] = OBJECT_ID(N'[SkillGroupSkills]'))
        SET IDENTITY_INSERT [SkillGroupSkills] ON;
    EXEC(N'INSERT INTO [SkillGroupSkills] ([SkillGroupId], [SkillId])
    VALUES (1, 33),
    (1, 34),
    (1, 35),
    (1, 36),
    (2, 31),
    (2, 37),
    (3, 2),
    (3, 29),
    (3, 30),
    (3, 38),
    (4, 39),
    (4, 40),
    (5, 5),
    (5, 41),
    (5, 42),
    (6, 43),
    (6, 44),
    (6, 45),
    (7, 12),
    (7, 46),
    (7, 47),
    (8, 1),
    (8, 8),
    (8, 21),
    (8, 25),
    (8, 48),
    (9, 22),
    (9, 49),
    (9, 50),
    (10, 3),
    (10, 26),
    (10, 51),
    (10, 52),
    (10, 53),
    (11, 54),
    (11, 55),
    (11, 56),
    (12, 39),
    (12, 40),
    (13, 57),
    (13, 58),
    (13, 59);
    INSERT INTO [SkillGroupSkills] ([SkillGroupId], [SkillId])
    VALUES (13, 60),
    (14, 12),
    (14, 46),
    (14, 61),
    (14, 62),
    (15, 63),
    (15, 64),
    (15, 65),
    (16, 66),
    (16, 67),
    (16, 68),
    (17, 69),
    (17, 70),
    (17, 71),
    (18, 12),
    (18, 47),
    (18, 72),
    (19, 73),
    (19, 74),
    (19, 75),
    (20, 6),
    (20, 76),
    (21, 7),
    (21, 77),
    (21, 78),
    (22, 79),
    (22, 80),
    (22, 81),
    (22, 82),
    (23, 32),
    (23, 83),
    (23, 84),
    (24, 85),
    (24, 86),
    (24, 87),
    (25, 88),
    (25, 89),
    (25, 90),
    (26, 23),
    (26, 91),
    (26, 92),
    (26, 93);
    INSERT INTO [SkillGroupSkills] ([SkillGroupId], [SkillId])
    VALUES (27, 94),
    (27, 95),
    (27, 96),
    (28, 97),
    (28, 98),
    (29, 99),
    (29, 100),
    (29, 101),
    (30, 102),
    (30, 103),
    (30, 104),
    (31, 3),
    (31, 26),
    (31, 105),
    (32, 106),
    (32, 107),
    (32, 108),
    (33, 8),
    (33, 109),
    (34, 110),
    (34, 111),
    (34, 112),
    (35, 113),
    (35, 114),
    (36, 9),
    (36, 115),
    (37, 116),
    (37, 117),
    (37, 118),
    (37, 119),
    (38, 120),
    (38, 121),
    (38, 122),
    (39, 123),
    (39, 124),
    (39, 125),
    (40, 126),
    (40, 127),
    (40, 128),
    (40, 129),
    (41, 130),
    (41, 131);
    INSERT INTO [SkillGroupSkills] ([SkillGroupId], [SkillId])
    VALUES (41, 132),
    (41, 133),
    (42, 134),
    (42, 135),
    (42, 136),
    (43, 28),
    (43, 137),
    (43, 138),
    (43, 139),
    (44, 8),
    (44, 109),
    (44, 140),
    (45, 141),
    (45, 142),
    (45, 143),
    (45, 144),
    (46, 3),
    (46, 9),
    (46, 145),
    (46, 146),
    (47, 6),
    (47, 147),
    (47, 148),
    (48, 149),
    (48, 150),
    (48, 151),
    (48, 152)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SkillGroupId', N'SkillId') AND [object_id] = OBJECT_ID(N'[SkillGroupSkills]'))
        SET IDENTITY_INSERT [SkillGroupSkills] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SkillTestId', N'AchievedDate', N'Name', N'Score', N'UserId') AND [object_id] = OBJECT_ID(N'[SkillTests]'))
        SET IDENTITY_INSERT [SkillTests] ON;
    EXEC(N'INSERT INTO [SkillTests] ([SkillTestId], [AchievedDate], [Name], [Score], [UserId])
    VALUES (1, ''2026-01-07'', N''C# Fundamentals'', 82, 1),
    (2, ''2026-01-07'', N''SQL Server'', 76, 1),
    (3, ''2026-01-07'', N''Software Design'', 88, 1)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SkillTestId', N'AchievedDate', N'Name', N'Score', N'UserId') AND [object_id] = OBJECT_ID(N'[SkillTests]'))
        SET IDENTITY_INSERT [SkillTests] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'job_id', N'amount_payed', N'available_positions', N'company_id', N'deadline', N'end_date', N'experience_level', N'industry_field', N'job_description', N'job_location', N'job_role', N'job_title', N'job_type', N'photo', N'posted_at', N'promotion_level', N'salary', N'start_date') AND [object_id] = OBJECT_ID(N'[jobs]'))
        SET IDENTITY_INSERT [jobs] ON;
    EXEC(N'INSERT INTO [jobs] ([job_id], [amount_payed], [available_positions], [company_id], [deadline], [end_date], [experience_level], [industry_field], [job_description], [job_location], [job_role], [job_title], [job_type], [photo], [posted_at], [promotion_level], [salary], [start_date])
    VALUES (101, 0, 3, 1, ''2026-05-15'', NULL, N''Mid-Level'', N''IT'', N''Develop robust REST APIs using .NET Core.'', N''Remote'', 1, N''Backend C# Developer'', N''Full-time'', NULL, ''2026-04-15T09:00:00.000'', 2, 95000, NULL),
    (102, 0, 1, 2, ''2026-06-01'', NULL, N''Senior'', N''Data Science'', N''Maintain cloud data pipelines and warehouses.'', N''New York, NY'', 5, N''Data Engineer'', N''Contract'', NULL, ''2026-04-18T10:30:00.000'', 1, 120000, NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'job_id', N'amount_payed', N'available_positions', N'company_id', N'deadline', N'end_date', N'experience_level', N'industry_field', N'job_description', N'job_location', N'job_role', N'job_title', N'job_type', N'photo', N'posted_at', N'promotion_level', N'salary', N'start_date') AND [object_id] = OBJECT_ID(N'[jobs]'))
        SET IDENTITY_INSERT [jobs] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'JobId', N'SkillId', N'RequiredLevel') AND [object_id] = OBJECT_ID(N'[JobSkills]'))
        SET IDENTITY_INSERT [JobSkills] ON;
    EXEC(N'INSERT INTO [JobSkills] ([JobId], [SkillId], [RequiredLevel])
    VALUES (101, 1, 80),
    (101, 3, 75),
    (102, 8, 68),
    (102, 9, 62)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'JobId', N'SkillId', N'RequiredLevel') AND [object_id] = OBJECT_ID(N'[JobSkills]'))
        SET IDENTITY_INSERT [JobSkills] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_Chats_BlockedByUserId] ON [Chats] ([BlockedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_Chats_CompanyId] ON [Chats] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_Chats_JobId] ON [Chats] ([JobId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_Chats_SecondUserId] ON [Chats] ([SecondUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_Chats_UserId] ON [Chats] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_Documents_UserId] ON [Documents] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_ExtraCurricularActivities_UserId] ON [ExtraCurricularActivities] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_jobs_company_id] ON [jobs] ([company_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_jobs_job_location] ON [jobs] ([job_location]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_jobs_job_type] ON [jobs] ([job_type]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_JobSkills_SkillId] ON [JobSkills] ([SkillId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_Matches_JobId] ON [Matches] ([JobId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_Matches_Status] ON [Matches] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_Matches_UserId_JobId] ON [Matches] ([UserId], [JobId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_Messages_ChatId] ON [Messages] ([ChatId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PersonalityTestResults_UserId] ON [PersonalityTestResults] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_PersonalityTraitScores_PersonalityTestResultId] ON [PersonalityTraitScores] ([PersonalityTestResultId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_Projects_UserId] ON [Projects] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_Recommendations_JobId] ON [Recommendations] ([JobId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_Recommendations_UserId] ON [Recommendations] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_Recommendations_UserId_JobId_Timestamp] ON [Recommendations] ([UserId], [JobId], [Timestamp]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_SkillGroups_JobRole] ON [SkillGroups] ([JobRole]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_SkillGroupSkills_SkillId] ON [SkillGroupSkills] ([SkillId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Skills_Name] ON [Skills] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_SkillTests_UserId] ON [SkillTests] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_UserSkills_SkillId] ON [UserSkills] ([SkillId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    CREATE INDEX [IX_WorkExperiences_UserId] ON [WorkExperiences] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260528184410_InitialMergedSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260528184410_InitialMergedSchema', N'10.0.7');
END;

COMMIT;
GO


-- ===========================================================
-- Part 2: TestsAndInterviews-specific tables
-- (Tests, Questions, TestAttempts, Answers, applicants,
--  events, collaborators, Recruiters, Slots,
--  interview_sessions, LeaderboardEntries)
-- ===========================================================


BEGIN TRANSACTION;
CREATE TABLE [applicants] (
    [applicant_id] int NOT NULL IDENTITY,
    [job_id] int NOT NULL,
    [user_id] int NOT NULL,
    [app_test_grade] decimal(5,2) NULL,
    [cv_grade] decimal(5,2) NULL,
    [company_test_grade] decimal(5,2) NULL,
    [interview_grade] decimal(5,2) NULL,
    [application_status] nvarchar(50) NULL,
    [applied_at] datetime NOT NULL,
    [recommended_from_company_id] int NULL,
    [cv_file_url] nvarchar(500) NULL,
    CONSTRAINT [PK_applicants] PRIMARY KEY ([applicant_id]),
    CONSTRAINT [FK_applicants_Users_user_id] FOREIGN KEY ([user_id]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE,
    CONSTRAINT [FK_applicants_companies_recommended_from_company_id] FOREIGN KEY ([recommended_from_company_id]) REFERENCES [companies] ([company_id]),
    CONSTRAINT [FK_applicants_jobs_job_id] FOREIGN KEY ([job_id]) REFERENCES [jobs] ([job_id]) ON DELETE CASCADE
);

CREATE TABLE [events] (
    [event_id] int NOT NULL IDENTITY,
    [photo] nvarchar(max) NOT NULL,
    [title] nvarchar(200) NOT NULL,
    [description] nvarchar(max) NOT NULL,
    [start_date] date NOT NULL,
    [end_date] date NOT NULL,
    [location] nvarchar(300) NOT NULL,
    [host_company_id] int NOT NULL,
    [posted_at] datetime NOT NULL,
    CONSTRAINT [PK_events] PRIMARY KEY ([event_id]),
    CONSTRAINT [FK_events_companies_host_company_id] FOREIGN KEY ([host_company_id]) REFERENCES [companies] ([company_id]) ON DELETE CASCADE
);

CREATE TABLE [interview_sessions] (
    [session_id] int NOT NULL IDENTITY,
    [position_id] int NOT NULL,
    [external_user_id] int NULL,
    [interviewer_id] int NOT NULL,
    [date_start] datetime2 NOT NULL,
    [video] nvarchar(200) NULL,
    [status] nvarchar(200) NULL,
    [score] decimal(18,2) NULL,
    CONSTRAINT [PK_interview_sessions] PRIMARY KEY ([session_id]),
    CONSTRAINT [FK_interview_sessions_Users_external_user_id] FOREIGN KEY ([external_user_id]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL
);

CREATE TABLE [Recruiters] (
    [company_id] int NOT NULL,
    [user_id] int NOT NULL,
    [name] nvarchar(255) NOT NULL,
    CONSTRAINT [PK_Recruiters] PRIMARY KEY ([company_id], [user_id]),
    CONSTRAINT [FK_Recruiters_Users_user_id] FOREIGN KEY ([user_id]) REFERENCES [Users] ([UserId]),
    CONSTRAINT [FK_Recruiters_companies_company_id] FOREIGN KEY ([company_id]) REFERENCES [companies] ([company_id])
);

CREATE TABLE [Tests] (
    [id] int NOT NULL IDENTITY,
    [title] nvarchar(200) NOT NULL,
    [category] nvarchar(200) NOT NULL,
    [created_at] datetime2 NOT NULL,
    CONSTRAINT [PK_Tests] PRIMARY KEY ([id])
);

CREATE TABLE [collaborators] (
    [event_id] int NOT NULL,
    [company_id] int NOT NULL,
    CONSTRAINT [PK_collaborators] PRIMARY KEY ([event_id], [company_id]),
    CONSTRAINT [FK_collaborators_companies_company_id] FOREIGN KEY ([company_id]) REFERENCES [companies] ([company_id]),
    CONSTRAINT [FK_collaborators_events_event_id] FOREIGN KEY ([event_id]) REFERENCES [events] ([event_id]) ON DELETE CASCADE
);

CREATE TABLE [Slots] (
    [id] int NOT NULL IDENTITY,
    [recruiter_id] int NOT NULL,
    [RecruiterCompanyId] int NOT NULL,
    [RecruiterUserId] int NOT NULL,
    [candidate_id] int NULL,
    [start_time] datetime2 NOT NULL,
    [end_time] datetime2 NOT NULL,
    [duration] int NOT NULL,
    [status] int NOT NULL,
    [interview_type] nvarchar(255) NOT NULL,
    CONSTRAINT [PK_Slots] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Slots_Recruiters_RecruiterCompanyId_RecruiterUserId] FOREIGN KEY ([RecruiterCompanyId], [RecruiterUserId]) REFERENCES [Recruiters] ([company_id], [user_id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Slots_Users_candidate_id] FOREIGN KEY ([candidate_id]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL
);

CREATE TABLE [LeaderboardEntries] (
    [id] int NOT NULL IDENTITY,
    [test_id] int NOT NULL,
    [user_id] int NOT NULL,
    [normalized_score] decimal(18,2) NOT NULL,
    [rank_position] int NOT NULL,
    [tie_break_priority] int NOT NULL,
    [last_recalculation_at] datetime2 NOT NULL,
    CONSTRAINT [PK_LeaderboardEntries] PRIMARY KEY ([id]),
    CONSTRAINT [FK_LeaderboardEntries_Tests_test_id] FOREIGN KEY ([test_id]) REFERENCES [Tests] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LeaderboardEntries_Users_user_id] FOREIGN KEY ([user_id]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [Questions] (
    [id] int NOT NULL IDENTITY,
    [position_id] int NULL,
    [test_id] int NULL,
    [question_text] nvarchar(200) NOT NULL,
    [question_type_string] nvarchar(max) NOT NULL,
    [question_score] real NOT NULL,
    [question_answer] nvarchar(200) NULL,
    [options_json] nvarchar(1000) NULL,
    CONSTRAINT [PK_Questions] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Questions_Tests_test_id] FOREIGN KEY ([test_id]) REFERENCES [Tests] ([id]) ON DELETE SET NULL
);

CREATE TABLE [TestAttempts] (
    [id] int NOT NULL IDENTITY,
    [test_id] int NOT NULL,
    [external_user_id] int NULL,
    [score] decimal(18,2) NULL,
    [status] nvarchar(200) NOT NULL,
    [started_at] datetime2 NULL,
    [completed_at] datetime2 NULL,
    [answers_file_path] nvarchar(200) NOT NULL,
    [is_validated] bit NOT NULL,
    [percentage_score] decimal(18,2) NULL,
    [rejection_reason] nvarchar(500) NULL,
    [rejected_at] datetime2 NULL,
    CONSTRAINT [PK_TestAttempts] PRIMARY KEY ([id]),
    CONSTRAINT [FK_TestAttempts_Tests_test_id] FOREIGN KEY ([test_id]) REFERENCES [Tests] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TestAttempts_Users_external_user_id] FOREIGN KEY ([external_user_id]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL
);

CREATE TABLE [Answers] (
    [id] int NOT NULL IDENTITY,
    [question_id] int NOT NULL,
    [attempt_id] int NOT NULL,
    [value] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Answers] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Answers_Questions_question_id] FOREIGN KEY ([question_id]) REFERENCES [Questions] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Answers_TestAttempts_attempt_id] FOREIGN KEY ([attempt_id]) REFERENCES [TestAttempts] ([id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Answers_attempt_id] ON [Answers] ([attempt_id]);

CREATE INDEX [IX_Answers_question_id] ON [Answers] ([question_id]);

CREATE INDEX [IX_applicants_job_id] ON [applicants] ([job_id]);

CREATE INDEX [IX_applicants_recommended_from_company_id] ON [applicants] ([recommended_from_company_id]);

CREATE INDEX [IX_applicants_user_id] ON [applicants] ([user_id]);

CREATE INDEX [IX_collaborators_company_id] ON [collaborators] ([company_id]);

CREATE INDEX [IX_events_host_company_id] ON [events] ([host_company_id]);

CREATE INDEX [IX_interview_sessions_external_user_id] ON [interview_sessions] ([external_user_id]);

CREATE INDEX [IX_LeaderboardEntries_test_id] ON [LeaderboardEntries] ([test_id]);

CREATE INDEX [IX_LeaderboardEntries_user_id] ON [LeaderboardEntries] ([user_id]);

CREATE INDEX [IX_Questions_test_id] ON [Questions] ([test_id]);

CREATE INDEX [IX_Recruiters_user_id] ON [Recruiters] ([user_id]);

CREATE INDEX [IX_Slots_candidate_id] ON [Slots] ([candidate_id]);

CREATE INDEX [IX_Slots_RecruiterCompanyId_RecruiterUserId] ON [Slots] ([RecruiterCompanyId], [RecruiterUserId]);

CREATE INDEX [IX_TestAttempts_external_user_id] ON [TestAttempts] ([external_user_id]);

CREATE INDEX [IX_TestAttempts_test_id] ON [TestAttempts] ([test_id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260528183714_InitialMergedSchema', N'10.0.7');

COMMIT;
GO

  GO

  -- ── Tests ────────────────────────────────────────────────────────────────────
  SET IDENTITY_INSERT [Tests] ON;
  INSERT INTO [Tests] ([id], [title], [category], [created_at]) VALUES
  (1, 'C# Fundamentals',  'Programming',      '2026-01-10T09:00:00'),
  (2, 'SQL Basics',       'Database',         '2026-02-05T10:00:00'),
  (3, 'OOP Principles',   'Programming',      '2026-03-01T09:00:00'),
  (4, 'Data Structures',  'Computer Science', '2026-03-10T09:00:00'),
  (5, 'Database Design',  'Database',         '2026-03-15T09:00:00');
  SET IDENTITY_INSERT [Tests] OFF;
  GO

  -- ── Questions ────────────────────────────────────────────────────────────────
  INSERT INTO [Questions]
  ([position_id],[test_id],[question_text],[question_type_string],[question_score],[question_answer],[options_json])
  VALUES
  -- Interview questions (no test)
  (1, NULL, 'Tell us about your favourite project.',     'INTERVIEW', 4.0, NULL, NULL),
  (1, NULL, 'How do you handle conflict in a team?',     'INTERVIEW', 4.0, NULL, NULL),
  (2, NULL, 'Where do you see yourself in 5 years?',     'INTERVIEW', 4.0, NULL, NULL),
  -- Test 1 – C# Fundamentals
  (NULL, 1, 'C# is a statically typed language.',                      'TRUE_FALSE',     4.0, 'true',  NULL),
  (NULL, 1, 'In C#, a string is a value type.',                        'TRUE_FALSE',     4.0, 'false', NULL),
  (NULL, 1, 'C# supports multiple inheritance through classes.',       'TRUE_FALSE',     4.0, 'false', NULL),
  (NULL, 1, 'The var keyword in C# means the variable is dynamic.',    'TRUE_FALSE',     4.0, 'false', NULL),
  (NULL, 1, 'In C#, int is an alias for System.Int32.',                'TRUE_FALSE',     4.0, 'true',  NULL),
  -- Test 2 – SQL Basics
  (NULL, 2, 'What SQL keyword is used to retrieve data from a table?', 'TEXT', 4.0, 'SELECT',   NULL),
  (NULL, 2, 'What SQL clause is used to filter rows?',                 'TEXT', 4.0, 'WHERE',    NULL),
  (NULL, 2, 'What SQL clause filters rows after grouping?',            'TEXT', 4.0, 'HAVING',   NULL),
  (NULL, 2, 'What SQL keyword is used to sort results?',               'TEXT', 4.0, 'ORDER BY', NULL),
  (NULL, 2, 'What SQL keyword groups rows with the same values?',      'TEXT', 4.0, 'GROUP BY', NULL),
  -- Test 3 – OOP Principles
  (NULL, 3, 'Which OOP principle hides internal implementation details?',  'SINGLE_CHOICE', 4.0, '1',
  '["Inheritance","Encapsulation","Polymorphism","Abstraction"]'),
  (NULL, 3, 'Which OOP principle allows a class to inherit from another?', 'SINGLE_CHOICE', 4.0, '0',
  '["Inheritance","Encapsulation","Polymorphism","Abstraction"]'),
  (NULL, 3, 'Which OOP principle allows objects to take multiple forms?',  'SINGLE_CHOICE', 4.0, '2',
  '["Inheritance","Encapsulation","Polymorphism","Abstraction"]'),
  -- Test 4 – Data Structures
  (NULL, 4, 'Which of the following are linear data structures?',  'MULTIPLE_CHOICE', 4.0, '[0,1]', '["Array","Linked
  List","Tree","Graph"]'),
  (NULL, 4, 'Which of the following use LIFO ordering?',           'MULTIPLE_CHOICE', 4.0, '[1,3]',
  '["Queue","Stack","Deque","Call Stack"]'),
  (NULL, 4, 'Which data structures allow duplicate values?',       'MULTIPLE_CHOICE', 4.0, '[0,2]',
  '["List","Set","Bag","Map"]'),
  -- Test 5 – Database Design
  (NULL, 5, 'A primary key can contain NULL values.',                      'TRUE_FALSE', 4.0, 'false', NULL),
  (NULL, 5, 'A foreign key references the primary key of another table.',  'TRUE_FALSE', 4.0, 'true',  NULL),
  (NULL, 5, 'Second normal form eliminates partial dependencies.',         'TRUE_FALSE', 4.0, 'true',  NULL);
  GO

  --── Events ───────────────────────────────────────────────────────────────────
  SET IDENTITY_INSERT [events] ON;
  INSERT INTO [events]
  ([event_id],[host_company_id],[photo],[title],[description],[start_date],[end_date],[location],[posted_at]) VALUES
  (201, 1, 'hackathon.jpg',        'TechNova Spring Hackathon',
      'Join us for 48 hours of intense coding and problem solving.',
      '2026-07-10', '2026-07-12', 'San Francisco HQ', '2026-05-10 08:00:00'),
  (202, 2, 'summit.jpg',           'Data Summit 2026',
      'Exploring the future of big data, AI, and machine learning.',
      '2026-08-20', '2026-08-21', 'New York Convention Center', '2026-05-12 11:00:00'),
  (203, 3, 'winter_summit.jpg',    'Winter Web Summit 2026',
      'Our annual kickoff exploring sustainable tech.',
      '2026-12-15', '2026-12-17', 'Seattle Convention Center', '2026-06-01 10:00:00'),
  (204, 4, 'fintech_panel.jpg',    'The Future of FinTech Security',
      'A live panel discussion on securing high-frequency trading platforms.',
      '2026-09-05', '2026-09-05', 'Virtual', '2026-07-01 09:00:00'),
  (205, 1, 'cloud_workshop.jpg',   'TechNova Cloud Architecture Workshop',
      'A hands-on deep dive into building scalable cloud solutions using .NET and Azure.',
      '2026-10-10', '2026-10-11', 'TechNova HQ, San Francisco', '2026-07-01 09:00:00'),
  (206, 1, 'opensource_summit.jpg','TechNova Open Source Summit 2026',
      'Join our core developers as we contribute to major open-source projects.',
      '2026-09-05', '2026-09-06', 'Virtual', '2026-06-10 12:00:00');
  SET IDENTITY_INSERT [events] OFF;
  GO

  -- ── Collaborators ─────────────────────────────────────────────────────────────
  INSERT INTO [collaborators] ([event_id],[company_id]) VALUES
  (201, 2),
  (202, 1),
  (203, 1),
  (203, 4),
  (204, 2),
  (205, 2),
  (206, 3);
  GO

  -- ── Recruiters (company 1 and 2, linked to existing user 1) ──────────────────
  -- Only run if you have a user with UserId = 1 in the Users table.
  -- If not, register a Recruiter account first and use that UserId.
  INSERT INTO [Recruiters] ([company_id],[user_id],[name]) VALUES
  (1, 1, 'TechNova'),
  (2, 1, 'DataFlow Inc');
  GO

ALTER TABLE Users ADD role nvarchar(50) NOT NULL DEFAULT 'Candidate';
GO

ALTER TABLE [Users] ALTER COLUMN [ParsedCv] nvarchar(max) NULL;
ALTER TABLE [Users] ALTER COLUMN [Address] nvarchar(max) NULL;
ALTER TABLE [Users] ALTER COLUMN [City] nvarchar(max) NULL;
ALTER TABLE [Users] ALTER COLUMN [Country] nvarchar(max) NULL;
ALTER TABLE [Users] ALTER COLUMN [GitHub] nvarchar(max) NULL;
ALTER TABLE [Users] ALTER COLUMN [LinkedIn] nvarchar(max) NULL;
ALTER TABLE [Users] ALTER COLUMN [Phone] nvarchar(max) NULL;
ALTER TABLE [Users] ALTER COLUMN [University] nvarchar(max) NULL;
ALTER TABLE [Users] ALTER COLUMN [Degree] nvarchar(max) NULL;
ALTER TABLE [Users] ALTER COLUMN [Motivation] nvarchar(max) NULL;
ALTER TABLE [Users] ALTER COLUMN [ProfilePicturePath] nvarchar(max) NULL;
ALTER TABLE [Users] ALTER COLUMN [LocationPreference] nvarchar(max) NULL;
ALTER TABLE [Users] ALTER COLUMN [WorkModePreference] nvarchar(max) NULL;
ALTER TABLE [Users] ALTER COLUMN [PreferredEmploymentType] nvarchar(max) NULL;
ALTER TABLE [Users] ALTER COLUMN [Gender] nvarchar(max) NULL;
ALTER TABLE [Users] ALTER COLUMN [Age] int NULL;
ALTER TABLE [Users] ALTER COLUMN [UniversityStartYear] int NULL;
ALTER TABLE [Users] ALTER COLUMN [ExpectedGraduationYear] int NULL;
ALTER TABLE [Users] ALTER COLUMN [HasDisabilities] bit NULL;


-- ============================================================
-- PART 3: Test-data fixtures
-- (users 100-108, extra jobs, matches, chats, T&I data, etc.)
-- ============================================================
-- ============================================================
-- HELPERS:  all inserts are idempotent (skip if key exists)
-- ============================================================

-- ────────────────────────────────────────────────────────────
-- 1.  USERS
--     Password hash is empty → these accounts cannot log in;
--     they exist purely as data fixtures.
-- ────────────────────────────────────────────────────────────

-- 100  Bob Martin  – full-profile backend candidate
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [UserId] = 100)
BEGIN
    SET IDENTITY_INSERT [Users] ON;
    INSERT INTO [Users] (
        [UserId],[FirstName],[LastName],[Age],[Gender],[Email],[Phone],
        [Country],[City],[Address],[University],[Degree],
        [UniversityStartYear],[ExpectedGraduationYear],
        [GitHub],[LinkedIn],[Motivation],[HasDisabilities],
        [ProfilePicturePath],[ParsedCv],[PasswordHash],
        [PreferredEmploymentType],[WorkModePreference],[LocationPreference],
        [YearsOfExperience],[TotalExperiencePoints],[CurrentLevel],
        [ActiveAccount],[CreatedAt],[LastUpdated])
    VALUES (
        100,'Bob','Martin',28,'Male','bob.martin@test.com','+40700111001',
        'Romania','Cluj-Napoca','10 Eroilor Blvd','Babes-Bolyai University','Software Engineering',
        2018,2022,
        'https://github.com/bobmartin','https://linkedin.com/in/bobmartin',
        'Passionate about clean code and distributed systems.',0,
        '','','',
        'Full-time','Remote','Cluj-Napoca',
        4,450,3,
        1,'2026-01-10','2026-05-01');
    SET IDENTITY_INSERT [Users] OFF;
END;

-- 101  Carol Davies  – EDGE CASE: minimal profile, NO skills, no experience
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [UserId] = 101)
BEGIN
    SET IDENTITY_INSERT [Users] ON;
    INSERT INTO [Users] (
        [UserId],[FirstName],[LastName],[Age],[Gender],[Email],[Phone],
        [Country],[City],[Address],[University],[Degree],
        [UniversityStartYear],[ExpectedGraduationYear],
        [GitHub],[LinkedIn],[Motivation],[HasDisabilities],
        [ProfilePicturePath],[ParsedCv],[PasswordHash],
        [PreferredEmploymentType],[WorkModePreference],[LocationPreference],
        [YearsOfExperience],[TotalExperiencePoints],[CurrentLevel],
        [ActiveAccount],[CreatedAt],[LastUpdated])
    VALUES (
        101,'Carol','Davies',22,'Female','carol.davies@test.com','+40700111002',
        'Romania','Bucharest','','University of Bucharest','Computer Science',
        2020,2024,
        '','','',0,
        '','','',
        '','','',
        0,0,1,
        1,'2026-03-01','2026-03-01');
    SET IDENTITY_INSERT [Users] OFF;
END;

-- 102  Dan Zhang  – senior candidate, 8 yrs exp, many verified skills
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [UserId] = 102)
BEGIN
    SET IDENTITY_INSERT [Users] ON;
    INSERT INTO [Users] (
        [UserId],[FirstName],[LastName],[Age],[Gender],[Email],[Phone],
        [Country],[City],[Address],[University],[Degree],
        [UniversityStartYear],[ExpectedGraduationYear],
        [GitHub],[LinkedIn],[Motivation],[HasDisabilities],
        [ProfilePicturePath],[ParsedCv],[PasswordHash],
        [PreferredEmploymentType],[WorkModePreference],[LocationPreference],
        [YearsOfExperience],[TotalExperiencePoints],[CurrentLevel],
        [ActiveAccount],[CreatedAt],[LastUpdated])
    VALUES (
        102,'Dan','Zhang',30,'Male','dan.zhang@test.com','+40700111003',
        'Romania','Timisoara','5 Republicii St','Politehnica Timisoara','Computer Science',
        2014,2018,
        'https://github.com/danzhang','https://linkedin.com/in/danzhang',
        'Senior engineer with focus on ML pipelines and data platforms.',0,
        '','','',
        'Full-time','Hybrid','Timisoara',
        8,1200,6,
        1,'2025-11-01','2026-04-20');
    SET IDENTITY_INSERT [Users] OFF;
END;

-- 103  Eva Kowalski  – extra recruiter @ TechNova (company_id = 1)
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [UserId] = 103)
BEGIN
    SET IDENTITY_INSERT [Users] ON;
    INSERT INTO [Users] (
        [UserId],[FirstName],[LastName],[Age],[Gender],[Email],[Phone],
        [Country],[City],[Address],[University],[Degree],
        [UniversityStartYear],[ExpectedGraduationYear],
        [GitHub],[LinkedIn],[Motivation],[HasDisabilities],
        [ProfilePicturePath],[ParsedCv],[PasswordHash],
        [PreferredEmploymentType],[WorkModePreference],[LocationPreference],
        [YearsOfExperience],[TotalExperiencePoints],[CurrentLevel],
        [ActiveAccount],[CreatedAt],[LastUpdated])
    VALUES (
        103,'Eva','Kowalski',35,'Female','eva.kowalski@technova.com','+14151110001',
        'USA','San Francisco','1 Market St','Stanford University','Business Informatics',
        2008,2012,
        '','','',0,
        '','','',
        'Full-time','On-site','San Francisco',
        10,800,5,
        1,'2025-06-01','2026-01-15');
    SET IDENTITY_INSERT [Users] OFF;
END;

-- 104  Frank Weber  – extra recruiter @ DataFlow (company_id = 2), no slots (edge case)
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [UserId] = 104)
BEGIN
    SET IDENTITY_INSERT [Users] ON;
    INSERT INTO [Users] (
        [UserId],[FirstName],[LastName],[Age],[Gender],[Email],[Phone],
        [Country],[City],[Address],[University],[Degree],
        [UniversityStartYear],[ExpectedGraduationYear],
        [GitHub],[LinkedIn],[Motivation],[HasDisabilities],
        [ProfilePicturePath],[ParsedCv],[PasswordHash],
        [PreferredEmploymentType],[WorkModePreference],[LocationPreference],
        [YearsOfExperience],[TotalExperiencePoints],[CurrentLevel],
        [ActiveAccount],[CreatedAt],[LastUpdated])
    VALUES (
        104,'Frank','Weber',40,'Male','frank.weber@dataflow.com','+12121110002',
        'USA','New York','300 Park Ave','Columbia University','Data Management',
        2002,2006,
        '','','',0,
        '','','',
        'Full-time','On-site','New York',
        15,950,6,
        1,'2025-06-01','2026-01-15');
    SET IDENTITY_INSERT [Users] OFF;
END;

-- 105  Grace Kim  – fresh graduate 2025, low XP
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [UserId] = 105)
BEGIN
    SET IDENTITY_INSERT [Users] ON;
    INSERT INTO [Users] (
        [UserId],[FirstName],[LastName],[Age],[Gender],[Email],[Phone],
        [Country],[City],[Address],[University],[Degree],
        [UniversityStartYear],[ExpectedGraduationYear],
        [GitHub],[LinkedIn],[Motivation],[HasDisabilities],
        [ProfilePicturePath],[ParsedCv],[PasswordHash],
        [PreferredEmploymentType],[WorkModePreference],[LocationPreference],
        [YearsOfExperience],[TotalExperiencePoints],[CurrentLevel],
        [ActiveAccount],[CreatedAt],[LastUpdated])
    VALUES (
        105,'Grace','Kim',23,'Female','grace.kim@test.com','+40700111005',
        'Romania','Iasi','12 Copou Blvd','Alexandru Ioan Cuza University','Informatics',
        2021,2025,
        'https://github.com/gracekim','',
        'Looking for my first full-time job in frontend development.',0,
        '','','',
        'Full-time','Remote','',
        0,50,1,
        1,'2026-04-01','2026-04-01');
    SET IDENTITY_INSERT [Users] OFF;
END;

-- 106  Henry Blackwood  – EDGE CASE: soft-deleted (ActiveAccount = 0)
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [UserId] = 106)
BEGIN
    SET IDENTITY_INSERT [Users] ON;
    INSERT INTO [Users] (
        [UserId],[FirstName],[LastName],[Age],[Gender],[Email],[Phone],
        [Country],[City],[Address],[University],[Degree],
        [UniversityStartYear],[ExpectedGraduationYear],
        [GitHub],[LinkedIn],[Motivation],[HasDisabilities],
        [ProfilePicturePath],[ParsedCv],[PasswordHash],
        [PreferredEmploymentType],[WorkModePreference],[LocationPreference],
        [YearsOfExperience],[TotalExperiencePoints],[CurrentLevel],
        [ActiveAccount],[CreatedAt],[LastUpdated])
    VALUES (
        106,'Henry','Blackwood',27,'Male','henry.blackwood@test.com','+40700111006',
        'Romania','Brasov','','Transilvania University','Computer Engineering',
        2016,2020,
        '','','',0,
        '','','',
        '','','',
        2,100,1,
        0,'2025-08-01','2026-02-01');  -- ActiveAccount = 0 ← inactive
    SET IDENTITY_INSERT [Users] OFF;
END;

-- 107  Iris Fontaine  – frontend candidate, liked two jobs
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [UserId] = 107)
BEGIN
    SET IDENTITY_INSERT [Users] ON;
    INSERT INTO [Users] (
        [UserId],[FirstName],[LastName],[Age],[Gender],[Email],[Phone],
        [Country],[City],[Address],[University],[Degree],
        [UniversityStartYear],[ExpectedGraduationYear],
        [GitHub],[LinkedIn],[Motivation],[HasDisabilities],
        [ProfilePicturePath],[ParsedCv],[PasswordHash],
        [PreferredEmploymentType],[WorkModePreference],[LocationPreference],
        [YearsOfExperience],[TotalExperiencePoints],[CurrentLevel],
        [ActiveAccount],[CreatedAt],[LastUpdated])
    VALUES (
        107,'Iris','Fontaine',26,'Female','iris.fontaine@test.com','+40700111007',
        'Romania','Cluj-Napoca','7 Dorobantilor','Technical University of Cluj','Computer Science',
        2018,2022,
        'https://github.com/irisfontaine','https://linkedin.com/in/irisfontaine',
        'Frontend enthusiast with strong React & TypeScript skills.',0,
        '','','',
        'Full-time','Remote','Cluj-Napoca',
        3,320,2,
        1,'2026-02-15','2026-05-01');
    SET IDENTITY_INSERT [Users] OFF;
END;

-- 108  Jake Morrison  – candidate with a Rejected match (edge case)
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [UserId] = 108)
BEGIN
    SET IDENTITY_INSERT [Users] ON;
    INSERT INTO [Users] (
        [UserId],[FirstName],[LastName],[Age],[Gender],[Email],[Phone],
        [Country],[City],[Address],[University],[Degree],
        [UniversityStartYear],[ExpectedGraduationYear],
        [GitHub],[LinkedIn],[Motivation],[HasDisabilities],
        [ProfilePicturePath],[ParsedCv],[PasswordHash],
        [PreferredEmploymentType],[WorkModePreference],[LocationPreference],
        [YearsOfExperience],[TotalExperiencePoints],[CurrentLevel],
        [ActiveAccount],[CreatedAt],[LastUpdated])
    VALUES (
        108,'Jake','Morrison',29,'Male','jake.morrison@test.com','+40700111008',
        'Romania','Oradea','','University of Oradea','IT & Communication',
        2015,2019,
        '','','',0,
        '','','',
        'Contract','On-site','',
        5,200,2,
        1,'2025-12-01','2026-03-10');
    SET IDENTITY_INSERT [Users] OFF;
END;

-- ────────────────────────────────────────────────────────────
-- 2.  RECRUITERS  (T&I table)
--     Existing recruiter (registered via app) is already there.
--     Add Eva and Frank as extra recruiter fixtures.
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [Recruiters] WHERE [user_id] = 103)
    INSERT INTO [Recruiters] ([company_id],[user_id],[name]) VALUES (1, 103, 'Eva Kowalski');

IF NOT EXISTS (SELECT 1 FROM [Recruiters] WHERE [user_id] = 104)
    INSERT INTO [Recruiters] ([company_id],[user_id],[name]) VALUES (2, 104, 'Frank Weber');

-- ────────────────────────────────────────────────────────────
-- 3.  JOBS
--     job_role ints: 0=Frontend 1=Backend 2=UiUx 3=DevOps
--                    4=ProjectManager 5=DataAnalyst
--                    6=Cybersecurity 7=AiMl
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [jobs] WHERE [job_id] = 200)
BEGIN
    SET IDENTITY_INSERT [jobs] ON;
    INSERT INTO [jobs] (
        [job_id],[company_id],[job_title],[industry_field],[job_type],
        [experience_level],[job_description],[job_location],
        [available_positions],[posted_at],[salary],[amount_payed],
        [deadline],[promotion_level],[job_role])
    VALUES
    -- 200  Frontend React Dev @ TechNova – remote, active
    (200,1,'Frontend React Developer','IT','Full-time',
     'Mid-Level','Build responsive UIs with React 18 and TypeScript.','Remote',
     2,'2026-04-01',85000,0,'2026-07-01',2,0),

    -- 201  DevOps Engineer @ DataFlow – EXPIRED (deadline in past)
    (201,2,'DevOps Engineer','Cloud','Full-time',
     'Senior','Manage Kubernetes clusters and CI/CD pipelines.','New York, NY',
     1,'2026-01-15',115000,0,'2026-04-01',1,3),   -- deadline past!

    -- 202  UI/UX Designer @ EcoCode – NO required skills (edge case)
    (202,3,'UI/UX Designer','Design','Part-time',
     'Junior','Create wireframes and user flows in Figma.','Seattle, WA',
     3,'2026-05-01',60000,0,'2026-08-01',0,2),

    -- 203  Cybersecurity Analyst @ TechNova – many required skills
    (203,1,'Cybersecurity Analyst','Security','Full-time',
     'Senior','Lead threat analysis and incident response.','San Francisco, CA',
     1,'2026-03-20',130000,0,'2026-07-15',3,6),

    -- 204  AI/ML Engineer @ DataFlow – high salary
    (204,2,'AI/ML Engineer','Data Science','Full-time',
     'Senior','Design and deploy large-scale ML models.','New York, NY',
     2,'2026-04-10',150000,0,'2026-08-01',2,7),

    -- 205  Project Manager @ FinEdge – no applications exist (edge case)
    (205,4,'Project Manager','Finance','Full-time',
     'Mid-Level','Coordinate fintech product delivery via Agile.','London, UK',
     1,'2026-05-15',95000,0,'2026-09-01',1,4);
    SET IDENTITY_INSERT [jobs] OFF;
END;

-- ────────────────────────────────────────────────────────────
-- 4.  JOB SKILLS
-- ────────────────────────────────────────────────────────────
-- Job 200  Frontend: React(2), TypeScript(31), HTML(33), CSS(34)
IF NOT EXISTS (SELECT 1 FROM [JobSkills] WHERE [JobId]=200 AND [SkillId]=2)
    INSERT INTO [JobSkills] VALUES (200,2,75),(200,31,70),(200,33,80),(200,34,75);

-- Job 201  DevOps: Docker(6), Kubernetes(7), GitHub Actions(80), Terraform(85)
IF NOT EXISTS (SELECT 1 FROM [JobSkills] WHERE [JobId]=201 AND [SkillId]=6)
    INSERT INTO [JobSkills] VALUES (201,6,80),(201,7,80),(201,80,70),(201,85,65);

-- Job 202  UX: NO skills (intentional edge case – nothing to insert)

-- Job 203  Cybersecurity: Firewalls(116), Metasploit(120), Splunk(123),
--          Penetration Testing(19), SIEM(20), ISO 27001(130)
IF NOT EXISTS (SELECT 1 FROM [JobSkills] WHERE [JobId]=203 AND [SkillId]=116)
    INSERT INTO [JobSkills] VALUES (203,116,70),(203,120,65),(203,123,75),
                                   (203,19,80),(203,20,70),(203,130,60);

-- Job 204  AI/ML: Python(8), PyTorch(28), TensorFlow(137), NumPy(145), Spark(24)
IF NOT EXISTS (SELECT 1 FROM [JobSkills] WHERE [JobId]=204 AND [SkillId]=8)
    INSERT INTO [JobSkills] VALUES (204,8,85),(204,28,80),(204,137,75),
                                   (204,145,70),(204,24,70);

-- Job 205  Project Manager: Agile(23), Jira(94), Risk Assessment(97)
IF NOT EXISTS (SELECT 1 FROM [JobSkills] WHERE [JobId]=205 AND [SkillId]=23)
    INSERT INTO [JobSkills] VALUES (205,23,70),(205,94,65),(205,97,60);

-- ────────────────────────────────────────────────────────────
-- 5.  USER SKILLS
-- ────────────────────────────────────────────────────────────
-- Bob  (100): strong backend, partially unverified DevOps
IF NOT EXISTS (SELECT 1 FROM [UserSkills] WHERE [UserId]=100 AND [SkillId]=1)
    INSERT INTO [UserSkills] ([UserId],[SkillId],[Score],[IsVerified],[AchievedDate])
    VALUES (100,1,85,1,'2025-11-01'),   -- C#        verified
           (100,3,78,1,'2025-11-01'),   -- SQL       verified
           (100,49,72,1,'2025-12-01'),  -- ASP.NET   verified
           (100,4,60,0,NULL),           -- Testing   unverified
           (100,6,45,0,NULL),           -- Docker    unverified
           (100,39,80,1,'2026-01-10');  -- Git       verified

-- Dan  (102): senior AI/ML + backend, all verified
IF NOT EXISTS (SELECT 1 FROM [UserSkills] WHERE [UserId]=102 AND [SkillId]=8)
    INSERT INTO [UserSkills] ([UserId],[SkillId],[Score],[IsVerified],[AchievedDate])
    VALUES (102,8,95,1,'2024-06-01'),   -- Python
           (102,10,90,1,'2024-06-01'),  -- Machine Learning
           (102,28,88,1,'2024-09-01'),  -- PyTorch
           (102,137,85,1,'2024-09-01'), -- TensorFlow
           (102,24,82,1,'2024-12-01'),  -- Spark
           (102,145,80,1,'2025-01-01'), -- NumPy
           (102,3,88,1,'2024-06-01'),   -- SQL
           (102,26,75,1,'2025-03-01');  -- PostgreSQL

-- Grace (105): beginner frontend, all unverified
IF NOT EXISTS (SELECT 1 FROM [UserSkills] WHERE [UserId]=105 AND [SkillId]=33)
    INSERT INTO [UserSkills] ([UserId],[SkillId],[Score],[IsVerified],[AchievedDate])
    VALUES (105,33,50,0,NULL),   -- HTML
           (105,34,45,0,NULL),   -- CSS
           (105,2,38,0,NULL),    -- React (low score)
           (105,37,42,0,NULL);   -- JavaScript

-- Iris (107): strong frontend, verified
IF NOT EXISTS (SELECT 1 FROM [UserSkills] WHERE [UserId]=107 AND [SkillId]=2)
    INSERT INTO [UserSkills] ([UserId],[SkillId],[Score],[IsVerified],[AchievedDate])
    VALUES (107,2,82,1,'2025-10-01'),   -- React      verified
           (107,31,78,1,'2025-10-01'),  -- TypeScript verified
           (107,33,90,1,'2025-09-01'),  -- HTML       verified
           (107,34,88,1,'2025-09-01'),  -- CSS        verified
           (107,37,80,1,'2025-10-01'),  -- JavaScript verified
           (107,12,65,0,NULL);          -- Figma      unverified

-- Carol (101): NO skills – intentional empty state

-- ────────────────────────────────────────────────────────────
-- 6.  WORK EXPERIENCES
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [WorkExperiences] WHERE [UserId]=100)
    INSERT INTO [WorkExperiences] (
        [UserId],[Company],[JobTitle],[StartDate],[EndDate],[Description],[CurrentlyWorking])
    VALUES
    (100,'Codewave SRL','Junior .NET Developer',
     '2022-07-01 00:00:00 +00:00','2024-01-01 00:00:00 +00:00',
     'Built REST APIs and maintained SQL Server databases.',0),
    (100,'Betfair Romania','Mid .NET Developer',
     '2024-02-01 00:00:00 +00:00',NULL,
     'Designing microservices with ASP.NET Core and Azure Service Bus.',1);

IF NOT EXISTS (SELECT 1 FROM [WorkExperiences] WHERE [UserId]=102)
    INSERT INTO [WorkExperiences] (
        [UserId],[Company],[JobTitle],[StartDate],[EndDate],[Description],[CurrentlyWorking])
    VALUES
    (102,'Endava','Software Engineer',
     '2018-09-01 00:00:00 +00:00','2021-03-01 00:00:00 +00:00',
     'Python backend services and data pipelines.',0),
    (102,'Cognizant','Senior Data Engineer',
     '2021-04-01 00:00:00 +00:00','2023-12-01 00:00:00 +00:00',
     'Led migration to Spark-based ETL on AWS EMR.',0),
    (102,'Adobe Romania','Staff ML Engineer',
     '2024-01-01 00:00:00 +00:00',NULL,
     'Training and productionising LLM-based features.',1);

-- ────────────────────────────────────────────────────────────
-- 7.  PROJECTS
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [Projects] WHERE [UserId]=100)
    INSERT INTO [Projects] ([UserId],[Name],[Description],[Technologies],[Url])
    VALUES
    (100,'Task Manager API','REST API for task management with JWT auth.',
     '["C#","ASP.NET Core","SQL Server","Docker"]',
     'https://github.com/bobmartin/taskmanager'),
    (100,'CLI Budget Tool','Console app to track personal expenses.',
     '["C#","SQLite"]','https://github.com/bobmartin/budgettool');

IF NOT EXISTS (SELECT 1 FROM [Projects] WHERE [UserId]=107)
    INSERT INTO [Projects] ([UserId],[Name],[Description],[Technologies],[Url])
    VALUES
    (107,'Portfolio Site','Personal portfolio built with Next.js.',
     '["React","TypeScript","Tailwind CSS"]',
     'https://irisfontaine.dev');

-- ────────────────────────────────────────────────────────────
-- 8.  EXTRA-CURRICULAR ACTIVITIES
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [ExtraCurricularActivities] WHERE [UserId]=100)
    INSERT INTO [ExtraCurricularActivities] (
        [UserId],[ActivityName],[Organization],[Role],[Period],[Description])
    VALUES
    (100,'Hackathon','HackTM','Participant','2023',
     'Built a smart-home prototype in 24 h; placed 2nd.'),
    (100,'Open Source Contribution','ASP.NET Core','Contributor','2024-present',
     'Fixed documentation bugs and minor runtime issues.');

IF NOT EXISTS (SELECT 1 FROM [ExtraCurricularActivities] WHERE [UserId]=105)
    INSERT INTO [ExtraCurricularActivities] (
        [UserId],[ActivityName],[Organization],[Role],[Period],[Description])
    VALUES
    (105,'Web Dev Club','UAIC','Member','2022-2025',
     'Built club website and organised coding workshops.');

-- ────────────────────────────────────────────────────────────
-- 9.  PERSONALITY TEST RESULTS + TRAIT SCORES
--     TraitType ints stored as strings; values: Abstraction,
--     Visibility, Interaction, Depth, Creativity, Pace
--     SelectedRole: matches JobRole enum (0-7)
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [PersonalityTestResults] WHERE [UserId]=100)
BEGIN
    INSERT INTO [PersonalityTestResults] ([UserId],[CompletedAt],[SelectedRole])
    VALUES (100,'2026-02-10',1);  -- SelectedRole=1 (BackendDeveloper)
    DECLARE @ptr100 INT = SCOPE_IDENTITY();
    INSERT INTO [PersonalityTraitScores] ([PersonalityTestResultId],[Trait],[Score])
    VALUES (@ptr100,'Abstraction',4),(@ptr100,'Visibility',2),
           (@ptr100,'Interaction',3),(@ptr100,'Depth',5),
           (@ptr100,'Creativity',3),(@ptr100,'Pace',4);
END;

IF NOT EXISTS (SELECT 1 FROM [PersonalityTestResults] WHERE [UserId]=102)
BEGIN
    INSERT INTO [PersonalityTestResults] ([UserId],[CompletedAt],[SelectedRole])
    VALUES (102,'2025-12-01',7);  -- SelectedRole=7 (AiMlEngineer)
    DECLARE @ptr102 INT = SCOPE_IDENTITY();
    INSERT INTO [PersonalityTraitScores] ([PersonalityTestResultId],[Trait],[Score])
    VALUES (@ptr102,'Abstraction',5),(@ptr102,'Visibility',1),
           (@ptr102,'Interaction',2),(@ptr102,'Depth',5),
           (@ptr102,'Creativity',4),(@ptr102,'Pace',3);
END;

IF NOT EXISTS (SELECT 1 FROM [PersonalityTestResults] WHERE [UserId]=105)
BEGIN
    INSERT INTO [PersonalityTestResults] ([UserId],[CompletedAt],[SelectedRole])
    VALUES (105,'2026-04-20',NULL);  -- SelectedRole NULL (not decided)
    DECLARE @ptr105 INT = SCOPE_IDENTITY();
    INSERT INTO [PersonalityTraitScores] ([PersonalityTestResultId],[Trait],[Score])
    VALUES (@ptr105,'Abstraction',2),(@ptr105,'Visibility',4),
           (@ptr105,'Interaction',5),(@ptr105,'Depth',2),
           (@ptr105,'Creativity',5),(@ptr105,'Pace',4);
END;
-- Carol (101) has NO personality test result – edge case

-- ────────────────────────────────────────────────────────────
-- 10. SKILL TESTS
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [SkillTests] WHERE [UserId]=100)
    INSERT INTO [SkillTests] ([UserId],[Name],[Score],[AchievedDate])
    VALUES (100,'C# Advanced',88,'2026-01-20'),
           (100,'SQL Server',81,'2026-02-05');

IF NOT EXISTS (SELECT 1 FROM [SkillTests] WHERE [UserId]=102)
    INSERT INTO [SkillTests] ([UserId],[Name],[Score],[AchievedDate])
    VALUES (102,'Python Data Science',94,'2025-11-10'),
           (102,'Machine Learning Fundamentals',91,'2025-12-01'),
           (102,'SQL & Analytics',89,'2026-01-05');

-- ────────────────────────────────────────────────────────────
-- 11. MATCHES  (Status stored as string)
--     Applied=0 → 'Applied', Accepted=1 → 'Accepted'
--     Rejected=2 → 'Rejected', Advanced=3 → 'Advanced'
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [Matches] WHERE [UserId]=100 AND [JobId]=101)
    INSERT INTO [Matches] ([UserId],[JobId],[Status],[Timestamp],[FeedbackMessage])
    VALUES
    -- Bob applied to existing backend job
    (100,101,'Applied','2026-05-01',''),
    -- Bob accepted for new frontend job (even though role differs – tests mixed-role logic)
    (100,200,'Accepted','2026-05-03','Great portfolio, moving to next stage.');

IF NOT EXISTS (SELECT 1 FROM [Matches] WHERE [UserId]=101 AND [JobId]=101)
    -- Carol (no skills) applied to job with skill requirements – edge case
    INSERT INTO [Matches] ([UserId],[JobId],[Status],[Timestamp],[FeedbackMessage])
    VALUES (101,101,'Applied','2026-05-10','');

IF NOT EXISTS (SELECT 1 FROM [Matches] WHERE [UserId]=102 AND [JobId]=102)
    INSERT INTO [Matches] ([UserId],[JobId],[Status],[Timestamp],[FeedbackMessage])
    VALUES
    -- Dan advanced on Data Engineer job
    (102,102,'Advanced','2026-04-25','Top candidate – scheduled for final interview.'),
    -- Dan accepted for AI/ML job
    (102,204,'Accepted','2026-05-05','Skills match perfectly.');

IF NOT EXISTS (SELECT 1 FROM [Matches] WHERE [UserId]=105 AND [JobId]=200)
    INSERT INTO [Matches] ([UserId],[JobId],[Status],[Timestamp],[FeedbackMessage])
    VALUES
    -- Grace rejected (low skill scores)
    (105,200,'Rejected','2026-05-02','Score below threshold for mid-level position.');

IF NOT EXISTS (SELECT 1 FROM [Matches] WHERE [UserId]=105 AND [JobId]=201)
    -- Grace applied to expired job (tests display of expired postings)
    INSERT INTO [Matches] ([UserId],[JobId],[Status],[Timestamp],[FeedbackMessage])
    VALUES (105,201,'Applied','2026-03-20','');

IF NOT EXISTS (SELECT 1 FROM [Matches] WHERE [UserId]=107 AND [JobId]=200)
    INSERT INTO [Matches] ([UserId],[JobId],[Status],[Timestamp],[FeedbackMessage])
    VALUES
    -- Iris applied to frontend job
    (107,200,'Applied','2026-05-04',''),
    -- Iris applied to UX job (no skill requirement – always matches)
    (107,202,'Applied','2026-05-04','');

IF NOT EXISTS (SELECT 1 FROM [Matches] WHERE [UserId]=108 AND [JobId]=203)
    -- Jake rejected from cybersecurity job (wrong skill set entirely)
    INSERT INTO [Matches] ([UserId],[JobId],[Status],[Timestamp],[FeedbackMessage])
    VALUES (108,203,'Rejected','2026-04-15','Required security certifications not met.');

-- ────────────────────────────────────────────────────────────
-- 12. RECOMMENDATIONS
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [Recommendations] WHERE [UserId]=100 AND [JobId]=200)
    INSERT INTO [Recommendations] ([UserId],[JobId],[Timestamp])
    VALUES (100,200,'2026-05-01'),
           (100,204,'2026-05-01');

IF NOT EXISTS (SELECT 1 FROM [Recommendations] WHERE [UserId]=102 AND [JobId]=204)
    INSERT INTO [Recommendations] ([UserId],[JobId],[Timestamp])
    VALUES (102,204,'2026-04-20'),
           (102,102,'2026-04-20');

IF NOT EXISTS (SELECT 1 FROM [Recommendations] WHERE [UserId]=107 AND [JobId]=200)
    INSERT INTO [Recommendations] ([UserId],[JobId],[Timestamp])
    VALUES (107,200,'2026-05-03');

-- ────────────────────────────────────────────────────────────
-- 13. CHATS + MESSAGES
--     Chat.Type for Messages: 0=Text, 1=File (MessageType enum)
-- ────────────────────────────────────────────────────────────

-- Chat 1: Bob ↔ TechNova / Job 200, active with messages
IF NOT EXISTS (SELECT 1 FROM [Chats] WHERE [UserId]=100 AND [CompanyId]=1)
BEGIN
    INSERT INTO [Chats] (
        [UserId],[CompanyId],[SecondUserId],[JobId],
        [IsBlocked],[BlockedByUserId],[DeletedAtByUser],[DeletedAtBySecondParty])
    VALUES (100,1,NULL,200, 0,NULL,NULL,NULL);
    DECLARE @chat1 INT = SCOPE_IDENTITY();
    INSERT INTO [Messages] ([ChatId],[SenderId],[Content],[Timestamp],[Type],[IsRead],[OriginalFileName])
    VALUES
    (@chat1,100,'Hello, I am very interested in the Frontend React Developer role.',
     '2026-05-04 09:00:00',0,1,''),
    (@chat1,103,'Thanks for reaching out, Bob! Your GitHub looks impressive.',
     '2026-05-04 09:15:00',0,1,''),
    (@chat1,100,'I have 3 years with React and TypeScript. Happy to do a technical screen.',
     '2026-05-04 09:20:00',0,1,''),
    (@chat1,103,'Sounds great. I will schedule a slot for next week.',
     '2026-05-04 09:30:00',0,0,'');   -- last message unread
END;

-- Chat 2: Alice ↔ DataFlow / Job 102
IF NOT EXISTS (SELECT 1 FROM [Chats] WHERE [UserId]=1 AND [CompanyId]=2)
BEGIN
    INSERT INTO [Chats] (
        [UserId],[CompanyId],[SecondUserId],[JobId],
        [IsBlocked],[BlockedByUserId],[DeletedAtByUser],[DeletedAtBySecondParty])
    VALUES (1,2,NULL,102, 0,NULL,NULL,NULL);
    DECLARE @chat2 INT = SCOPE_IDENTITY();
    INSERT INTO [Messages] ([ChatId],[SenderId],[Content],[Timestamp],[Type],[IsRead],[OriginalFileName])
    VALUES
    (@chat2,1,'Hi, I applied for the Data Engineer position.',
     '2026-04-20 10:00:00',0,1,''),
    (@chat2,104,'Hi Alice! We reviewed your profile. Can you share your CV?',
     '2026-04-20 11:00:00',0,1,''),
    (@chat2,1,'Sure, attached.',
     '2026-04-20 11:05:00',1,1,'Alice_CV.pdf');  -- Type=1 File message
END;

-- Chat 3: Dan ↔ TechNova – BLOCKED (Dan blocked by company)
IF NOT EXISTS (SELECT 1 FROM [Chats] WHERE [UserId]=102 AND [CompanyId]=1)
BEGIN
    INSERT INTO [Chats] (
        [UserId],[CompanyId],[SecondUserId],[JobId],
        [IsBlocked],[BlockedByUserId],[DeletedAtByUser],[DeletedAtBySecondParty])
    VALUES (102,1,NULL,NULL, 1,103,NULL,NULL);  -- IsBlocked=1, blocked by Eva(103)
    DECLARE @chat3 INT = SCOPE_IDENTITY();
    INSERT INTO [Messages] ([ChatId],[SenderId],[Content],[Timestamp],[Type],[IsRead],[OriginalFileName])
    VALUES
    (@chat3,102,'I am interested in any senior backend openings.',
     '2026-03-01 08:00:00',0,1,'');
END;

-- Chat 4: Carol ↔ EcoCode – EMPTY (no messages, edge case)
IF NOT EXISTS (SELECT 1 FROM [Chats] WHERE [UserId]=101 AND [CompanyId]=3)
    INSERT INTO [Chats] (
        [UserId],[CompanyId],[SecondUserId],[JobId],
        [IsBlocked],[BlockedByUserId],[DeletedAtByUser],[DeletedAtBySecondParty])
    VALUES (101,3,NULL,202, 0,NULL,NULL,NULL);

-- ────────────────────────────────────────────────────────────
-- 14. T&I: TESTS + QUESTIONS
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [Tests] WHERE [title]='C# Fundamentals')
BEGIN
    INSERT INTO [Tests] ([title],[category],[created_at])
    VALUES ('C# Fundamentals','Backend','2026-01-01'),
           ('React Basics','Frontend','2026-01-01'),
           ('SQL Queries','Database','2026-02-01');
END;

-- Questions for Test 1 (C# Fundamentals)
IF NOT EXISTS (SELECT 1 FROM [Questions] WHERE [question_text]='What is a delegate in C#?')
BEGIN
    DECLARE @t1 INT = (SELECT [id] FROM [Tests] WHERE [title]='C# Fundamentals');
    DECLARE @t2 INT = (SELECT [id] FROM [Tests] WHERE [title]='React Basics');
    DECLARE @t3 INT = (SELECT [id] FROM [Tests] WHERE [title]='SQL Queries');

    INSERT INTO [Questions] (
        [test_id],[question_text],[question_type_string],[question_score],
        [question_answer],[options_json])
    VALUES
    -- C# Fundamentals questions
    (@t1,'What is a delegate in C#?','MultipleChoice',2.0,
     'A type that represents references to methods',
     '["A class","A type that represents references to methods","An interface","A keyword"]'),
    (@t1,'Which keyword prevents a class from being inherited?','MultipleChoice',2.0,
     'sealed','["abstract","static","sealed","readonly"]'),
    (@t1,'What does the async/await pattern do?','MultipleChoice',2.0,
     'Enables non-blocking asynchronous code',
     '["Runs code in parallel","Enables non-blocking asynchronous code","Creates a new thread","Locks a resource"]'),
    (@t1,'What is boxing in C#?','MultipleChoice',2.0,
     'Converting a value type to object',
     '["Wrapping a class","Converting a value type to object","A design pattern","Encrypting data"]'),

    -- React Basics questions
    (@t2,'What hook manages local component state?','MultipleChoice',2.0,
     'useState','["useEffect","useState","useReducer","useContext"]'),
    (@t2,'What does the useEffect hook do?','MultipleChoice',2.0,
     'Runs side effects after render',
     '["Fetches data only","Runs side effects after render","Replaces state","Memoises a value"]'),
    (@t2,'What is the virtual DOM?','MultipleChoice',2.0,
     'An in-memory representation of the real DOM',
     '["A browser API","A CSS framework","An in-memory representation of the real DOM","A database"]'),

    -- SQL Queries questions
    (@t3,'Which clause filters rows after aggregation?','MultipleChoice',2.0,
     'HAVING','["WHERE","HAVING","GROUP BY","ORDER BY"]'),
    (@t3,'What does a LEFT JOIN return?','MultipleChoice',2.0,
     'All rows from the left table plus matching rows',
     '["Only matching rows","All rows from both tables","All rows from the left table plus matching rows","All rows from the right table"]'),
    (@t3,'What does the DISTINCT keyword do?','MultipleChoice',2.0,
     'Removes duplicate rows from results',
     '["Sorts results","Removes duplicate rows from results","Filters NULLs","Creates an index"]');
END;

-- ────────────────────────────────────────────────────────────
-- 15. T&I: TEST ATTEMPTS + ANSWERS
--     Bob has two attempts on Test 1: first abandoned, second complete.
--     Dan completed Test 1.  Grace started Test 2 but did not finish.
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [TestAttempts] WHERE [external_user_id]=100)
BEGIN
    DECLARE @t1id INT = (SELECT [id] FROM [Tests] WHERE [title]='C# Fundamentals');
    DECLARE @t2id INT = (SELECT [id] FROM [Tests] WHERE [title]='React Basics');

    -- Bob: abandoned attempt (edge case)
    INSERT INTO [TestAttempts] (
        [test_id],[external_user_id],[score],[status],
        [started_at],[completed_at],[answers_file_path],[is_validated],
        [percentage_score],[rejection_reason],[rejected_at])
    VALUES (@t1id,100,NULL,'Abandoned',
            '2026-03-01 10:00:00',NULL,'',0,NULL,NULL,NULL);

    -- Bob: completed attempt
    INSERT INTO [TestAttempts] (
        [test_id],[external_user_id],[score],[status],
        [started_at],[completed_at],[answers_file_path],[is_validated],
        [percentage_score],[rejection_reason],[rejected_at])
    VALUES (@t1id,100,8.0,'Completed',
            '2026-03-15 09:00:00','2026-03-15 09:25:00','',1,100.0,NULL,NULL);

    -- Dan: completed attempt (slightly lower score → same rank to test tie-break)
    INSERT INTO [TestAttempts] (
        [test_id],[external_user_id],[score],[status],
        [started_at],[completed_at],[answers_file_path],[is_validated],
        [percentage_score],[rejection_reason],[rejected_at])
    VALUES (@t1id,102,8.0,'Completed',
            '2026-03-16 14:00:00','2026-03-16 14:22:00','',1,100.0,NULL,NULL);

    -- Grace: in-progress (never submitted)
    INSERT INTO [TestAttempts] (
        [test_id],[external_user_id],[score],[status],
        [started_at],[completed_at],[answers_file_path],[is_validated],
        [percentage_score],[rejection_reason],[rejected_at])
    VALUES (@t2id,105,NULL,'InProgress',
            '2026-05-10 11:00:00',NULL,'',0,NULL,NULL,NULL);
END;

-- Answers for Bob's completed attempt
IF NOT EXISTS (SELECT 1 FROM [Answers] WHERE [attempt_id] =
    (SELECT TOP 1 [id] FROM [TestAttempts]
     WHERE [external_user_id]=100 AND [status]='Completed'))
BEGIN
    DECLARE @bobAttempt INT =
        (SELECT TOP 1 [id] FROM [TestAttempts]
         WHERE [external_user_id]=100 AND [status]='Completed');
    DECLARE @t1id2 INT = (SELECT [id] FROM [Tests] WHERE [title]='C# Fundamentals');

    INSERT INTO [Answers] ([question_id],[attempt_id],[value])
    SELECT q.[id], @bobAttempt, q.[question_answer]
    FROM [Questions] q WHERE q.[test_id] = @t1id2;
END;

-- ────────────────────────────────────────────────────────────
-- 16. T&I: LEADERBOARD
--     Bob and Dan have identical scores on Test 1 → tie-break needed
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [LeaderboardEntries] WHERE [user_id]=100)
BEGIN
    DECLARE @t1lb INT = (SELECT [id] FROM [Tests] WHERE [title]='C# Fundamentals');
    DECLARE @t2lb INT = (SELECT [id] FROM [Tests] WHERE [title]='React Basics');

    INSERT INTO [LeaderboardEntries] (
        [test_id],[user_id],[normalized_score],[rank_position],
        [tie_break_priority],[last_recalculation_at])
    VALUES
    -- Bob rank 1 (earlier completion → lower tie-break value wins)
    (@t1lb, 100, 100.0, 1, 1, '2026-03-15 09:30:00'),
    -- Dan rank 2 (same score, later completion)
    (@t1lb, 102, 100.0, 2, 2, '2026-03-16 14:30:00'),
    -- Bob on React test
    (@t2lb, 100, 0.0, 1, 1, '2026-03-15 09:30:00');  -- only participant
END;

-- ────────────────────────────────────────────────────────────
-- 17. T&I: APPLICANTS
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [applicants] WHERE [user_id]=100 AND [job_id]=101)
    INSERT INTO [applicants] (
        [job_id],[user_id],[app_test_grade],[cv_grade],[company_test_grade],
        [interview_grade],[application_status],[applied_at],[recommended_from_company_id],[cv_file_url])
    VALUES
    (101,100,82.0,75.0,NULL,NULL,'Screening','2026-05-01',NULL,''),
    (102,102,91.0,88.0,90.0,NULL,'Interview','2026-04-25',NULL,'');

-- ────────────────────────────────────────────────────────────
-- 18. T&I: EVENTS + COLLABORATORS
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [events] WHERE [title]='TechNova Tech Fair 2026')
BEGIN
    INSERT INTO [events] ([photo],[title],[description],[start_date],[end_date],
                          [location],[host_company_id],[posted_at])
    VALUES
    ('','TechNova Tech Fair 2026',
     'Annual tech fair with live coding challenges and networking.',
     '2026-09-15','2026-09-16','San Francisco Convention Center',1,'2026-05-01'),
    ('','DataFlow Data Summit',
     'Data engineering and ML conference, co-hosted with EcoCode.',
     '2026-10-10','2026-10-11','New York Hilton',2,'2026-05-15');

    DECLARE @evt1 INT = (SELECT [event_id] FROM [events] WHERE [title]='TechNova Tech Fair 2026');
    DECLARE @evt2 INT = (SELECT [event_id] FROM [events] WHERE [title]='DataFlow Data Summit');

    -- EcoCode co-hosts DataFlow summit (collaborator edge case)
    INSERT INTO [collaborators] ([event_id],[company_id]) VALUES (@evt2, 3);
END;

-- ────────────────────────────────────────────────────────────
-- 19. T&I: INTERVIEW SLOTS
--     Eva (103) @ TechNova: one open slot, one booked with Bob
--     Frank (104) @ DataFlow: NO slots (edge case)
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [Slots] WHERE [RecruiterUserId]=103)
    INSERT INTO [Slots] (
        [recruiter_id],[RecruiterCompanyId],[RecruiterUserId],[candidate_id],
        [start_time],[end_time],[duration],[status],[interview_type])
    VALUES
    -- Available slot
    (103,1,103,NULL,
     '2026-06-10 10:00:00','2026-06-10 10:45:00',45,0,'Online'),
    -- Booked slot with Bob (candidate_id = 100)
    (103,1,103,100,
     '2026-06-12 14:00:00','2026-06-12 14:45:00',45,1,'Online'),
    -- Cancelled slot (edge case)
    (103,1,103,NULL,
     '2026-06-05 09:00:00','2026-06-05 09:45:00',45,2,'In-Person');

-- ────────────────────────────────────────────────────────────
-- 20. T&I: INTERVIEW SESSIONS
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [interview_sessions] WHERE [external_user_id]=102)
    INSERT INTO [interview_sessions] (
        [position_id],[external_user_id],[interviewer_id],
        [date_start],[video],[status],[score])
    VALUES
    -- Dan's completed interview for Data Engineer job (position_id = job_id = 102)
    (102, 102, 104,
     '2026-05-10 15:00:00', NULL, 'Completed', 87.5),
    -- Bob's upcoming interview (no score yet)
    (200, 100, 103,
     '2026-06-12 14:00:00', NULL, 'Scheduled', NULL);

-- ============================================================
-- Done.  Tables populated, all edge cases present.
-- ============================================================

-- ============================================================
-- PART 4: Interview slots (20 slots after June 10 2026)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM [Slots]
               WHERE [RecruiterUserId] = 103
                 AND [start_time] = '2026-06-11 09:00:00')
BEGIN
    INSERT INTO [Slots]
        ([recruiter_id],[RecruiterCompanyId],[RecruiterUserId],[candidate_id],
         [start_time],[end_time],[duration],[status],[interview_type])
    VALUES
    --  Eva (103 / TechNova)  ───────────────────────────────────────────
    -- 1
    (103,1,103,NULL, '2026-06-11 09:00:00','2026-06-11 09:45:00',45,0,'Online'),
    -- 2
    (103,1,103,NULL, '2026-06-11 11:00:00','2026-06-11 11:30:00',30,0,'Online'),
    -- 3
    (103,1,103,NULL, '2026-06-13 10:00:00','2026-06-13 11:00:00',60,0,'In-Person'),
    -- 4
    (103,1,103,NULL, '2026-06-13 14:00:00','2026-06-13 14:45:00',45,0,'Online'),
    -- 5
    (103,1,103,NULL, '2026-06-16 09:00:00','2026-06-16 09:45:00',45,0,'Online'),
    -- 6
    (103,1,103,NULL, '2026-06-16 14:00:00','2026-06-16 15:00:00',60,0,'In-Person'),
    -- 7
    (103,1,103,NULL, '2026-06-18 09:00:00','2026-06-18 09:30:00',30,0,'Online'),
    -- 8
    (103,1,103,NULL, '2026-06-19 10:00:00','2026-06-19 10:45:00',45,0,'Online'),
    -- 9  Booked with Bob (100) – tests the "occupied" display
    (103,1,103,100,  '2026-06-20 14:00:00','2026-06-20 15:00:00',60,1,'Online'),
    -- 10
    (103,1,103,NULL, '2026-06-25 09:00:00','2026-06-25 09:45:00',45,0,'In-Person'),

    --  Frank (104 / DataFlow)  ─────────────────────────────────────────
    -- 11
    (104,2,104,NULL, '2026-06-11 10:00:00','2026-06-11 10:45:00',45,0,'Online'),
    -- 12
    (104,2,104,NULL, '2026-06-12 09:00:00','2026-06-12 09:30:00',30,0,'Online'),
    -- 13
    (104,2,104,NULL, '2026-06-14 10:00:00','2026-06-14 11:00:00',60,0,'In-Person'),
    -- 14
    (104,2,104,NULL, '2026-06-15 14:00:00','2026-06-15 14:45:00',45,0,'Online'),
    -- 15
    (104,2,104,NULL, '2026-06-17 09:00:00','2026-06-17 09:45:00',45,0,'Online'),
    -- 16
    (104,2,104,NULL, '2026-06-18 13:00:00','2026-06-18 13:30:00',30,0,'Online'),
    -- 17
    (104,2,104,NULL, '2026-06-19 11:00:00','2026-06-19 12:00:00',60,0,'In-Person'),
    -- 18
    (104,2,104,NULL, '2026-06-23 10:00:00','2026-06-23 10:45:00',45,0,'Online'),
    -- 19
    (104,2,104,NULL, '2026-06-26 14:00:00','2026-06-26 15:00:00',60,0,'In-Person'),
    -- 20
    (104,2,104,NULL, '2026-07-01 09:00:00','2026-07-01 09:45:00',45,0,'Online');
END;
GO
