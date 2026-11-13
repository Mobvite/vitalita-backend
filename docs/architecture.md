# Vitalita API – Architecture overview

Vitalita API is a **modular monolith** built with ASP.NET Core (.NET 10, C# 14) and organized by
**bounded contexts** following Domain-Driven Design (DDD). It exposes a REST API under `/api/v1`
and persists data in MySQL through Entity Framework Core.

## Bounded contexts

| Context | Responsibility | Main aggregates |
|---|---|---|
| `IAM` | Users, authentication (JWT), roles (Caregiver, FamilyMember, Admin). | `User` |
| `Profiles` | Caregiver and family member profiles, older adults and family access (invitations). | `CaregiverProfile`, `FamilyMemberProfile`, `OlderAdult`, `FamilyAccess` |
| `Monitoring` | Daily health monitoring of an older adult. **Core context.** | `DailyReport`, `VitalSign`, `Medication` |
| `Shared` | Cross-cutting kernel: persistence base classes, `Result` type, localization, time, validation, security helpers. | – |

Contexts planned for the next sprints: Planning, Subscriptions, AssetManagement and Dashboard.

## Layers inside a context

```
<Context>/
  Domain/           aggregates, value objects, commands, queries, exceptions, repository interfaces
  Application/      command services, query services, access policies, error enums
  Infrastructure/   EF Core configuration, repository implementations, hashing, tokens
  Interfaces/       REST controllers, resources, assemblers, ACL facades
```

Dependencies point inwards: `Interfaces` and `Infrastructure` depend on `Application` and `Domain`;
`Domain` depends on nothing. Repositories are interfaces in the domain and implemented in infrastructure,
which makes it possible to replace them with in-memory fakes in unit tests.

## Communication between contexts (Anti-Corruption Layer)

A context never references another context's domain model. It consumes a **facade** published by the
other context:

| Facade | Published by | Used by | Purpose |
|---|---|---|---|
| `IIamContextFacade` | IAM | Profiles | Check that a user exists. |
| `IProfilesContextFacade` | Profiles | Monitoring | Check that an older adult exists and whether a user can access or manage it. |

## Request flow

```
HTTP request
  -> Controller (Interfaces/REST)           validates the input and builds a command or query
  -> Command / Query service (Application)  applies access policy and business rules
  -> Aggregate (Domain)                     enforces its own invariants
  -> Repository + Unit of Work (Infrastructure) -> MySQL
  <- Result<TValue, TError>                 mapped to a resource or to a ProblemDetails response
```

## Cross-cutting decisions

- **Security:** JWT bearer (15 minutes), BCrypt password hashing, roles and an `AdminOnly` policy; ownership
  rules decide who can read or write each older adult's data.
- **Errors:** RFC 7807 `application/problem+json` with a `message` extension.
- **Localization:** `Accept-Language` `en` (default) or `es`, resolved with `.resx` resources.
- **Dates:** UTC instants ending in `"Z"`; date-only fields as `yyyy-MM-dd`; Peru time derived from UTC-5.
- **Persistence:** snake_case plural tables, EF Core Fluent API, migrations applied at startup, audit fields.
- **Documentation:** OpenAPI document plus Swagger UI at `/swagger`.
- **Deployment:** Docker image published to a cloud host, MySQL as a managed database,
  configuration through environment variables.

## Suggested diagrams for the report (Chapter IV)

- C4 context and container diagrams: Web Application (Vue) -> Vitalita API -> MySQL.
- C4 component diagram of one context (for example Profiles): controllers, services, repositories.
- Class diagram of the `Profiles` and `Monitoring` aggregates.
