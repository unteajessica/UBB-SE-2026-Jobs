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

### Phase 3b — Port PussyCatsApp services (done, committed)

**3b.1 — design-call services (done, committed)**

Ported four PussyCatsApp services with real design calls plus extended
the existing matchmaking-side `MatchService`. Includes a Library schema
migration (`AddSelectedRoleAndJobRole`) that adds:

- `PersonalityTestResult.SelectedRole` (nullable `JobRole?`)
- `Job.JobRole` (non-nullable, with seed data updated for the three
  existing seed jobs)

Migration applied to `ISS-921-1`. `__EFMigrationsHistory` has both
`InitialCreate` and `AddSelectedRoleAndJobRole` rows.

Services in 3b.1: `PreferenceService`, `UserProfileService`,
`PersonalityTestService`, `CompatibilityService`. `MatchService` gains
`GetMatchesForUserAsync` and `GetMatchStatisticsAsync`.

**3b.2 — mechanical ports + constants source-of-truth (done, committed)**

`SimpleModelOperations` (constants source-of-truth), `SkillTestService`,
`DocumentService`, `UserLevelService`, `PredefinedLocations`.
`UserProfileService` and `PreferenceService` updated to use
`SimpleModelOperations` and `PredefinedLocations` respectively.

**3b.3 — pure helpers (done, committed)**

`CvParsingService`, `CompletenessService`, `ImageStorageService`,
`LocalFileStorageService`, `PdfExportService` — all ported. Write/upload
methods stubbed to `NotImplementedException`. Constants preserved verbatim.

### Phase 3c — Reconciliation (done — keep separate, decided 2026-05-06)

**Decision: `UserService` and `UserProfileService` remain separate services.**

`UserService` handles identity-level CRUD consumed by matchmaking logic.
`UserProfileService` owns the candidate-facing surface: profile picture,
account activation, level recalculation, CV text generation, `SaveAsync`
façade. No code changes needed — the separation that fell out of 3b.1 is
already correct.

### Phase 4 — Web API (done, committed)

13 `IXRepository → XRepository` DI registrations in `Program.cs`.
JSON options: camelCase, `ReferenceHandler.IgnoreCycles`,
`JsonStringEnumConverter`.

Controllers in `PussyCats.Api/Controllers/`:

| Controller | Route prefix | Key notes |
|---|---|---|
| `UsersController` | `api/users` | CRUD + active/profile-picture PATCH + nested matches/documents. CV and compatibility routes → 501. |
| `JobsController` | `api/jobs` | CRUD + optional `?companyId=` filter. |
| `CompaniesController` | `api/companies` | CRUD + `GET .../jobs`. |
| `MatchesController` | `api/matches` | CRUD + `?userId=&jobId=` filter + `PATCH .../decision` (state machine via `MatchStatusTransitions`) + `PATCH .../advance`. |
| `UserSkillsController` | `api/users/{userId}/skills` | Composite-key CRUD + PATCH score. |
| `JobSkillsController` | `api/jobs/{jobId}/skills` | Composite-key CRUD + flat `GET /api/job-skills`. |
| `SkillsController` | `api/skills` | Catalog CRUD. |
| `SkillTestsController` | `api/skill-tests` | CRUD + PATCH score/date. |
| `PersonalityTestsController` | `api/personality-tests` | CRUD via `?userId=` query. |
| `DocumentsController` | `api/documents` | CRUD via `?userId=` query. |
| `SkillGroupsController` | `api/skill-groups` | GET all + `?jobRole=` filter. |
| `RecommendationsController` | `api/recommendations` | CRUD + `?userId=&jobId=` filter. |
| `FilesController` | `api/files` | Both endpoints → 501. |

`MatchStatusTransitions.IsDecisionTransitionAllowed` extracted to
`PussyCats.Library/Domain/` so both Api and App can reference it.

### Phase 5 — App services + RepositoryProxies + view-model migration (done, committed)

**Packages added to App.csproj:** `CommunityToolkit.Mvvm 8.4.2`,
`Microsoft.Extensions.DependencyInjection 10.0.0`,
`Microsoft.Extensions.Http 10.0.0`.

**`App/Configuration/`:** `ApiConfiguration` record,
`ApiConfigurationLoader` (hardcoded `https://localhost:7000` — TODO Phase 8:
read from bundled config), `SessionContext` singleton
(`UserId`, `CompanyId?`, `AppMode`).

**`App/RepositoryProxies/`:** 13 proxy classes. Shared
`RepositoryProxyJson` utility handles 404→null/empty, `JsonSerializerOptions`
with `IgnoreCycles` + `JsonStringEnumConverter`. Key special cases:
- `UserRepositoryProxy.TouchLastUpdatedAsync` → no-op (API handles it atomically).
- `MatchRepositoryProxy.GetByUserIdAndJobIdAsync` → `GET api/matches?userId=X&jobId=Y`.
- `QuestionRepositoryProxy` → stubs throwing `NotSupportedException` (dead table).

**`App/Services/RecommendationAlgorithm.cs`:** 4-component weighted scorer
(skill match, keyword similarity, preference match, promotion bonus) implementing
`IRecommendationAlgorithm`. Default constructor only — dynamic-weights
constructor stubbed per §8.

**`App.xaml.cs`:** Full DI wiring. Generic `RegisterRepositoryProxy<TRepo, TProxy>`
helper. Auto-scan registers all `*ViewModel` types as Transient. Startup assertion
that every `IXRepository` resolves to a `*Proxy` type.

**`App/ViewModels/`:** 25 view models. MVVM Toolkit standard throughout.
`ViewModelSupport` centralises `FormatJobRole`, `BuildFreshnessLabel`,
`MaskEmail`/`MaskPhone`, `BuildSkillDisplay`. `ChatViewModel` and
`DeveloperViewModel` are stubs per §8. Key deltas resolved:
- `UpdateAccountStatusAsync(bool)` — not string.
- `ProfilePicturePath` — not `ProfilePicture`.
- `ProfileFormViewModel` — no static `Create()` factory; constructor injection.
- `UserRecommendationViewModel` — no inline `SkillRepository` construction.
- `UserStatusViewModel`/`SkillGapViewModel` — no inline repo construction.

`JobRecommendationResult` display helpers restored on the Library DTO:
`JobTitleLine`, `DescriptionExcerpt`, `BuildExcerpt`, `TakeTopSkills`.

### Phase 6 — UI shell (not started, playbook below)

**Pre-steps before touching XAML (fix Phase 5 issues):**

1. `MatchRepositoryProxy.AddAsync` — replace `PostAsJsonAsync("api/matches", match, ...)` with
   `PostAsJsonAsync("api/matches", new { match.UserId, match.JobId }, ...)`.
   The controller expects `CreateMatchRequest { UserId, JobId }`, not a full Match entity.

2. `[JsonIgnore]` on all back-navigation properties in Library domain entities
   (`WorkExperience.User`, `Match.User`, `Match.Job`, `UserSkill.User`,
   `JobSkill.Job`, and any other child → parent nav properties). Grep for
   `public.*User\|public.*Job\|public.*Match` across Domain/*.cs to catch all of them.
   EF ignores `[JsonIgnore]` so queries are unaffected.

---

**Design decisions locked in:**

- All pages land in `App/Views/`. Subfolders: `Candidate/`, `Company/`, `Controls/`.
- ViewModel binding via `App.Services.GetRequiredService<XViewModel>()` in page
  constructors. XAML binds with `x:Bind ViewModel.X`.
- `PdfExportService` is page-scoped — `ExportCVPage` creates it directly with the
  `WebView2` instance after `InitializeComponent()`.
- File pickers need `WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow))`.
- Secondary windows become `ContentDialog`. `ContentDialog.XamlRoot` must be set
  before `ShowAsync()`.
- NavigationView `SelectionChanged` maps item tags to page types and calls
  `contentFrame.Navigate(typeof(XPage))`. Page state flows through `SessionContext`.
- Mode toggle (`ToggleSwitch`) switches `SessionContext.Mode`; nav items for the
  inactive mode are hidden.
- Deferred pages (`ChatPage`, `DeveloperPage`) get "Coming soon" stub pages.
- XAML namespace: `x:Class="PussyCats_App.Views.X"`. No `PussyCatsApp.*` or
  `matchmaking.*` references.

---

**6a — NavigationView shell + candidate pages (commit)**

`MainWindow.xaml`: Replace `<Grid />` with a `NavigationView` + `Frame` shell.
Left rail: My Profile, Edit Profile, Skill Tests, Personality Test, Compatibility,
Browse Jobs, My Applications (candidate); Review Applicants, Applicant Status
(company, initially hidden); Chat (deferred stub). Footer: Preferences, Documents,
Export CV. `PaneCustomContent`: mode `ToggleSwitch`.

Port all 14 PussyCats views and 3 matchmaking candidate views to
`App/Views/Candidate/`. Update `x:Class`, `xmlns:local`, `x:Bind` property names
(`PhoneNumber→Phone`, `ProfilePicture→ProfilePicturePath`). Wire ViewModels from
`App.Services`; load data in `OnNavigatedTo`. `ProfileCompletenessBar` and
`SkillTestCardView` become UserControls in `App/Views/Controls/`.

Stub `ChatPage` and `DeveloperPage` with centred "Coming soon" TextBlock.

End of 6a: build green, app launches with NavigationView and real data. Commit.

---

**6b — Company pages + full nav wiring + ContentDialog cleanup (commit)**

Port `CompanyMatchmakingPage` and `CompanyStatusPage` to `App/Views/Company/`.
Guard navigation to company pages for null `SessionContext.CompanyId`.

`UpdateNavVisibility()` in `MainWindow.xaml.cs`: hide/show nav items based on
`SessionContext.Mode`. Call on toggle and initial load.

ContentDialog audit: grep all ported pages for `new Window(`. Convert hits to
`ContentDialog` (document upload, personality test role selection).

End of 6b: build green, both modes navigate correctly, no secondary windows. Commit.
Phase 6 complete.

### Phase 7 — Tests (not started)

Per-aggregate `Fake*Repository` in `Tests/Fakes/`. Port matchmaking
xUnit tests as-is. Rewrite PussyCats MSTest tests to xUnit. Service
tests use fakes only — no DB, no network.

### Phase 8 — Polish (not started)

StyleCop pass. Coverage. Dry-run demo. Tag `v4.0`.

## Open items / known issues

- `CompanyStatusService.ComputeCompatibilityFallback`: `User.City`
  vs `Job.Location` format mismatch → silent false negatives. TODO in source.
- N+1 query pattern in `UserStatusService` and similar. Preserved from originals.
  Phase 6 demo will surface if it's a real problem.
- CV parser caps `Motivation` at 1000 chars; DB column at 2000. Intentional.
- `IRecommendationAlgorithm` interface lives in App/Services, not Library.
  Mild CodingStyle §6 violation; trivial to move later.
- `Preference` DTO is a legacy translation layer. Replace with flat record in Phase 5.
- `Questions` table is dead weight — questions are hardcoded in `PersonalityTestService`.
  Drop in Phase 8.
- `SimpleModelOperations.GetExperiencePoints` landed on `SimpleModelOperations`
  rather than `SkillTestService` per the original playbook. Defensible; no action needed.
- **Circular back-navigation properties** — `[JsonIgnore]` on back-nav properties
  not yet applied. `RepositoryProxyJson.Options` has `IgnoreCycles` as a backstop.
  **Fix in Phase 6 pre-step.**
- `MatchRepositoryProxy.AddAsync` sends full `Match` entity where controller expects
  `CreateMatchRequest { UserId, JobId }`. Works by accident. **Fix in Phase 6 pre-step.**
- `CompletenessService` case 18: maps `PreferredJobRoles` → `PersonalityResult?.SelectedRole`.
  Behavior change documented in source.
- `ImageStorageService.CheckFileSize` is public but not on `IImageStorageService`.
  Phase 8 cleanup.
- `PersonalityTestResult.SelectedRole` is nullable — all readers must null-check.

## Working norms (carried from earlier sessions)

- One phase per Claude Code session. Fresh context each time.
- Commit at every phase boundary. Small commits over big ones.
- For service-layer work: consult original repos for state machines
  and constants. Preserve verbatim. Flag deviations.
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
6. `PussyCats.Library/Repositories/` — interface signatures.
7. `PussyCats.App/Services/` — current service ports. Read
   `MatchService` first (state machine is the highest-stakes logic).
