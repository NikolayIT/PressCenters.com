namespace PressCenters.Services.Sources.Tests
{
    using PressCenters.Common;

    using Xunit;

    public class ProxyUrlBuilderTests
    {
        [Fact]
        public void PickHostWithoutAvoidReturnsAConfiguredHost()
        {
            var host = ProxyUrlBuilder.PickHost();

            Assert.Contains(host, GlobalConstants.ProxyHosts);
        }

        [Fact]
        public void PickHostAvoidingAHostNeverReturnsThatHostWhenAlternativesExist()
        {
            // The retry failover guarantee: with two or more relays, a retry that avoids the relay it just
            // used must land on a different one, so it does not re-hit the egress IP that just failed.
            Assert.True(GlobalConstants.ProxyHosts.Length >= 2, "This test assumes multiple relay hosts.");
            var avoid = GlobalConstants.ProxyHosts[0];

            for (var i = 0; i < 200; i++)
            {
                var host = ProxyUrlBuilder.PickHost(avoidHost: avoid);

                Assert.NotEqual(avoid, host);
                Assert.Contains(host, GlobalConstants.ProxyHosts);
            }
        }

        [Fact]
        public void WrapWithRewritesSchemeAndHostPreservingPathAndQuery()
        {
            var result = ProxyUrlBuilder.WrapWith("https://www.bnb.bg/some/path?q=1", "relay.example.dev");

            Assert.Equal("https://relay.example.dev/_plain/https/www.bnb.bg/some/path?q=1", result);
        }

        [Fact]
        public void WrapWithRewritesHttpScheme()
        {
            var result = ProxyUrlBuilder.WrapWith("http://example.bg/page", "relay.example.dev");

            Assert.Equal("https://relay.example.dev/_plain/http/example.bg/page", result);
        }
    }
}
