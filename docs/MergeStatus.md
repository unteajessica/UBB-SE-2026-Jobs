# Merge Status

Last updated: 2026-05-07
Current phase: Phase 6 done (runs locally end-to-end). Phase 7 playbook below.

This document tracks where the merge stands right now. The architectural
plan is in `MergePlan.md` and is unchanged. This file records what's been
decided and built since the plan was written, plus what's left.

## TL;DR

- Repo: `UBB-SE-2026-921-1`. Database: `ISS-921-1` on `DESKTOP-M6HSOV2`,
  Windows auth.
- Build green across all four projects (`PussyCats.App`, `PussyCats.Library`,
  `PussyCats.Api`, `PussyCats.Tests`).
- Phases 0–6 done. App launches, navigates, and round-trips data through
  the API to SQL Server.
- Phase 7 (tests) not started. Test project currently empty (xUnit
  packaged, no test files yet).
- Phase 8 (polish) not started.

## Phase status

### Phase 0 — Skeleton (done, committed)

Four-project solution created with all references wired:
`App→Library`, `Api→Library`, `Tests→Library+App`, `Library→nothing`.
Solution uses legacy `.sln`, not `.slnx`, because WinUI platform
mappings need it. Tests retargeted to `net10.0-windows10.0.26100.0`
because referencing the WinUI App project requires it.

### Phase 1 — Domain + interfaces (done, committed)

`PussyCats.Library/Domain/` holds 19 entities and 9 enums. 13 DTOs in
`Library/DTOs/`. 13 `IRepository` interfaces in
`Library/Repositories/<aggregate>/`. All repository methods are
`async`, return `Task<T>`, and accept `CancellationToken ct = default`
as the last parameter.

Key domain decisions made:

- `User` is the merged rich shape from PussyCats's `UserProfile`,
  with matchmaking's preference fields folded in
  (`PreferredEmploymentType`, `WorkModePreference`,
  `LocationPreference`).
- `UserLevel` collapsed into two fields on `User`: `CurrentLevel`,
  `TotalExperiencePoints`. No separate entity.
- Skills are three tables: `Skill` (catalog), `UserSkill` (per-user
  with `Score`), `JobSkill` (per-job with `RequiredLevel`). Plus
  `SkillGroup` for role-compatibility scoring (separate concept from
  `Skill`, do not merge).
- `Match` keeps matchmaking's `MatchStatus` enum and the state
  machine. Navigation properties (`Match.User`, `Match.Job`)
  preferred over bare FKs per the assignment rule.
- `Developer`, `DeveloperInteraction`, `DeveloperPost`, `Chat`, and
  `Message` were added in Phase 8d for the app-local Chat and Developer
  screens. Persistence/API backing remains future hardening.

### Phase 2 — EF Core + initial migration (done, committed)

`PussyCatsDbContext` and per-entity `IEntityTypeConfiguration<T>`
classes under `Library/Persistence/Configurations/`. EF
`*Repository` implementations alongside the interfaces in
`Library/Repositories/<aggregate>/`.

`InitialCreate` migration applied to `ISS-921-1`. 18 tables, 18
foreign keys, 23 indexes, 365 seed rows.

Key persistence decisions:

- Cascade behavior: User-owned data cascades on User delete (Documents,
  WorkExperiences, Projects, ExtraCurricularActivities, SkillTests,
  PersonalityTestResults, Recommendations). `User → Match` and
  `Job → Match` are `Restrict` to preserve match history.
  `Skill → anything` is `Restrict` because the catalog is reference
  data. Verified column-by-column post-deploy.
- `Project.Technologies` stored as a single `nvarchar(max)` JSON
  column on the `Projects` table via EF Core 8 primitive collections.
  Not a separate junction table. **LINQ queries against this column
  must use `EF.Functions.JsonContains`, not `.Contains()`** —
  `.Contains()` won't translate.
- `SkillGroupSkills` join: `SkillGroup → Cascade`, `Skill → Restrict`.
  The Cascade-on-both-sides version was caught and fixed before
  apply.
- 152-skill catalog seeded, 48 SkillGroups with 153 group/skill
  memberships, 3 sample companies, 3 sample jobs, 6 JobSkill rows.
- Connection string lives in `PussyCats.Api/appsettings.Development.json`
  only. **Never in App.**

Connection string format that works with this setup:
Server=DESKTOP-M6HSOV2;Database=ISS-921-1;Trusted_Connection=True;TrustServerCertificate=True;

### Phase 3a — Port matchmaking services (done, committed)

`PussyCats.App/Services/` contains 12 service ports plus the algorithm
implementation. All are async, take `CancellationToken`, use
`ConfigureAwait(false)`, return `IReadOnlyList<T>` where applicable.

Services ported from `matchmaking/Services/`:

- `UserService`, `JobService`, `CompanyService`
- `UserSkillService` (renamed during port — was `SkillService` in
  matchmaking, conflated catalog and per-user; the merged version
  wires only to `IUserSkillRepository`)
- `JobSkillService`, `CooldownService`
- `MatchService` (state machine preserved verbatim — see
  `IsDecisionTransitionAllowed` and `SubmitDecisionAsync`)
- `CompanyStatusService`, `UserStatusService`, `SkillGapService`
- `CompanyRecommendationService`, `UserRecommendationService`

Interface added: `IRecommendationAlgorithm` (signatures only at the time).
Phase 3b ported the algorithm class from `matchmaking/algorithm/`; Phase
8d moved the interface contract to `PussyCats.Library/Services/`.

DTOs moved from `matchmaking/Models/` into `Library/DTOs/`:
`ApplicationCardModel`, `MissingSkillModel`, `UnderscoredSkillModel`,
`SkillGapSummaryModel`.

Key decisions during 3a:

- `IUserStatusMatchRepository` retired. Status filters
  (`m.Status == MatchStatus.Rejected`) live in service bodies, not
  repository methods, per CodingStyle §8.
- `IJobSkillRepository.GetAllAsync` added to support
  `JobSkillService.GetAllAsync`.
- `CompanyStatusService.ComputeCompatibilityFallback` substitutes
  `User.City` for the missing `User.Location`. Phase 8d normalizes
  the city portion of comma-separated job locations such as
  `Bucharest, Romania`.
- `CompanyRecommendationService` is stateful (queue + currentIndex).
  Comment at the top of the class flags that **DI registration must
  be Transient or per-view-model** in Phase 5 to avoid leaking
  applicants between users.
- `IRecommendationAlgorithm` was originally specified as an empty
  marker interface but has two real call sites in the recommendation
  services. It carries two real method signatures now. The matchmaking
  algorithm implementation is Phase 3b work.

Async patterns set in 3a (followed by all subsequent service ports):

- `AddAsync` returns `Task<T>` with the saved entity. Service surfaces
  pass it through. View models that don't need the result can
  ignore.
- Sync wrappers around async methods are forbidden (CodingStyle §11).
  Methods that touch I/O are async, end in `Async`, take a
  `CancellationToken`. Pure-computation methods stay sync.

### Phase 3b — Port PussyCatsApp services (in progress)

Subdivided into three sessions because the source is messier than
matchmaking and there are real design decisions to make. Each
sub-phase commits separately.

**3b.1 — design-call services (done, committed)**

Ported four PussyCatsApp services with real design calls plus extended
the existing matchmaking-side `MatchService`. Includes a Library schema migration
(`AddSelectedRoleAndJobRole`) that adds:

- `PersonalityTestResult.SelectedRole` (nullable `JobRole?`)
- `Job.JobRole` (non-nullable, with seed data updated for the three
  existing seed jobs)

Migration applied to `ISS-921-1`. `__EFMigrationsHistory` has both
`InitialCreate` and `AddSelectedRoleAndJobRole` rows.

Services in 3b.1:

- `PreferenceService` — facade on `IUserRepository`. The original
  `IPreferenceRepository` was retired (preferences are User fields
  now). View models bind to `IPreferenceService` and don't notice.
- `UserProfileService` — `SaveAsync(int, User)` is a facade that
  branches internally to `AddAsync` or `UpdateAsync` based on
  existence. Phase 4 controllers call `AddAsync`/`UpdateAsync`
  directly per HTTP verb semantics; only services use the facade.
- `PersonalityTestService` — `SaveResultAsync(int, IReadOnlyDictionary<Question, AnswerValue>, JobRole, ct)`
  computes scores AND persists structured `PersonalityTestResult` +
  `PersonalityTraitScore` rows. The original blob-string `Save` is
  replaced. The 24 hardcoded questions stay in the service file
  (`MergePlan §4`). Trait-score `int`/`double` mismatch handled with
  `Math.Round` cast; flagged in the source.
- `CompatibilityService` — adds `IUserRepository` dependency to
  read `user.ParsedCv` directly, replacing the original's
  `IUserSkillRepository.GetParsedCvByUserId` workaround that was a
  raw SQL leak.
- `MatchService` (existing 3a file) gains
  `GetMatchesForUserAsync` and `GetMatchStatisticsAsync`. The
  statistics method groups matches by `JobRole` enum. Constants
  preserved verbatim: `LastMonth = 1`, `LastSixMonths = 6`,
  `LastYear = 12`.

**3b.2 — mechanical ports + constants source-of-truth (done, pending commit)**

Six-step session. No DI registration, no commit until the end (Phase 3
commits are 3b.1, 3b.2, 3b.3 as three reviewable chunks). Source files
live under `PussyCatsApp/Services/`; targets land in
`PussyCats.App/Services/` unless noted. Universal porting rules from
3b.1 apply (sync→async + `CancellationToken`, `ConfigureAwait(false)`
in services, `IReadOnlyList<T>` returns, etc.).

*Step 1 — `SimpleModelOperations` (the constants source-of-truth).*
New file `PussyCats.App/Services/SimpleModelOperations.cs`. Static class.
Constants exposed as `public const int` (match source types — keep float
if the original was float):
- `GoldScoreThreshold = 90`, `SilverScoreThreshold = 70`,
  `BronzeScoreThreshold = 50`
- `GoldExperiencePoints = 100`, `SilverExperiencePoints = 60`,
  `BronzeExperiencePoints = 30`, `ParticipantExperiencePoints = 10`
- Level thresholds discovered during 3b.1: `Level2 = 100`, `Level3 = 250`,
  `Level4 = 500`, `Level5 = 800`

`AssignTier` static method ported verbatim. **SkillTestService and
UserProfileService reference these from `SimpleModelOperations`; they
do not redefine locally.**

*Step 2 — `SkillTestService` (+ `ISkillTestService`).*
Constructor: `ISkillTestRepository`. All methods sync→async +
`CancellationToken`:
- `GetTestsForUser` → `GetTestsForUserAsync`
- `CanRetakeTest` → `CanRetakeTestAsync` (loads `SkillTest` by id — I/O)
- `SubmitRetake` → `SubmitRetakeAsync`
- `GetSkillTestById` → `GetSkillTestByIdAsync`

Local constant: `RetakeEligibilityMonths = 3` stays on this class — it's a
skill-test rule, not a tier rule. The static `GetExperiencePoints` helper
ports but switches on `SimpleModelOperations.GoldScoreThreshold` etc.,
not local copies.

*Step 3 — `DocumentService` (+ `IDocumentService`).*
Constructor: `IDocumentRepository` **and** `ILocalFileStorageService`.
The file-storage dependency stays — DocumentService orchestrates metadata
persistence with file storage (a service-layer concern). Mechanical
sigdiffs:
- `GetDocumentsByUserId` → `repo.GetByUserIdAsync`
- `AddDocument(Document)` (was `void`) → `repo.AddAsync(Document, ct)`
  returns `Task<Document>`; service returns the inserted Document.
- `GetDocumentById` → `repo.GetByIdAsync`
- `DeleteDocument` → `repo.RemoveAsync`

`ILocalFileStorageService` ports in 3b.3 with upload methods stubbed to
`NotImplementedException`; DocumentService still compiles, Phase 5 wires
the real file routing.

*Step 4 — `UserLevelService`.*
If recon's "static helper, no repo" call is right, port verbatim as a
static class. If recon was wrong and it has a repo dependency, port like
the others (sync→async + `CancellationToken`). If `MatchService`'s
3b.1-added XP/level statistics overlap with `UserLevelService`: flag,
don't merge — Phase 7 polish.

*Step 5 — `PredefinedLocations`.*
New file `PussyCats.App/Configuration/PredefinedLocations.cs`:

```csharp
namespace PussyCats.App.Configuration;
public static class PredefinedLocations
{
    public static IReadOnlyList<string> All { get; } = new[] { /* ~80 cities */ };
}
```

City strings copied **verbatim** from `PussyCatsApp/Services/PreferenceService.LoadPredefinedLocations`.

*Step 6 — touch existing files.*
- `PussyCats.App/Services/UserProfileService.cs` — replace inlined
  `GetExperiencePoints` and `GetLevelFromXp` helpers with
  `SimpleModelOperations.X` references. Delete the now-unused private
  methods.
- `PussyCats.App/Services/PreferenceService.cs` — replace the
  `SearchLocationsAsync` empty-list stub with `OrdinalIgnoreCase`
  `Contains` against `PredefinedLocations.All`, wrapped in
  `Task.FromResult` (no actual I/O; interface stays async per rule 13's
  spirit; sync alternative is also defensible).

End of session: `dotnet build` (full output), list new + modified files,
list deviations. Do not touch `CVParsingService`, `CompletenessService`,
`ImageStorageService`, `LocalFileStorageService`, `PdfExportService` —
those are 3b.3.

**3b.3 — pure helpers (done, pending commit)**

Five PussyCatsApp services with no `IRepository` dependencies, all ported.
Constants preserved verbatim. Write/upload methods stubbed to
`NotImplementedException`. No DI registration.

Files to port (each with its interface, all into
`PussyCats.App/Services/`):
- `CVParsingService` (+ `ICVParsingService`)
- `CompletenessService` (+ `ICompletenessService`)
- `ImageStorageService` (+ `IImageStorageService`)
- `LocalFileStorageService` (+ `ILocalFileStorageService`)
- `PdfExportService` (+ `IPdfExportService`)

*File-storage write methods get stubbed.* For `LocalFileStorageService`
and `ImageStorageService`: read methods (load existing file by path)
port fully working — they only read from a known local directory.
Write/upload methods (save file, store image, etc.) get their bodies
replaced with:

```csharp
// Phase 5 routes uploads through /api/files; silent disk writes during
// demo would mask the bug.
throw new NotImplementedException(
    "Phase 5 routes file uploads through /api/files per MergePlan §4.");
```

Signatures, interface, and class structure stay intact — only the upload-
method bodies become the throw. Reason: silent disk writes during demo
are worse than a loud failure that points at the right phase.

*PdfExportService exception to the "no `Microsoft.UI.*` in services"
guideline.* WebView2 is the printer; the UI dependency is intrinsic.
Add at the top of the class:

```csharp
// justification: PdfExportService relies on WebView2 for HTML→PDF
// conversion; Microsoft.UI dependency is intrinsic to the chosen
// approach. Considered a service-layer exception per the layering
// rule in CodingStyle.
```

*Constants to preserve verbatim (recon-flagged business rules):*
- `CVParsingService`: `MaxSkills=30`, `MaxSkillLength=60`,
  `MaxFirstNameLength=50`, `MaxLastNameLength=60`, `MaxCountryLength=100`,
  `MaxCityLength=100`, `MaxUniversityLength=200`, `MaxGitHubLength=200`,
  `MaxLinkedInLength=200`, `MaxAddressLength=500`,
  `MaxMotivationLength=1000`, `MaxCompanyNameLength=150`,
  `MaxJobTitleLength=100`, `MaxWorkDescriptionLength=500`,
  `MinValidDate=1980-01-01`, `MaxYearsAheadForDate=1`. Also all
  in-method local constants inside private validation helpers — preserve
  every one.
- `CompletenessService`: `TotalFields=21`, plus the 21-string `Labels`
  array (order matters — tied to case index in the prompt-generation
  switch).
- `ImageStorageService`: `BytesPerKilobyte=1024`, `MaxFileSizeInMb=5`.
- `LocalFileStorageService` and `PdfExportService` — port whatever
  constants exist verbatim.

End of session: `dotnet build` (full output); confirm green across all
four projects; full list of upload/write methods now throwing
`NotImplementedException`, by service; deviations.

Phase 3 is complete once 3b.3 commits.

### Phase 3c — Reconciliation (done — keep separate, decided 2026-05-06)

**Decision: `UserService` and `UserProfileService` remain separate services.**

Rationale: the two services have genuinely different concerns.
`UserService` handles identity-level CRUD and is consumed directly by
matchmaking logic (status checks, skill gap analysis, recommendation
feeds). `UserProfileService` owns the candidate-facing surface: profile
picture path, account activation, level recalculation from skill test
history, CV text generation, and the `SaveAsync` façade. Merging would
create an oversized service with two unrelated caller audiences and
would force every matchmaking consumer to depend on profile-content
methods it doesn't use.

No code changes needed — the separation that fell out of 3b.1 is
already correct. Phase 4 controllers call `IUserRepository` directly
for CRUD; both services remain available for their respective
consumers in Phase 5.

`MatchService` reconciliation (absorbing `GetMatchesForUserAsync` and
`GetMatchStatisticsAsync`) was completed in 3b.1. No further match
reconciliation work.

### Phase 4 — Web API (done, pending commit)

Two sessions. Thin controllers (20-50 lines each), all in
`PussyCats.Api/Controllers/`. DI registration goes in `Program.cs`.
No App.xaml.cs changes — that's Phase 5. No RepositoryProxy work.
No view-model migration.

**Design decisions locked in:**

- Controllers inject `IXRepository` directly. The only exception is
  the match decision route — it needs the state machine. `MatchService`
  is in App and cannot be referenced from Api. Solution: extract a
  4-line static helper `MatchStatusTransitions.IsDecisionTransitionAllowed`
  into `PussyCats.Library/Domain/` so both Api and App can call it.
  App's `MatchService.IsDecisionTransitionAllowed` becomes a one-line
  wrapper. Do this before writing the controller.

- Three repositories have **no controller**: `IQuestionRepository`
  (dead table — questions live in PersonalityTestService code),
  `ISkillGroupRepository` (internal to CompatibilityService, not a
  public API concern), `IRecommendationRepository` (recommendations
  are computed server-side, not CRUD'd via the API).

- `POST /api/users/{id}/cv` and `GET /api/users/{id}/compatibility`
  are **stubbed** — return `501 Not Implemented`. CV parsing requires
  `CvParsingService` (in App), compatibility requires `CompatibilityService`
  (in App). Both wire properly in Phase 5.

- `FilesController` stub: both `POST /api/files` and `GET /api/files/{id}`
  return `501 Not Implemented`. Phase 5 implements.

- `IUserRepository.TouchLastUpdatedAsync` is **not** its own endpoint.
  The two endpoints that need it (`PATCH .../active` and
  `PATCH .../profile-picture`) call it server-side after their main
  repository call, coordinating two repo calls inside the action method.
  That is still "thin" — no business logic, just sequential I/O.

- HTTP conventions: `201 Created` + `Location` header for POST that
  returns an entity. `204 No Content` for PUT, PATCH, DELETE.
  `404` when `GetByIdAsync` returns null. `400` for bad input (null
  body, invalid enum). Use `Problem()` / `ValidationProblem()` for
  error responses (RFC 7807).

- No auth. Read `X-User-Id` from the request header for endpoints
  where the caller identity matters (match decision), but don't
  enforce it — just pass it through as context for Phase 5 to
  secure properly.

**Repositories to register in Program.cs** (11 pairs):
`IUserRepository → UserRepository`,
`IJobRepository → JobRepository`,
`IMatchRepository → MatchRepository`,
`ICompanyRepository → CompanyRepository`,
`IDocumentRepository → DocumentRepository`,
`ISkillRepository → SkillRepository`,
`IJobSkillRepository → JobSkillRepository`,
`IUserSkillRepository → UserSkillRepository`,
`ISkillGroupRepository → SkillGroupRepository`,
`ISkillTestRepository → SkillTestRepository`,
`IPersonalityTestRepository → PersonalityTestRepository`
(`IQuestionRepository` and `IRecommendationRepository` also
registered to keep DI graph consistent, but no controllers expose
them.)

---

**4a — Core controllers (done, pending commit)**

*Pre-step — Library helper.*
New file `PussyCats.Library/Domain/MatchStatusTransitions.cs`:

```csharp
namespace PussyCats.Library.Domain;
public static class MatchStatusTransitions
{
    public static bool IsDecisionTransitionAllowed(MatchStatus current, MatchStatus next)
    {
        if (current == MatchStatus.Applied)
            return next is MatchStatus.Accepted or MatchStatus.Rejected or MatchStatus.Advanced;
        if (current == MatchStatus.Advanced)
            return next is MatchStatus.Accepted or MatchStatus.Rejected;
        return false;
    }
}
```

Update `MatchService.IsDecisionTransitionAllowed(Match current, MatchStatus next)` in App:
replace the body with `return MatchStatusTransitions.IsDecisionTransitionAllowed(current.Status, next);`.

*Step 1 — `Program.cs`.*
Add all 13 `AddScoped<IXRepository, XRepository>()` registrations after the
DbContext registration. Add `using` directives for all repository namespaces.

*Step 2 — `UsersController`.*
Route: `[Route("api/users")]`.
Inject: `IUserRepository`, `IMatchRepository`, `IDocumentRepository`.
Methods (10):
- `GET /api/users` → `GetAllAsync`; 200
- `GET /api/users/{id}` → `GetByIdAsync`; 200 or 404
- `POST /api/users` → `AddAsync`; 201 + Location
- `PUT /api/users/{id}` → `GetByIdAsync` (404 guard) then `UpdateAsync`; 204
- `DELETE /api/users/{id}` → `GetByIdAsync` (404 guard) then `RemoveAsync`; 204
- `PATCH /api/users/{id}/active` — body `{ "isActive": bool }` → `UpdateActiveAccountAsync` + `TouchLastUpdatedAsync`; 204
- `PATCH /api/users/{id}/profile-picture` — body `{ "path": string }` → `UpdateProfilePicturePathAsync` + `TouchLastUpdatedAsync`; 204
- `DELETE /api/users/{id}/profile-picture` → `UpdateProfilePicturePathAsync("")` + `TouchLastUpdatedAsync`; 204
- `GET /api/users/{id}/matches` → `IMatchRepository.GetByUserIdAsync`; 200 or 404
- `GET /api/users/{id}/documents` → `IDocumentRepository.GetByUserIdAsync`; 200 or 404
- `POST /api/users/{id}/cv` → `Problem("Not implemented", statusCode: 501)`
- `GET /api/users/{id}/compatibility` → `Problem("Not implemented", statusCode: 501)`

*Step 3 — `JobsController`.*
Route: `[Route("api/jobs")]`.
Inject: `IJobRepository`.
Methods (5):
- `GET /api/jobs` — optional `?companyId=` filter; loads all, filters in-process if param present; 200
- `GET /api/jobs/{id}` → 200 or 404
- `POST /api/jobs` → 201 + Location
- `PUT /api/jobs/{id}` → 404 guard then `UpdateAsync`; 204
- `DELETE /api/jobs/{id}` → 404 guard then `RemoveAsync`; 204

*Step 4 — `CompaniesController`.*
Route: `[Route("api/companies")]`.
Inject: `ICompanyRepository`, `IJobRepository`.
Methods (6):
- `GET /api/companies` → 200
- `GET /api/companies/{id}` → 200 or 404
- `GET /api/companies/{id}/jobs` → `IJobRepository.GetByCompanyIdAsync`; 200 or 404
- `POST /api/companies` → 201 + Location
- `PUT /api/companies/{id}` → 204
- `DELETE /api/companies/{id}` → 204

*Step 5 — `MatchesController`.*
Route: `[Route("api/matches")]`.
Inject: `IMatchRepository`.
Methods (6):
- `GET /api/matches/{id}` → 200 or 404
- `GET /api/matches` — optional `?userId=` filter; 200
- `POST /api/matches` — body `{ userId, jobId }`; duplicate check via `GetByUserIdAndJobIdAsync`
  (409 Conflict if already exists); creates with `Status = Applied`; 201 + Location
- `DELETE /api/matches/{id}` → 204
- `PATCH /api/matches/{id}/decision` — body `{ "decision": MatchStatus, "feedback": string }`;
  calls `MatchStatusTransitions.IsDecisionTransitionAllowed` (422 if invalid transition);
  validates feedback present for Rejected (400 if missing); updates match; 204
- `PATCH /api/matches/{id}/advance` — loads match, requires `Status == Applied`
  (422 otherwise), sets `Status = Advanced`; 204

End of 4a: `dotnet build`. List controllers + route table. Commit.

---

**4b — Supporting controllers (done, pending commit)**

*Step 1 — `UserSkillsController`.*
Route: `[Route("api/users/{userId}/skills")]`.
Inject: `IUserSkillRepository`.
Methods (7):
- `GET api/users/{userId}/skills` → `GetByUserIdAsync`; 200
- `GET api/users/{userId}/skills/verified` → `GetVerifiedByUserIdAsync`; 200
- `GET api/users/{userId}/skills/{skillId}` → `GetAsync`; 200 or 404
- `POST api/users/{userId}/skills` → `AddAsync`; 201
- `PUT api/users/{userId}/skills/{skillId}` → 404 guard + `UpdateAsync`; 204
- `PATCH api/users/{userId}/skills/{skillId}/score` — body `{ "score": int }` → `UpdateScoreAsync`; 204
- `DELETE api/users/{userId}/skills/{skillId}` → `RemoveAsync`; 204

*Step 2 — `JobSkillsController`.*
Route: `[Route("api/jobs/{jobId}/skills")]`.
Inject: `IJobSkillRepository`.
Methods (5):
- `GET api/jobs/{jobId}/skills` → `GetByJobIdAsync`; 200
- `GET api/jobs/{jobId}/skills/{skillId}` → `GetAsync`; 200 or 404
- `POST api/jobs/{jobId}/skills` → `AddAsync`; 201
- `PUT api/jobs/{jobId}/skills/{skillId}` → 404 guard + `UpdateAsync`; 204
- `DELETE api/jobs/{jobId}/skills/{skillId}` → `RemoveAsync`; 204

*Step 3 — `SkillsController`.*
Route: `[Route("api/skills")]`.
Inject: `ISkillRepository`.
Methods (5):
- `GET /api/skills` → 200
- `GET /api/skills/{id}` → 200 or 404
- `POST /api/skills` → 201
- `PUT /api/skills/{id}` → 204
- `DELETE /api/skills/{id}` → 204

*Step 4 — `SkillTestsController`.*
Route: `[Route("api/skill-tests")]`.
Inject: `ISkillTestRepository`.
Methods (6):
- `GET /api/skill-tests/{id}` → 200 or 404
- `GET /api/skill-tests?userId={id}` → `GetByUserIdAsync`; 200
- `POST /api/skill-tests` → 201
- `PATCH /api/skill-tests/{id}/score` — body `{ "score": int }`; 204
- `PATCH /api/skill-tests/{id}/date` — body `{ "achievedDate": "yyyy-MM-dd" }`; 204
- `DELETE /api/skill-tests/{id}` → 204

*Step 5 — `PersonalityTestsController`.*
Route: `[Route("api/personality-tests")]`.
Inject: `IPersonalityTestRepository`.
Methods (4):
- `GET /api/personality-tests?userId={id}` → `GetByUserIdAsync`; 200 or 404
- `POST /api/personality-tests` → 201
- `PUT /api/personality-tests/{id}` → 204
- `DELETE /api/personality-tests/{id}` → 204

*Step 6 — `DocumentsController`.*
Route: `[Route("api/documents")]`.
Inject: `IDocumentRepository`.
Methods (4):
- `GET /api/documents/{id}` → 200 or 404
- `GET /api/documents?userId={id}` → `GetByUserIdAsync`; 200
- `POST /api/documents` → 201
- `DELETE /api/documents/{id}` → 204

*Step 7 — `FilesController`.*
Route: `[Route("api/files")]`.
No repository injection.
Methods (2), both stubs:
- `GET /api/files/{id}` → `Problem("Not implemented", statusCode: 501)`
- `POST /api/files` → `Problem("Not implemented", statusCode: 501)`

End of 4b: `dotnet build` across all four projects green. Full route
table. List all 501-stubbed endpoints. Deviations from playbook.
Commit. Phase 4 complete.

### Phase 5 — App services + RepositoryProxies + view-model migration (not started, playbook below)

Three sessions. No DI wired until 5b is complete. No view models
touched until 5c.

**Design decisions locked in:**

- **Typed `HttpClient` pattern.** Each proxy is registered via
  `AddHttpClient<IXRepository, XRepositoryProxy>(c => c.BaseAddress = new Uri(cfg.BaseUrl))`.
  Never share one `HttpClient` across proxies — base-address conflicts.

- **`UserRepositoryProxy.TouchLastUpdatedAsync` is a no-op.** The API's
  `PATCH .../active` and `PATCH .../profile-picture` actions call
  `TouchLastUpdatedAsync` server-side atomically. `UserProfileService`
  still calls the repo method, but the proxy skips the extra round-trip.

- **`RecommendationAlgorithm` default constructor only.** The
  dynamic-weights constructor depends on `SqlPostRepository` +
  `SqlInteractionRepository` (Developer/Interaction feed, deferred per
  §8). Port the default-constructor path (uniform 25% weights, no
  keyword signals). Stub the dynamic constructor with a comment pointing
  at §8. This is the correct behaviour for Phase 5; Phase 8 revisits.

- **`SessionContext` is a singleton.** Holds `int UserId`,
  `int? CompanyId`, `AppMode Mode` (Candidate/Company). Injected into
  every VM that needs to know the current user.

- **MVVM Toolkit as the merged standard.** All view models use
  `CommunityToolkit.Mvvm` (`ObservableObject`, `[ObservableProperty]`,
  `[RelayCommand]`). Both original repos already used it; no mixed
  patterns.

- **Chat and Developer VMs were deferred during Phase 5.** Phase 8d ports
  usable app-local implementations from the Varis repo: chat inbox/search/
  compose/block/delete and the developer algorithm proposal feed.

- **`JobRecommendationResult` display helpers restored.** The DTO
  dropped `JobTitleLine`, `DescriptionExcerpt`, `BuildExcerpt`,
  `TakeTopSkills` when moved to Library. These are pure computed
  properties (no I/O). Restore them on the DTO in Library during 5c
  so view models don't need wrapper objects.

- **Startup DI assertion.** At the end of `App()`, iterate every
  `IXRepository` type and assert the resolved concrete type name ends
  with `Proxy`. Throws at launch if anyone accidentally registers an
  EF repository in the App — better to crash loudly than silently hit
  the DB directly.

- **Phase 4 gaps fixed in 5a pre-step.** Three omissions from Phase 4
  are discovered when building the proxies:
  1. `MatchesController.GetAll` needs a `?jobId=` filter
     (`GetByUserIdAndJobIdAsync` proxy has nowhere to call).
  2. `SkillGroupsController` is missing (`CompatibilityService` needs
     `ISkillGroupRepository`; proxy needs endpoints).
  3. `RecommendationsController` is missing (`UserRecommendationService`
     needs `IRecommendationRepository`; proxy needs endpoints).
  Fix all three in the API project at the start of 5a before touching
  the App project.

---

**5a — API gap fixes + packages + RecommendationAlgorithm (commit)**

*Pre-step — fix Phase 4 API gaps.*

1. `MatchesController.GetAll`: add `[FromQuery] int? jobId` alongside
   the existing `userId` param. When both are present call
   `GetByUserIdAndJobIdAsync` and wrap the nullable result in a
   single-element list (or empty list).

2. New `SkillGroupsController` in `PussyCats.Api/Controllers/`:
   Route `api/skill-groups`. Inject `ISkillGroupRepository`.
   - `GET /api/skill-groups` → `GetAllAsync`; 200
   - `GET /api/skill-groups?jobRole={jobRole}` → `GetByJobRoleAsync`; 200

3. New `RecommendationsController` in `PussyCats.Api/Controllers/`:
   Route `api/recommendations`. Inject `IRecommendationRepository`.
   - `GET /api/recommendations/{id}` → `GetByIdAsync`; 200 or 404
   - `GET /api/recommendations` — optional `?userId=&jobId=` →
     `GetLatestByUserIdAndJobIdAsync` when both present, `GetAllAsync`
     otherwise; 200
   - `POST /api/recommendations` → `AddAsync`; 201
   - `DELETE /api/recommendations/{id}` → `RemoveAsync`; 204

`dotnet build` green in Api after each addition.

*App-side work.*

4. Add packages to `PussyCats.App/PussyCats.App.csproj`:
   - `Microsoft.Extensions.DependencyInjection`
   - `Microsoft.Extensions.Http`
   - `CommunityToolkit.Mvvm`

5. `App/Configuration/ApiConfiguration.cs`:
   ```csharp
   namespace PussyCats.App.Configuration;
   public record ApiConfiguration(string BaseUrl);
   ```
   `App/Configuration/ApiConfigurationLoader.cs` — static class with a
   `Load()` method. Initially hard-coded during the merge; Phase 8d reads
   bundled/local JSON config with a demo-safe default.

6. Port `RecommendationAlgorithm` from
   `matchmaking/algorithm/RecommendationAlgorithm.cs` (494 lines) into
   `PussyCats.App/Services/RecommendationAlgorithm.cs` implementing
   `IRecommendationAlgorithm`:
   - The interface uses Library types (`User`, `Job`, `UserSkill`,
     `JobSkill`, `CompatibilityBreakdown`). Adapt type references
     throughout (matchmaking used `List<Skill>` for both sides; the
     merged interface passes `IReadOnlyList<UserSkill>` / `IReadOnlyList<JobSkill>`).
   - Keep the default constructor and all computation helpers verbatim.
   - Stub the `SqlPostRepository + SqlInteractionRepository` constructor
     with `throw new NotSupportedException(` + §8 comment.
   - Preserve all constants and weights verbatim.

End of 5a: `dotnet build` green across all four projects. Commit.

---

**5b — RepositoryProxies + App.xaml.cs DI (commit)**

*Step 1 — 13 RepositoryProxy classes in `PussyCats.App/RepositoryProxies/`.*

Each proxy takes `HttpClient http` injected via the typed-client
pattern; `ApiConfiguration` is not injected directly — the base
address is set by DI registration.

Standard methods use `GetFromJsonAsync`, `PostAsJsonAsync`,
`PutAsJsonAsync`, `PatchAsJsonAsync`, `DeleteAsync`.
For 404 responses (entity not found), return `null` / empty list.

Special cases to call out per proxy:

- **`UserRepositoryProxy`**:
  `UpdateActiveAccountAsync` → `PATCH api/users/{id}/active`
  with `{"isActive": bool}`.
  `UpdateProfilePicturePathAsync` → `PATCH api/users/{id}/profile-picture`
  with `{"path": string}`.
  `TouchLastUpdatedAsync` → **no-op** (see design decisions above).

- **`MatchRepositoryProxy`**:
  `GetByUserIdAndJobIdAsync(userId, jobId)` →
  `GET api/matches?userId={userId}&jobId={jobId}`, parse list, return
  first element or null.

- **`SkillTestRepositoryProxy`**:
  `UpdateScoreAsync` → `PATCH api/skill-tests/{id}/score` with `{"score": int}`.
  `UpdateAchievedDateAsync` → `PATCH api/skill-tests/{id}/date`
  with `{"achievedDate": "yyyy-MM-dd"}`.

- **`UserSkillRepositoryProxy`**:
  `UpdateScoreAsync(userId, skillId, score)` →
  `PATCH api/users/{userId}/skills/{skillId}/score` with `{"score": int}`.

- **`PersonalityTestRepositoryProxy`**:
  `GetByUserIdAsync` → `GET api/personality-tests?userId={id}`;
  handle 404 as null.

- **`QuestionRepositoryProxy`**: Stub both methods to throw
  `NotSupportedException` (questions are hardcoded in
  `PersonalityTestService`; this repo is dead weight registered for
  DI graph completeness).

*Step 2 — `SessionContext` in `App/Configuration/SessionContext.cs`.*
```csharp
public sealed class SessionContext
{
    public int UserId { get; set; }
    public int? CompanyId { get; set; }
    public AppMode Mode { get; set; } = AppMode.Candidate;
}
```

*Step 3 — `App.xaml.cs` DI wiring.*

Add `Microsoft.Extensions.DependencyInjection` and
`Microsoft.Extensions.Http` usings. In `App()`:

```csharp
private IServiceProvider serviceProvider = null!;
public static IServiceProvider Services => ((App)Current).serviceProvider;
```

`ConfigureServices(IServiceCollection)` method wires:
1. `AddSingleton<ApiConfiguration>(_ => ApiConfigurationLoader.Load())`
2. `AddSingleton<SessionContext>()`
3. 13 × `AddHttpClient<IXRepository, XRepositoryProxy>(...)` with
   `c.BaseAddress = new Uri(cfg.BaseUrl)` — note `cfg` must be resolved
   before registering; read it at the start of `ConfigureServices`.
4. All services as `AddTransient<IXService, XService>()`.
   `CompanyRecommendationService` and `UserRecommendationService` must
   be `AddTransient` (stateful queues — MergeStatus open item).
   `IRecommendationAlgorithm` → `RecommendationAlgorithm`.
5. All view models as `AddTransient<XViewModel>()`.
6. Startup assertion after `BuildServiceProvider()`:
   ```csharp
   var repoInterfaces = typeof(IUserRepository).Assembly
       .GetExportedTypes()
       .Where(t => t.IsInterface && t.Name.StartsWith("I") && t.Name.EndsWith("Repository"));
   foreach (var iface in repoInterfaces)
       Debug.Assert(serviceProvider.GetService(iface)?.GetType().Name.EndsWith("Proxy") == true,
           $"DI violation: {iface.Name} is not bound to a *Proxy implementation.");
   ```

End of 5b: `dotnet build` green. Commit.

---

**5c — View model migration (commit)**

All view models land in `PussyCats.App/ViewModels/`.
MVVM Toolkit standard throughout.

*From PussyCats (15 VMs):*

`UserProfileViewModel` — delta: `UpdateAccountStatusAsync(bool)` not
string; `user.ProfilePicturePath` not `ProfilePicture`.

`ProfileFormViewModel` — delta: remove static `Create()` factory;
receive `IUserProfileService` + `ICvParsingService` via constructor.
Eliminate any inline `new XRepository(...)` calls.

`PersonalityTestViewModel` + `QuestionViewModel` + `RoleResultViewModel`
— near-verbatim; adapt `UserProfile` → `User` references.

`SkillTestCardViewModel` — near-verbatim; adapt type refs.

`TestDashboardViewModel` — near-verbatim.

`PreferencesViewModel` — near-verbatim; `IPreferenceService` interface
unchanged.

`CompatibilityOverviewViewModel` + `CompatibilityDetailViewModel` —
near-verbatim; helpers that called
`Helpers.GetFormattedNameFromJobRole()` (original static helper class)
should switch to a local switch expression or a helper in the VM.

`MatchHistoryViewModel` — near-verbatim.

`ExportCVViewModel` — near-verbatim; `IPdfExportService` interface
unchanged.

`DocumentListViewModel` + `UploadDocumentViewModel` — near-verbatim;
`IDocumentService` interface unchanged.

`PublicProfileViewModel` — near-verbatim.

*From matchmaking (7 VMs; 2 deferred):*

`ShellViewModel` — near-verbatim; inject `SessionContext` for mode
toggle.

`CompanyRecommendationViewModel` — adapt to merged service interfaces;
inject `SessionContext` for `CompanyId`; `ICompanyRecommendationService`
interface unchanged.

`CompanyStatusViewModel` — adapt; `IMatchService.SubmitDecisionAsync`
interface unchanged; remove `ITestingModuleAdapter` (mocked per §8,
replace with `// mock: see MergePlan §8`).

`UserRecommendationViewModel` — **delta**: remove inline
`new SkillRepository(...)` construction; `IUserRecommendationService`
already takes the right repositories via its own constructor so the VM
just receives `IUserRecommendationService` from DI. Inject
`SessionContext` for `UserId`.

`UserStatusViewModel` — **delta**: remove inline repo construction;
receive `IUserStatusService` + `ISkillGapService` via DI. Inject
`SessionContext`.

`SkillGapViewModel` — **delta**: remove inline repo construction;
receive `ISkillGapService` via DI.

`PostCardViewModel` — near-verbatim (pure display model).

`ChatViewModel` and `DeveloperViewModel` were initially stubbed here. Phase
8d replaces them with usable app-local implementations backed by
`IChatService` and `IDeveloperService`.

*`JobRecommendationResult` display helpers:* Add `JobTitleLine`,
`DescriptionExcerpt` (first 200 chars), `BuildExcerpt` static helper,
and `TakeTopSkills(int n)` back onto
`PussyCats.Library/DTOs/JobRecommendationResult.cs`. They are pure
computed properties — no I/O, safe in Library.

End of 5c: `dotnet build` green across all four projects. Commit.
Phase 5 complete.

### Phase 6 — UI shell (not started, playbook below)

**Pre-steps before touching XAML (fix Phase 5 issues):**

1. `MatchRepositoryProxy.AddAsync` — replace `PostAsJsonAsync("api/matches", match, ...)` with
   `PostAsJsonAsync("api/matches", new { match.UserId, match.JobId }, ...)`.
   The controller expects `CreateMatchRequest { UserId, JobId }`, not a full Match entity.

2. `[JsonIgnore]` on all back-navigation properties in Library domain entities
   (`WorkExperience.User`, `Match.User`, `Match.Job`, `UserSkill.User`,
   `JobSkill.Job`, and any other child → parent nav properties). Grep for
   `public.*User\|public.*Job\|public.*Match` across Domain/*.cs to catch all of them.
   EF ignores `[JsonIgnore]` so queries are unaffected. Once done, the
   `ReferenceHandler.IgnoreCycles` in Program.cs can stay (harmless) or be removed.

---

**Design decisions locked in:**

- **All pages land in `App/Views/`.** Two subfolders: `Candidate/` and `Company/`.
  Controls (reusable sub-views, not full pages) go in `App/Views/Controls/`.

- **ViewModel binding via `App.Services`.** Each page's code-behind resolves its
  ViewModel from `App.Services.GetRequiredService<XViewModel>()` in the constructor
  and assigns it to a `public XViewModel ViewModel` property. XAML binds with
  `x:Bind ViewModel.X`.

- **`PdfExportService` is page-scoped, not DI-resolved.** It takes a `WebView2`
  instance in its constructor. `ExportCVPage` creates it directly after
  `InitializeComponent()`: `pdfService = new PdfExportService(webView)`.

- **File pickers need the window handle.** Any page that opens a `FileOpenPicker` or
  `FileSavePicker` must call
  `WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow))`.
  `App.MainAppWindow` is already exposed as a static property.

- **Secondary windows become `ContentDialog`.** Where either original repo opened a
  new `Window` for sub-screens (e.g. document upload, personality test result
  selection), replace with a `ContentDialog` hosted in the current page. WinUI 3's
  `ContentDialog.XamlRoot` must be set to the page's `XamlRoot` before `ShowAsync()`.

- **NavigationView item selection drives `Frame.Navigate`.** The shell
  `MainWindow.xaml` hosts one `NavigationView` and one `Frame`. A
  `SelectionChanged` handler maps nav-item tags to page types and calls
  `contentFrame.Navigate(typeof(XPage))`. Page state (current user, company mode)
  flows through `SessionContext`, not through navigation parameters — pages load
  data from services in their `OnNavigatedTo` override.

- **Mode toggle.** A `ToggleSwitch` or top-level nav item switches
  `SessionContext.Mode` between `Candidate` and `Company`. The nav items
  for the inactive mode are hidden. The Frame navigates to the default page
  for the newly selected mode.

- **Deferred pages get placeholder stubs.** `ChatPage` and `DeveloperPage`
  started as single-Grid placeholders. Phase 8d replaces them with usable
  pages ported from Varis: a two-panel chat surface and a developer proposal
  feed.

- **XAML namespace.** All ported XAML files use `x:Class="PussyCats_App.Views.X"`
  and `xmlns:local="using:PussyCats_App.Views"`. Remove all references to
  `PussyCatsApp.*` and `matchmaking.*` namespaces.

---

**6a — NavigationView shell + candidate pages (done, pending commit)**

*Step 1 — `MainWindow.xaml` + `MainWindow.xaml.cs`.*

Replace `<Grid />` with:
```xml
<NavigationView x:Name="navView" SelectionChanged="NavView_SelectionChanged"
                IsBackButtonVisible="Collapsed" PaneDisplayMode="Left">
    <NavigationView.MenuItems>
        <!-- Candidate items, tag="page type name" -->
        <NavigationViewItem Content="My Profile"       Tag="UserProfilePage"    Icon="Contact" />
        <NavigationViewItem Content="Edit Profile"     Tag="ProfileFormPage"    Icon="Edit" />
        <NavigationViewItem Content="Skill Tests"      Tag="TestDashboardPage"  Icon="Permissions" />
        <NavigationViewItem Content="Personality Test" Tag="PersonalityTestPage" Icon="Favorite" />
        <NavigationViewItem Content="Compatibility"    Tag="CompatibilityOverviewPage" Icon="Filter" />
        <NavigationViewItem Content="Browse Jobs"      Tag="UserRecommendationPage" Icon="Find" />
        <NavigationViewItem Content="My Applications"  Tag="UserStatusPage"     Icon="List" />
        <!-- Company items (initially collapsed) -->
        <NavigationViewItem Content="Review Applicants" Tag="CompanyRecommendationPage" Icon="People" />
        <NavigationViewItem Content="Applicant Status"  Tag="CompanyStatusPage"  Icon="Accept" />
        <!-- Deferred -->
        <NavigationViewItem Content="Chat"             Tag="ChatPage"           Icon="Message" />
    </NavigationView.MenuItems>
    <NavigationView.FooterMenuItems>
        <NavigationViewItem Content="Preferences" Tag="PreferencesPage" Icon="Setting" />
        <NavigationViewItem Content="Documents"   Tag="DocumentsPage"   Icon="Document" />
        <NavigationViewItem Content="Export CV"   Tag="ExportCVPage"    Icon="Download" />
    </NavigationView.FooterMenuItems>
    <NavigationView.PaneCustomContent>
        <!-- Mode toggle: Candidate ↔ Company -->
        <ToggleSwitch x:Name="modeToggle" Header="Company mode"
                      Toggled="ModeToggle_Toggled" Margin="12,0" />
    </NavigationView.PaneCustomContent>
    <Frame x:Name="contentFrame" />
</NavigationView>
```

Code-behind wires `SelectionChanged` to navigate by tag, `ModeToggle_Toggled`
to flip `SessionContext.Mode` and refresh nav-item visibility.
Navigate to `UserRecommendationPage` on first load.

*Step 2 — Port candidate pages from PussyCats.*

For each of the 14 PussyCats views, create a corresponding Page in
`App/Views/Candidate/`. Update:
- `x:Class` → `PussyCats_App.Views.Candidate.XPage`
- All `xmlns:local` to `using:PussyCats_App.Views.Candidate`
- Remove old `PussyCatsApp.*` xmlns references
- Code-behind: resolve ViewModel via `App.Services`; call load method in
  `OnNavigatedTo`
- Any `x:Bind` property names that changed (e.g. `PhoneNumber` → `Phone`,
  `ProfilePicture` → `ProfilePicturePath`)
- `ProfileCompletenessBar` and `SkillTestCardView` become UserControls in
  `App/Views/Controls/`

Secondary-window patterns to convert to `ContentDialog`:
- `UploadDocumentViewModel` flow (was a separate dialog window in PussyCats) →
  `ContentDialog` inside `DocumentsPage`

*Step 3 — Port matchmaking candidate pages.*

Port from matchmaking: `UserRecommendationPageView`, `UserStatusPage`,
`SkillGapPage` into `App/Views/Candidate/`. Update namespaces, wire ViewModels.
`SkillGapPage` may be embedded as a panel inside `UserStatusPage` (it was a
sidebar in the original) — preserve that layout.

*Step 4 — Deferred pages.*

`App/Views/ChatPage.xaml` and `App/Views/Developer/DeveloperPage.xaml` were
created as placeholders in Phase 5. Phase 8d replaces them with working
surfaces.

End of 6a: `dotnet build` green. App launches, NavigationView shows, candidate
pages load with real data from the API. Commit.

---

**6b — Company pages + full nav wiring + ContentDialog cleanup (done, pending commit)**

*Step 1 — Port company pages.*

Port from matchmaking: `CompanyMatchmakingPage`, `CompanyStatusPage` into
`App/Views/Company/`. Wire `CompanyRecommendationViewModel` and
`CompanyStatusViewModel` via `App.Services`. Both need `SessionContext.CompanyId`
set to work — navigation to company pages should guard for null `CompanyId`.

`ITestingModuleAdapter` calls in `CompanyStatusViewModel` were already replaced
with `// mock: see MergePlan §8` stubs in Phase 5.

*Step 2 — Mode-aware nav item visibility.*

In `MainWindow.xaml.cs`, implement `UpdateNavVisibility()`: iterate
`navView.MenuItems`, check each item's `Tag` against a candidate-page set and a
company-page set, set `IsEnabled` and `Visibility` based on `SessionContext.Mode`.
Call on `ModeToggle_Toggled` and on initial load.

*Step 3 — `AppHeaderControl` (optional).*

If the matchmaking `AppHeaderControl` (user avatar + name display) is worth
keeping, port it as `App/Views/Controls/AppHeaderControl.xaml`. Wire it into the
`NavigationView.PaneHeader`. Otherwise skip — Phase 8 polish.

*Step 4 — ContentDialog audit.*

Grep all ported pages for `new Window(`, `window.Activate()`, `Window window`.
Each one must become a `ContentDialog`. The common cases:
- Document upload → `ContentDialog` in `DocumentsPage`
- Profile picture picker — uses `FileOpenPicker` (not a window), keep as-is with
  window handle initialisation
- Personality test role selection → `ContentDialog` in `PersonalityTestPage`

End of 6b: `dotnet build` green. Full app works — both candidate and company modes
navigate correctly, all pages load real data, no secondary windows open. Commit.

---

**6c — Runtime fixes from local end-to-end testing (done, pending commit)**

This wasn't on the original 6 playbook — it landed organically during the first
full local launch. Captured here so reviewers know why the patches exist.

*WinUI 3 cross-thread `INotifyPropertyChanged` crash (RPC_E_WRONG_THREAD,
0x8001010E).* Service-layer `ConfigureAwait(false)` + the WinUI native COM
proxy combine to fire `PropertyChanged` off the UI thread, which crashes inside
`NativeDelegateWrapper.Invoke` before any view handler runs. Fixed systemically:
- New `App/Configuration/UIDispatcher.cs` — static holder for the UI thread's
  `DispatcherQueue`. Set in `App()` before DI is built.
- New `App/ViewModels/DispatchableObservableObject.cs` — abstract base that
  overrides `OnPropertyChanged` to marshal through `UIDispatcher.Enqueue`.
- All 25 ViewModels rebased onto `DispatchableObservableObject` (was
  `ObservableObject`).
- Pages with `Action<string> ErrorOccurred` handlers wrap their `ContentDialog`
  creation in `DispatcherQueue.TryEnqueue` (the events also fire off-thread).

*EF Core identity-insert rejected on POST.* Scalar/Swagger UI pre-fills `id`
fields with non-zero example values, EF Core tries to INSERT explicit identity
values, SQL Server rejects with `IDENTITY_INSERT is OFF`. Fix: every controller
`Add` method now zeros the surrogate key before calling the repository
(`user.UserId = 0;` etc.). Applies to Users, Companies, Jobs, Documents, Skills,
SkillTests, Recommendations.

*Empty 200 responses crashed `ReadFromJsonAsync`.* `RepositoryProxyJson` now
short-circuits on `Content-Length: 0` / 204 / empty stream and returns
`default(T)` instead of letting `JsonSerializer` throw "no JSON tokens".

*Implicit `[Required]` on non-nullable navigation properties.* ASP.NET Core
auto-treats `User User { get; set; } = null!;` as required for model
validation, so any POST whose body omits navigation refs (which is all of them
because of `[JsonIgnore]`) returned 400. Fix:
`SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true` in
`Program.cs`.

*`JobSkill.Skill` / `UserSkill.Skill` were `[JsonIgnore]`'d unnecessarily.*
`Skill` is a leaf (no back-reference), so there was no cycle to break. Removed
the attribute on both. Consumers (`JobRecommendationResult.TakeTopSkills`,
`UserRecommendationService.CreateCardAsync`) also got null-defensive
(`jobSkill.Skill?.Name ?? $"Skill #{jobSkill.SkillId}"`) so a stale API
deployment can't NRE the client.

*Sidebar collapsed pane.* `ToggleSwitch` in `NavigationView.PaneCustomContent`
overflowed the compact pane width and showed truncated header text ("Con").
Fix: `MainWindow.xaml.cs` hides the toggle entirely on `PaneClosed` and
restores it on `PaneOpened`.

*Filter pane bled through page header.* `SplitView.PaneBackground` was
`CardBackgroundFillColorDefaultBrush` which is semi-transparent. Switched to
`SolidBackgroundFillColorBaseBrush`.

*Light theme.* `App.xaml` `RequestedTheme="Light"` — closer to the
Varis_vs_Clavicular look than the system-default dark theme. Removed the
short-lived MicaBackdrop experiment.

*OpenAPI exploration.* `AddOpenApi()` only serves raw JSON in .NET 9/10. Added
`Scalar.AspNetCore` package and `app.MapScalarApiReference()` so the API has a
browsable UI at `https://localhost:7134/scalar/v1` for seeding test users.

*Dev-loop ergonomics.* `PussyCats.Api.csproj` had `<Platforms>x86;x64;ARM64</Platforms>`
and a `<RuntimeIdentifiers>` block copied from the WinUI app, which broke "Any
CPU" builds in VS. Both removed. `ApiConfigurationLoader.cs` URL corrected from
the placeholder `https://localhost:7000` to the real `https://localhost:7134`.

*ExportCV resources.* `ExportCVPage.OnPageLoaded` looks up
`AppContext.BaseDirectory/resources/`. Created `PussyCats.App/resources/` and
copied `CVHtmlTemplate.html`, `CVCSSTemplate.css`, `CVGenerator.js` from the
PussyCatsApp original. Added as `<Content CopyToOutputDirectory="PreserveNewest">`
in the csproj so they ship to the output directory.

End of 6c: app launches, candidate and company flows both work end-to-end
against a live API + SQL Server. Phase 6 complete.

### Phase 7 — Tests (7a + 7b done — pending commit; 7c/7d not started, playbook below)

Four sessions. The two source repos disagree on the test framework
(matchmaking is xUnit + FluentAssertions, PussyCatsApp is MSTest), and they
disagree on the seam (matchmaking unit-tests services with hand-rolled mocks,
PussyCatsApp uses Moq for repositories and integration-tests against real SQL).
The merged test project standardizes on **xUnit + FluentAssertions + NSubstitute**
and **fakes over mocks** (per-aggregate in-memory `Fake*Repository` implementing
`IXRepository`). No DB, no network, no `WebApplicationFactory` in 7a–7c. Optional
controller integration suite in 7d if time permits.

**Design decisions locked in:**

- **Test framework: xUnit 2.9 (already packaged) + FluentAssertions + NSubstitute.**
  FluentAssertions for readable assertions (carries over from matchmaking tests).
  NSubstitute over Moq because it's lighter and the two original suites used
  three different mocking styles — picking one. Add both packages in 7a.

- **Fakes over mocks for repositories.** Each `IXRepository` gets an in-memory
  `Fake*Repository` in `Tests/Fakes/`. Backed by a `Dictionary<int, T>` or
  `List<T>`. This is closer to matchmaking's existing pattern and makes service
  tests read like business specs rather than mock-setup walls. Mocks (via
  NSubstitute) are reserved for service-to-service collaborators where
  verifying call sites is the point.

- **Tests target App-layer services with fakes substituted for repositories.**
  This is the highest-value layer — it's where the merged business logic lives.
  No EF Core in the test project. No SQL connection string.

- **Integration tests deferred.** PussyCatsApp's
  `*RepositoryIntegrationTests` and matchmaking's `Sql*RepositoryIntegrationTests`
  hit a real SQL Server with `EnsureSchema()` setup. The merged repos already
  use EF migrations, not raw SQL, so the original integration tests don't port
  cleanly. **Skip them in Phase 7.** EF + the real DB are exercised by the live
  app daily; if a Phase 8 task needs DB-level coverage, write a focused
  `IntegrationFixture` then.

- **Controller tests via `WebApplicationFactory` are optional 7d work.** Tier
  them as nice-to-have. Service-layer coverage via fakes hits the same business
  logic with much less ceremony.

- **ViewModel tests use NSubstitute for service interfaces.** ViewModels are
  thin glue (per Phase 5 design), so VM tests are mostly: "given this fake
  service returns X, the VM property exposes Y after calling Load." Don't
  re-test service behaviour through the VM.

- **One test class per service / per VM / per aggregate fake.** Mirror the
  source tree under `Tests/`:
  ```
  Tests/
    Fakes/                       # Fake*Repository per aggregate (13 fakes)
    Services/                    # *ServiceTests — one per service
    ViewModels/                  # *ViewModelTests — one per VM
    Algorithm/                   # RecommendationAlgorithmTests
    Helpers/                     # builder helpers (UserBuilder, JobBuilder, ...)
  ```

- **Builders for entity test data.** `UserBuilder`, `JobBuilder`, `MatchBuilder`
  in `Tests/Helpers/`. Fluent `.With…()` chain ending in `.Build()`. Avoids
  re-typing 30 default fields per test and makes intent visible at the call
  site. Pattern is in the matchmaking test sources — port it.

- **Constants ladder verbatim.** When a test asserts a tier threshold or
  level cutoff, reference `SimpleModelOperations.GoldScoreThreshold` etc. — do
  not hardcode `90` in test code. The constant is the spec.

- **WinUI bootstrap is suppressed for the test project.** The test csproj
  references `PussyCats.App` (which is WinUI), but
  `WindowsAppSdk*Initialize=false` should be set so tests don't need the
  Windows App SDK runtime. Already partially set; verify in 7a pre-step.

---

**7a — Test infrastructure (done, pending commit)**

*Pre-step — package additions.*
Added to `PussyCats.Tests.csproj`:
- `FluentAssertions 7.2.0` (last 7.x — v8 went commercial)
- `NSubstitute 5.3.0`

`WindowsAppSdkSelfContained=false` and `WindowsAppSdkBootstrapInitialize=false`
were **not** previously set on the test csproj (playbook said "already partially
set; verify" — they weren't). Both flags added so `dotnet test` doesn't try to
boot the Windows App SDK runtime via the transitive App reference.

*Step 1 — `Tests/Fakes/` (13 fakes).*
One file per aggregate. Each implements the corresponding `IXRepository`
verbatim — every interface method, every cancellation token. Backing store
is a `Dictionary<int, T>` for surrogate-key entities, a
`Dictionary<(int, int), T>` for composite-key entities (`UserSkill`, `JobSkill`).

Required fakes (matching the 13 registered repository interfaces):
- `FakeUserRepository`
- `FakeJobRepository`
- `FakeCompanyRepository`
- `FakeMatchRepository`
- `FakeDocumentRepository`
- `FakeSkillRepository`
- `FakeJobSkillRepository`
- `FakeUserSkillRepository`
- `FakeSkillGroupRepository`
- `FakeSkillTestRepository`
- `FakePersonalityTestRepository`
- `FakeQuestionRepository` (mostly empty — questions live in service code)
- `FakeRecommendationRepository`

Each fake has an internal `Seed(...)` overload taking an array of entities so
tests can preload state in one line. `AddAsync` assigns a new identity by
`store.Count + 1` for surrogate-key tables; preserves any caller-supplied id
on composite keys.

*Step 2 — `Tests/Helpers/` (builders + utilities).*
- `UserBuilder` with `.WithId`, `.WithEmail`, `.WithSkills(params (int skillId, int score)[])`,
  `.WithLevel`, etc. → `.Build()` returns a fully-populated `User`.
- `JobBuilder` (`.WithId`, `.WithCompanyId`, `.WithRole`, `.WithEmploymentType`).
- `MatchBuilder` (`.AppliedFor(userId, jobId)`, `.WithStatus`, `.WithFeedback`).
- `CompanyBuilder`, `SkillBuilder`, `SkillTestBuilder`,
  `PersonalityResultBuilder`.
- Static helper `Clock.FixedAt(DateTime.UtcNow)` if any service ends up taking
  an `IClock` (currently they call `DateTime.UtcNow` directly — leave for now).

*Step 3 — first smoke test.*
`Tests/Smoke/SolutionLoadsTest.cs` — single test that constructs every fake
plus `UserBuilder().Build()` and asserts non-null. Confirms the test project
references resolve before any real assertion logic lands.

End of 7a: `dotnet test` runs (1 passing test). Commit.

*7a done summary (pending commit).* `dotnet test` green: 1 passing, 0 failed.

New files:
- `PussyCats.Tests/Fakes/FakeUserRepository.cs`
- `PussyCats.Tests/Fakes/FakeJobRepository.cs`
- `PussyCats.Tests/Fakes/FakeCompanyRepository.cs`
- `PussyCats.Tests/Fakes/FakeMatchRepository.cs`
- `PussyCats.Tests/Fakes/FakeDocumentRepository.cs`
- `PussyCats.Tests/Fakes/FakeSkillRepository.cs`
- `PussyCats.Tests/Fakes/FakeJobSkillRepository.cs`
- `PussyCats.Tests/Fakes/FakeUserSkillRepository.cs`
- `PussyCats.Tests/Fakes/FakeSkillGroupRepository.cs`
- `PussyCats.Tests/Fakes/FakeSkillTestRepository.cs`
- `PussyCats.Tests/Fakes/FakePersonalityTestRepository.cs`
- `PussyCats.Tests/Fakes/FakeQuestionRepository.cs`
- `PussyCats.Tests/Fakes/FakeRecommendationRepository.cs`
- `PussyCats.Tests/Helpers/UserBuilder.cs`
- `PussyCats.Tests/Helpers/JobBuilder.cs`
- `PussyCats.Tests/Helpers/MatchBuilder.cs`
- `PussyCats.Tests/Helpers/CompanyBuilder.cs`
- `PussyCats.Tests/Helpers/SkillBuilder.cs`
- `PussyCats.Tests/Helpers/SkillTestBuilder.cs`
- `PussyCats.Tests/Helpers/PersonalityResultBuilder.cs`
- `PussyCats.Tests/Smoke/SolutionLoadsTest.cs`

Modified files:
- `PussyCats.Tests/PussyCats.Tests.csproj` — added FluentAssertions, NSubstitute,
  `WindowsAppSdkSelfContained=false`, `WindowsAppSdkBootstrapInitialize=false`.

Deviations:
- Fake `AddAsync` next-id strategy uses `store.Keys.Max() + 1` rather than the
  playbook's literal `store.Count + 1`. Reason: `Count + 1` collides when seeded
  rows leave gaps (seed id 5, then `AddAsync` picks 2 — collision). Max+1 is
  safer and equivalent on the no-gap path the playbook had in mind.
- `FakeUserRepository` mirrors the EF `UserRepository`'s `LastUpdated` /
  `CreatedAt` stamping behavior on `AddAsync`/`UpdateAsync` and the soft-update
  primitives — so service tests asserting on `LastUpdated` won't see surprises
  vs. production.
- `FakeRecommendationRepository.AddAsync` mirrors EF's "stamp `Timestamp` if
  default" behavior for the same reason.
- `WithSkills(...)` on `UserBuilder` auto-marks `IsVerified = true` and stamps
  `AchievedDate = today` when the supplied score is > 0. Keeps tests terse;
  callers that need an unverified self-claim can still mutate the returned
  `UserSkill` directly.

---

**7b — Service tests (done, pending commit)**

Port both source repos' service test suites onto the merged services + fakes.

*From matchmaking (xUnit, near-direct ports):*

Each landed as `Tests/Services/<Name>ServiceTests.cs`:
- `MatchServiceTests` — state-machine transitions, statistics aggregation
  (the expanded `GetMatchesForUserAsync` / `GetMatchStatisticsAsync` from
  3b.1 included). Verify `MatchStatusTransitions.IsDecisionTransitionAllowed`
  matrix exhaustively.
- `UserStatusServiceTests`
- `CompanyStatusServiceTests` — exercise the
  `ComputeCompatibilityFallback` `User.City` vs `Job.Location` fallback path
  explicitly.
- `SkillGapServiceTests`
- `JobSkillServiceTests`
- `SkillServiceTests` (matchmaking version) — confirm catalog reads still pass.
- `CompanyRecommendationServiceTests` — both queue advancement and reload
  paths. `AddTransient` registration was a deliberate Phase 5 choice; one test
  asserts that two service instances do not share `currentIndex` state (a
  smoke test for "we picked the right lifetime").
- `UserRecommendationServiceTests` — covered scoring path and dismissal /
  cooldown flow.

*From PussyCatsApp (MSTest → xUnit rewrite):*

Each MSTest `[TestClass]` becomes `class XServiceTests`; `[TestMethod]` →
`[Fact]`. `[TestInitialize]` → constructor (xUnit creates a fresh instance per
test). `[DataRow]` → `[Theory] + [InlineData]`. `Assert.AreEqual(x, y)` →
`y.Should().Be(x)` (FluentAssertions).
- `UserProfileServiceTests` — `SaveAsync` facade (Add vs Update branching),
  `RecalculateLevelAsync` against `SimpleModelOperations` constants.
- `PersonalityTestServiceTests` — score computation against the 24 hardcoded
  questions; `SelectedRole` persistence; trait-score `int`/`double` round-trip.
- `SkillTestServiceTests` — `CanRetakeTest` against `RetakeEligibilityMonths = 3`;
  `SubmitRetake` updates score + `AchievedDate`; XP awarded matches
  `SimpleModelOperations.GetExperiencePoints`.
- `CompatibilityServiceTests` — exercises `IUserRepository.user.ParsedCv`
  path that replaced `GetParsedCvByUserId`.
- `PreferenceServiceTests` — `User`-fields-as-`Preference` translation;
  `SearchLocationsAsync` against `PredefinedLocations.All`.
- `DocumentServiceTests` — metadata persistence + `ILocalFileStorageService`
  collaboration. Storage's write methods are stubbed
  `NotImplementedException` — assert the service surfaces a sensible error,
  not that the throw bubbles.
- `MatchServiceTests` (PussyCats half) — anything not already covered by the
  matchmaking port; merge into the single `MatchServiceTests` class, don't
  duplicate.
- `BadgeTests`, `UserLevelTests`, `SkillTestTests` — these test pure
  computation in `SimpleModelOperations`. Move into a single
  `SimpleModelOperationsTests` class.
- `CompletenessServiceTests` — assert all 21 `Labels` and case-18 deviation
  (`PersonalityResult?.SelectedRole != null`) per the open-item note.
- `CVParsingServiceTests` — verbatim port; constants are the spec.
- `LocalFileStorageServiceTests`, `ImageStorageServiceTests` — port read-side
  tests; write/upload tests skipped (those methods now throw — Phase 5
  decision).

End of 7b: `dotnet test` green; ≥ 50 service tests passing. Commit.

*7b done summary (pending commit).* `dotnet test` green: **232 passing, 1 skipped, 0 failed.**

New test files (24 in `PussyCats.Tests/Services/`):
- `SimpleModelOperationsTests` — tier thresholds, level cutoffs, AssignTier, GetExperiencePoints,
  CalculateLevelNumber. Replaces playbook's BadgeTests/UserLevelTests/SkillTestTests merge.
- `UserLevelServiceTests` — XP-to-next-level math, level progress percentage, max-level sentinel.
- `JobSkillServiceTests`, `UserSkillServiceTests`, `SkillGapServiceTests` — skill aggregate coverage.
- `DocumentServiceTests` — file-type validation, NotImplementedException pass-through, missing-doc errors.
- `MatchServiceTests` — full state-machine matrix (14 transitions), statistics windows, position grouping,
  decision/advance/revert, company-scoped queries.
- `UserServiceTests`, `JobServiceTests`, `CompanyServiceTests` — thin facade coverage.
- `UserProfileServiceTests` — SaveAsync facade branching, RecalculateLevelAsync, profile picture flows.
- `PersonalityTestServiceTests` — 24-question loader, trait averaging, role scoring, Save with rounded scores.
- `SkillTestServiceTests` — 3-month retake window, badge return after retake, eligibility error path.
- `CompatibilityServiceTests` — invalid-score-when-no-groups, ParsedCv merge with verified skills,
  3-suggestion cap, all-roles enumeration.
- `PreferenceServiceTests` — User-fields-as-Preference translation, role count validation,
  PredefinedLocations search.
- `UserStatusServiceTests`, `CompanyStatusServiceTests` — application card composition,
  decided-matches filter, City-vs-Location format normalization asserted explicitly.
- `CompanyRecommendationServiceTests` — queue advancement, sort by score, transient-lifetime smoke
  (two instances, no shared state), GetBreakdownAsync algorithm delegation.
- `UserRecommendationServiceTests` — top-card scoring, cooldown skip, ApplyLike/Dismiss, Undo paths,
  experience-bucket mapping, RecalculateTopCardIgnoringCooldownAsync override.
- `CooldownServiceTests` — 24h window default, latest-wins selection, zero/negative fallback.
- `CompletenessServiceTests` — 21-field total, case-18 deviation (PersonalityResult.SelectedRole),
  prompt-walks-fields-in-label-order.
- `CvParsingServiceTests` — JSON happy path, age/year/length caps, dedup, gender normalization,
  malformed JSON error. **XML branch test skipped** (open item: XmlSerializer doesn't support
  `DateTimeOffset` — production code has the same limitation).
- `LocalFileStorageServiceTests`, `ImageStorageServiceTests` — read paths only; SaveFile/SaveImage
  assert NotImplementedException per Phase 5 design.

Modified files:
- `PussyCats.App/PussyCats.App.csproj` — added `WindowsAppSdk*Initialize=false` (and master
  `WindowsAppSdkAutoInitialize=false`) to suppress `[ModuleInitializer]` injection into App.dll.
  **Required because** the auto-initializer fires on assembly load (when tests reference App
  service types) and COMExceptions in unpackaged test processes (`Class not registered`). The
  packaged App still works because MSIX provides the runtime — auto-init was redundant in
  production. Explanatory comment in the csproj.
- `PussyCats.Tests/PussyCats.Tests.csproj` — added the matching suppression flags symmetrically
  (defensive; the App-side flags are what actually mattered, but Tests-side keeps the intent
  visible if someone copies this project elsewhere).

Deviations:
- **`MatchServiceTests` is single class, not split between matchmaking-port and PussyCatsApp-port halves.**
  Playbook said merge into one — done up-front rather than after duplicating.
- **`SkillServiceTests` skipped.** Phase 3 retired matchmaking's `SkillService` — catalog reads now
  go through `ISkillRepository` directly, no service layer to test. Repo-level tests are deferred
  per the "no DB" rule.
- **`CompatibilityService` ParsedCv test relies on the 3rd line of `\n`-split text** as the skill
  list — that's what `ExtractSkillsFromParsedCv` does (`SkillsLineIndex = 2`). The test's
  `"Ada\nCambridge\nC#, Python"` matches that contract. If the parsed-CV format ever changes,
  the test will be the canary.
- **One test skipped** (`CvParsingServiceTests.ParseCvFile_parses_valid_xml`) — production code
  has the same `XmlSerializer`/`DateTimeOffset` limitation. Flagged as Phase 8 open item below.
- **No FluentAssertions `.Should().BeOfType<>` chains** in places where xUnit `[InlineData]` covers
  the variants more concisely (per CodingStyle's "one logical assertion per test" guidance).
- **`Math.Round` rounds 4.5 → 4** (banker's rounding) — caught by initial PersonalityTestServiceTests
  trait-score test; rewrote with three StronglyAgree answers averaging cleanly to 5.

---

**7c — ViewModel tests (commit)**

ViewModels post-Phase-5 are thin: bind `IXService` results to observable
properties, expose `RelayCommand`s. Tests substitute the service interfaces
with NSubstitute and assert the VM exposes the expected state after a load /
command.

*From matchmaking:*
- `UserRecommendationViewModelTests` — Like / Dismiss / Undo flow.
- `UserStatusViewModelTests`
- `CompanyRecommendationViewModelTests`
- `CompanyStatusViewModelTests`
- `SkillGapViewModelTests`
- `UserProfileViewModelTests` (matchmaking half — small)

*From PussyCatsApp:*
- `UserProfileViewModelTests` — merge with matchmaking half if there's overlap.
- `ProfileFormViewModelTests` — constructor-injection variant (no static
  `Create()`).
- `PreferencesViewModelTests`
- `PersonalityTestViewModelTests` (uses `QuestionViewModel`;
  `RoleResultViewModel` covered as a sub-fixture, not its own file)

`ChatViewModelTests` and `DeveloperViewModelTests` were skipped while both VMs
were placeholders. Phase 8d ports the screens; focused tests can now be added
when the team decides whether the backing services stay app-local or become
API-backed.

*WinUI threading note for VM tests.* Tests do not run inside a `DispatcherQueue`,
so `DispatchableObservableObject.OnPropertyChanged` falls through to the base
`ObservableObject` path (the dispatch helper falls back when `UIDispatcher.Queue`
is null). Verify: write one test that asserts `PropertyChanged` fires on a
property change on a VM that uses `DispatchableObservableObject`. If it doesn't,
fix `UIDispatcher.Enqueue`'s no-dispatcher fallback to invoke synchronously.

End of 7c: `dotnet test` green; total ≥ 80 tests. Commit.

---

**7d — Algorithm + (optional) controller integration tests (commit)**

*Step 1 — `RecommendationAlgorithmTests` (port from matchmaking).*
The matchmaking test file is the most prescriptive in the repo and exercises
weighted scoring, breakdown decomposition, and the default-vs-dynamic
constructor split. Port verbatim, with two adjustments:
- The dynamic constructor (`SqlPostRepository + SqlInteractionRepository`) is
  stubbed in our merged port. Tests for the dynamic-weights path become
  `[Fact(Skip = "Dynamic weights deferred per MergePlan §8")]`. Don't delete —
  Phase 8 may revisit.
- Library type adjustments: matchmaking's tests pass `List<Skill>` for both
  user and job sides; the merged interface takes `IReadOnlyList<UserSkill>` /
  `IReadOnlyList<JobSkill>`. Adapt the test fixtures.

*Step 2 — (optional) controller integration suite via `WebApplicationFactory`.*
If time permits, add a `Tests/Api/` subfolder with one fixture per controller.
Use `Microsoft.AspNetCore.Mvc.Testing`'s `WebApplicationFactory<Program>` and
override the `IXRepository` registrations to point at the in-memory fakes from
7a. This gives end-to-end verification of route table, status codes, and
controller-level guards without standing up SQL Server.

If skipped, document under "Open items" that controller routing is verified
manually only.

End of 7d: `dotnet test` green; ≥ 100 tests; coverage report generated via
`coverlet.collector` (already packaged). Commit. Phase 7 complete.

### Phase 8 - Cleanup, polish, demo prep

Three sessions. The functional bug list from `docs/issues.pdf` was already
resolved across the `phase 7c`, `phase 7d`, and `bug fixes + phase 7d` commits.
Phase 8 scope is now: clean up tech debt those fixes introduced, finish the
polish items the original Phase 8 outlined, and prep for the demo.

**Design decisions locked in:**

- **Typed `HttpClient` for storage services.** The current `new HttpClient()`
  per upload is a Phase 5 violation and a socket-exhaustion risk. Both
  `LocalFileStorageService` and `ImageStorageService` move to constructor-
  injected `HttpClient` via the same `AddHttpClient<TInterface, TImpl>`
  pattern the proxies use.
- **Seed defaults at the migration layer, not at read time.**
  `SkillTestDefaults.GetOrCreateAsync` mutates the DB during a `GET` —
  surprising side effect. Move the three default tests into a follow-up
  migration `SeedDemoSkillTests` and delete `SkillTestDefaults`.
- **Detach only the conflicting entry, not every tracked entity.**
  `UserRepository.UpdateAsync`'s current "detach everything" hammer is fine
  for a per-request scoped DbContext but ugly. Replace with
  `db.Entry(user).State = EntityState.Modified;` so EF doesn't traverse
  navigation properties and doesn't conflict with the controller's prior
  `GetByIdAsync`.
- **Skill-test runner stays placeholder.** Per open-items decision — out of
  scope.
- **No new feature work.** Phase 8 only fixes / cleans up / polishes.

---

**8a — Tech debt cleanup from 7c/7d bug-fix commits (done, pending commit)**

*Step 1.* All eight `*Repository.UpdateAsync` methods (User, Match, Company,
Job, PersonalityTest, UserSkill, Skill, JobSkill) replace the original
`db.{Set}.Update(entity)` (and the temporary `ChangeTracker.Entries()`
detach-all hammer GPT had introduced in the User one) with a
local-tracked-instance lookup pattern:

```csharp
var tracked = db.{Set}.Local.FirstOrDefault(e => e.{Key} == incoming.{Key});
if (tracked is not null)
    db.Entry(tracked).CurrentValues.SetValues(incoming);
else
    db.Entry(incoming).State = EntityState.Modified;
```

*Why not just `Entry(incoming).State = EntityState.Modified;`?* That was the
first attempt — but `Entry(...).State = ...` still routes through the
`IdentityMap.Add` codepath. When the controller's 404 guard called
`GetByIdAsync` first (which **tracks** the entity, since the merged
`GetByIdAsync` overloads use `Include(...)` without `AsNoTracking()`), the
DbSet's local store already holds an instance with that key. Trying to
attach a fresh request-body instance with the same key throws "another
instance with the same key value is already being tracked." The local
lookup pattern reconciles by copying values onto the tracked instance
instead of attaching a duplicate.

Match crashed first in the running app because it's the hottest write path
(decision/advance/revert). Same root cause across all 8 — fixed
symmetrically.

No new tests needed — existing service tests use `Fake*Repository` and
don't exercise the EF tracker. The fix was validated by re-running the
crashing UI flow.

*Step 2.* Storage services use typed `HttpClient`.
- Add `IFilesProxy` + `FilesProxy` in `App/RepositoryProxies/` exposing
  `Task<string> UploadAsync(Stream, string, ct)`,
  `Task DeleteAsync(string, ct)`, `string GetUrl(string)`.
- `ImageStorageService` and `LocalFileStorageService` accept `IFilesProxy`
  via constructor; their `SaveImage`/`SaveFile` delegate to
  `proxy.UploadAsync` (no more `new HttpClient()`, no more `.Result`).
- `App.xaml.cs` registers
  `AddHttpClient<IFilesProxy, FilesProxy>(c => c.BaseAddress = new Uri(cfg.BaseUrl));`
  and updates the storage-service registrations to inject `IFilesProxy`.
- Remove the commented-out `NotImplementedException` block in
  `ImageStorageService.SaveImage`.

*Step 3.* Move skill-test defaults from runtime to seed.
- New migration `SeedDemoSkillTests` that inserts three rows for
  `UserId = 1` (C# Fundamentals 82, SQL Server 76, Software Design 88) with
  `AchievedDate = '2026-01-07'` (deterministic).
- Delete `SkillTestDefaults.cs`.
- `UserProfileService.GetSkillTestsForUserAsync` reverts to
  `skillTestRepository.GetByUserIdAsync(...)`.
- Same for any other call sites (grep `SkillTestDefaults`).

*Step 4.* Audit any remaining `.Result` patterns introduced by the storage
fixes — convert to proper `await`.

End of 8a: storage services Phase 5 compliant, no silent DB mutations on
read paths, `UpdateAsync` is one line.

*8a done summary (pending commit).* `dotnet test` green: 287 passing,
2 skipped (pre-existing skips for `RecommendationAlgorithmTests.Dynamic_*`
and `CvParsingServiceTests.ParseCvFile_parses_valid_xml`), 0 failed.

New files:
- `PussyCats.App/RepositoryProxies/IFilesProxy.cs`
- `PussyCats.App/RepositoryProxies/FilesProxy.cs`
- `PussyCats.Library/Migrations/20260507175325_SeedDemoSkillTests.cs` (+ Designer)

Modified files:
- `PussyCats.Library/Repositories/Users/UserRepository.cs` — Step 1 fix.
- `PussyCats.Library/Persistence/Configurations/SkillTestConfiguration.cs` — added
  `HasData(...)` for the three demo tests; deterministic `2026-01-07` date.
- `PussyCats.Library/Migrations/PussyCatsDbContextModelSnapshot.cs` — auto-updated
  by `dotnet ef migrations add`.
- `PussyCats.App/App.xaml.cs` — registers
  `AddHttpClient<IFilesProxy, FilesProxy>(...)`.
- `PussyCats.App/Services/{ILocalFileStorageService,IImageStorageService}.cs` —
  signatures changed to `*Async` returning `Task` / `Task<string>`,
  `CancellationToken` arg added.
- `PussyCats.App/Services/{LocalFileStorageService,ImageStorageService}.cs` —
  delegate to `IFilesProxy`. No more `new HttpClient()`, no more `.Result`.
- `PussyCats.App/Services/{DocumentService,UserProfileService,SkillTestService}.cs`,
  `PussyCats.App/ViewModels/UserProfileViewModel.cs` — updated to await the
  new async signatures and call repository directly (no more
  `SkillTestDefaults.GetOrCreateAsync`).
- `PussyCats.Tests/Services/{LocalFileStorageServiceTests,ImageStorageServiceTests,DocumentServiceTests}.cs`,
  `PussyCats.Tests/ViewModels/UserProfileViewModelTests.cs` — adapted to
  async signatures + `IFilesProxy` mocking via NSubstitute.

Deleted:
- `PussyCats.App/Services/SkillTestDefaults.cs` — runtime mutate-on-read
  hack replaced by the seed migration.

Deviations / notes:
- The `SeedDemoSkillTests` migration uses static `SkillTestId` values 1, 2, 3.
  On dev machines that have already run the App with the old `SkillTestDefaults`,
  three skill-test rows already exist with those IDs (and a runtime-stamped
  `AchievedDate`). Running `dotnet ef database update` after this migration
  will fail with PK conflict on those machines. **Recipe:**
  `DELETE FROM SkillTests WHERE UserId = 1;` then re-run the update.
  Same shape as the `AddDummyUser` PK-conflict recipe.
- `LocalFileStorageService` lost its constructor that takes a local-disk
  base path. The local-disk fallback is gone — uploads always go through
  `/api/files` now. Tests dropped the `Constructor_creates_directory_when_missing`
  case along with the local-disk path. Net change: −1 test, +3 mock-based
  tests covering proxy delegation (LocalFileStorageServiceTests went from
  6 cases to 4; ImageStorageServiceTests went from 4 cases to 5).

---

**8b — Polish + cleanup (commit)**

*Step 1.* Drop unused `Questions` table. New migration `DropQuestionsTable`.
`IQuestionRepository` and `QuestionRepository` get deleted along with the DI
registration. `FakeQuestionRepository` and any references in tests removed.
(Questions live in `PersonalityTestService` per `MergePlan §4`.)

*Step 2.* `IImageStorageService.CheckFileSize` — add to interface so
consumers can call it through the abstraction. Open item from earlier
phases.

*Step 3.* Replace legacy `Preference` DTO with a flat `UserPreferences`
record.
- New `Library/DTOs/UserPreferences.cs`:
  `record UserPreferences(IReadOnlyList<JobRole> Roles, WorkMode WorkMode, string Location);`
- `IPreferenceService.GetByUserIdAsync` returns `Task<UserPreferences>`
  (single object, not a list of `Preference` rows).
- `PreferencesViewModel` adapts.
- Delete `Library/DTOs/Preference.cs`.

*Step 4.* Upgrade `System.Security.Cryptography.Xml` to a non-vulnerable
version. Pin via explicit `<PackageReference>` in
`PussyCats.Library.csproj` since it's transitive from
`Microsoft.EntityFrameworkCore.Design`.

*Step 5.* N+1 in `UserStatusService.GetApplicationsForUserAsync`. Currently
does one repo call per match. Replace with bulk loads: pull all matches,
then one bulk job/company/job-skill query, join in memory.

*Step 6.* `CvParsingService` XML branch. Two options:
- (a) Replace `DateTimeOffset` with `DateTime` on `CvData` so
  `XmlSerializer` accepts it; un-skip the test from 7b.
- (b) Drop the XML branch entirely and only support JSON. Update
  `ParseCvFile` to throw on `.xml`, remove the skipped test.
Option (b) is simpler if no real CV uploads use XML.

*Step 7.* StyleCop pass + coverage report. Backfill obvious coverage gaps
surfaced by `coverlet`.

End of 8b: warnings clean, vuln gone, dead code removed, perf regression on
My Applications fixed.

*8b done summary (pending commit).* Step 1 was already applied in the tree:
the `Questions` table is dropped by `DropQuestionsTable`, question repository
types and DI registrations are removed, and the solution smoke test was updated.
This pass completed the remaining 8b cleanup:

- `IImageStorageService` now exposes `CheckFileSize(...)`, matching the
  implementation and allowing callers/tests to use the abstraction.
- Legacy `Preference` DTO was replaced with `UserPreferences`; preference
  loading now returns one flat object (`Roles`, `WorkMode`, `Location`) instead
  of fake row-shaped values.
- `UserStatusViewModel` collection mutations now dispatch through
  `UIDispatcher.EnqueueAsync(...)`, fixing WinUI `RPC_E_WRONG_THREAD`
  collection-change crashes in My Applications.
- `UserStatusService.GetApplicationsForUserAsync` bulk-loads jobs, companies,
  and job skills once and joins in memory, removing the per-match N+1 calls.
- `CvParsingService` now supports JSON only; `.xml` fails explicitly and the
  previously skipped XML test was replaced by an active assertion.
- Coverage generated through `dotnet test --collect:"XPlat Code Coverage"` at
  `PussyCats.Tests/TestResults/.../coverage.cobertura.xml`.

Validation: `dotnet build` green, `dotnet test` green (288 passed, 1 skipped).
The only remaining build warning is `NETSDK1198` for missing `win-x64.pubxml`.

---

**8c — Demo prep + tag (commit)**

*Step 1.* `appsettings.local.json` for per-dev connection strings. Add
`appsettings.local.json` to `.gitignore`. `Program.cs` reads it after
`appsettings.Development.json`. Ship a `appsettings.local.json.example` with
`Server=YOUR-MACHINE-NAME` so new devs copy and edit.

*Step 2.* Dry-run on a clean Windows machine.
- Walk every nav entry as Candidate: Browse Jobs, My Applications, My
  Profile, Edit Profile, Skill Tests, Personality Test, Compatibility,
  Documents, Preferences.
- Toggle to Company mode: Review Applicants, Applicant Status.
- Capture screenshots in `docs/demo-screenshots/`.
- Note any new issues found in the open-items list (don't fix in 8c — that
  would slip).

*Step 3.* Tag `v4.0`. Push tag. Confirm CI still green.

End of 8c: repo demo-ready, tagged release.

*8c progress summary (pending commit).* Local API configuration is wired:
`Program.cs` reads optional `appsettings.local.json`, `.gitignore` excludes
the local file, and `PussyCats.Api/appsettings.local.json.example` documents
the per-machine connection string shape. Removed the stale
`PublishProfile=win-$(Platform).pubxml` property from `PussyCats.App.csproj`,
which cleared the final `NETSDK1198` build warning. Added
`docs/DemoDryRunChecklist.md` and `docs/demo-screenshots/README.md` for the
manual clean-machine walkthrough. Tagging/pushing `v4.0` is still pending
explicit release approval.

*8d issue pass progress summary (pending commit).* Fixed
`CompanyStatusService.ComputeCompatibilityFallback` so `User.City` matches
the city portion of a `Job.Location` value such as `Bucharest, Romania`.
`CompanyStatusServiceTests` now asserts the location bonus for that merged
format instead of documenting it as an open-item failure. `ApiConfigurationLoader`
now reads `appsettings.local.json` / `appsettings.json` with a default
fallback, and `PussyCats.App.csproj` copies both bundled config and optional
local config to output. Ported the Varis Chat and Developer pages into the
merged app with app-local `IChatService` and `IDeveloperService`
implementations, Library chat/developer domain models, real view models, and
usable XAML surfaces. Moved
`IRecommendationAlgorithm` from `App/Services` to `Library/Services` and
updated DI/consumers/tests to use the Library contract. Pruned resolved
open-item notes for recommendation-service DI, the dropped `Questions` table,
`UserStatusService` N+1 loading, the legacy `Preference` DTO, and domain
back-navigation JSON cycles.

---

*Out of scope (intentionally not in Phase 8):*
- Skill-test runner UI (open-items decision — stays placeholder).
- Top-bar Varis-style header (cosmetic, not blocking demo). Leave for v4.1.
- Auth, real company onboarding, deployment — Phase 9+ if it ever happens.
- The `WindowsAppSdk*Initialize=false` flags on `App.csproj` — these are a
  Phase 7 workaround and should not be revisited.

## Open items / known issues

Remaining deliberate deviations and known issues:

- **Skill-test runner UI is a placeholder.** `SkillTestCardViewModel.RetakeAsync`
  rolls `Random.Shared.Next(0, 101)` and submits that as the new score —
  there's no actual quiz page. The retake plumbing (eligibility check,
  score update, XP recalc) is real; only the test-runner is fake. Stays
  fake. The 3-month `RetakeEligibilityMonths` cooldown is intentional;
  the button greying out immediately after a retake is by design, not a bug.
- The CV parser caps `Motivation` at 1000 chars; the DB column caps
  at 2000. Intentional — parser is the friendly cap, DB is the
  safety net.
- `UserProfileService.RecalculateLevelAsync` now uses
  `SimpleModelOperations.GetExperiencePoints` and `CalculateLevelNumber`
  (resolved in 3b.2).

- `SimpleModelOperations.GetExperiencePoints` landed on
  `SimpleModelOperations` (not `SkillTestService` per the playbook).
  This is a minor deviation — centralising XP calculation in the
  constants class is defensible. Both `UserProfileService` and any
  future callers use `SimpleModelOperations.GetExperiencePoints`
  directly.

- `CompletenessService` case 18 deviation: original checked
  `PreferredJobRoles` (a `List<string>` on `UserProfile`). Merged
  `User` has no `PreferredJobRoles`; case 18 now checks
  `user.PersonalityResult?.SelectedRole != null`. Behavior change:
  field is "filled" only after the personality test is completed and
  a role is selected, not just because a list is non-empty. Comment
  in source documents this.

- `MatchService.GetPositionKey` returns display labels for
  `MatchesPerPosition`. Original PussyCatsApp stored arbitrary strings;
  only "Backend" and "Frontend" had test coverage. The other six labels
  are designer-chosen for legibility ("UI/UX Design", "AI/ML Engineering",
  etc.) and may be revised by Phase 6 design.

- `PersonalityTestResult.SelectedRole` is nullable. Null = test not
  taken or role not yet selected. Code reading this field must
  null-check.

## Working norms (carried from earlier sessions)

- One phase per Claude Code session. Fresh context each time.
- Commit at every phase boundary. Small commits over big ones.
- For service-layer work: consult original repos for state machines
  and constants. Preserve verbatim. Flag deviations.
- View models stay frozen until Phase 5. Service ports in 3b must
  not break view-model compilation surfaces (use facades when the
  data layer's natural shape would).
- Push back on design issues before commit, not after.
- Stop and show output before destructive or irreversible actions
  (migrations, `database update`, anything mutating).

## Reading order for the new teammate

1. `MergePlan.md` — the full architectural plan.
2. `CodingStyle.md` — 22 rules. Especially 6, 8, 10, 11, 12, 13.
3. `CodeReviewChecklist.md` — what reviewers look for.
4. This file — current state.
5. `PussyCats.Library/Domain/` — 30 minutes browsing the entities
   and enums to internalize the merged shape.
6. `PussyCats.Library/Repositories/` — interface signatures. The
   repo public surface is the contract everything else depends on.
7. `PussyCats.App/Services/` — current service ports. Read
   `MatchService` first because the state machine is the
   highest-stakes piece of preserved business logic in the codebase.

