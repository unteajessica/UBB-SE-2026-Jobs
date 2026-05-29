USE TestsAndInterviews;
GO

DROP TABLE IF EXISTS Answers;
DROP TABLE IF EXISTS Slots;
DROP TABLE IF EXISTS LeaderboardEntries;
DROP TABLE IF EXISTS InterviewSessions;
DROP TABLE IF EXISTS TestAttempts;
DROP TABLE IF EXISTS Questions;
DROP TABLE IF EXISTS Recruiters;
DROP TABLE IF EXISTS Tests;
DROP TABLE IF EXISTS collaborators;
DROP TABLE IF EXISTS events;
DROP TABLE IF EXISTS applicants;
DROP TABLE IF EXISTS job_skills;
DROP TABLE IF EXISTS jobs;
DROP TABLE IF EXISTS companies;
DROP TABLE IF EXISTS skills;
DROP TABLE IF EXISTS Users;
GO

CREATE TABLE Users (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(255) NOT NULL,
    email NVARCHAR(255) NOT NULL,
    cv_xml NVARCHAR(MAX) NULL
);

CREATE TABLE companies (
    company_id int PRIMARY KEY,
    company_name nvarchar(255) not null,
    about_us text,
    profile_picture_url nvarchar(max),
    logo_picture_url nvarchar(max) not null,
    location nvarchar(300),
    email nvarchar(100),
    buddy_name nvarchar(255),
    avatar_id int,
    final_quote text,
    scen_1_text text,
    scen1_answer1 text,
    scen1_answer2 text,
    scen1_answer3 text,
    scen1_reaction1 text,
    scen1_reaction2 text,
    scen1_reaction3 text,
    scen2_text text,
    scen2_answer1 text,
    scen2_answer2 text,
    scen2_answer3 text,
    scen2_reaction1 text,
    scen2_reaction2 text,
    scen2_reaction3 text,
    buddy_description nvarchar(255),
    posted_jobs_count int,
    collaborators_count int    
);

CREATE TABLE jobs (
    job_id int PRIMARY KEY,
    company_id int,
    photo nvarchar(255),
    job_title nvarchar(255) NOT NULL,
    industry_field nvarchar(255) NOT NULL,
    job_type nvarchar(255) NOT NULL,
    experience_level nvarchar(255) NOT NULL,
    start_date date,
    end_date date,
    job_description text NOT NULL,
    job_location nvarchar(255) NOT NULL,
    available_positions int NOT NULL,
    posted_at datetime, 
    salary int,
    amount_payed int,
    deadline date,
    FOREIGN KEY (company_id) REFERENCES companies(company_id) ON DELETE CASCADE
);

CREATE TABLE skills (
    skill_id int PRIMARY KEY IDENTITY,
    skill_name nvarchar(255) NOT NULL
);

CREATE TABLE job_skills (
    skill_id int,
    job_id int,
    PRIMARY KEY (skill_id , job_id),
    required_percentage int NOT NULL,
    FOREIGN KEY (job_id) REFERENCES jobs(job_id) ON DELETE CASCADE,
    FOREIGN KEY (skill_id) REFERENCES skills(skill_id) ON DELETE CASCADE
);

CREATE TABLE applicants (
    applicant_id int PRIMARY KEY,
    job_id int,
    cv_file_url nvarchar(500),
    app_test_grade decimal(5,2),
    cv_grade decimal(5,2),
    company_test_grade decimal(5,2),
    interview_grade decimal(5,2),
    application_status nvarchar(50),
    recommended_from_company_id int,
    applied_at datetime,
    user_id int NOT NULL,
    FOREIGN KEY (job_id) REFERENCES jobs(job_id) ON DELETE CASCADE,
    FOREIGN KEY (recommended_from_company_id) REFERENCES companies(company_id),
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE
);

CREATE TABLE events (
    event_id int PRIMARY KEY,
    host_company_id int NOT NULL,
    photo varchar(max),
    title varchar(200) not null,
    description text,
    start_date date not null,
    end_date date not null,
    location varchar(300) not null,
    posted_at datetime,
    FOREIGN KEY (host_company_id) REFERENCES companies(company_id) ON DELETE CASCADE
);

CREATE TABLE collaborators (
    event_id int NOT NULL,
    company_id int NOT NULL,
    PRIMARY KEY (event_id, company_id),
    FOREIGN KEY (event_id) REFERENCES events(event_id) ON DELETE CASCADE,
    FOREIGN KEY (company_id) REFERENCES companies(company_id)
);

CREATE TABLE Tests (
    id INT IDENTITY(1,1) PRIMARY KEY,
    title NVARCHAR(255) NULL,
    category NVARCHAR(255) NULL,
    created_at DATETIME2 NOT NULL
);

CREATE TABLE Recruiters (
    company_id INT PRIMARY KEY,
    name NVARCHAR(255) NULL
);

CREATE TABLE Questions (
    id INT IDENTITY(1,1) PRIMARY KEY,
    position_id INT NULL,
    test_id INT NULL,
    question_text NVARCHAR(MAX) NULL,
    question_type_string NVARCHAR(50) NULL,
    question_score REAL NOT NULL,
    question_answer NVARCHAR(MAX) NULL,
    options_json NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Questions_Tests FOREIGN KEY (test_id) REFERENCES Tests(id) ON DELETE SET NULL
);

CREATE TABLE TestAttempts (
    id INT IDENTITY(1,1) PRIMARY KEY,
    test_id INT NOT NULL,
    external_user_id INT NULL,
    score DECIMAL(18,2) NULL,
    status NVARCHAR(50) NULL,
    started_at DATETIME2 NULL,
    completed_at DATETIME2 NULL,
    answers_file_path NVARCHAR(MAX) NULL,
    is_validated BIT NOT NULL DEFAULT 0,
    percentage_score DECIMAL(18,2) NULL,
    rejection_reason NVARCHAR(MAX) NULL,
    rejected_at DATETIME2 NULL,
    CONSTRAINT FK_TestAttempts_Tests FOREIGN KEY (test_id) REFERENCES Tests(id) ON DELETE CASCADE,
    CONSTRAINT FK_TestAttempts_Users FOREIGN KEY (external_user_id) REFERENCES Users(id) ON DELETE SET NULL
);

CREATE TABLE InterviewSessions (
    id INT IDENTITY(1,1) PRIMARY KEY,
    position_id INT NOT NULL,
    external_user_id INT NULL,
    interviewer_id INT NOT NULL,
    date_start DATETIME2 NOT NULL,
    video NVARCHAR(MAX) NULL,
    status NVARCHAR(50) NULL,
    score DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_InterviewSessions_Users FOREIGN KEY (external_user_id) REFERENCES Users(id) ON DELETE SET NULL
);

CREATE TABLE LeaderboardEntries (
    id INT IDENTITY(1,1) PRIMARY KEY,
    test_id INT NOT NULL,
    user_id INT NOT NULL,
    normalized_score DECIMAL(18,2) NOT NULL,
    rank_position INT NOT NULL,
    tie_break_priority INT NOT NULL,
    last_recalculation_at DATETIME2 NOT NULL,
    CONSTRAINT FK_Leaderboard_Tests FOREIGN KEY (test_id) REFERENCES Tests(id) ON DELETE CASCADE,
    CONSTRAINT FK_Leaderboard_Users FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE
);

CREATE TABLE Slots (
    id INT IDENTITY(1,1) PRIMARY KEY,
    recruiter_id INT NOT NULL,
    start_time DATETIME2 NOT NULL,
    end_time DATETIME2 NOT NULL,
    duration INT NOT NULL DEFAULT 30,
    status INT NOT NULL DEFAULT 0,
    interview_type NVARCHAR(255) NULL,
    CONSTRAINT FK_Slots_Recruiters FOREIGN KEY (recruiter_id) REFERENCES Recruiters(company_id) ON DELETE CASCADE,
    candidate_id INT NULL,
    CONSTRAINT FK_Slots_Users FOREIGN KEY (candidate_id) REFERENCES Users(id)
);

CREATE TABLE Answers (
    id INT IDENTITY(1,1) PRIMARY KEY,
    attempt_id INT NOT NULL,
    question_id INT NOT NULL,
    value NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Answers_TestAttempts FOREIGN KEY (attempt_id) REFERENCES TestAttempts(id) ON DELETE CASCADE,
    CONSTRAINT FK_Answers_Questions FOREIGN KEY (question_id) REFERENCES Questions(id) ON DELETE CASCADE
);
GO

INSERT INTO Users (name, email, cv_xml) VALUES 
('Alice Johnson', 'alice@example.com', NULL),
('Bob Smith', 'bob@example.com', NULL),
('Carol Williams', 'carol@example.com', NULL),
('Dan Ionescu', 'dan@example.com', NULL),
('Elena Popescu', 'elena@example.com', NULL),
('Alice Smith', 'alice.smith@email.com', '<cv><summary>Experienced C# Developer</summary><skills><skill>C#</skill><skill>SQL</skill></skills></cv>'),
('Bob Jones', 'bob.jones@email.com', '<cv><summary>Data Enthusiast</summary><skills><skill>Python</skill><skill>SQL</skill></skills></cv>');

SET IDENTITY_INSERT Tests ON;
INSERT INTO Tests (id, title, category, created_at) VALUES 
(1, 'C# Fundamentals', 'Programming', '2026-01-10T09:00:00Z'),
(2, 'SQL Basics', 'Database', '2026-02-05T10:00:00Z'),
(3, 'OOP Principles', 'Programming', '2026-03-01T09:00:00Z'),
(4, 'Data Structures', 'Computer Science', '2026-03-10T09:00:00Z'),
(5, 'Database Design', 'Database', '2026-03-15T09:00:00Z');
SET IDENTITY_INSERT Tests OFF;

INSERT INTO Recruiters (company_id, name) VALUES
(1, 'Google'),
(2, 'Amazon');

INSERT INTO Questions (position_id, test_id, question_text, question_type_string, question_score, question_answer, options_json) VALUES 
(1, NULL, 'Tell us about your favourite project.', 'INTERVIEW', 4.0, NULL, NULL),
(1, NULL, 'How do you handle conflict in a team?', 'INTERVIEW', 4.0, NULL, NULL),
(2, NULL, 'Where do you see yourself in 5 years?', 'INTERVIEW', 4.0, NULL, NULL),
(NULL, 1, 'C# is a statically typed language.', 'TRUE_FALSE', 4.0, 'true', NULL),
(NULL, 1, 'In C#, a string is a value type.', 'TRUE_FALSE', 4.0, 'false', NULL),
(NULL, 1, 'C# supports multiple inheritance through classes.', 'TRUE_FALSE', 4.0, 'false', NULL),
(NULL, 1, 'The var keyword in C# means the variable is dynamic.', 'TRUE_FALSE', 4.0, 'false', NULL),
(NULL, 1, 'In C#, int is an alias for System.Int32.', 'TRUE_FALSE', 4.0, 'true', NULL),
(NULL, 2, 'What SQL keyword is used to retrieve data from a table?', 'TEXT', 4.0, 'SELECT', NULL),
(NULL, 2, 'What SQL clause is used to filter rows?', 'TEXT', 4.0, 'WHERE', NULL),
(NULL, 2, 'What SQL clause filters rows after grouping?', 'TEXT', 4.0, 'HAVING', NULL),
(NULL, 2, 'What SQL keyword is used to sort results?', 'TEXT', 4.0, 'ORDER BY', NULL),
(NULL, 2, 'What SQL keyword groups rows with the same values?', 'TEXT', 4.0, 'GROUP BY', NULL),
(NULL, 3, 'Which OOP principle hides internal implementation details?', 'SINGLE_CHOICE', 4.0, '1', '["Inheritance","Encapsulation","Polymorphism","Abstraction","Composition","Delegation"]'),
(NULL, 3, 'Which OOP principle allows a class to inherit from another?', 'SINGLE_CHOICE', 4.0, '0', '["Inheritance","Encapsulation","Polymorphism","Abstraction","Composition","Delegation"]'),
(NULL, 3, 'Which OOP principle allows objects to take multiple forms?', 'SINGLE_CHOICE', 4.0, '2', '["Inheritance","Encapsulation","Polymorphism","Abstraction","Composition","Delegation"]'),
(NULL, 4, 'Which of the following are linear data structures?', 'MULTIPLE_CHOICE', 4.0, '[0,1]', '["Array","Linked List","Tree","Graph","Heap","Trie"]'),
(NULL, 4, 'Which of the following use LIFO ordering?', 'MULTIPLE_CHOICE', 4.0, '[1,3]', '["Queue","Stack","Deque","Call Stack","Priority Queue","Circular Buffer"]'),
(NULL, 4, 'Which data structures allow duplicate values?', 'MULTIPLE_CHOICE', 4.0, '[0,2]', '["List","Set","Bag","Map","HashSet","TreeSet"]'),
(NULL, 5, 'A primary key can contain NULL values.', 'TRUE_FALSE', 4.0, 'false', NULL),
(NULL, 5, 'A foreign key references the primary key of another table.', 'TRUE_FALSE', 4.0, 'true', NULL),
(NULL, 5, 'Second normal form eliminates partial dependencies.', 'TRUE_FALSE', 4.0, 'true', NULL);

INSERT INTO TestAttempts (test_id, external_user_id, score, status, started_at, completed_at, answers_file_path, is_validated, percentage_score) VALUES 
(1, 2, 25.0, 'COMPLETED', '2026-03-01T10:00:00Z', '2026-03-01T10:45:00Z', 'answers/attempt_1.json', 1, 25.0),
(2, 2, 18.0, 'COMPLETED', '2026-03-05T14:00:00Z', '2026-03-05T14:30:00Z', 'answers/attempt_2.json', 1, 18.0),
(3, 3, 40.0, 'COMPLETED', '2026-03-10T09:00:00Z', '2026-03-10T09:28:00Z', 'answers/attempt_3.json', 1, 40.0),
(5, 4, 35.0, 'COMPLETED', '2026-03-12T11:00:00Z', '2026-03-12T11:25:00Z', 'answers/attempt_4.json', 1, 35.0),
(5, 5, 50.0, 'COMPLETED', '2026-03-15T13:00:00Z', '2026-03-15T13:29:00Z', 'answers/attempt_5.json', 1, 50.0);

INSERT INTO Answers (attempt_id, question_id, value) VALUES 
(1, 4, 'true'),
(1, 5, 'false'),
(2, 9, 'SELECT'),
(2, 10, 'WHERE');

INSERT INTO InterviewSessions (position_id, external_user_id, interviewer_id, date_start, video, status, score) VALUES 
(1, 1, 2, '2025-04-01T10:00:00Z', 'recordings/session_1.mp4', 'Completed', 8.5),
(2, 3, 2, '2025-04-15T14:00:00Z', '', 'Scheduled', 0.0),
(1, 2, 3, '2025-04-20T11:00:00Z', 'recordings/session_3.mp4', 'Completed', 7.0);

INSERT INTO skills (skill_name) VALUES
('C#'),
('SQL'),
('React'),
('Python'),
('Azure');

INSERT INTO companies (
    company_id, company_name, about_us, profile_picture_url, logo_picture_url, 
    location, email, buddy_name, avatar_id, final_quote, 
    scen_1_text, scen1_answer1, scen1_answer2, scen1_answer3, 
    scen1_reaction1, scen1_reaction2, scen1_reaction3, 
    scen2_text, scen2_answer1, scen2_answer2, scen2_answer3, 
    scen2_reaction1, scen2_reaction2, scen2_reaction3, 
    buddy_description, posted_jobs_count, collaborators_count
) VALUES
(
    1, 'TechNova', 'We build scalable web applications.', 'technova_profile.jpg', 'technova_logo.png', 
    'San Francisco, CA', 'hr@technova.com', 'NovaBot', 1, 'Keep innovating!', 
    'You found a critical bug in production.', 'Ignore it', 'Fix it immediately', 'Report to QA', 
    'Poor choice.', 'Excellent initiative.', 'Good protocol.', 
    'A client is unhappy with a feature.', 'Apologize', 'Blame the dev team', 'Schedule a review call', 
    'Polite but passive.', 'Unprofessional.', 'Great problem-solving approach.', 
    'Friendly technical assistant', 1, 1
),
(
    2, 'DataFlow Inc', 'Pioneering data analytics.', 'dataflow_profile.jpg', 'dataflow_logo.png', 
    'New York, NY', 'careers@dataflow.com', 'DataDan', 2, 'Trust the data.', 
    'A dataset is missing values.', 'Delete the rows', 'Impute the data', 'Leave them blank', 
    'Destructive approach.', 'Standard industry practice.', 'Will cause errors later.', 
    'The database server crashed.', 'Wait for it to fix itself', 'Reboot server', 'Alert DevOps', 
    'Too passive.', 'Could cause data loss.', 'Safest and most professional action.', 
    'Analytical companion', 1, 1
),
(
    3, 'EcoCode', 'Building sustainable and green software solutions.', 'ecocode_profile.jpg', 'ecocode_logo.png', 
    'Seattle, WA', 'hello@ecocode.com', 'Leafy', 3, 'Code for the planet!', 
    'You notice a memory leak in the background process.', 'Ignore it', 'Log it for next sprint', 'Fix it immediately', 
    'Irresponsible.', 'Acceptable, but risky.', 'Excellent initiative.', 
    'The client wants a feature delivered faster than possible.', 'Agree to rush it', 'Explain the tradeoffs', 'Work the weekend', 
    'Will lead to bugs.', 'Perfect communication.', 'Not sustainable long-term.', 
    'Eco-friendly assistant', 0, 2
),
(
    4, 'FinEdge', 'High-security financial trading platforms.', 'finedge_profile.jpg', 'finedge_logo.png', 
    'London, UK', 'hr@finedge.com', 'VaultBot', 4, 'Security is not a feature, it is the foundation.', 
    'A minor security vulnerability is found in an old module.', 'Hide it', 'Patch it next week', 'Patch it immediately', 
    'Illegal and unethical.', 'Too slow for finance.', 'The only correct answer.', 
    'The development team is falling behind schedule.', 'Yell at them', 'Reassess the sprint goals', 'Ignore the delay', 
    'Toxic leadership.', 'Excellent agile management.', 'Negligent.', 
    'Strict professional companion', 0, 1
);

INSERT INTO jobs (
    job_id, company_id, photo, job_title, industry_field, job_type, 
    experience_level, start_date, end_date, job_description, 
    job_location, available_positions, posted_at, salary, amount_payed, deadline
) VALUES
(
    101, 1, 'backend_job.jpg', 'Backend C# Developer', 'IT', 'Full-time', 
    'Mid-Level', '2026-06-01', NULL, 'Develop robust REST APIs using .NET Core.', 
    'Remote', 3, '2026-04-15 09:00:00', 95000, 0, '2026-05-15'
),
(
    102, 2, 'data_job.jpg', 'Data Engineer', 'Data Science', 'Contract', 
    'Senior', '2026-07-01', '2027-07-01', 'Maintain cloud data pipelines and warehouses.', 
    'New York, NY', 1, '2026-04-18 10:30:00', 120000, 0, '2026-06-01'
);

INSERT INTO job_skills (skill_id, job_id, required_percentage) VALUES
(1, 101, 90),
(2, 101, 60),
(5, 101, 40),
(2, 102, 95),
(4, 102, 85);

INSERT INTO applicants (
    applicant_id, job_id, cv_file_url, app_test_grade, cv_grade, 
    company_test_grade, interview_grade, application_status, 
    recommended_from_company_id, applied_at, user_id
) VALUES
(
    501, 101, 'alice_smith_cv.pdf', 8.50, 9.20, 8.00, 9.50, 
    'Accepted', NULL, '2026-04-19 14:00:00', 6
),
(
    502, 102, 'bob_jones_cv.pdf', 7.00, 8.00, 7.50, 6.50, 
    'Rejected', 1, '2026-04-20 09:15:00', 7
);

INSERT INTO events (
    event_id, host_company_id, photo, title, description, 
    start_date, end_date, location, posted_at
) VALUES
(
    201, 1, 'hackathon.jpg', 'TechNova Spring Hackathon', 
    'Join us for 48 hours of intense coding and problem solving.', 
    '2026-05-10', '2026-05-12', 'San Francisco HQ', '2026-04-10 08:00:00'
),
(
    202, 2, 'summit.jpg', 'Data Summit 2026', 
    'Exploring the future of big data, AI, and machine learning.', 
    '2026-08-20', '2026-08-21', 'New York Convention Center', '2026-04-12 11:00:00'
),
(
    203, 3, 'winter_summit_2026.jpg', 'Winter Web Summit 2026', 
    'Our annual kickoff exploring sustainable tech. Thank you to everyone who attended!', 
    '2026-01-15', '2026-01-17', 'Seattle Convention Center', '2025-11-01 10:00:00'
),
(
    204, 4, 'fintech_panel.jpg', 'The Future of FinTech Security', 
    'A live panel discussion on securing high-frequency trading platforms against modern threats.', 
    '2026-05-05', '2026-05-05', 'Virtual', '2026-04-01 09:00:00'
),
(
    205, 1, 'cloud_workshop_2025.jpg', 'TechNova Cloud Architecture Workshop', 
    'A hands-on deep dive into building scalable cloud solutions using .NET and Azure.', 
    '2025-10-10', '2025-10-11', 'TechNova HQ, San Francisco', '2025-08-01 09:00:00'
),
(
    206, 1, 'opensource_summit_2026.jpg', 'TechNova Open Source Summit 2026', 
    'Join our core developers as we contribute to major open-source projects for the weekend.', 
    '2026-09-05', '2026-09-06', 'Virtual', '2026-04-10 12:00:00'
);

INSERT INTO collaborators (event_id, company_id) VALUES
(201, 2),
(202, 1),
(203, 1),
(203, 4),
(204, 2),
(205, 2),
(206, 3);