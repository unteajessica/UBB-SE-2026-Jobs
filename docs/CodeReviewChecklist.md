# Code Review Checklist

Use this when reviewing a PR. Skip sections that don't apply (a docs-only PR doesn't need the EF Core block). Leave a comment like `Reviewed with checklist v1` so the author knows it was a real pass.

---

## Build and correctness

- [ ] Branch builds clean locally (`dotnet build`) — no errors, no new warnings.
- [ ] All tests pass (`dotnet test`).
- [ ] No commented-out code blocks. If it's not used, delete it; git remembers.
- [ ] No `Console.WriteLine`, `Debug.WriteLine`, or `TODO/FIXME` left behind without a linked issue.
- [ ] No hardcoded paths, connection strings, URLs, or magic numbers — they live in config or named constants.

## Style and conventions

- [ ] Follows `CodingStyle.md` (naming, file structure, namespace = folder).
- [ ] StyleCop reports no new warnings. Any suppression has a `// justification: ...` comment.
- [ ] `.editorconfig` formatting applied (`dotnet format` before push).
- [ ] No unused `using` directives, fields, parameters, or private methods.

## Layering (the assignment hard rule)

- [ ] No business logic in a view, code-behind, or `*RepositoryProxy` — only orchestration and HTTP/JSON.
- [ ] No business logic in a repository — only data access.
- [ ] App project does not register the EF `Repository` class for any `IRepository` — only the `*RepositoryProxy`.
- [ ] Controllers are thin — they call a repository or service method, return its result.
- [ ] Services don't reference WinUI types (`Microsoft.UI.*`) or ASP.NET types (`Microsoft.AspNetCore.*`).
- [ ] App's `.csproj` does not reference `PussyCats.Api`.

## Domain modeling

- [ ] New entity relationships use object navigation properties, not bare `IdOfX` ints (FK property is OK alongside the nav prop, not as the only access path).
- [ ] No new duplicated entities — check `Library/Domain/` first before adding a new class.
- [ ] Enums used instead of string constants for closed sets (e.g. `MatchStatus`, `JobRole`).
- [ ] Public DTOs aren't leaking EF tracking concerns (lazy-loaded collections, `ICollection<T>`).

## EF Core and migrations

- [ ] If the entity model changed, a migration was added (`dotnet ef migrations add ...`) and committed.
- [ ] Migration is named descriptively (`AddUserPersonalityResult`, not `Migration5`).
- [ ] No edits to old migrations.
- [ ] No raw SQL, stored procs, or `FromSqlRaw` calls.
- [ ] Queries don't trigger N+1: `.Include()` or projection used where related data is read in a loop.
- [ ] No tracked entities returned from a read-only query — `.AsNoTracking()` for query endpoints.

## API

- [ ] New endpoints follow the route conventions in `MergePlan.md` (`/api/{aggregate}/{id}/...`).
- [ ] Verbs match semantics: GET reads, POST creates, PUT replaces, PATCH partial-updates, DELETE removes.
- [ ] Response codes correct: 200 with body, 201 + Location on create, 204 on delete, 400 on bad input, 404 on not found.
- [ ] Validation runs (DataAnnotations + ModelState check, or FluentValidation).
- [ ] Errors return ProblemDetails — no raw exception messages leaking to the client.
- [ ] File uploads cap at a sensible size and reject unsupported content types.

## Repository proxies

- [ ] Every new endpoint added in the API has a matching method on the corresponding `IRepository` and on the `*RepositoryProxy`.
- [ ] Proxy uses the configured base URL from `ApiConfiguration`, not a hardcoded string.
- [ ] HTTP errors are translated into the same return shape the EF repo would use (e.g. 404 → `null`, not a thrown exception, when the EF version returns `null`).

## WinUI and MVVM

- [ ] No new `Window` opened — additional surfaces use `ContentDialog`, `Flyout`, or navigation to a new page in the existing Frame.
- [ ] View model exposes data through `[ObservableProperty]` / `[RelayCommand]` — not hand-rolled `INotifyPropertyChanged`.
- [ ] No `Task.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` on the UI thread.
- [ ] Services and repositories arrive via constructor — no `new` of dependencies inside a view model.
- [ ] Bindings work in design-time mode (or are documented as runtime-only).

## Tests

- [ ] New code has tests. Bug fixes include a failing-then-passing test.
- [ ] Test method name describes condition and expected result (`Method_condition_expected_result`).
- [ ] Service tests use `Fake*Repository` from `Tests/Fakes/`, never the EF repo.
- [ ] No test depends on test execution order.
- [ ] No real network, real database, or real file system in unit tests.
- [ ] Coverage didn't drop on the changed files.

## Security and safety

- [ ] No secrets committed (API keys, DB passwords, tokens).
- [ ] User-supplied input is validated server-side (client-side validation is UX, not security).
- [ ] File paths from user input are sanitized — no `../` traversal possible.
- [ ] No `eval`-equivalent (dynamic SQL string concat, `Activator.CreateInstance` from user data).

## Documentation

- [ ] Public service and repository interfaces have XML doc comments on each method (`/// <summary>`).
- [ ] If the public API surface changed, the README or relevant doc was updated.
- [ ] If a new external dependency was added, it's mentioned in the PR description with rationale.

## Merge-specific (during the merge sprint only)

- [ ] When porting a PussyCats class: namespace renamed to `PussyCats.<Project>.<Area>`, MSTest replaced with xUnit, raw SQL repo replaced with EF.
- [ ] When porting a matchmaking class: `matchmaking.*` namespace renamed to `PussyCats.*`, lowercase fixed.
- [ ] If a mock was replaced with a real call: confirm the real implementation is reachable (project reference + DI registration both present).
- [ ] If a mock was kept: documented why in code (`// mock: belongs to <other half>, see MergePlan.md §8`).
- [ ] No duplicate `User`/`UserProfile`/`Skill`/`Job` definitions remain after this PR lands.

## PR hygiene

- [ ] PR title summarizes the change in <=72 characters.
- [ ] Description explains *why*, not just *what*.
- [ ] PR is reasonably scoped — under ~500 lines of net change where possible. If huge, justify in description.
- [ ] Linked to the task / issue.
- [ ] At least one approving review from someone other than the author. The technical lead approves anything that touches layering, the domain model, or migrations.
