# WebTestApp — Project History & Context

## Overview
ASP.NET Core Web API (.NET 8) with JWT authentication and a single-page HTML frontend served as static files. No database — data is stored in memory for the lifetime of the process.

## Tech Stack
- **Runtime:** .NET 8 (SDK 10 installed on machine)
- **Auth:** JWT Bearer tokens via `Microsoft.AspNetCore.Authentication.JwtBearer`
- **API docs:** Swagger/OpenAPI via `Swashbuckle.AspNetCore`
- **Frontend:** Vanilla HTML + CSS + JavaScript (no framework, no build step)
- **Storage:** In-memory (`ConcurrentDictionary`) — data resets on app restart

## Running the App
```bash
cd "f:\SynologyDrive\My Documents\Work\AI\WebTestApp"
dotnet run
```
Then open `http://localhost:5000`.

> **Note:** A `NuGet.config` was added to force `nuget.org` as the sole package source, bypassing the corporate Azure DevOps feed that requires authentication.

## Demo Credentials
| Username | Password  | Role  |
|----------|-----------|-------|
| admin    | password123 | Admin |
| user     | letmein   | User  |

Credentials are hard-coded in `Services/AuthService.cs`. Replace with hashed passwords and a real user store before any production use.

## Project Structure
```
WebTestApp/
├── CLAUDE.md                          ← this file
├── NuGet.config                       ← forces nuget.org (bypasses corporate feed)
├── WebTestApp.csproj
├── appsettings.json                   ← JWT config (SecretKey, Issuer, Audience, ExpiryMinutes)
├── appsettings.Development.json
├── Program.cs                         ← DI, middleware pipeline, Swagger
├── Controllers/
│   ├── AuthController.cs              ← POST /api/auth/login (public)
│   ├── CompaniesController.cs         ← CRUD /api/companies (protected)
│   └── WeatherForecastController.cs   ← GET /api/weatherforecast (protected)
├── Models/
│   ├── Company.cs                     ← Company class + CompanyRequest record
│   ├── LoginRequest.cs
│   ├── LoginResponse.cs
│   └── WeatherForecast.cs
├── Services/
│   ├── IAuthService.cs
│   ├── AuthService.cs                 ← validates credentials, mints JWT
│   ├── ICompanyService.cs
│   └── CompanyService.cs              ← in-memory ConcurrentDictionary, Singleton
└── wwwroot/
    └── index.html                     ← SPA: login, sidebar nav, companies CRUD
```

## API Endpoints
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/auth/login` | None | Returns JWT token |
| GET | `/api/weatherforecast` | JWT | Sample protected endpoint |
| GET | `/api/companies` | JWT | List all companies (sorted by name) |
| GET | `/api/companies/{id}` | JWT | Get single company |
| POST | `/api/companies` | JWT | Create company |
| PUT | `/api/companies/{id}` | JWT | Update company |
| DELETE | `/api/companies/{id}` | JWT | Delete company |

Swagger UI is available at `/swagger` in Development.

## Company Model
Fields: `Id` (Guid, auto-generated), `Name` (required), `Description`, `Address`.

## Frontend (wwwroot/index.html)
Single self-contained HTML file — no build step, no npm, no framework.

- **Login page:** username + password form, posts to `/api/auth/login`
- **App shell:** fixed top bar (username, role badge, logout) + right sidebar navigation
- **Dashboard page:** fetches and displays weather data from the protected API
- **Companies page:** table with Add / Edit / Delete, modal form, delete confirmation, toast notifications
- JWT token stored in a JS variable (lost on page refresh — intentional for simplicity)

## Change History

### Session 1
- Created project from scratch (empty directory)
- Set up JWT authentication, Swagger with Bearer support
- Added `AuthController` (login) and `WeatherForecastController` (protected sample)
- Created `wwwroot/index.html` with basic login form → token display → fetch weather

### Session 2
- Added **Companies** feature (full CRUD, in-memory storage)
  - `Models/Company.cs` — `Company` class + `CompanyRequest` DTO
  - `Services/CompanyService.cs` — `ConcurrentDictionary`-backed singleton service
  - `Controllers/CompaniesController.cs` — REST CRUD endpoints, all protected
- Rewrote `wwwroot/index.html` as a proper SPA:
  - Fixed top bar with user info and logout
  - Right-side navigation sidebar (Dashboard, Companies)
  - Companies page with sortable table, Add/Edit modal, delete confirmation modal, toast notifications
  - Keyboard shortcuts: Enter to login, Escape to close modals
