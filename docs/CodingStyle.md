# Coding Style and Conventions

Internal coding standard for the merged PussyCats team. Where these rules conflict with default StyleCop, **change StyleCop** to match (per assignment).

22 rules total, grouped by topic. The technical lead reviews PRs against this document.

---

## Naming and structure

**1. PascalCase for types, methods, properties, and public fields. camelCase for parameters and locals. `_camelCase` for private fields is forbidden — use plain `camelCase` and rely on `this.` only when there's a real name collision.**

```csharp
// Good
public class JobService { private readonly IJobRepository jobRepository; }

// Bad
public class jobService { private readonly IJobRepository _jobRepository; }
```

**2. Interfaces are prefixed with `I`. Abstract classes are not prefixed. Don't suffix concretes with `Impl`.**

```csharp
public interface IMatchService { }
public class MatchService : IMatchService { }   // not MatchServiceImpl
```

**3. Namespaces follow the folder structure exactly, rooted at `PussyCats.<ProjectName>`.** A file at `src/PussyCats.Library/Repositories/Users/UserRepository.cs` is in `PussyCats.Library.Repositories.Users`. No exceptions.

**4. One public type per file. Filename equals type name.** Nested private types are OK in the same file. This makes git diffs and code review readable.

**5. Acronyms longer than two letters use PascalCase, not all caps.** `HtmlParser`, `XmlReader`, `CvParsingService` — not `HTMLParser`, `XMLReader`, `CVParsingService`.

---

## Layering and architecture

**6. Strict project references — enforced by `.csproj` references, not just convention:**
- `App` references `Library` only (never `Api`)
- `Api` references `Library` only
- `Tests` references `Library` and `App`
- `Library` references nothing from us

If a PR adds a forbidden reference, it's rejected.

**7. The App registers `*RepositoryProxy` implementations of `IRepository`. The API registers EF `*Repository` implementations. Tests register `Fake*Repository` implementations.** A `Repository` (EF) class instantiated in App code is a bug, even though the type is reachable.

**8. No business logic in the UI layer or in repositories.** Views and view models orchestrate; services decide; repositories return data. Repositories never `if`/`else` on domain rules. State machines (e.g. `MatchStatus` transitions) live in services.

**9. Domain links are object references, not foreign-key ints.** `Match.User` (type `User`), not `Match.UserId` alone. Keep the FK property only because EF needs it; expose the navigation property as the primary access path.

```csharp
// Good
public class Match { public int UserId { get; set; } public User User { get; set; } = null!; }

// Bad
public class Match { public int UserId { get; set; } /* no nav property */ }
```

**10. Repositories expose `IReadOnlyList<T>`, never `List<T>` or `IList<T>`.** Mutation happens through `Add`/`Update`/`Remove` methods.

---

## Async and threading

**11. Methods that perform I/O are async and end in `Async`. Sync versions exist only when WinUI binding requires sync.** Don't mix `.Result` / `.Wait()` — that deadlocks WinUI.

**12. Always `ConfigureAwait(false)` in service, repository, and proxy code.** The UI layer (view models, code-behind) skips it because we want to resume on the UI thread.

**13. Cancellation tokens are accepted on every public async method that could take more than 100ms.** Default to `default` so callers can opt in.

---

## EF Core and data access

**14. The `DbContext` is internal to `Library`. Services depend on `IXRepository` interfaces, never on `DbContext` directly.**

**15. All entity configuration goes in `IEntityTypeConfiguration<T>` files under `Library/Persistence/Configurations/`.** No fluent configuration in `OnModelCreating` itself beyond `ApplyConfigurationsFromAssembly`.

**16. Migrations are committed to the repo. Never edit a migration after it's been merged to main.** If the schema is wrong, write a new migration.

**17. No raw SQL, stored procedures, or DB views.** LINQ to Entities only. If a query is too complex for LINQ, split it into multiple queries or change the model.

---

## Repository proxies (App side)

**18. A `*RepositoryProxy` implements the same `IRepository` interface as the EF version. It does HTTP and nothing else — no caching, no business logic, no model mapping beyond JSON deserialization.** A bug in the API surface should be visible in the proxy diff, not buried under transformation logic.

---

## WinUI and MVVM

**19. View code-behind contains only event-to-command wiring and view-only logic (animations, focus). All state and business calls go through the view model.**

**20. View models inherit from `CommunityToolkit.Mvvm.ComponentModel.ObservableObject`. Use `[ObservableProperty]` and `[RelayCommand]` source generators rather than hand-rolled `INotifyPropertyChanged` boilerplate.**

**21. No `new ServiceX()` or `new XRepositoryProxy()` inside a view model — dependencies arrive via constructor injection.** Makes view models testable without WinUI.

---

## Errors and validation

**22. Exceptions are for exceptional cases. Expected failures (validation errors, not-found lookups) return `null`, a result type, or a domain-specific response — they don't throw.** A user typing a bad email is not exceptional. A database connection dying is.

---

## Tests

Test framework is xUnit + FluentAssertions. One test class per production class. Test method names use `Method_condition_expected_result`:

```csharp
[Fact]
public void Add_stores_new_job_and_get_by_id_returns_it() { ... }
```

Each test arranges, acts, asserts in that order. One logical assertion per test (it's fine if it expands to several `Should()` calls).

---

## Tooling

StyleCop runs on every build with `TreatWarningsAsErrors=true` for the `PussyCats.*` projects. Suppressions require an inline `// justification: ...` comment. The technical lead maintains the ruleset in `stylecop.json` at the solution root.

---

## Out of scope (intentionally not rules)

- File header copyright comments — not required.
- `var` vs explicit type — author's choice. Reviewer can suggest but not block.
- Brace style, spacing, line length — handled by `.editorconfig` + `dotnet format`. Run before pushing.
- Comments quantity — write them when intent isn't obvious. XML doc comments required only on public service interfaces.
