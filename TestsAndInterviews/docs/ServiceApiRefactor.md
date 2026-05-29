# Service-API Refactor — Assignment 5

This is the pattern every dev follows when migrating their assigned entity for
Assignment 5. The Tech Lead has already migrated four pilot services
(`User`, `Job`, `Match`, `Document`) plus the helper service for file storage.
`Skill` was migrated earlier and is also a working reference.

When in doubt, copy the `User` pilot end-to-end (it has the most surface area,
including business-logic methods on the service that wrap multi-repo calls).

---

## What changes

Before Assignment 5:

```
PussyCats.App.csproj  ──▶  IUserService / UserService     (in App/Services/UserService/)
PussyCats.Api.csproj  ──▶  IUserRepository                (controllers inject the repo)
```

After Assignment 5:

```
PussyCats.Library     ──▶  IUserService / UserService     (Library/Services/Users/)
PussyCats.Api         ──▶  IUserService                   (controllers inject the service)
PussyCats.Web         ──▶  IUserService ⇒ UserServiceProxy (HTTP → Api)
PussyCats.App         ──▶  IUserService (unchanged consumers; just the using changed)
```

Reference rules are unchanged: `Library` references nothing of ours; `App`,
`Api`, `Web` each reference only `Library`. `Tests` references `Library` + `App`.

---

## The seven mechanical steps (for your entity `X`)

### 1. Move `IXService` and `XService` from App to Library

Source: `PussyCats.App/Services/XService/IXService.cs` + `XService.cs`.
Destination: `PussyCats.Library/Services/Xs/` (plural folder name — matches
existing pilots: `Users/`, `Jobs/`, `Matches/`, `Documents/`).

Rename the namespace declaration in both files:

```csharp
// before
namespace PussyCats_App.Services.XService;

// after
namespace PussyCats.Library.Services.Xs;
```

Delete the old App folder (`PussyCats.App/Services/XService/`).

### 2. Fix the `using` directives in every consumer

Two greps catch them all:

```bash
# find every usage of the old namespace
grep -rn "PussyCats_App.Services.XService" PussyCats.App PussyCats.Tests
```

For each match, replace the using line:

```csharp
// before
using PussyCats_App.Services.XService;

// after
using PussyCats.Library.Services.Xs;
```

That's typically 4–10 files: App services that depend on `IXService`, App
ViewModels, and the test files. `App.xaml.cs` is one of them (DI
registration).

### 3. Grow `IXService` to absorb controller business logic

This is the rule from the assignment: **no business logic in controllers.**

Look at the current API controller (`Api/Controllers/XsController.cs`).
Anywhere it injects multiple repositories and chains them, that chaining is
business logic and belongs on the service.

Concrete example from the `User` pilot — `UsersController` used to do this:

```csharp
// PATCH /api/users/{id}/active
await users.UpdateActiveAccountAsync(id, body.IsActive, ct);
await users.TouchLastUpdatedAsync(id, ct);
```

The "do A then B" sequencing is business logic. It now lives on `IUserService`:

```csharp
// IUserService.cs
Task SetActiveAsync(int userId, bool isActive, CancellationToken ct = default);

// UserService.cs
public async Task SetActiveAsync(int userId, bool isActive, CancellationToken ct = default)
{
    await userRepository.UpdateActiveAccountAsync(userId, isActive, ct).ConfigureAwait(false);
    await userRepository.TouchLastUpdatedAsync(userId, ct).ConfigureAwait(false);
}
```

And the controller becomes one line:

```csharp
await users.SetActiveAsync(id, body.IsActive, ct);
```

**Rules of thumb for what to put on the service vs. leave on the controller:**

- Loading two entities to build a third (e.g. `MatchService.CreatePendingApplicationAsync` loads `User` and `Job`, builds a `Match`) → service.
- Validation rules tied to the domain (e.g. "feedback required when rejecting") → service.
- Status transitions / state machines → service.
- HTTP-level concerns (`Created` vs `Ok`, query string parsing, `404 NotFound`) → controller.
- Mapping exceptions to HTTP status codes → controller (catch block).

### 4. Flip the API controller to inject `IXService`

Open `Api/Controllers/XsController.cs`. Replace the repository injection
with the service:

```csharp
// before
private readonly IXRepository xs;
public XsController(IXRepository xs) { this.xs = xs; }

// after
private readonly IXService xs;
public XsController(IXService xs) { this.xs = xs; }
```

Update the method bodies. Most calls should look identical (the service
method names mostly mirror the repo's). Where you absorbed business logic
into the service (step 3), the controller body shrinks.

If your controller has methods that throw from inside the service (state
machine violations, missing entities), catch the relevant exceptions in
the action method:

```csharp
try
{
    await xs.SomeBusinessOpAsync(...);
    return NoContent();
}
catch (KeyNotFoundException)     { return NotFound(); }
catch (ArgumentException ex)     { return Problem(detail: ex.Message, statusCode: 400); }
catch (InvalidOperationException ex) { return Problem(detail: ex.Message, statusCode: 422); }
```

### 5. Register `IXService` in `Api/Program.cs`

```csharp
builder.Services.AddScoped<IXService, XService>();
```

Add right after the repository registration block. The four pilots show the
order.

### 6. Write `XServiceProxy` in `Web/ServiceProxies/`

Same shape as `SkillServiceProxy` / `UserServiceProxy` / `JobServiceProxy`:

```csharp
using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Xs;

namespace PussyCats.Web.ServiceProxies;

public class XServiceProxy : IXService
{
    private readonly HttpClient http;
    public XServiceProxy(HttpClient http) { this.http = http; }

    public async Task<IReadOnlyList<X>> GetAllAsync(CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<X>>("api/x", ct) ?? new List<X>();

    public async Task<X?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"api/x/{id}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<X>(cancellationToken: ct);
    }

    public async Task<X> AddAsync(X x, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/x", x, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<X>(cancellationToken: ct))!;
    }

    public async Task UpdateAsync(X x, CancellationToken ct = default)
    {
        var response = await http.PutAsJsonAsync($"api/x/{x.XId}", x, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveAsync(int id, CancellationToken ct = default)
    {
        var response = await http.DeleteAsync($"api/x/{id}", ct);
        response.EnsureSuccessStatusCode();
    }
}
```

Notes:

- Use `HttpClient` straight from the constructor — **no** `ApiConfiguration`
  injected directly. The base address is set by `AddHttpClient<>` registration
  (step 7).
- `GET /{id}` returns `null` on 404 (matches the repository contract).
- For PATCH endpoints, use `http.PatchAsJsonAsync` with an anonymous object body.
- Don't put any business logic here — that's what `XService` is for. The
  proxy is mechanical HTTP serialization.

### 7. Register the proxy in `Web/Program.cs`

```csharp
builder.Services.AddHttpClient<IXService, XServiceProxy>(client =>
{
    client.BaseAddress = new Uri(apiConfig.BaseUrl);
});
```

Each proxy gets its own typed `HttpClient` — never share one across proxies
(base address conflicts).

---

## Three reference examples in the repo

| Service | Where to look | Notes |
|---|---|---|
| `Skill` | `Library/Services/Skills/`, `Api/Controllers/SkillsController.cs`, `Web/ServiceProxies/SkillServiceProxy.cs`, `Web/Controllers/SkillsController.cs`, `Web/Views/Skills/` | Simplest. Full MVC scaffold already exists. End-to-end. |
| `User` | `Library/Services/Users/`, `Api/Controllers/UsersController.cs`, `Web/ServiceProxies/UserServiceProxy.cs` | The pilot. Demonstrates business-logic absorption (`SetActiveAsync`, `SetProfilePicturePathAsync`). |
| `Job` | `Library/Services/Jobs/`, `Api/Controllers/JobsController.cs`, `Web/ServiceProxies/JobServiceProxy.cs` | The second pilot. Cleaner than `User`, demonstrates `?companyId=` query passing. |

The `Match` and `Document` services were also migrated to Library, but the
entity owners (you) write their Web proxies + Web controllers + views as
part of Phase B. The Library + API side is done.

---

## Special cases

### `DocumentService` and `ILocalFileStorageService`

`DocumentService` depends on `ILocalFileStorageService`. The interface was
moved to `Library/Services/FileStorage/`; the implementation (which routes
through `IFilesProxy` in App) stayed in `PussyCats.App/Services/LocalFileStorageService/`.

The API registers a stub:

```csharp
// Api/Configuration/StubLocalFileStorageService.cs
public sealed class StubLocalFileStorageService : ILocalFileStorageService
{
    // All methods throw NotSupportedException.
}
```

Why a stub? The API's `DocumentsController` only uses
`IDocumentService.GetByIdAsync` / `GetDocumentsByUserIdAsync` / `AddAsync` /
`RemoveAsync` — the metadata-only methods. The file-touching methods
(`UploadDocumentAsync`, `DeleteDocumentAsync`, `GetDocumentAbsolutePathAsync`)
are only called by the App, where the real `LocalFileStorageService` is
registered. If the API ever needs file IO, replace the stub with an impl
backed by `wwwroot/files/` and wire `FilesController` (currently `501`).

**Don't add file-touching methods to your service** if the API doesn't need
them. Keep the API's DocumentsController limited to metadata methods.

### Stateful services

`CompanyRecommendationService` and `UserRecommendationService` hold queue
state across calls. They are registered `Transient` (per-resolution). If
you find yourself adding a stateful service, follow the same registration —
singleton scope leaks state between users.

### Exception throwing in services

The codebase predates the "exceptions are exceptional" rule for the
match-decision flow. `MatchService` throws `InvalidOperationException` for
illegal transitions, `ArgumentException` for bad inputs, `KeyNotFoundException`
for missing entities. `MatchesController` catches these in action methods and
maps to HTTP status codes (`422`, `400`, `404`).

For new services, **prefer returning `null` / a result type / a domain
response over throwing** (per `docs/CodingStyle.md` §17). If you need to
throw, follow `MatchService`'s exception types so the catch block in the
controller can map cleanly.

---

## Definition of done (per entity)

The team-wide DoD is in `Assignment5_Plan.md` (Team Lead's copy). For the
service-side refactor specifically, you're done when:

- [ ] `IXService` and `XService` live in `Library/Services/Xs/` with the new namespace.
- [ ] The old `App/Services/XService/` folder is deleted.
- [ ] Every consumer's `using` directive is updated. `dotnet build` is green.
- [ ] API `XsController` injects `IXService` only (no `IXRepository`, no other repos).
- [ ] `Api/Program.cs` registers `IXService` scoped.
- [ ] `XServiceProxy` exists in `Web/ServiceProxies/` and is registered via `AddHttpClient<>` in `Web/Program.cs`.
- [ ] Any controller-side business logic (multi-call sequences, validation) moved onto the service interface.
- [ ] The rest of your Phase B work (Web MVC controller, views, tests, sequence diagram) is on top of this.

PR opened, Tech Lead reviews against this doc.
