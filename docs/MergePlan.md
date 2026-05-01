# Merge Plan — PussyCatsApp + matchmaking

Working document for the unified team. The goal is one solution, one database, one window, accessed through one ASP.NET Core Web API. The architecture follows the structure agreed with the lab — four projects, with `IRepository` as the seam between the App, the API, and the tests.

## 1. Source assessment

**PussyCatsApp (candidate side)**
- Strengths: rich `UserProfile` model, CV parsing pipeline (XML/JSON), personality test, skill tests, profile completeness service, role compatibility via weighted `SkillGroup`.
- Weaknesses: flatter folder layout, some business logic leaks into views, MSTest+Moq, mixed raw SQL + in-memory repos, local file storage.

**matchmaking (company side)**
- Strengths: cleaner layering (`Domain.Entities` / `Services` / `Repositories` / `DTOs`), proper interface-per-service, xUnit + FluentAssertions, `SessionContext`, `MatchStatus` state machine in `MatchService`.
- Weaknesses: thin `User` (overlaps with `UserProfile`), raw SQL repos via `SqlRepositoryBase`, in-memory `JobRepository` with hardcoded seeds, an unfinished `Developer`/`Interaction` feed of unclear scope.

**Conflicts to resolve up front**
- Two `User`-shaped entities (`matchmaking.Domain.Entities.User` vs `PussyCatsApp.Models.UserProfile`)
- Two `UserProfileViewModel` classes
- Two skill concepts (`SkillGroup` weights vs per-user `Skill` with score)
- Two test frameworks (MSTest vs xUnit) — pick one
- Different namespace casing (`matchmaking.*` lowercase vs `PussyCatsApp.*` PascalCase)

## 2. Target solution structure

Four projects, matching the lab's reference architecture:

```
UBB-SE-2026-XXX.sln
├── PussyCats.App/        // WinUI 3. Views + ViewModels + Services + RepositoryProxies.
├── PussyCats.Library/    // Class library. Domain entities + IRepository interfaces +
│                         //   EF Core Repository implementations + DbContext + migrations.
├── PussyCats.Api/        // ASP.NET Core Web API. Controllers that use IRepository directly.
└── PussyCats.Tests/      // xUnit. Tests against Services using FakeRepository implementations.
```

**The `IRepository` seam — this is what makes the architecture work**

For each aggregate (User, Job, Match, Skill, Company, Document, ...) there is one interface, three implementations:

| Implementation | Project | What it does | Used by |
|---|---|---|---|
| `XRepository` | Library | Real EF Core, hits the database | API controllers |
| `XRepositoryProxy` | App | HTTP client, calls the API | App services |
| `FakeXRepository` | Tests | In-memory, deterministic | Test methods |

The `Service` code is identical regardless of which one is injected. That gives us:
- App talks to DB only through the API (the proxy makes the HTTP call) ✓
- API talks to DB directly (no extra hop) ✓
- Tests run with no network and no database ✓

**Inside the App project**, organize code in folders, not separate csproj files:

```
PussyCats.App/
├── Views/              // .xaml + .xaml.cs (one folder per area: Profile, Jobs, Matching, ...)
├── ViewModels/         // *ViewModel.cs
├── Models/             // ProductModel-style binding wrappers if needed (often you don't)
├── Services/           // IUserService, UserService, IMatchService, MatchService, ...
├── RepositoryProxies/  // UserRepositoryProxy, JobRepositoryProxy, ...
├── Configuration/      // ApiConfiguration (base URL, timeouts), DI registration
└── App.xaml.cs         // Single MainWindow, DI container setup
```

**Inside the Library project**:

```
PussyCats.Library/
├── Domain/             // Entities (User, Job, Match, ...), enums, value objects
├── DTOs/               // Cross-boundary DTOs (UserApplicationResult, CompatibilityBreakdown)
├── Repositories/       // IRepository interfaces + concrete EF implementations side by side
│                       //   per aggregate folder, e.g. Repositories/Users/{IUserRepository,
│                       //   UserRepository}.cs
├── Persistence/        // PussyCatsDbContext, IEntityTypeConfiguration<T> classes
└── Migrations/         // EF Core generated
```

**Inside the API project**:

```
PussyCats.Api/
├── Controllers/        // One per aggregate. Inject IXRepository, return DTOs.
├── Files/              // FilesController + filesystem storage helper
├── appsettings.json    // Connection string, file storage path
└── Program.cs          // DI: register DbContext + concrete EF Repositories
```

**Inside the Tests project**:

```
PussyCats.Tests/
├── Services/           // Service tests using fake repositories
├── ViewModels/         // VM tests using fake services
├── Fakes/              // FakeUserRepository, FakeJobRepository, ...
└── Api/                // Optional: integration tests with WebApplicationFactory + EF in-memory
```

**Critical project reference rules** (enforced via `.csproj`):

- `App` references `Library` only (never `Api`).
- `Api` references `Library` only.
- `Tests` references `Library` and `App` (to test their classes).
- `Library` references nothing from us.

Because both `App` and `Api` reference `Library`, the App *can technically* see the EF `Repository` class. The discipline is: **the App's DI container only ever registers `RepositoryProxy` implementations, never the EF `Repository` ones.** Code-review enforced. Optionally, add a startup assertion in App that resolves each `IXRepository` and confirms the concrete type name ends with `Proxy`.

## 3. Domain unification

**`User` (the merged entity)** — keep PussyCats's rich shape, fold in matchmaking's preference fields:

```csharp
public class User
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public int Age { get; set; }
    public string Gender { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Country { get; set; } = "";
    public string City { get; set; } = "";
    public string University { get; set; } = "";
    public string Degree { get; set; } = "";
    public int ExpectedGraduationYear { get; set; }
    public string GitHub { get; set; } = "";
    public string LinkedIn { get; set; } = "";
    public string Address { get; set; } = "";
    public string Motivation { get; set; } = "";
    public bool HasDisabilities { get; set; }
    public string ProfilePicturePath { get; set; } = "";   // file lives on API server
    public string PreferredEmploymentType { get; set; } = "";
    public string WorkModePreference { get; set; } = "";
    public string LocationPreference { get; set; } = "";
    public int YearsOfExperience { get; set; }
    public bool ActiveAccount { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }

    // Navigation properties — assignment requires this, not "IdOfX"
    public List<WorkExperience> WorkExperiences { get; set; } = new();
    public List<Project> Projects { get; set; } = new();
    public List<ExtraCurricularActivity> ExtraCurricularActivities { get; set; } = new();
    public List<UserSkill> Skills { get; set; } = new();
    public List<Match> Matches { get; set; } = new();
    public PersonalityTestResult? PersonalityResult { get; set; }
    public UserLevel UserLevel { get; set; } = new();
}
```

**Skills consolidation** — three tables, one separate concept:
- `Skill` — catalog row (`SkillId`, `Name`, `Category`). Seed from PussyCatsApp's hardcoded list.
- `UserSkill` — `(User, Skill, Score)`. Score comes from skill tests.
- `JobSkill` — `(Job, Skill, RequiredLevel)`. Drives matching.
- `SkillGroup` — keep separate. Used only by `CompatibilityService` for role recommendations (groups + weight per `JobRole`). Don't merge with `Skill`.

**`Match`** — keep matchmaking's version with `MatchStatus` enum and the state machine in `MatchService`. Replace bare `UserId`/`JobId` ints with `User`/`Job` navigation properties (FK shadow properties stay for EF).

**`Job`, `Company`** — keep matchmaking's. Add nav property `Company.Jobs`.

**Drop**
- Duplicate `UserProfile`/`User` definitions
- Raw SQL repositories on both sides
- `SqlRepositoryBase`, all `Sql*Repository` classes
- `DocumentRepository` direct DB access — replace with API endpoint + EF entity
- Hardcoded duplicate job/company tables

**Defer (mock for now)** — `Developer`/`Interaction`/`Post` from matchmaking. If those belong to a different team's "feed" half, keep mocks. **Decide this in the first technical meeting.**

## 4. Database (EF Core, code-first)

Lives in `PussyCats.Library`. One `DbSet<T>` per aggregate root in `PussyCatsDbContext`. Per-entity configuration files under `Persistence/Configurations/` using `IEntityTypeConfiguration<T>`.

**Steps**
1. In Library: `dotnet add package Microsoft.EntityFrameworkCore.SqlServer`
2. In Api: `dotnet add package Microsoft.EntityFrameworkCore.Design`
3. Define entities + configurations (FKs, navigation props, indexes).
4. `dotnet ef migrations add InitialCreate --project src/PussyCats.Library --startup-project src/PussyCats.Api`
5. `dotnet ef database update --project src/PussyCats.Library --startup-project src/PussyCats.Api`
6. Seed via `HasData()` in configurations: skill catalog, sample jobs, sample companies. Personality test questions stay in code (configuration, not data).

**Connection string** lives in `PussyCats.Api/appsettings.json` and `appsettings.Development.json`. **Never** in the App.

**Files** — profile pictures, parsed CVs, exported PDFs go under a `files/` directory on the API server, served behind `/api/files/{id}`. Path stored on the entity, binary on disk.

## 5. Web API

Thin controllers — one per aggregate. Each method delegates to the injected `IXRepository` (or to a domain service when there's logic, like `MatchService.SubmitDecision`).

```
GET    /api/users/{id}
PUT    /api/users/{id}
POST   /api/users/{id}/skills
DELETE /api/users/{id}/skills/{skillId}
POST   /api/users/{id}/cv               (multipart upload, returns parsed User)
GET    /api/users/{id}/compatibility
POST   /api/users/{id}/personality-test
GET    /api/users/{id}/matches

GET    /api/jobs?location=&type=
GET    /api/jobs/{id}
POST   /api/jobs
GET    /api/jobs/{id}/applicants

POST   /api/matches                     (apply to job)
GET    /api/matches/{id}
PATCH  /api/matches/{id}/decision       (accept/reject/advance)
DELETE /api/matches/{id}

GET    /api/companies/{id}
GET    /api/companies/{id}/applicants
GET    /api/companies/{id}/recommendations

POST   /api/files                       (upload, returns id)
GET    /api/files/{id}
```

**Conventions**
- JSON, default System.Text.Json camelCase.
- Errors as RFC 7807 problem details.
- Validation via DataAnnotations + `ModelState.IsValid`.
- No auth this sprint. Pass `X-User-Id` from the App's `SessionContext` for now.

**Program.cs DI registrations** in API:
```csharp
builder.Services.AddDbContext<PussyCatsDbContext>(opts => opts.UseSqlServer(connStr));
builder.Services.AddScoped<IUserRepository, UserRepository>();      // EF
builder.Services.AddScoped<IJobRepository, JobRepository>();        // EF
builder.Services.AddScoped<IMatchRepository, MatchRepository>();    // EF
// ... one per aggregate
```

## 6. App project DI

In `App.xaml.cs`:
```csharp
services.AddSingleton<ApiConfiguration>(_ => ApiConfigurationLoader.Load());
services.AddHttpClient();

// Proxy implementations of IRepository — same interfaces as the EF ones, different impls
services.AddScoped<IUserRepository, UserRepositoryProxy>();
services.AddScoped<IJobRepository, JobRepositoryProxy>();
services.AddScoped<IMatchRepository, MatchRepositoryProxy>();
// ... one per aggregate

// Services depend on IXRepository and don't know it's a proxy
services.AddScoped<IUserService, UserService>();
services.AddScoped<IJobService, JobService>();
services.AddScoped<IMatchService, MatchService>();

// View models
services.AddTransient<UserProfileViewModel>();
// ...
```

A typical proxy:
```csharp
public class JobRepositoryProxy : IJobRepository
{
    private readonly HttpClient http;
    public JobRepositoryProxy(HttpClient http, ApiConfiguration cfg)
    {
        this.http = http;
        this.http.BaseAddress = new Uri(cfg.ApiBaseUrl);
    }

    public async Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<Job>>("api/jobs", ct) ?? [];

    public async Task<Job?> GetByIdAsync(int jobId, CancellationToken ct = default)
        => await http.GetFromJsonAsync<Job?>($"api/jobs/{jobId}", ct);
    // ...
}
```

## 7. GUI unification (one window)

Single `MainWindow` with one `Frame`. WinUI's `NavigationView` as the shell. All navigation goes through `Frame.Navigate(typeof(...))`. No new windows.

**Left rail layout**

```
[Candidate]
  ├── My Profile
  ├── Edit Profile / CV
  ├── Skill Tests
  ├── Personality Test
  ├── Compatibility
  ├── Browse Jobs
  └── My Applications

[Company]
  ├── Job Postings
  ├── Review Applicants
  └── Applicant Status

[Settings]
```

**Mode switch** — top-right `ToggleSwitch` for `Candidate` / `Company`. Hides irrelevant nav items; mode stored in `SessionContext`.

**Replace popups-as-windows with `ContentDialog`** wherever either project opened a secondary window.

## 8. Mocks: real vs still-mocked

The assignment rule: replace mocks of the *other team's* code with real calls; keep mocks for what's outside the merged scope.

| Boundary | Currently mocked? | Action after merge |
|---|---|---|
| matchmaking's `ITestingModuleAdapter` (test results) | Yes | Wire to real `PersonalityTestService` + `SkillTestService` |
| PussyCats compatibility scoring against real jobs | Hardcoded | Call real `IJobService` |
| matchmaking's `User` lookup | Thin User | Use real merged `User` via `IUserService` |
| Chat / Developer feed | Possibly the "other half" | Keep mocked unless explicitly in scope |
| External CV providers, LinkedIn import | External | Stay mocked |
| Email sending | External | Stay mocked |

When a mock stays, leave a comment: `// mock: belongs to <other half>, see MergePlan.md §8`.

## 9. Phased execution

Each phase ends with a green build and passing tests before the next starts.

**Phase 0 — Foundations (Day 1)**
- Create the GitHub repo `UBB-SE-2026-XXX`. Invite imre.zsigmond@ubbcluj.ro and the lab assistant.
- `.gitignore` from `dotnet new gitignore`.
- Both leads agree on: target structure (this doc), naming, test framework (xUnit + FluentAssertions — already used by matchmaking), StyleCop ruleset.
- Commit empty 4-project skeleton with references wired.
- Commit `CodingStyle.md` and `CodeReviewChecklist.md`.

**Phase 1 — Library: domain + interfaces (Day 2)**
- Move all entities into `PussyCats.Library/Domain/`. Unify `User`. Add navigation properties.
- Move enums (`MatchStatus`, `JobRole`, `TraitType`, ...).
- Move DTOs.
- Define every `IXRepository` interface (no implementations yet).

**Phase 2 — Library: EF Core (Day 3)**
- `PussyCatsDbContext` + `IEntityTypeConfiguration<T>` files in `Persistence/`.
- Implement EF `XRepository` classes.
- First migration. Verify schema.
- Seed data via `HasData()`.

**Phase 3 — API (Day 4)**
- Controllers per the API spec. Each ~20-50 lines.
- DI: register `DbContext` + EF repositories.
- Run locally on `https://localhost:7000`. Test with Swagger / `.http` file.
- File storage endpoints + `wwwroot/files/` directory.

**Phase 4 — App: services + proxies (Day 5)**
- Move services from both projects into `App/Services/`. Unify duplicates.
- Implement `XRepositoryProxy` classes in `App/RepositoryProxies/`.
- DI in `App.xaml.cs`: services + proxies (not EF repos).
- Verify the existing view models still build; their dependencies didn't change shape.

**Phase 5 — App: UI unification (Day 6-7)**
- Build `NavigationView` shell in `MainWindow`.
- Migrate every page to one Frame. Rip out secondary-window code.
- Mode toggle (Candidate / Company).
- Replace mocks where now-internal services are reachable.

**Phase 6 — Tests (Day 7)**
- `Fakes/` folder with `FakeXRepository` per aggregate.
- Port matchmaking xUnit tests as-is.
- Rewrite PussyCats MSTest tests as xUnit. Same logic, different attributes.
- Optional: integration tests using `WebApplicationFactory` + EF in-memory.

**Phase 7 — Polish (Day 8)**
- StyleCop pass.
- Coverage gaps.
- Manual end-to-end with both leads.
- Tag `v4.0`. Push API to server. Demo dry-run.

## 10. Risk register

| Risk | Mitigation |
|---|---|
| Different DI styles | Standardize on `Microsoft.Extensions.DependencyInjection` in `App.xaml.cs` |
| WinUI csproj merge conflicts | Start the App project fresh from the WinUI template, copy code in |
| EF can't model rich nested types cleanly | Make `WorkExperience`, `Project`, etc. separate entities with FK back to User |
| `Developer`/`Interaction` half is actually in scope | Clarify in lab; if so, add to Phase 1 |
| Test framework mismatch slows things down | Schedule explicit half-day for the rewrite, don't wing it |
| App accidentally registers the EF Repository instead of Proxy | Code review rule + a startup assert that `IXRepository` resolves to a `*Proxy` type in the App |
| App and API version drift during demo | Run both from same machine, same `git pull`. Document API base URL in `App/Configuration` |

## 11. Open questions for the first joint meeting

1. Which of us is team lead, which is technical lead?
2. Who owns the `Developer`/`Interaction`/`Post` feed — is it part of this app or "the other half"?
3. Where does the API get deployed? (Lab server, Azure free tier, local for now?)
4. Are we going to need auth before grading? If yes, plan for it now.
5. StyleCop ruleset: start strict and loosen, or start permissive and tighten?
6. Branch strategy: trunk-based with PRs, or feature branches?

## 12. Definition of done

- One Git repo, clients invited, no IDE-generated files committed.
- 4 projects build clean (`dotnet build` returns 0).
- All tests pass (`dotnet test`).
- One window, accesses every functionality from both original apps.
- One database, code-first EF Core, navigation properties (no bare `IdOfX`).
- App connects to DB only via the API.
- Tests use `FakeXRepository`, no DB or network.
- StyleCop configured per the technical lead's rules; warnings = build error.
- `CodingStyle.md` (15-30 rules) and `CodeReviewChecklist.md` in repo root.
- Team lead has a contributions report ready (git log, task assignments, chat history).
- Group event happened. Pics committed (or in a shared folder).
