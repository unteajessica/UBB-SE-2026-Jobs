# Architecture

The **Tests and Interviews API** is a .NET 10 backend service that manages technical assessments, job postings, and interview coordination. It follows a layered architecture pattern that separates concerns across multiple layers:

```
HTTP Request → API Controller → Service → Repository → Database
```

## Layers Overview

### 1. **API Controller Layer** (`/Controllers`)
- **Responsibility**: Handle HTTP requests and route them to appropriate services
- **Domain Examples**: 
  - `UsersController` - User authentication and profile management
  - `TestsController` - Test/assessment management
  - `JobsController` - Job posting management
  - `SlotsController` - Interview slot scheduling
  - `ApplicantsController` - Job applicant tracking
  - `CompaniesController` - Company information and management
  - `CollaboratorsController` - Collaboration management
  - `TestAttemptsController` - Test attempt tracking
  - `QuestionsController` - Question management
  - `AnswersController` - Answer submission and evaluation
  - `LeaderboardController` - Ranking and leaderboard data
  - `PaymentController` - Payment processing
- **Role**: Accept client requests, invoke service methods, validate input, return HTTP responses

### 2. **Service Layer** (`/Services`)
- **Responsibility**: Implement business logic specific to the domain
- **Examples**:
  - `JobsService` - Job posting operations, skill matching
  - `SlotService` - Slot availability and scheduling logic
  - `CollaboratorsService` - Collaboration workflows
- **Interfaces**: Defined in `/Services/Interfaces` (e.g., `IJobsService`)
- **Role**: 
  - Process assessment data
  - Validate business rules (e.g., required skills for jobs)
  - Orchestrate complex workflows (test creation → submission → evaluation)
  - Coordinate multiple repositories

### 3. **Repository Layer** (`/Repositories`)
- **Responsibility**: Abstract database access operations
- **Examples**: `SlotRepository` - Data access for interview slots
- **Role**: 
  - Execute SQL queries
  - Handle data persistence and retrieval
  - Manage transactions

### 4. **Database**
- **Responsibility**: Store and manage persistent data for users, tests, jobs, slots, applicants, etc.

## Data Flow Example: Submitting a Test

1. Client sends POST request to `TestAttemptsController` with test answers
2. **Controller** validates the request and calls `TestService.SubmitTestAttempt()`
3. **Service** evaluates answers, calculates score, applies business logic
4. **Service** calls `TestAttemptRepository.SaveAttempt()` and `LeaderboardRepository.UpdateRanking()`
5. **Repository** persists data to the **Database**
6. Response flows back through layers to the client

## Domain Areas

- **Assessment Management**: Tests, Questions, Answers, Test Attempts, Evaluation
- **Job Management**: Job Postings, Skills, Requirements, Applicants
- **Interview Coordination**: Slots, Scheduling, Collaborators
- **User Management**: Users, Profiles, Leaderboards, Payments
- **Analytics**: Leaderboard tracking, Performance metrics

## Benefits

- **Separation of Concerns**: Each layer handles specific responsibilities
- **Testability**: Services can be unit tested with mocked repositories
- **Maintainability**: Changes in database schema only affect repository layer
- **Scalability**: Services can be easily extended for new features
- **Reusability**: Multiple controllers can use the same service logic
