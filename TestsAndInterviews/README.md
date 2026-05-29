# Setup

### SQL Server

1. Create a new Database in MSSM
2. Copy the connection string from MSSM
3. Add "Initial Catalog=TestsAndInterviews;" to the connection string
4. In the project in the Env.cs class change the connection string
5. In MSSM run the sql script in the SQL folder from the project

---

# Roles

## Roles overall:

Candidate, Recruiter, Admin

## Roles in detail:

### 1. Candidate

What can the candidate view? - their own profile and the results of their tests and interviews - the profile of the companies and their job offers - the tests available and the leaderboard - the companies that have accepted their application to a job offer
What can the candidate manage/edit? - edit their profile and upload their CV - apply to job offers - take tests and interviews - programming an interview slot with a recruiter - recording and sending a video interview to a recruiter

### 2. Recruiter

What can the recruiter view? - their company profile - the pogress bar of their profile completion and the remaining tasks to complete their profile - top 3 most required skills on the platform - current job offers and expired job offers - the list of candidates that applied to their now expired job offers and the details about their appliance process - list with current and past events, and a list with their collaborators for each event (other companies) - the profile of other companies and their job offers
What can the recruiter manage/edit? - edit their company profile and the game - CRUD operations on their job offers - reposting old job offers - evaluating candidates that applied to their job offers (introducing the grades for the interview, the final decision and the possible recommendation to other collaborators) - creating events and inviting other companies to collaborate in those events

### 3. Admin

What can the admin view? - everything a candidate and a recruiter can view - full leaderboard with all the candidates and their etailed scores - system logs regarding failed logins, failed tests and interviews, and any other relevant information regarding the platform
What can the admin manage/edit? - delete or suspend accounts of candidates and recruiters in case of misconduct

### How to use role guards in the project:

policy variant

```
[Authorize(Policy = "RecruiterOrAdmin")]
public IActionResult CreateJob() { }
```

role variant

```
[Authorize(Roles = "Recruiter,Admin")]
public IActionResult CreateJob() { }
```

add these to the methods in the controllers

### What you need to check before running

1. That the file path in InterviewSessionService in api is a valid path on your machine. General one: "C:\Users\Administrator\Documents"
2. That the database connection string is the one from your machine
