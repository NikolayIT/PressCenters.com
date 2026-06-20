# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

PressCenters.com is an ASP.NET Core news aggregator that scrapes press releases from Bulgarian
government sites, institutions, NGOs, and state companies, then republishes them. Content is in
Bulgarian. The codebase follows the SoftUni "Simple Web Template" layered architecture.

## Commands

All commands are run from the repository root. The solution lives in `src/`.

```bash
# Build the whole solution
dotnet build src/PressCenters.sln

# Run all tests
dotnet test src/PressCenters.sln

# Run tests for a single project
dotnet test src/Tests/PressCenters.Services.Sources.Tests

# Run a single test class or method (xUnit filter)
dotnet test src/Tests/PressCenters.Services.Sources.Tests --filter "FullyQualifiedName~BnbBgSourceTests"
dotnet test src/Tests/PressCenters.Services.Sources.Tests --filter "FullyQualifiedName~BnbBgSourceTests.GetNewsShouldReturnResults"

# Run the web app (requires SQL Server + a DefaultConnection connection string)
dotnet run --project src/Web/PressCenters.Web

# Run the Sandbox console app (ad-hoc scraping, backfills, one-off maintenance)
dotnet run --project src/Tests/Sandbox
```

Projects target **net7.0** (`PressCenters.Data.Models` multi-targets `netstandard2.1;net6.0;net7.0`).
Visual Studio 2022 is the primary IDE (`.sln`, `.csproj.user`, `.vs/` present). The CI build
(`azure-pipelines.yml`) uses `VSBuild` + `VSTest` on `windows-2022`; there is also a CodeQL workflow
in `.github/workflows/`.

## Architecture

### Layered structure (`src/`)

- **PressCenters.Common** — cross-cutting helpers: `GlobalConstants` (system name, default user-agent),
  `ReflectionHelpers.GetInstance<T>(typeName)`, `StringExtensions`.
- **Data/** — `PressCenters.Data.Models` (EF entities), `PressCenters.Data` (`ApplicationDbContext`,
  migrations, repositories, seeding), `PressCenters.Data.Common` (base model types + repository interfaces).
- **Services/** — `PressCenters.Services` (DTOs like `RemoteNews`, `SlugGenerator`),
  `PressCenters.Services.Data` (business services over repositories), `PressCenters.Services.Sources`
  (the scrapers — the heart of the app), `PressCenters.Services.CronJobs` (Hangfire jobs),
  `PressCenters.Services.Mapping` (reflection-based AutoMapper), `PressCenters.Services.Messaging` (email).
- **Web/** — `PressCenters.Web` (MVC app, admin area, Hangfire wiring in `Startup.cs`),
  `PressCenters.Web.Infrastructure`, `PressCenters.Web.Proxy` (a thin HTML/asset proxy used by sources
  that set `UseProxy`, served at `proxy.presscenters.com`).
- **Tests/** — xUnit test projects + the `Sandbox` console runner.

### The scraping system (most important concept)

Each news provider is a class deriving from `BaseSource` (`Services/PressCenters.Services.Sources/`),
organized into folders by category: `BgInstitutions`, `Ministries`, `Municipalities`, `BgNgos`,
`BgStateCompanies`. A source implements:

- `BaseUrl` — the site root.
- `GetLatestPublications()` — fetch the listing page, select article links, call `GetPublication(url)`
  on each. The `BaseSource.GetPublications(address, anchorSelector, ...)` helper does this generically.
- `ParseDocument(IDocument, url)` — parse one article into a `RemoteNews` (title, HTML content, post
  date, image URL). Uses **AngleSharp** for HTML parsing/CSS selectors.
- Optional: `GetAllPublications()` for full backfills, `ExtractIdFromUrl(url)` override,
  `UseProxy => true`, custom `Encoding` or `Headers`.

`BaseSource.GetPublication()` orchestrates fetch → parse → normalize (trims title/content, clamps
future dates, normalizes image URL, sets `OriginalUrl`, derives `RemoteId` via `ExtractIdFromUrl`,
which defaults to the last URL path segment).

`RemoteNews` (the scraper DTO in `PressCenters.Services`) is distinct from `News` (the EF entity).
`NewsService.AddAsync` maps one to the other and **deduplicates on `(SourceId, RemoteId)`** — if a
row with the same remote id already exists for that source, it is skipped.

`MainNews` is a separate, simpler concept: homepage "top story" providers derive from
`BaseMainNewsProvider` (`Sources/MainNews/`, e.g. BTA, CNN, Reuters, Nova) and return a single
`RemoteMainNews` headline.

### Source registration & scheduling (reflection-driven)

Sources are **not** auto-discovered by type scanning. They are listed in
`Data/PressCenters.Data/Seeding/SourcesSeeder.cs` as rows keyed by `TypeName` (the fully-qualified
class name). On startup `Startup.Configure` runs `dbContext.Database.Migrate()`, seeds, then
`SeedHangfireJobs` registers one recurring Hangfire job **per DB source row**:

- `GetLatestPublicationsJob` per source — every 7 minutes; resolves the source instance with
  `ReflectionHelpers.GetInstance<BaseSource>(typeName)`, fetches latest, adds new ones, downloads images.
- `MainNewsGetterJob` — every 2 minutes.
- `DbCleanupJob` — weekly.

The Hangfire dashboard is at `/hangfire` (production only, Administrator role required).

### Images

`NewsService.SaveImageLocallyAsync` downloads each article image and writes two resized PNGs via
**SixLabors.ImageSharp** to `wwwroot/images/news/{newsId % 1000}/` as `big_{id}.png` (730px wide) and
`small_{id}.png` (200×120 cropped). Main-news images go to `wwwroot/images/mainnews/{sourceId}.png`.

### Data access & mapping

- Repository pattern: `IRepository<T>` and `IDeletableEntityRepository<T>` (soft-delete via
  `BaseDeletableModel`), implemented by `EfRepository` / `EfDeletableEntityRepository`. `AllWithDeleted()`
  bypasses the soft-delete filter.
- SQL Server via EF Core; migrations in `Data/PressCenters.Data/Migrations`. The app auto-migrates and
  seeds on startup, so a reachable SQL Server is required to run the web app or Sandbox.
- AutoMapper mappings are wired by reflection (`AutoMapperConfig`): a view model opts in by implementing
  `IMapFrom<TEntity>`, `IMapTo<T>`, or `IHaveCustomMappings`.

## Adding a new source (common task)

1. Create a class under the appropriate `Services/PressCenters.Services.Sources/<Category>/` folder
   deriving from `BaseSource`; implement `BaseUrl`, `GetLatestPublications()`, and `ParseDocument()`.
2. Add a row to `SourcesSeeder.cs` with the fully-qualified `TypeName`, short/long names, description,
   site URL, and a default image under `wwwroot/images/sources/`.
3. Add a test class under `Tests/PressCenters.Services.Sources.Tests/<Category>/` (see conventions below).

No further wiring is needed — the Hangfire job is registered automatically from the seeded DB row on the
next startup.

## Testing conventions

Source tests are **live integration tests**: they make real HTTP requests to the target websites, so
they are network-dependent and will break when a site changes its markup or removes an article. Each
source test class typically has:

- `ExtractIdFromUrlShouldWorkCorrectly` — `[Theory]`/`[InlineData]` asserting `RemoteId` extraction
  (this is why `BaseSource` exposes `internal` members to the test project via `InternalsVisibleTo`).
- `ParseRemoteNewsShouldWorkCorrectly` — calls `GetPublication(knownUrl)` and asserts title, date,
  `RemoteId`, and `Contains`/`DoesNotContain` on the parsed content.
- `GetNewsShouldReturnResults` — asserts `GetLatestPublications()` returns the expected count.

Test stack: xUnit + Moq.

## Code style

StyleCop.Analyzers is enabled solution-wide (`Rules.ruleset`, `stylecop.json`, `Settings.StyleCop`).
Build treats analyzer findings seriously, so match the existing style:

- `using` directives go **inside** the namespace, `System.*` first, blank line between groups.
- Qualify instance members with `this.`.
- Files end with a newline.
- StyleCop company name is "PressCenters" (file-header copyright text is configured but headers are not
  enforced on most files — follow the surrounding files in the folder you edit).
