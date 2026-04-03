# Insurance Platform

A full-stack insurance software platform serving underwriters, agents, brokers, and clients.

## Tech Stack

| Layer    | Technology                          |
|----------|-------------------------------------|
| Backend  | ASP.NET Core 10, EF Core 10, MediatR, Hangfire |
| Database | PostgreSQL                          |
| Frontend | Angular 21 (zoneless), Angular Material, TailwindCSS v4 |
| Auth     | JWT (bearer tokens)                 |
| Testing  | xUnit (backend), Vitest (frontend)  |
| CI       | GitHub Actions                      |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) with npm
- [PostgreSQL 16+](https://www.postgresql.org/)
- [Angular CLI 21](https://angular.dev/tools/cli): `npm install -g @angular/cli`

## Local Development Setup

### 1. Clone the repo

```bash
git clone <repo-url>
cd insurance-platform
```

### 2. Configure the backend

Copy and edit the development settings:

```bash
cp backend/InsurancePlatform.Api/appsettings.Development.json.example \
   backend/InsurancePlatform.Api/appsettings.Development.json
```

Set your database connection string and JWT secret in `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=insurance_platform_dev;Username=postgres;Password=yourpassword"
  },
  "JwtSettings": {
    "Secret": "your-local-dev-secret-min-32-chars",
    "Issuer": "InsurancePlatform",
    "Audience": "InsurancePlatform",
    "ExpiryMinutes": 60
  }
}
```

### 3. Set up the database

```bash
cd backend
dotnet ef database update --project InsurancePlatform.Infrastructure --startup-project InsurancePlatform.Api
```

### 4. Run the backend

```bash
cd backend
dotnet run --project InsurancePlatform.Api
```

API is available at `https://localhost:5001`.
Health check: `GET https://localhost:5001/health`
OpenAPI docs: `https://localhost:5001/openapi`

### 5. Install frontend dependencies

```bash
cd frontend
npm install
```

### 6. Run the frontend

```bash
cd frontend
ng serve
```

Frontend is available at `http://localhost:4200`.

## Running Tests

### Backend (xUnit)

```bash
cd backend
dotnet test
```

### Frontend (Vitest)

```bash
cd frontend
ng test
```

For a single run (no watch):

```bash
ng test --watch=false
```

## Code Style

- **Backend**: follows standard C# conventions, nullable reference types enabled.
- **Frontend**: Prettier (`.prettierrc`) + EditorConfig (`.editorconfig`). Run `npx prettier --write .` to format.

## Project Structure

```
.
├── .github/workflows/     # CI pipelines (backend.yml, frontend.yml)
├── backend/
│   ├── InsurancePlatform.Api/          # ASP.NET Core entry point, controllers, middleware
│   ├── InsurancePlatform.Application/  # CQRS commands/queries (MediatR), validators
│   ├── InsurancePlatform.Domain/       # Entities, enums, repository interfaces
│   ├── InsurancePlatform.Infrastructure/ # EF Core DbContext, repositories, Hangfire
│   └── InsurancePlatform.Tests/        # xUnit test project
└── frontend/
    └── src/
        ├── app/
        │   ├── core/        # Auth service, guards, interceptors
        │   ├── features/    # Lazy-loaded feature modules (admin, underwriter, agent, client)
        │   └── shared/      # Shared components, pipes, directives
        └── environments/    # Environment configs
```

## CI

GitHub Actions runs on every push and PR to `main`:

- **Backend**: restore → build → test → publish dry-run
- **Frontend**: install → test → production build

See `.github/workflows/` for details.
