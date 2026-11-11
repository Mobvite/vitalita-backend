# Vitalita API

REST API of Vitalita. ASP.NET Core, .NET 10, DDD, MySQL.

## Local setup

Requirements: .NET SDK 10, Docker.

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

3. **Run**

   ```bash
   dotnet run --project Vitalita.API --launch-profile http
   ```

   Pending migrations are applied on startup.

   - API: `http://localhost:5240/api/v1`
   - OpenAPI document: `http://localhost:5240/openapi/v1.json`
   - Scalar: `http://localhost:5240/scalar`

4. **Tests**

   ```bash
   dotnet test
   ```

## Configuration in production

Use environment variables, never committed files:

| Setting | Environment variable |
|---|---|
| MySQL connection string | `ConnectionStrings__DefaultConnection` |
| Allowed frontend origins (CORS) | `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, ... |

In development, CORS allows `http://localhost:5173` (Vite) via `appsettings.Development.json`.

## Conventions

- Routes: `api/v1/<kebab-case-plural>`; JSON in camelCase; enums as strings.
- Errors: RFC 7807 `application/problem+json` with a `message` extension.
- Localization: `Accept-Language: en` (default) or `es`.
- Dates: UTC instants ending in `"Z"`; date-only fields as `yyyy-MM-dd`.
