# WebTestApp — Project History & Context

## Overview
ASP.NET Core Web API (.NET 8) with JWT authentication and a single-page HTML frontend served as static files. Data persisted in PostgreSQL via EF Core.

## Tech Stack
- **Runtime:** .NET 8 (SDK 10 installed on machine)
- **Auth:** JWT Bearer tokens via `Microsoft.AspNetCore.Authentication.JwtBearer`
- **API docs:** Swagger/OpenAPI via `Swashbuckle.AspNetCore`
- **Frontend:** Vanilla HTML + CSS + JavaScript (no framework, no build step)
- **Database:** PostgreSQL via Entity Framework Core 8 + Npgsql provider
- **ORM:** EF Core with code-first migrations (migrations auto-applied on startup)
- **AI:** Anthropic C# SDK (`Anthropic` NuGet) — `claude-opus-4-6` with adaptive thinking + tool use

## Running the App
```bash
cd "f:\SynologyDrive\My Documents\Work\AI\WebTestApp"
dotnet run
```
Then open `http://localhost:5000`. Migrations are applied automatically on startup.

> **Note:** A `NuGet.config` was added to force `nuget.org` as the sole package source, bypassing the corporate Azure DevOps feed that requires authentication.

## Database
- **Engine:** PostgreSQL (local, `localhost:5432`)
- **Database name:** `WebTestApp`
- **Connection string:** stored in `appsettings.Development.json` (git-ignored)
- `appsettings.json` contains a placeholder password (`CHANGE_ME`) — safe to commit
- Run migrations manually: `dotnet ef migrations add <Name>` then `dotnet ef database update`
- Tables: `Companies`, `Articles`, `CompanyArticle` (many-to-many join), `Users`

## Demo Credentials
| Username | Password | Role  |
|----------|----------|-------|
| admin    | pwd123   | Admin |
| user     | letmein  | User  |

Credentials are stored in the `Users` PostgreSQL table with hashed passwords (`PasswordHasher<User>` from ASP.NET Core Identity). Seeded automatically on first startup in `Program.cs`.

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
│   ├── ArticlesController.cs          ← CRUD /api/articles (protected, async)
│   ├── ChatController.cs              ← POST /api/chat (protected) — AI agent endpoint
│   ├── CompaniesController.cs         ← CRUD /api/companies + article sub-resources (protected, async)
│   └── WeatherForecastController.cs   ← GET /api/weatherforecast (protected)
├── Data/
│   └── AppDbContext.cs                ← EF Core DbContext (Companies, Articles, CompanyArticle, Users)
├── Migrations/                        ← EF Core generated migrations
├── Models/
│   ├── Article.cs                     ← Article entity + ArticleRequest DTO
│   ├── ChatModels.cs                  ← ChatMessage, ChatRequest, ChatResponse DTOs
│   ├── Company.cs                     ← Company entity + CompanyRequest + CompanyResponse DTOs
│   ├── LoginRequest.cs
│   ├── LoginResponse.cs
│   ├── User.cs                        ← User entity (Id, Username, PasswordHash, Role)
│   └── WeatherForecast.cs
├── Services/
│   ├── IArticleService.cs
│   ├── ArticleService.cs              ← EF Core async, Scoped
│   ├── IAuthService.cs                ← AuthenticateAsync
│   ├── AuthService.cs                 ← queries DB, verifies with PasswordHasher, mints JWT
│   ├── IChatService.cs                ← ChatAsync
│   ├── ChatService.cs                 ← Anthropic SDK, agentic tool-use loop, Scoped
│   ├── ICompanyService.cs
│   └── CompanyService.cs              ← EF Core async, Scoped
└── wwwroot/
    └── index.html                     ← SPA: login, sidebar nav, companies + articles CRUD, AI chat
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
| GET | `/api/companies/{id}/articles` | JWT | List articles linked to a company |
| POST | `/api/companies/{id}/articles/{articleId}` | JWT | Link an article to a company |
| DELETE | `/api/companies/{id}/articles/{articleId}` | JWT | Unlink an article from a company |
| GET | `/api/articles` | JWT | List all articles (sorted by code) |
| GET | `/api/articles/{id}` | JWT | Get single article |
| POST | `/api/articles` | JWT | Create article |
| PUT | `/api/articles/{id}` | JWT | Update article |
| DELETE | `/api/articles/{id}` | JWT | Delete article |
| POST | `/api/chat` | JWT | Send message to AI agent, returns reply |

Swagger UI is available at `/swagger` in Development.

## Company Model
Fields: `Id` (Guid, auto-generated), `Name` (required), `Description`, `Address`, `Articles` (navigation collection).

## Article Model
Fields: `Id` (Guid, auto-generated), `Code` (required, unique), `Description`, `ProductCode`.

## User Model
Fields: `Id` (Guid, auto-generated), `Username` (required, unique), `PasswordHash`, `Role`.

## Frontend (wwwroot/index.html)
Single self-contained HTML file — no build step, no npm, no framework.

- **Login page:** username + password form, posts to `/api/auth/login`
- **App shell:** fixed top bar (username, role badge, logout) + right sidebar navigation
- **Dashboard page:** fetches and displays weather data from the protected API
- **Companies page:** table with Add / Edit / Delete, modal form, delete confirmation, toast notifications
- **AI Chat:** floating "🤖 AI Chat" button (bottom-right), slide-up panel with message bubbles, typing indicator, conversation history maintained client-side per session
- JWT token stored in a JS variable (lost on page refresh — intentional for simplicity)

## AI Chat Agent
- **Endpoint:** `POST /api/chat` — body `{ messages: [{role, content}] }`, response `{ reply }`
- **Model:** `claude-opus-4-6` with adaptive thinking
- **Tools the agent can call:**
  - `list_companies` — all companies with article count
  - `get_company(id)` — company details + linked articles
  - `list_articles` — all articles
  - `get_article(id)` — article details + linked companies
- **Config:** `Claude:ApiKey` in `appsettings.Development.json` (git-ignored — user must supply key)
- Conversation history is kept client-side; full history sent with each request (stateless server)

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

### Session 3
- Created `CLAUDE.md` (this file) to track project history and context
- Initialised Git repository and pushed to GitHub
  - Remote: https://github.com/costiboshca/WebTestApp
  - Branch: `main`
  - Added `.gitignore` (excludes `bin/`, `obj/`, `.vs/`, secrets)
  - Git identity for this repo: Constantin Bosca &lt;costiboshca@yahoo.com&gt;
  - `gh` CLI not available; repo created via GitHub REST API, pushed over HTTPS
  - Token removed from remote URL after push (stored clean as `https://github.com/costiboshca/WebTestApp.git`)

### Session 6
- Moved users from hard-coded dictionary to **PostgreSQL `Users` table**
  - Added `Models/User.cs` — `User` entity (Id, Username, PasswordHash, Role)
  - `AppDbContext` extended with `DbSet<User>` and unique index on `Username`
  - `AuthService` now queries DB and verifies passwords with `PasswordHasher<User>` (ASP.NET Core Identity, no extra package)
  - `IAuthService.Authenticate` renamed to `AuthenticateAsync` — fully async
  - `AuthController` updated to async
  - `Program.cs` seeds `admin/pwd123` (Admin) and `user/letmein` (User) on first startup
  - Migration `AddUsers` applied — `Users` table created in PostgreSQL
  - Admin password changed from `password123` → `pwd123` (user edit)
- Added `Properties/launchSettings.json` to set `ASPNETCORE_ENVIRONMENT=Development` so `appsettings.Development.json` (real DB password) is loaded on `dotnet run`

### Session 5
- Migrated data storage from in-memory to **PostgreSQL** via Entity Framework Core
  - Added `Npgsql.EntityFrameworkCore.PostgreSQL` and `Microsoft.EntityFrameworkCore.Design` packages
  - Created `Data/AppDbContext.cs` with EF many-to-many mapping (`CompanyArticle` join table)
  - `Company.ArticleIds` (HashSet) replaced with `ICollection<Article> Articles` navigation property
  - `Article` gained `ICollection<Company> Companies` navigation property
  - Added `CompanyResponse` DTO to preserve the original `articleIds` JSON shape for the frontend
  - All service methods converted to `async/await` (services changed from Singleton to Scoped)
  - `Program.cs` registers `AppDbContext` and auto-applies migrations on startup
  - `appsettings.json` stores a placeholder password; real credentials in `appsettings.Development.json` (git-ignored)
  - Ran `dotnet ef migrations add InitialCreate` + `dotnet ef database update` — DB and all tables created
  - `dotnet-ef` global tool installed (`dotnet tool install --global dotnet-ef`)

### Session 7
- Added **AI Chat Agent** feature
  - `Anthropic` NuGet package (v12.11.0) — official C# SDK
  - `Models/ChatModels.cs` — `ChatMessage`, `ChatRequest`, `ChatResponse` DTOs
  - `Services/ChatService.cs` — `claude-opus-4-6` with adaptive thinking; agentic loop handles tool use until `end_turn`; 4 DB query tools (list/get companies & articles)
  - `Controllers/ChatController.cs` — `POST /api/chat`, JWT-protected
  - `appsettings.json` — added `Claude:ApiKey` placeholder (`CHANGE_ME`)
  - `appsettings.Development.json` — slot for real API key (git-ignored)
  - `wwwroot/index.html` — floating chat button (bottom-right), slide-up chat panel, message bubbles (user blue / assistant gray), typing indicator, auto-scroll, clear conversation, Escape to close

### Session 4
- Added **Articles** entity (Code, Description, ProductCode) with full CRUD
  - `Models/Article.cs` — `Article` class + `ArticleRequest` DTO
  - `Services/ArticleService.cs` — `ConcurrentDictionary`-backed singleton service
  - `Controllers/ArticlesController.cs` — REST CRUD, all protected
- Added **Company → Articles** association (many-to-many, in-memory)
  - `Company.ArticleIds` (`HashSet<Guid>`) on the Company model
  - `ICompanyService` extended with `AddArticle`, `RemoveArticle`, `GetArticleIds`
  - `CompaniesController` extended with sub-resource endpoints:
    - `GET  /api/companies/{id}/articles`
    - `POST /api/companies/{id}/articles/{articleId}`
    - `DELETE /api/companies/{id}/articles/{articleId}`
- Updated `wwwroot/index.html`:
  - Added **Articles** page (sidebar nav + CRUD table + Add/Edit/Delete modals)
  - Companies table: "Articles" button per row showing linked count chip
  - **Company Articles modal**: dropdown to link unlinked articles, list of linked articles with Remove button
  - Shared confirm-delete modal now handles both companies and articles
- Pushed all changes to GitHub
