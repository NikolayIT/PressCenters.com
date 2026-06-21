# Diagnosing a broken / stale source

When a source's tests fail or it stops producing news in production, **don't assume it's the parser or
that the site is offline.** Most "dead" sources investigated in 2026 turned out to be reachable — just
mislabeled. Work through the layers below.

## 1. Is the site actually down, or did it migrate?

A failing `ParseRemoteNews` / `GetNews` test with a 5xx or `NullReferenceException` often means the
**old URLs are dead after a CMS migration**, not that the site is offline. Load the site root first.

- **McGov** (`mc.government.bg`) and **Cpdp** (`cpdp.bg`) looked "offline (500/502)" but had migrated to
  **WordPress** — the old `index.php` / `newsn.php` URLs just 404 / redirect to a dead path.
- **Sac** moved domain (`www.sac.government.bg` → `sac.justice.bg`), same Lotus Domino site.
- Fix = rewrite the parser for the new site (*camp-B*), or just repoint `BaseUrl` if the markup is
  unchanged (*camp-A*, like Sac).

WordPress tips: news may be a **custom post type** (`/wp-json/wp/v2/news`) or a **category**
(`/category/новини/`); the default `/wp-json/wp/v2/posts` and `/feed/` are often empty. Elementor sites
(McGov) have generic classes — use `og:title` (strip the site suffix), `.elementor-icon-box-title` for
the date, `.elementor-widget-theme-post-content` for the body. Standard themes (Cpdp) expose
`article:published_time` + `.entry-content` cleanly.

## 2. Reachability is a layered problem

`UseProxy` (the relay) exists for the **production server's** blocked IP — a dev box usually reaches the
sites directly. So a green test on your machine does **not** prove prod can fetch it. Test each layer:

| Layer | What it is |
|---|---|
| .NET `HttpClient` **direct** | the real path for `UseProxy = false` sources |
| .NET `HttpClient` **via relay** | the path for `UseProxy = true` sources |
| `curl` / browser | only a sanity check that the site is up + its structure |

Gotchas:
- **Don't probe with curl-via-relay and assume .NET behaves the same.** Git Bash `curl` uses OpenSSL;
  Windows `curl.exe` and .NET `HttpClient` both use **Schannel**.
- The 4 relays (`GlobalConstants.ProxyHosts`) are **not interchangeable**: the Cloudflare worker
  (`eu-relay-v2…workers.dev`) rejects .NET's handshake outright; the 3 Azure relays are IP-blocked by
  some sites; and some sites (Cpdp) the relays can't reach at all (526/502). `ReadStringFromUrl` now
  **fails over** across the hosts in random order.
- **DvParliament** broke because `dv.parliament.bg` started blocking the prod egress IP (the live issue
  advanced while prod froze) — fixed by `UseProxy`.

## 3. It is almost never the TLS cipher fingerprint

Schannel `curl.exe` (same TLS library as .NET) reaches the sites that "block .NET", so the cipher/JA3
fingerprint is **not** the cause. The real causes seen:

- **HTTP/2 vs HTTP/1.1** — `bfunion.bg` returns **403 on HTTP/1.1, 200 on HTTP/2**. `ReadStringFromUrl`
  now defaults to HTTP/2 (`DefaultRequestVersion = Version20`, `RequestVersionOrLower` so it falls back).
- **IP / deep block** — **MinFin**, **Ciaf** return 403 on *both* H2 and H1.1, even to OpenSSL curl →
  genuinely blocked; needs a **residential proxy** (infra, not code).
- **SSL / cert** — **Gallup** returns an SSL handshake error → site-side certificate problem.

To classify a 403 fast (PowerShell): build an `HttpRequestMessage`, set `.Version = 2.0` then `1.1` with
`VersionPolicy = RequestVersionExact`, and compare. 200 on H2 only → HTTP/2 fix. 403 on both → residential
proxy. SSL error → cert issue.

## 4. "Stale in prod but tests pass" — quiet vs. changed listing

- **Bivol / IsBgNet** were stale but actually **current** — prod already held the site's newest item;
  they're just quiet (compare the site's latest article date/id to the prod `MAX(CreatedOn)` / latest
  `RemoteId` before touching anything).
- **Mon / Mvr / Ime** are reachable from .NET but their listing URLs 404 → the site changed → rewrite.

## Quick checklist

1. Run the source's test → note the exact error (status code / assertion / NRE).
2. Load the site root. Up? → migration or stale URL. Down/5xx everywhere? → wait it out.
3. Fetch the listing URL the scraper uses → 404? site changed → find the new path.
4. On 403: compare .NET **H2 vs H1.1**, then **direct vs relay**. 403 on all → residential proxy.
5. Cyrillic-slug URLs work from .NET/AngleSharp; for curl/Python use
   `urllib.parse.quote(url, safe="%:/?&=#+")` and beware double-encoding already-encoded hrefs.
6. The DB is the truth for "working in prod": `SELECT MAX(CreatedOn) ... GROUP BY SourceId`.

Backfilling historical gaps is done with the env-driven `Sandbox` harness (`GetAllNewsSandbox`,
`BACKFILL_*` vars). See the agent memory notes `source-tests-are-live-and-flaky` and
`backfill-gaps-with-sandbox` for the running history.
