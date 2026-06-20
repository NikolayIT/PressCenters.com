namespace PressCenters.Services.Sources.MainNews
{
    using System;

    using PressCenters.Common;

    public class BtaBgMainNewsProvider : BaseMainNewsProvider
    {
        public override string BaseUrl { get; } = "https://www.bta.bg";

        public override bool UseProxy => true;

        public override RemoteMainNews GetMainNews()
        {
            var document = this.GetDocument(this.BaseUrl);

            var titleElement = document.QuerySelector(".leading-section .news-card__title a");
            var title = titleElement?.TextContent?.Trim().Trim('.').Trim();

            var url = this.MakeAbsoluteUrl(titleElement?.GetAttribute("href"));

            var imageElement = document.QuerySelector(".leading-section .news-card__image img");
            var imageUrl = this.MakeAbsoluteUrl(imageElement?.GetAttribute("data-src"));
            if (imageUrl != null && this.UseProxy)
            {
                imageUrl = new Uri(imageUrl).GetLeftPart(UriPartial.Query); // Remove hash fragment
                imageUrl = ProxyUrlBuilder.Wrap(imageUrl).Replace("+", "%20");
            }

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
