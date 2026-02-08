# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Demodeck Tenant API is an ASP.NET Core 8.0 Web API that serves as a multi-tenant configuration and management service for the Demodeck platform. It provides tenant configuration (including YARP routing info), manager authentication via JWT, and release management. All data is stored in-memory (no database) with seed data — this is a demo/showcase application.

## Build & Run Commands

```bash
# Build
dotnet build

# Run (launches on http://localhost:5121)
dotnet run

# Run with specific environment
dotnet run --environment Development

# Build for release
dotnet build -c Release

# Docker build
docker build -t demodeck-tenant-api .
```

Swagger UI is available at `/swagger` in Development environment only.

There are no tests in this project currently.

## Architecture

### Layered Structure

```
Controllers/  →  Services/  →  Repositories (in-memory)
     ↓              ↓               ↓
  API layer    Business logic    Data access (ConcurrentDictionary/List)
```

All interfaces are defined in `Services/Interfaces.cs`. All models and DTOs are in `Models/TenantModels.cs`.

### Dependency Injection Lifetimes

- **Repositories** — registered as **Singleton** (in-memory state must persist for app lifetime)
- **Services** — registered as **Scoped**
- **JwtSettings** — registered as **Singleton**
- **ApiEndpointsSettings** — bound via `IOptions<>` pattern

### Key Domains

| Domain | Controller | Service | Repository |
|--------|-----------|---------|------------|
| Tenants | `TenantController` | `TenantService` | `InMemoryTenantRepository` |
| Managers (Auth) | `ManagerController` | `ManagerService` | `InMemoryManagerRepository` |
| Releases | `ReleaseController` | `ReleaseService` | `InMemoryReleaseRepository` |
| Health | `HealthController` | — | — |

### API Response Pattern

All endpoints return `ApiResponse<T>` which wraps responses with `Success`, `Data`, `Message`, and optional `ErrorCode` fields. Error codes follow the pattern `TENANT_NOT_FOUND`, `INVALID_CREDENTIALS`, etc.

### Authentication

JWT bearer authentication using symmetric key (HMAC-SHA256). Token endpoint is `POST /api/manager/token` which accepts `LoginRequest` and returns `LoginResponse` with token. BCrypt is used for password hashing.

### Multi-Tenant Design

Each `TenantDto` contains embedded YARP routing configuration: `ServiceName`, `IsHealthy`, `CustomHeaders`, and per-tenant API endpoint URLs (`AuthAPI`, `ProductAPI`). API endpoint base URLs come from `ApiEndpoints` configuration and are injected into tenant responses.

### Middleware Pipeline Order

Swagger (dev only) → HTTPS Redirection → CORS → Authentication → Authorization → Controllers

### Observability

OpenTelemetry tracing is configured with ASP.NET Core and HTTP client instrumentation. Traces export via OTLP to a Tempo backend. The `/health` endpoint is excluded from tracing.

## Configuration

Environment-specific settings are in `appsettings.json` and `appsettings.Development.json`. Key configuration sections:

- `JwtSettings` — secret key, issuer, audience, token lifetime
- `Cors` — allowed origins and credential sharing
- `ApiEndpoints` — AuthAPI and ProductAPI base URLs
- `OpenTelemetry:OtlpEndpoint` — trace export destination

Development overrides point API endpoints to localhost services (ports 5130 for Auth, 5142 for Product) and CORS to `http://localhost:8080`.
