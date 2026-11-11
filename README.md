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
- Send it as `Authorization: Bearer <token>`. In Swagger UI: sign in, copy `token`, click **Authorize** and paste it.
- Roles: `Caregiver`, `FamilyMember`, `Admin`. The public sign-up never creates an `Admin` (400).
- `DELETE /api/v1/users/{id}` is a logical delete (status `Inactive`). **A deactivated user keeps their current token
  until it expires (15 minutes at most)**, but cannot sign in again.
- `GET /api/v1/users?email=` is public and returns only `[{id, email}]`; it exists for compatibility with the frontend.

## Conventions

- Routes: `api/v1/<kebab-case-plural>`; JSON in camelCase; enums as strings.
- Errors: RFC 7807 `application/problem+json` with a `message` extension.
- Localization: `Accept-Language: en` (default) or `es`.
- Dates: UTC instants ending in `"Z"`; date-only fields as `yyyy-MM-dd`.
