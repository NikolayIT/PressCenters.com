namespace PressCenters.Common
{
    public static class GlobalConstants
    {
        public const string SystemName = "PressCenters.com";
        public const string SystemBaseUrl = "https://presscenters.com";
        public const string SystemEmail = "presscenters.com@nikolay.it";

        public const string SystemSlogan = "Новините без редакция";

        public const string AdministratorRoleName = "Administrator";
        public const string ProUserRoleName = "ProUser";

        public const string DefaultUserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36";

        // Relay hosts that proxied scraper/image requests are spread across — a random host is chosen per
        // request via ProxyUrlBuilder.Wrap — so a burst of scraping is not pinned to one egress IP that a
        // target site can rate-limit or block. The relays span different providers/regions (Cloudflare +
        // Azure), giving distinct egress IPs. All honour the same contract (/_plain/{scheme}/{host}/...);
        // any host honouring it can be added here with no code change.
        public static readonly string[] ProxyHosts =
        {
            "eu-relay-v2.proxy0001.workers.dev",          // Cloudflare Worker (SOF colo)
            "delabg-ecase-relay-we.azurewebsites.net",    // Azure Functions, West Europe
            "delabg-ecase-relay-gwc.azurewebsites.net",   // Azure Functions, Germany West Central
            "delabg-ecase-relay-fc.azurewebsites.net",    // Azure Functions, France Central
        };

        public static string SystemVersion { get; set; }
    }
}
