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

All projects target **net10.0**. Visual Studio 2022 is the primary IDE (`.sln`, `.csproj.user`,
`.vs/` present). CI runs on GitHub Actions (`.github/workflows/ci.yml`): `dotnet build` + `dotnet test` of
`src/PressCenters.sln` in Release on `windows-latest` (Windows keeps the legacy code-page encodings
the scrapers depend on). There is also a CodeQL workflow in `.github/workflows/`.

## Architecture

### Layered structure (`src/`)

- **PressCenters.Common** — cross-cutting helpers: `GlobalConstants` (system name, default user-agent),
  `ReflectionHelpers.GetInstance<T>(typeName)`, `StringExtensions`.
- **Data/** — `PressCenters.Data.Models` (EF entities), `PressCenters.Data` (`ApplicationDbContext`,
  migrations, repositories, seeding), `PressCenters.Data.Common` (base model types + repository interfaces).
- **Services/** — `PressCenters.Services` (DTOs like `RemoteNews`, `SlugGenerator`),
  `PressCenters.Services.Data` (business services over repositories), `PressCenters.Services.Sources`
  (the scrapers — the heart of the app), `PressCenters.Services.CronJobs` (Hangfire jobs),
  `PressCenters.Services.Mapping` (reflection-based Mapster), `PressCenters.Services.Messaging` (email).
- **Web/** — `PressCenters.Web` (MVC app, admin area, Hangfire wiring in `Startup.cs`) and
  `PressCenters.Web.Infrastructure`.
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
  `UseProxy => true` (routes fetches through a random relay host via
  `PressCenters.Common.ProxyUrlBuilder` over `GlobalConstants.ProxyHosts`), custom `Encoding` or `Headers`.

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

- `GetLatestPublicationsJob` per source — every 10 minutes; resolves the source instance with
  `ReflectionHelpers.GetInstance<BaseSource>(typeName)`, fetches latest, adds new ones, downloads images.
- `MainNewsGetterJob` — every 2 minutes.
- `DbCleanupJob` — weekly.

The Hangfire dashboard is at `/hangfire` (production only, Administrator role required).

### Images

`NewsService.SaveImageAsync` downloads each article image, renders two lossy WebP thumbnails with `ImageThumbnails`
(**SkiaSharp**; `big_{id}.webp` shrunk to 730px wide, `small_{id}.webp` 200×120 cropped) and uploads them through
`IImageStorage` (`Services/PressCenters.Services.Data/Images/`) under the keys from `NewsImagePaths`:
`news/{newsId % 1000}/big_{id}.webp`. In production `IImageStorage` is `R2ImageStorage` (Cloudflare R2 bucket
`presscenters-images`, S3 API via AWSSDK.S3, served from `https://images.presscenters.com`); when the `R2`
config section is empty it is `LocalImageStorage` rooted at `wwwroot/images`, i.e. the pre-R2 on-disk layout.
`AddImageStorage` (Startup / Sandbox `Program`) picks the implementation and returns the public base URL,
stored in `GlobalConstants.ImagesBaseUrl`, which `NewsViewModel.Small/BigImageUrl` prepend to the key.
Main-news images stay local PNGs in `wwwroot/images/mainnews/{sourceId}.png` (150×110, also `ImageThumbnails`; the
API returns these URLs). `ImageThumbnails` decodes JPEG, PNG, GIF, WebP, BMP and ICO (no TIFF) and throws
`InvalidDataException` for anything else; `src/Directory.Build.targets` keeps SkiaSharp's ~95 MB native PDBs out of
the build/publish output.

Until October 2026 the news thumbnails were PNGs (`big_{id}.png`). `ConvertThumbnailsToWebpSandbox` (Sandbox with
`IMAGES_WEBP=1`, or the "Convert thumbnails to WebP and delete the PNGs" launch profile; see its doc comment for the
other `IMAGES_WEBP_*` variables) converts them to `.webp` in the configured storage, resumably, with a live
Spectre.Console view, and with `IMAGES_WEBP_DELETE_PNG=1` deletes each converted PNG at the end of the run. Meanwhile
the news pages fall back to the PNG when the WebP is missing (`data-fallback` + the error handler in `_BaseLayout`);
once a run finds nothing left to convert, that fallback can be removed.

The R2 credentials live only in the git-ignored `appsettings.Production.json` files (Web and Sandbox); the
repo is public, so never put them in `appsettings.json` or in code. The Sandbox loads
`appsettings.Production.json` next to its exe when present. One-time migration of the historical folder:
Sandbox with `R2:UploadRoot` set (to `<wwwroot>\images\news`) runs `UploadImagesToR2Sandbox` (resumable,
skips keys already in the bucket, never touches the database).

### Data access & mapping

- Repository pattern: `IRepository<T>` and `IDeletableEntityRepository<T>` (soft-delete via
  `BaseDeletableModel`), implemented by `EfRepository` / `EfDeletableEntityRepository`. `AllWithDeleted()`
  bypasses the soft-delete filter.
- SQL Server via EF Core; migrations in `Data/PressCenters.Data/Migrations`. The app auto-migrates and
  seeds on startup, so a reachable SQL Server is required to run the web app or Sandbox.
- Mapster mappings are wired by reflection (`MappingConfig`): a view model opts in by implementing
  `IMapFrom<TEntity>`, `IMapTo<T>`, or `IHaveCustomMappings`. Projection uses `IQueryable.To<T>()`
  (`ProjectToType` over `MappingConfig.GlobalConfig`). Mapster auto-flattens single-level navigations
  (e.g. `SourceName` ← `Source.Name`); collection counts and other computed values need an explicit
  `IHaveCustomMappings.CreateMappings` (e.g. `SourceViewModel.NewsCount` ← `Source.News.Count()`).

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

Test stack: xUnit (xunit.v3) + Moq.

When a source test fails or a source stops producing news, **triage before "fixing"** — most "dead"
sources are actually reachable (CMS migration, changed listing URL, HTTP/2-only anti-bot, or just quiet),
not offline. See `src/Services/PressCenters.Services.Sources/TROUBLESHOOTING.md` for the diagnostic
playbook (reachability layers, HTTP/2 vs HTTP/1.1, relay/TLS gotchas, WordPress/Elementor parsing).

## Code style

StyleCop.Analyzers is enabled solution-wide (`Rules.ruleset`, `stylecop.json`, `Settings.StyleCop`).
Build treats analyzer findings seriously, so match the existing style:

- `using` directives go **inside** the namespace, `System.*` first, blank line between groups.
- Qualify instance members with `this.`.
- Files end with a newline.
- StyleCop company name is "PressCenters" (file-header copyright text is configured but headers are not
  enforced on most files — follow the surrounding files in the folder you edit).
