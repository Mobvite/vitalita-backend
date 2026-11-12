# Vitalita API

REST API of Vitalita. ASP.NET Core, .NET 10, DDD, MySQL.

## Local setup

Requirements: .NET SDK 10, Docker.

0. **Tools** (local manifest `dotnet-tools.json`, installs `dotnet-ef` 10.0.13)

   ```bash
   dotnet tool restore
   ```

1. **Database (MySQL 8.4, development only)**

   ```bash
   cp .env.example .env        # set your own passwords; .env is git-ignored
   docker compose up -d
   docker compose ps           # wait until vitalita-mysql is "healthy"
   ```

   If port 3306 is already in use, set `MYSQL_PORT=3307` (or another port) in `.env`.

2. **Connection string (user-secrets, never in appsettings)**

   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
     "server=localhost;port=3306;database=vitalita;user=vitalita;password=<MYSQL_PASSWORD from .env>" \
     --project Vitalita.API
   ```

3. **Authentication secrets (user-secrets, never in appsettings)**

   ```bash
   # JWT signing secret (required, at least 32 bytes)
   dotnet user-secrets set "Authentication:Jwt:Secret" "$(openssl rand -base64 48)" --project Vitalita.API

   # First administrator (optional; created on startup only when both values are set)
   dotnet user-secrets set "Authentication:Admin:Email" "<admin email>" --project Vitalita.API
   dotnet user-secrets set "Authentication:Admin:Password" "<8-72 characters>" --project Vitalita.API
   ```

   Without the JWT secret the API does not start (the settings are validated on startup). If the administrator
   email already exists, that user is left unchanged.

4. **Run**

   ```bash
   dotnet run --project Vitalita.API --launch-profile http
   ```

   Pending migrations are applied on startup.

   - API: `http://localhost:5240/api/v1`
   - Swagger UI: `http://localhost:5240/swagger` (the root `http://localhost:5240/` redirects here)
   - OpenAPI document: `http://localhost:5240/swagger/v1/swagger.json`

5. **Tests**

   ```bash
   dotnet test
   ```

## Configuration in production

Use environment variables, never committed files:

| Setting | Environment variable |
|---|---|
| MySQL connection string | `ConnectionStrings__DefaultConnection` |
| Allowed frontend origins (CORS) | `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, ... |
| JWT signing secret (required, at least 32 bytes) | `Authentication__Jwt__Secret` |
| JWT lifetime in minutes (optional, 1-15, default 15) | `Authentication__Jwt__ExpirationMinutes` |
| First administrator email (optional) | `Authentication__Admin__Email` |
| First administrator password (optional, 8-72 characters) | `Authentication__Admin__Password` |

In development, CORS allows `http://localhost:5173` (Vite) via `appsettings.Development.json`.

## Authentication

- `POST /api/v1/authentication/sign-up` and `POST /api/v1/authentication/sign-in` are public. Sign-in returns a JWT
  (HS256, claims `sub`, `email`, `role`) that lasts `Authentication:Jwt:ExpirationMinutes` (15 minutes at most).
- `GET /api/v1/authentication/me` (Bearer) returns the user of the token (`id`, `email`, `role`, `status`,
  `createdAt`, `lastLoginAt`), read again from the database: `401` if the user no longer exists or is inactive.
- `POST /api/v1/authentication/refresh` (Bearer, no body) checks again that the user exists and is active and returns
  a new token with the same shape as sign-in. The new token lasts the configured minutes (15 at most); a deactivated
  user cannot refresh (`401`).
- Send it as `Authorization: Bearer <token>`. In Swagger UI: sign in, copy `token`, click **Authorize** and paste it.
- Roles: `Caregiver`, `FamilyMember`, `Admin`. The public sign-up never creates an `Admin` (400).
- `DELETE /api/v1/users/{id}` is a logical delete (status `Inactive`). **A deactivated user keeps their current token
  until it expires (15 minutes at most)**, but cannot sign in again.
- `GET /api/v1/users?email=` is public and returns only `[{id, email}]`; it exists for compatibility with the frontend.

## Profiles

Endpoints: `/api/v1/caregiver-profiles`, `/api/v1/family-member-profiles`, `/api/v1/older-adults` and
`/api/v1/family-access` (singular route, table `family_accesses`). Each one has `GET` (list with filters), `GET /{id}`,
`POST` and `PUT /{id}`; there is no `DELETE` (an older adult is archived with `status: "Archived"`).

- Every endpoint needs a token, except `GET /family-access?invitationCode=`, which is public and returns only
  `id`, `invitedEmail`, `invitationCode`, `relationship`, `status` and `expiresAt` of a pending, unexpired invitation
  (otherwise `[]`).
- Ownership: a user creates and edits only their own profile; a caregiver manages only the older adults in their charge
  and their invitations; a family member reads an older adult only with an `Active` access; `Admin` can do everything.
- Lists without filters are for administrators. A filter that points to another user's data returns `403`; batch
  lookups (`?id=1&id=2`) leave out what the requester cannot read.
- A family member profile can only be created when the email of the token has a pending, unexpired invitation.
- Invitation codes are generated by the server with a cryptographic generator (`VITA-` + 8 characters) and expire in
  7 days. Pending → Active only by the invited email; → Revoked only by the caregiver; Revoked is final.
- Other contexts use `IProfilesContextFacade` (`Profiles/Interfaces/ACL`).

Full flow against a running API (needs `curl` and `jq`):

```bash
./scripts/profiles-flow.sh                       # API_URL=http://localhost:5240 by default
```

## Monitoring

Endpoints: `/api/v1/daily-reports`, `/api/v1/vital-signs` and `/api/v1/medications`. Each one has `GET` (list),
`GET /{id}`, `POST` and `PUT /{id}`; there is no `DELETE` (a medication is deactivated with `active: false`).

- Every endpoint needs a token. Lists are filtered with `?olderAdultId=` (plus `?active=` for medications); without
  a filter they are for administrators only.
- Ownership through the Profiles ACL (`IProfilesContextFacade`): reads need the caregiver in charge or a family member
  with an `Active` access; writes need the caregiver in charge; `Admin` can do everything. An older adult the
  requester cannot access, or that does not exist, returns `403` (never `404`).
- `reportDate` and `measuredAt` are UTC instants (ending in `Z`) and cannot be in the future (5 minutes of tolerance).
  `recordedByUserId` comes from the token. `scheduleTimes` are wall-clock `HH:mm` values.
- Vital sign ranges (same as the frontend): HeartRate 20-250 bpm, BloodPressure 50-260 / 30-160 mmHg (systolic
  greater than diastolic, diastolic in `secondaryValue`), OxygenSaturation 50-100 %, Temperature 30-45 °C, Glucose
  20-600 mg/dL. The unit is derived from the type.
- Known limitation: records can still be added to an older adult with status `Archived` (the Profiles ACL does not
  expose the status yet).

Full flow against a running API (needs `curl` and `jq`):

```bash
./scripts/monitoring-flow.sh                     # API_URL=http://localhost:5240 by default
```

## Conventions

- Routes: `api/v1/<kebab-case-plural>`; JSON in camelCase; enums as strings.
- Errors: RFC 7807 `application/problem+json` with a `message` extension.
- Localization: `Accept-Language: en` (default) or `es`.
- Dates: UTC instants ending in `"Z"`; date-only fields as `yyyy-MM-dd`.
