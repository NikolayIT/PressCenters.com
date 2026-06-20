namespace PressCenters.Common
{
    using System;
    using System.Linq;

    // Rewrites absolute URLs to route through one of the relay hosts (GlobalConstants.ProxyHosts),
    // following the relay contract: https://{relayHost}/_plain/{scheme}/{targetHost}/{path}?{query}.
    // A relay is chosen at random per call, so consecutive requests spread across the relays (and their
    // egress IPs) instead of hammering one. Only the scheme prefix is rewritten, so the path and query
    // string are left intact. Non-http(s) input is returned unchanged.
    public static class ProxyUrlBuilder
    {
        private static readonly Random SharedRandom = new Random();

        // Wraps an absolute URL through a randomly chosen relay host. Returns the input unchanged when no
        // relays are configured or the input is not http(s).
        public static string Wrap(string absoluteUrl)
        {
            var host = PickHost();
            return host == null ? absoluteUrl : WrapWith(absoluteUrl, host);
        }

        // Picks a relay host at random. When avoidHost is supplied — a retry failing over from the relay it
        // just used — a DIFFERENT host is returned whenever an alternative exists, so the retry lands on
        // another egress IP rather than re-hitting the one that just failed. Returns null when no relays
        // are configured.
        public static string PickHost(string avoidHost = null)
        {
            var hosts = GlobalConstants.ProxyHosts;
            if (hosts.Length == 0)
            {
                return null;
            }

            if (hosts.Length == 1)
            {
                return hosts[0];
            }

            if (!string.IsNullOrEmpty(avoidHost))
            {
                var alternatives = hosts
                    .Where(host => !string.Equals(host, avoidHost, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                if (alternatives.Length > 0)
                {
                    return alternatives[NextRandom(alternatives.Length)];
                }
            }

            return hosts[NextRandom(hosts.Length)];
        }

        // Rewrites absoluteUrl through the given relay host. Only the scheme prefix is replaced, so the path
        // and query are preserved verbatim. Non-http(s) input is returned unchanged.
        public static string WrapWith(string absoluteUrl, string proxyHost)
        {
            if (string.IsNullOrWhiteSpace(absoluteUrl))
            {
                return absoluteUrl;
            }

            const string HttpsScheme = "https://";
            const string HttpScheme = "http://";

            if (absoluteUrl.StartsWith(HttpsScheme, StringComparison.OrdinalIgnoreCase))
            {
                return $"https://{proxyHost}/_plain/https/{absoluteUrl.Substring(HttpsScheme.Length)}";
            }

            if (absoluteUrl.StartsWith(HttpScheme, StringComparison.OrdinalIgnoreCase))
            {
                return $"https://{proxyHost}/_plain/http/{absoluteUrl.Substring(HttpScheme.Length)}";
            }

            return absoluteUrl;
        }

        private static int NextRandom(int maxValue)
        {
            lock (SharedRandom)
            {
                return SharedRandom.Next(maxValue);
            }
        }
    }
}
