namespace PressCenters.Services.Sources.MainNews
{
    using System;

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
            if (imageUrl != null)
            {
                // Return the site's own URL. It used to be wrapped through a randomly picked relay here, so the URL
                // changed on most runs and MainNewsGetterJob (which compares ImageUrl) re-inserted the same story
                // every 2 minutes. The job routes the download through the relays itself (UseProxy).
                imageUrl = new Uri(imageUrl).GetLeftPart(UriPartial.Query); // Remove hash fragment
            }

            return new RemoteMainNews(title, url, imageUrl);
        }
    }
}
