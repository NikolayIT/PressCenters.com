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

The 2026-10 pass found five more migrations, each with its own trap:

- **Sac** went from Lotus Domino to WordPress (`/novini/{year}/{slug}/`; the old `/news/bg/{id}` answer
  410 Gone). The new site re-published the old articles under new slugs, so its `GetAllPublications`
  stops at the migration date instead of re-importing them as duplicates.
- **Egov**: `egov.government.bg` 301s to `midt.gov.bg` — МЕУ merged with МИР into МИДТ. Same class (the
  `TypeName` keys the DB row); the seeder renames the source.
- **Ombudsman** moved to a query-string CMS (`?go=news&p=detail&newsId={id}`; the old `/bg/n/{slug}-{id}`
  redirect to the home page), so `ExtractIdFromUrl` reads the `newsId` parameter.
- **Acf** now links its articles by Cyrillic slugs, while the stored ids are the old transliterated
  ones. `ExtractIdFromUrl` transliterates (`седем-граждански-…` → `sedem-grazhdanski-…`), or the newest
  items already in the DB would come back as duplicates. **Check this whenever a site's URLs change**:
  dedup is on `(SourceId, RemoteId)` only. Transliteration is not exact either: the old Latin slugs were
  cut at another length (`…-na-vap` vs `…-na-vapr`), so a backfill can still bring back a few.
- Before a backfill, check that the ids match **deep in the history, not only the newest items**: a
  migrated site often re-publishes its whole archive under the new ids, and then
  `BACKFILL_STOP_EXISTING` never triggers. McGov did exactly that (the newest ids matched, the
  pre-2026-05 ones did not) and a 2026-10-22 backfill imported 815 old news a second time (soft-deleted
  again). Its `GetAllPublications` now stops at the migration date, like Sac's and Ombudsman's; cap a
  first run with `BACKFILL_MAX_PER_SOURCE` near the expected gap.
- **McGov**: `/новини/` became a hub of events; the news archive is `/вид-новина/новини/`.

Also watch for a listing that still parses but is no longer the news list (McGov), and for a parser that
silently falls back to `DateTime.Now` or reads the *modified* date (Mod, Damtn) — the test's date
assertion is what catches those.

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

## 3. The TLS handshake, the User-Agent, HTTP version and egress

Four things matter and they compound:

- **The TLS handshake (Cloudflare, since 2026-09)** — Cloudflare's bot management answers a TLS ClientHello
  without the `status_request` (OCSP stapling) extension with a "Just a moment..." challenge (403), on every
  egress. .NET's Schannel handshake sends that extension only when certificate revocation is checked; Schannel
  `curl.exe` checks revocation by default, which is why it reached the sites that "block .NET" with the very
  same TLS library. `CheckCertificateRevocation => true` (on `BaseSource` and `BaseMainNewsProvider`; the jobs
  pass it to `SaveImageAsync` for the images) revived **Mon, Bfunion, Ime** and the **AP** provider. Over
  HTTP/1.1 only (.NET's HTTP/2 is challenged either way), directly and through the Cloudflare relay (it passes
  the client's handshake on); the Azure relays are challenged regardless. To compare handshakes, fetch
  `https://tls.peet.ws/api/all`: JA4 `t13d2012…` with the extension, `t13d2011…` without.
- **The User-Agent** — ime.bg's WAF answers 403 to a UA of Chrome 120 or older, so
  `GlobalConstants.DefaultUserAgent` claims Chrome 141 since 2026-10. Bump it when a site starts refusing it.
- **HTTP/2 vs HTTP/1.1** — opt in per source via `UseHttp2` (default is HTTP/1.1). Some paths accept .NET
  only over HTTP/2 (MinFin and Mvr through the Cloudflare relay, see Egress); others reject .NET's HTTP/2
  fingerprint and want 1.1 (everything behind the Cloudflare challenge). It's opt-in, not global, because of
  that split. Verify with the **actual net10.0 source test, not a PowerShell probe** — PowerShell (older
  runtime) returned 200 for mon.bg/minfin/bfunion/mvr while net10.0 got 403.
- **Egress** — when net10.0-direct is blocked, the **Cloudflare relay reaches the site but accepts .NET
  only over HTTP/2** (and, for the sites behind the Cloudflare challenge, only with the handshake above, over
  HTTP/1.1). So `UseProxy + UseHttp2` *together* is the unlock for **MinFin, Mvr** (Azure relays are
  IP-blocked by these sites).

Still genuinely stuck (no net10.0 path — would need a real browser):
- **Mfa** (`mfa.bg`) — since 2026-10 behind Radware Bot Manager: a JS/captcha "Radware Page" with **HTTP
  200** to every client, the relays included, and on the feeds too.
- **Gallup** — the site itself is broken behind Cloudflare: HTTP answers "error code: 1001", HTTPS has no
  certificate for the host.

(**Ciaf** and **Ime**, on this list until 2026-10, are not: Ciaf moved to `ciaf.bg` and is reachable, Ime
was the handshake and the User-Agent.)

**Escape hatch — read an unprotected feed.** When the HTML is bot-blocked on *every* egress (net10.0
direct **and** all four relays), look for an RSS / sitemap endpoint before declaring it stuck — feeds are
meant for machines and are usually exempt from the bot-protection on the pages. **Dnevnik**'s homepage is
Cloudflare-403 to net10.0 and to all four relays (headers / HTTP-version / cookies make no difference — note
this is **Cloudflare**, not DataDome, despite the intermittent 200s an *older*-runtime PowerShell probe
gets), but `https://www.dnevnik.bg/rss/` returns clean UTF-8 XML. So `DnevnikBgMainNewsProvider` reads the
feed's first `<item>` — title + `<link>` (strip the `?ref=rss` query) + the thumbnail `<img>` from the
HTML-encoded `<description>` — instead of scraping the page. `BaseMainNewsProvider.GetContent` exposes the
raw response so a provider can parse non-HTML payloads itself.

To classify fast, run the **source's own test** (net10.0): a "Just a moment..." page → add
`CheckCertificateRevocation` (HTTP/1.1); another 403 → add `UseHttp2`; still 403 → add `UseProxy` (Cloudflare
relay + h2); 403 even via the relay, a 202/SSL error or a captcha page → stuck, needs new egress (or an
unprotected RSS/sitemap feed — see the escape hatch above).

## 4. "Stale in prod but tests pass" — quiet vs. changed listing

- **Bivol / IsBgNet** were stale but actually **current** — prod already held the site's newest item;
  they're just quiet (compare the site's latest article date/id to the prod `MAX(CreatedOn)` / latest
  `RemoteId` before touching anything).
- **Mon / Mvr / Bfunion / MinFin** combined *both* problems — changed markup **and** a net10.0-direct
  block — so each needed a parser rewrite **plus** `UseProxy + UseHttp2` (§3). Don't stop at the first
  cause. **DvParliament** was the opposite: markup unchanged, pure egress block → just `UseProxy`.
- The public site shows the freshness of every source without DB access: `/Sources/List` has the ids, and
  `/Rss/Latest/{id}` the 20 newest news of one source (`pubDate` = the post date).
- Some are stale because the *category* died, not the scraper: МЗ "Министерски съвет"
  (`/novini/ministerski-savet/` is gone, its last news is from 2023-06) and МЕ "Акценти" (`hot-news.html`, last used 2024-12).

## Quick checklist

1. Run the source's test → note the exact error (status code / assertion / NRE).
2. Load the site root. Up? → migration or stale URL. Down/5xx everywhere? → wait it out.
3. Fetch the listing URL the scraper uses → 404? site changed → find the new path.
4. On 403: a "Just a moment..." page → `CheckCertificateRevocation` (§3); else compare .NET **H2 vs H1.1**,
   then **direct vs relay**. 403 on all → residential proxy. Try a current Chrome User-Agent too.
5. Cyrillic-slug URLs work from .NET/AngleSharp; for curl/Python use
   `urllib.parse.quote(url, safe="%:/?&=#+")` and beware double-encoding already-encoded hrefs.
6. The DB is the truth for "working in prod": `SELECT MAX(CreatedOn) ... GROUP BY SourceId`.

Backfilling historical gaps is done with the env-driven `Sandbox` harness (`GetAllNewsSandbox`,
`BACKFILL_*` vars). See the agent memory notes `source-tests-are-live-and-flaky` and
`backfill-gaps-with-sandbox` for the running history.
