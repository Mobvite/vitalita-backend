# Vitalita API image (Render, or any Docker host).
# Build:  docker build -t vitalita-api .
# The configuration comes only from environment variables (see README > Deployment).

# --- Build stage: restore and publish Vitalita.API with the .NET 10 SDK ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, so the package layer is cached while only the source changes.
COPY global.json ./
COPY Vitalita.API/Vitalita.API.csproj Vitalita.API/
RUN dotnet restore Vitalita.API/Vitalita.API.csproj

COPY Vitalita.API/ Vitalita.API/
RUN dotnet publish Vitalita.API/Vitalita.API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

# --- Runtime stage: ASP.NET Core runtime only, non-root user ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Plain HTTP on 8080: TLS is terminated by the platform proxy (Render).
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

# Non-root user provided by the official .NET images (APP_UID = 1654).
USER $APP_UID

EXPOSE 8080
ENTRYPOINT ["dotnet", "Vitalita.API.dll"]
