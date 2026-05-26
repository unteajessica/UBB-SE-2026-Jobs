# Open Items Auth Review Report

Date: 2026-05-26
Status: Completed

## Summary

Resolved the five teacher-review items around controller authorization, API JWT authentication, AuthController layering, App/Web proxy usage, and desktop login/session guarding.

## Completed Work

### Item 1 - Web controllers missing `[Authorize]`

Added class-level authorization to the requested MVC controllers:

- `CompanyController`
- `CompatibilityController`
- `ChatController`
- `PersonalityTestController`
- `PreferencesController`
- `ExportCVController`
- `UserSkillsController`
- `UserStatusController`

`SkillTestsController` was already guarded and was left guarded. `AccountController` now keeps login/register anonymous at the action level while logout requires authorization.

### Item 2 - API controllers missing `[Authorize]`

Added JWT Bearer authentication to the API and guarded API controllers with `[Authorize]`, leaving only `AuthController` anonymous.

Implemented:

- JWT Bearer package reference in `PussyCats.Api`.
- JWT validation setup in `PussyCats.Api/Program.cs`.
- `app.UseAuthentication()` before `app.UseAuthorization()`.
- Dev JWT config in `appsettings.Development.json`.
- Placeholder JWT config in `appsettings.local.json.example`.
- JWT generation from `AuthController` login/register responses.
- Web JWT forwarding through `JwtForwardingHandler`.
- App JWT forwarding through `JwtForwardingHandler`.

### Item 3 - `AuthController` direct DbContext usage

Removed direct `PussyCatsDbContext` usage from `AuthController`.

Added email lookup methods through the service/repository path:

- `IUserService.GetByEmailAsync`
- `IUserService.ExistsWithEmailAsync`
- `IUserRepository.GetByEmailAsync`
- `IUserRepository.ExistsByEmailAsync`

`AuthController` now uses `IUserService` only for user lookup and registration checks.

### Item 4 - App concrete services instead of ServiceProxies

Switched App registrations to proxies where required:

- `ICooldownService` -> `CooldownServiceProxy`
- `ICompletenessService` -> `CompletenessServiceProxy`
- `ISkillGapService` -> `SkillGapServiceProxy`

Added API endpoints for proxy support:

- `GET /api/users/{id}/completeness`
- `GET /api/users/{id}/skill-gap`

Removed App-wide direct registration of `ICvParsingService` and routed CV upload through `IUserProfileService.UploadCvAsync`.

Removed direct App registration of `IRecommendationAlgorithm`.

Kept `ILocalDocumentFileService` local by design, as requested.

### Item 5 - Desktop App authorization

Added desktop login/session flow:

- New `LoginPage`.
- New `LoginViewModel`.
- `IAuthService` abstraction and `AuthServiceProxy` login/register support.
- `SessionContext` stores authenticated user data and JWT.
- Main window starts at login when unauthenticated.
- Navigation is guarded when unauthenticated.
- Logout clears session and returns to login.
- App service proxies forward the JWT automatically.

## Verification

Commands run successfully:

```powershell
dotnet build .\PussyCats.Tests\PussyCats.Tests.csproj -p:Platform=x64 --no-restore -v minimal
```

Result: build succeeded with warnings only.

```powershell
dotnet test .\PussyCats.Tests\PussyCats.Tests.csproj -p:Platform=x64 --no-build --logger "console;verbosity=minimal"
```

Result: 433 passed, 1 skipped, 0 failed.

```powershell
dotnet build "UBB-SE-2026-921-1.sln" -v:n
```

Result: solution build succeeded with warnings only.

## Remaining Notes

- The full solution build's default `Debug|Any CPU` configuration does not select `PussyCats.Tests`, so tests were built and run explicitly with `-p:Platform=x64`.
- Existing warning categories remain: low-severity `NU1901` advisories for NuGet packages and existing nullable/unused/async warnings in tests and app/library code.
- Production deployments must provide a real `Jwt:Key`; the committed key is development-only configuration.
- Existing untracked `setup-db.ps1` was left untouched.

## Main Branch Follow-up Pass - 2026-05-26

Checked the same auth/proxy surface after switching back to `main`.

Findings:

- JWT setup, API controller authorization, Web controller authorization, `AuthController` service-layer usage, login/session storage, and JWT forwarding were present.
- Found one App DI drift: `ILocalDocumentFileService` was registered to `DocumentServiceProxy`, whose local-file methods throw `NotSupportedException`.
- Fixed App DI so `ILocalDocumentFileService` is backed by `DocumentService` again, with `DocumentRepositoryProxy`, `ILocalFileStorageService`, `IUserService`, and `CvParsingService` dependencies.

Verification after the follow-up fix:

```powershell
dotnet build .\PussyCats.App\PussyCats.App.csproj -p:Platform=x64 --no-restore -v minimal
dotnet build .\PussyCats.Tests\PussyCats.Tests.csproj -p:Platform=x64 --no-restore -v minimal
dotnet test .\PussyCats.Tests\PussyCats.Tests.csproj -p:Platform=x64 --no-build --logger "console;verbosity=minimal"
dotnet build "UBB-SE-2026-921-1.sln" -v:n
```

Result: all builds succeeded; tests passed with 433 passed, 1 skipped, 0 failed.
