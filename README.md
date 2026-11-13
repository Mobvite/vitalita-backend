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
   dotnet test Vitalita.API.Tests              # unit tests, no database
   dotnet test Vitalita.API.IntegrationTests   # integration tests, need vitalita_test (see below)
   dotnet test                                 # both
   ```

## Integration tests

`Vitalita.API.IntegrationTests` hosts the real API in memory (`WebApplicationFactory<Program>`) and calls it over HTTP
against the Docker MySQL, in a separate database `vitalita_test`. Migrations and the test administrator are applied by
the API startup. The development database is never touched: the tests refuse to start unless the database name ends
with `_test`.

**1. Create the database (once).** It asks for the MySQL root password interactively:

```bash
docker exec -it vitalita-mysql mysql -uroot -p -e "CREATE DATABASE IF NOT EXISTS vitalita_test CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci; GRANT ALL PRIVILEGES ON vitalita_test.* TO 'vitalita'@'%'; FLUSH PRIVILEGES;"
```

**2. Set the connection string** (never in code or versioned files), with one of these options:

```bash
# a) environment variable (takes precedence)
export VITALITA_TEST_CONNECTION="server=127.0.0.1;port=3307;database=vitalita_test;user=vitalita;password=<your password>"

# b) git-ignored file with that same string on a single line
#    Vitalita.API.IntegrationTests/.test-connection
```

If neither exists, every test fails with a message that explains both options (it never prints the string).
The JWT secret and the administrator (`admin@vitalita.test`) are test-only values injected as in-memory configuration
by `VitalitaApiFactory`; the environment is `Testing`, so your user-secrets are not loaded.

**3. Run** `dotnet test Vitalita.API.IntegrationTests`.

**Isolation.** All classes share one API host (xUnit collection fixture `Api`, run one at a time). Each test creates
its own users with unique emails (`<prefix>.<guid>@vitalita.test`), so tests do not depend on their order or on data left
by earlier runs. Data accumulates in `vitalita_test`; drop and recreate the database whenever you want a clean slate.

| Test class | Endpoints covered |
|---|---|
| `IamEndpointsTests` | `POST /authentication/sign-up`, `POST /authentication/sign-in`, `GET /authentication/me`, `POST /authentication/refresh`, `GET /users` (admin list, anonymous `?email=` lookup), `GET /users/{id}`, `POST /users`, `PUT /users/{id}`, `DELETE /users/{id}` (soft delete) |
| `ProfilesEndpointsTests` | `POST`/`GET /caregiver-profiles` (`?userId=`, admin list, `/{id}`), `POST`/`PUT`/`GET /older-adults` (`?caregiverId=`, `?id=` batch, `/{id}`), `POST /family-access`, `GET /family-access?invitationCode=` (public), `GET /family-access?familyMemberId=&status=`, `PUT /family-access/{id}` (acceptance), `POST`/`GET /family-member-profiles` (`?id=`) |
| `MonitoringEndpointsTests` | `POST`/`PUT`/`GET /daily-reports` (`?olderAdultId=`, `/{id}`), `POST`/`GET /vital-signs` (`?olderAdultId=`, `/{id}`), `POST`/`PUT`/`GET /medications` (`?olderAdultId=&active=`, admin list) |
| `LocalizationTests` | `Accept-Language: en`/`es` on `POST /authentication/sign-up` (validation), `GET /daily-reports` (business 403) and `GET /authentication/me` (framework 401) |

Each test checks the status code, the key JSON fields and, for errors, the `application/problem+json` content type and
the `message` extension (literal `.resx` texts, so a changed translation fails a test).

## Deployment

The API ships as a Docker image (`Dockerfile` at the repository root): a multi-stage build that publishes
`Vitalita.API` with the .NET 10 SDK and runs it on the ASP.NET Core runtime image as the non-root user `app`, listening
on plain HTTP port **8080** (`ASPNETCORE_HTTP_PORTS=8080`, `ASPNETCORE_ENVIRONMENT=Production`). Test projects,
secrets and local files are excluded by `.dockerignore`.

### Configuration (environment variables only)

In Production the configuration comes only from environment variables; `appsettings.json` holds no secrets and
user-secrets are not loaded. The startup fails with one message that lists every missing variable by name.

| Variable | Required | Purpose |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Yes | MySQL connection string |
| `Authentication__Jwt__Secret` | Yes | JWT signing secret, at least 32 bytes (e.g. `openssl rand -base64 48`) |
| `Cors__AllowedOrigins__0` | Yes (Production) | Frontend origin allowed by CORS; add `Cors__AllowedOrigins__1`, ... for more |
| `Authentication__Admin__Email` | Optional, together with the password | First administrator, created on startup if it does not exist |
| `Authentication__Admin__Password` | Optional, together with the email | First administrator password (8-72 characters) |
| `Authentication__Jwt__ExpirationMinutes` | Optional | Token lifetime, 1-15 (default 15) |

**Azure Database for MySQL** requires TLS: add `SslMode=Required;` to the connection string, for example
`server=<host>.mysql.database.azure.com;port=3306;database=vitalita;user=<user>;password=<password>;SslMode=Required;`.

In development, CORS allows `http://localhost:5173` (Vite) via `appsettings.Development.json`.

### Build and run locally

```bash
docker build -t vitalita-api .

docker run --rm -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="server=host.docker.internal;port=3307;database=<db>;user=vitalita;password=<password>" \
  -e Authentication__Jwt__Secret="<at least 32 bytes>" \
  -e Cors__AllowedOrigins__0="http://localhost:5173" \
  vitalita-api
```

Then open `http://localhost:8080/health` and `http://localhost:8080/swagger`. Migrations are applied on startup.

### Render

- New **Web Service** from this repository, runtime **Docker** (it uses the root `Dockerfile`).
- Set the environment variables above in the service settings, never in the repository.
- **Health check path:** `/health`. It returns `200 {"status":"Healthy"}`, is anonymous and does not query the database.
- Render terminates TLS at its proxy and forwards plain HTTP to port 8080. The API trusts `X-Forwarded-For` and
  `X-Forwarded-Proto` (so it sees the original `https` scheme and client IP) and redirects to HTTPS only in Development.
- API documentation (all environments): `https://<service>.onrender.com/swagger` (the root `/` redirects there) and
  the OpenAPI document at `/swagger/v1/swagger.json`.

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
