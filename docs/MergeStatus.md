# Merge Status

Last updated: 2026-05-06
Current phase: Phase 5 done. Phase 6 playbook below.

This document tracks where the merge stands right now. The architectural
plan is in `MergePlan.md` and is unchanged. This file records what's been
decided and built since the plan was written, plus what's left.

## TL;DR

- Repo: `UBB-SE-2026-921-1`. Database: `ISS-921-1` on `DESKTOP-M6HSOV2`,
  Windows auth.
- Build green across all four projects (`PussyCats.App`, `PussyCats.Library`,
  `PussyCats.Api`, `PussyCats.Tests`).
- Phases 0, 1, 2, 3a committed. Phase 3b underway.
- DI not wired in `App.xaml.cs` yet. That's Phase 5. Don't add it during
  3b.
- View models from both original repos have not been touched. They
  still reference original namespaces and won't build against the
  merged code. Phase 5 ports them.

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
- `Developer`, `Interaction`, `Post`, `Chat` deferred per
  `MergePlan.md §8`. Stays mocked for now.

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
interface stub. All are async, take `CancellationToken`, use
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

Interface added: `IRecommendationAlgorithm` (signatures only, no
implementation yet — Phase 3b ports the algorithm class from
`matchmaking/algorithm/`).

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
  `User.City` for the missing `User.Location`. **Comment in the
  source flags that this comparison can produce false negatives
  because `User.City` stores bare city names while `Job.Location`
  may include country.** Phase 6 should normalize.
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

- **Chat and Developer VMs are deferred.** `ChatViewModel` and
  `DeveloperViewModel` depend on services outside the merged scope
  (§8). They are not ported. Each gets a stub file with
  `// mock: belongs to other half, see MergePlan.md §8`.

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
   single `Load()` that returns `new ApiConfiguration("https://localhost:7000")`.
   Add a `// TODO Phase 8: read from bundled config file` comment.

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

`ChatViewModel` stub → `// mock: belongs to other half, see MergePlan.md §8`.
`DeveloperViewModel` stub → same comment.

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

- **Deferred pages get placeholder stubs.** `ChatPage` and `DeveloperPage` are
  created as single-Grid pages with a `TextBlock` "Coming soon — see MergePlan §8."
  They appear in the nav rail so navigation doesn't crash, but have no logic.

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

*Step 4 — Stub deferred pages.*

`App/Views/ChatPage.xaml` and `App/Views/DeveloperPage.xaml` — each is just a
`Page` with a centred `TextBlock`:
```xml
<TextBlock Text="Coming soon — see MergePlan.md §8"
           HorizontalAlignment="Center" VerticalAlignment="Center" />
```

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
Phase 6 complete.

### Phase 7 — Tests (not started)

Per-aggregate `Fake*Repository` in `Tests/Fakes/`. Port matchmaking
xUnit tests as-is. Rewrite PussyCats MSTest tests to xUnit. Service
tests use fakes only — no DB, no network.

### Phase 8 — Polish (not started)

StyleCop pass. Coverage. Dry-run demo. Tag `v4.0`.

## Open items / known issues

These are flagged in code or in conversation but haven't been
resolved yet:

- `CompanyStatusService.ComputeCompatibilityFallback`: `User.City`
  vs `Job.Location` format mismatch produces silent false
  negatives. TODO in source.
- `CompanyRecommendationService` and `UserRecommendationService`
  cannot be DI-registered until 3b ports the
  `RecommendationAlgorithm` class from `matchmaking/algorithm/`.
- The empty `Questions` table from Phase 2 is dead weight — the
  questions live in `PersonalityTestService` per
  `MergePlan §4`. Drop in Phase 8.
- `UserStatusService` and similar services have an N+1 query
  pattern (one repository call per match in a loop). Preserved from
  matchmaking originals. Phase 6 demo will surface real
  performance issues if they exist.
- The CV parser caps `Motivation` at 1000 chars; the DB column caps
  at 2000. Intentional — parser is the friendly cap, DB is the
  safety net.
- `IRecommendationAlgorithm` interface in `App/Services` violates
  CodingStyle §6 strictly speaking (interfaces with implementations
  in App rather than Library), but the algorithm uses Library
  domain types so moving it later is trivial.
- `Preference` DTO in `Library/DTOs/` exists only to preserve
  `IPreferenceService.GetByUserIdAsync`'s legacy return shape. The
  service translates User fields into `Preference` objects on the way
  out and parses them back on the way in. Slated for replacement when
  view models migrate in Phase 5 — consider a flat `UserPreferences`
  record instead.

- `UserProfileService.RecalculateLevelAsync` now uses
  `SimpleModelOperations.GetExperiencePoints` and `CalculateLevelNumber`
  (resolved in 3b.2).

- `SimpleModelOperations.GetExperiencePoints` landed on
  `SimpleModelOperations` (not `SkillTestService` per the playbook).
  This is a minor deviation — centralising XP calculation in the
  constants class is defensible. Both `UserProfileService` and any
  future callers use `SimpleModelOperations.GetExperiencePoints`
  directly.

- **Circular back-navigation properties on domain entities.** Every
  child entity has a back-nav property to its parent (e.g.
  `WorkExperience.User`, `Match.User`, `Match.Job`, `UserSkill.User`,
  `JobSkill.Job`, likely also `Project.User`, `Document.User`,
  `SkillTest.User`, `PersonalityTestResult.User`, etc.). The API
  outbound side is currently covered by `ReferenceHandler.IgnoreCycles`
  in Program.cs. The proxy outbound side (POST/PUT from App to API) is
  not — if a VM ever manually wires back-navigation before calling save,
  `PostAsJsonAsync` will throw a JsonException. The correct fix is
  `[JsonIgnore]` on all back-navigation properties in Library; EF
  ignores JSON attributes so queries are unaffected. Once that lands,
  `ReferenceHandler.IgnoreCycles` in Program.cs can be removed (though
  it's harmless to keep). **Planned for Phase 5b** (Library change that
  the proxies should rely on being in place).

- `CompletenessService` case 18 deviation: original checked
  `PreferredJobRoles` (a `List<string>` on `UserProfile`). Merged
  `User` has no `PreferredJobRoles`; case 18 now checks
  `user.PersonalityResult?.SelectedRole != null`. Behavior change:
  field is "filled" only after the personality test is completed and
  a role is selected, not just because a list is non-empty. Comment
  in source documents this.

- `ImageStorageService.CheckFileSize` is `public` but not on
  `IImageStorageService`. Preserved verbatim from the original.
  Consumers injecting the interface cannot call it. Slated for cleanup
  in Phase 8.

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
