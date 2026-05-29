# Coding Style and Conventions

Internal coding standard for the merged team.

17 rules total, grouped by topic. The technical lead reviews PRs against this document.

## Naming and structure

1. PascalCase for types, methods, properties, and public fields. camelCase for parameters and locals. `_camelCase` for private fields is forbidden, use plain `camelCase` and rely on `this.` only when there's a real name collision.

2. Interfaces are prefixed with `I`. Abstract classes are not prefixed

3. Namespaces follow the folder structure exactly, rooted at `PussyCats.<ProjectName>`.\*\* A file at `src/PussyCats.Library/Repositories/Users/UserRepository.cs` is in `PussyCats.Library.Repositories.Users`.

4. One public type per file. Filename equals type name.

## Layering and architecture

5. Strict project references, enforced by `.csproj` references, not just convention:

- `App` references `Library` only (never `Api`)
- `Api` references `Library` only
- `Tests` references `Library` and `App`
- `Library` references nothing

If a PR adds a forbidden reference, it's rejected.

6. No business logic in the UI layer or in repositories. Views and view models are pure gui; services deal with business logic; repositories return data. State machines (e.g. `MatchStatus` ) live in services.

7. Domain links are object references, not foreign-key ints. `Match.User` (type `User`), not `Match.UserId` alone. Keep the FK property only because EF needs it; expose the navigation property as the primary access path.

8. Repositories expose `IReadOnlyList<T>`, never `List<T>` or `IList<T>`. Mutation happens through `Add`/`Update`/`Remove` methods.

## Async and threading

9. Methods that perform I/O are async and end in `Async`. Sync versions exist only when WinUI binding requires sync.

## EF Core and data access

10. The `DbContext` is internal to `Library`. Services depend on `IXRepository` interfaces, never on `DbContext` directly.

11. All entity configuration goes in `IEntityTypeConfiguration<T>` files under `Library/Persistence/Configurations/`. No fluent configuration in `OnModelCreating` itself beyond `ApplyConfigurationsFromAssembly`.

12. Migrations are committed to the repo. Never edit a migration after it's been merged to main. If the schema is wrong, write a new migration.

13. No raw SQL, stored procedures, or DB views. LINQ to Entities only. If a query is too complex for LINQ, split it into multiple queries or change the model.

## Repository proxies (App side)

14. A `*RepositoryProxy` implements the same `IRepository` interface as the EF version. It does HTTP and nothing else, no caching, no business logic, no model mapping beyond JSON deserialization.

## WinUI and MVVM

15. View code-behind contains only event-to-command wiring and view-only logic (animations, focus). All state and business calls go through the view model.

16. View models inherit from `CommunityToolkit.Mvvm.ComponentModel.ObservableObject`. Use `[ObservableProperty]` and `[RelayCommand]` source generators rather than hand-rolled `INotifyPropertyChanged` boilerplate.

## Errors and validation

17. Exceptions are for exceptional cases. Expected failures (validation errors, not-found lookups) return `null`, a result type, or a domain-specific response they don't throw. A user typing a bad email is not exceptional, but a database connection dying is exceptional.

## Tests

Test framework is xUnit + FluentAssertions. One test class per production class. Test method names use `MethodConditionExpectedResult`:

Each test arranges, acts, asserts in that order. One logical assertion per test.
