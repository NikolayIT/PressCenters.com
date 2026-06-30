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

## 3. It is almost never the TLS cipher fingerprint — it's HTTP version + runtime + egress

Schannel `curl.exe` (same TLS library as .NET) reaches the sites that "block .NET", so the cipher/JA3
fingerprint is **not** the cause. Three things actually matter and they compound:

- **HTTP/2 vs HTTP/1.1** — opt in per source via `UseHttp2` (default is HTTP/1.1). Some anti-bot reject
  HTTP/1.1 from non-browser clients (`bfunion.bg`: 403 on 1.1, 200 on 2.0); others reject .NET's HTTP/2
  fingerprint and want 1.1. It's opt-in, not global, because of that split.
- **Runtime fingerprint (net10.0)** — net10.0's `HttpClient` is blocked by some sites that **older .NET
  and curl reach**. So verify reachability with the **actual net10.0 source test, not a PowerShell probe**
  — PowerShell (older runtime) returned 200 for mon.bg/minfin/bfunion/mvr while net10.0 got 403.
- **Egress** — when net10.0-direct is blocked, the **Cloudflare relay reaches the site but accepts .NET
  only over HTTP/2**. So `UseProxy + UseHttp2` *together* is the unlock that revived **Mon, MinFin,
  Bfunion, Mvr** (Azure relays are IP-blocked by these sites; the Cloudflare worker needs h2).

Still genuinely stuck (no net10.0 path — would need a residential proxy / real browser):
- **Ciaf** (`caciaf.bg`) — 403 to direct *and* the Cloudflare relay.
- **Ime** (`ime.bg`) — Cloudflare bot-management: direct 403, and the Cloudflare relay gets a **202
  challenge** (worker → Cloudflare-site), Azure relays 403.
- **Gallup** — SSL handshake / 525 → site-side certificate problem.

**Escape hatch — read an unprotected feed.** When the HTML is bot-blocked on *every* egress (net10.0
direct **and** all four relays), look for an RSS / sitemap endpoint before declaring it stuck — feeds are
meant for machines and are usually exempt from the bot-protection on the pages. **Dnevnik**'s homepage is
Cloudflare-403 to net10.0 and to all four relays (headers / HTTP-version / cookies make no difference — note
this is **Cloudflare**, not DataDome, despite the intermittent 200s an *older*-runtime PowerShell probe
gets), but `https://www.dnevnik.bg/rss/` returns clean UTF-8 XML. So `DnevnikBgMainNewsProvider` reads the
feed's first `<item>` — title + `<link>` (strip the `?ref=rss` query) + the thumbnail `<img>` from the
HTML-encoded `<description>` — instead of scraping the page. `BaseMainNewsProvider.GetContent` exposes the
raw response so a provider can parse non-HTML payloads itself.

To classify fast, run the **source's own test** (net10.0): 403 → add `UseHttp2`; still 403 → add
`UseProxy` (Cloudflare relay + h2); 403 even via the relay, or a 202/SSL error → stuck, needs new egress
(or an unprotected RSS/sitemap feed — see the escape hatch above).

## 4. "Stale in prod but tests pass" — quiet vs. changed listing

- **Bivol / IsBgNet** were stale but actually **current** — prod already held the site's newest item;
  they're just quiet (compare the site's latest article date/id to the prod `MAX(CreatedOn)` / latest
  `RemoteId` before touching anything).
- **Mon / Mvr / Bfunion / MinFin** combined *both* problems — changed markup **and** a net10.0-direct
  block — so each needed a parser rewrite **plus** `UseProxy + UseHttp2` (§3). Don't stop at the first
  cause. **DvParliament** was the opposite: markup unchanged, pure egress block → just `UseProxy`.

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
