# Contributing to Vitalita API

This guide is the source code style guide and conventions of the Vitalita backend
(team Mobvite, course 1ASI0730 – Aplicaciones Web, UPC).

## Branching (GitFlow)

| Branch | Purpose |
|---|---|
| `main` | Stable code, updated only at sprint deliveries. |
| `develop` | Integration branch. Every feature is merged here first. |
| `feature/<topic>` | New functionality, e.g. `feature/profiles`. |
| `fix/<topic>` | Bug fixes. |
| `docs/<topic>` | Documentation only. |
| `test/<topic>` | Tests only. |

Rules:

1. Create your branch **from an up-to-date `develop`** (`git pull` first).
2. Open a pull request towards `develop`; `develop` is merged into `main` only at a delivery.
3. Merge pull requests with **"Create a merge commit"** (no squash), so each author's commits are preserved.
4. Before creating an EF Core migration, pull `develop`; merge migrations one pull request at a time.

## Commit messages (Conventional Commits)

Format: `<type>(<scope>): <description>`, in English, imperative mood, lowercase, no final period.

| Type | Use |
|---|---|
| `feat` | New functionality. |
| `fix` | Bug fix. |
| `refactor` | Code change that does not alter behavior. |
| `test` | Adding or changing tests. |
| `docs` | Documentation only. |
| `build` | Build, Docker or dependency changes. |
| `chore` | Maintenance tasks. |

The scope is the bounded context or area: `iam`, `profiles`, `monitoring`, `shared`.

Examples: `feat(profiles): add domain model`, `test(iam): add authentication integration tests`,
`fix(shared): translate validation problem titles`.

Keep commits small and focused: one logical change per commit.

## Architecture and layering

The API is a modular monolith organized by bounded context (`IAM`, `Profiles`, `Monitoring`, `Shared`).
Each context follows Domain-Driven Design with four layers:

| Layer | Contains | May depend on |
|---|---|---|
| `Domain` | Aggregates, value objects, commands, queries, domain exceptions, repository interfaces | Nothing |
| `Application` | Command and query services, access policies, error enums | Domain |
| `Infrastructure` | EF Core configuration, repositories, hashing, tokens | Domain, Application |
| `Interfaces` | REST controllers, resources, assemblers, ACL facades | Application, Domain |

Other conventions:

- Commands change state, queries read state (CQS). Services return `Result<TValue, TError>` instead of throwing for business errors.
- A context never references another context's domain. It talks to it through an ACL facade
  (for example `IIamContextFacade`, `IProfilesContextFacade`).
- Assemblers are static classes that convert between resources, commands and entities.

## C# style

- Target: .NET 10, C# 14, `Nullable` and `ImplicitUsings` enabled, file-scoped namespaces.
- `PascalCase` for types, methods, properties and public members; `camelCase` for locals and parameters.
- Interfaces start with `I` (`IUserRepository`); asynchronous methods end with `Async`.
- One public type per file; the file name matches the type name.
- No business logic in controllers: they only translate HTTP to commands/queries and back.

## API conventions

- Routes: `api/v1/<kebab-case-plural>`; JSON in camelCase; enums serialized as strings.
- Errors: RFC 7807 `application/problem+json` with a `message` extension.
- Localization: `Accept-Language: en` (default) or `es`.
- Dates: UTC instants ending in `"Z"`; date-only fields as `yyyy-MM-dd`.
- Every endpoint is documented in Swagger (summary, parameters, examples and responses).

## Database

- Tables in `snake_case` and plural (`older_adults`, `family_accesses`).
- Configuration with the EF Core Fluent API, never data annotations on entities.
- Schema changes go through migrations applied at startup.

## Testing

- `Vitalita.API.Tests`: unit tests of domain, policies and handlers, using in-memory fakes of the repositories.
- `Vitalita.API.IntegrationTests`: end-to-end tests with `WebApplicationFactory` against a MySQL test database.
- Test names describe the behavior (`Method_Scenario_ExpectedResult`).
- Acceptance tests are written as Gherkin `.feature` files; each scenario is tagged with the ID of the user story it verifies (for example `@US11`).
- A pull request must keep `dotnet test` green.

## Secrets

- Never commit secrets: `.env`, user-secrets and `.test-connection` stay local (git-ignored).
- Production values are provided as environment variables only.
