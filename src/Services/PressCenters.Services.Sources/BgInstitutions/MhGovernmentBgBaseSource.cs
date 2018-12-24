namespace PressCenters.Services.Sources.BgInstitutions
{
    using System;
    using System.Linq;

    using AngleSharp;

    public abstract class MhGovernmentBgBaseSource : BaseSource
    {
        public override RemoteDataResult GetLatestPublications()
        {
            var address = this.GetNewsListUrl();
            var document = this.BrowsingContext.OpenAsync(address).Result;
            var links = document.QuerySelectorAll(".news h2 a").Select(x => x.Attributes["href"].Value)
                .Select(x => this.NormalizeUrl(x, "http://www.mh.government.bg/")).ToList();
            var news = links.Select(this.ParseRemoteNews).ToList();
            return new RemoteDataResult { News = news, };
        }

        internal RemoteNews ParseRemoteNews(string url)
        {
            var document = this.BrowsingContext.OpenAsync(url).Result;
            var title = document.QuerySelector("h1").TextContent.Trim();
            var imageUrl = "http://www.mh.government.bg/static/images/Ministry_of_Health-heraldic.95741c3e92f7.svg";

            var contentElement = document.QuerySelector(".single_news");
            this.NormalizeUrlsRecursively(contentElement, "http://www.mh.government.bg/");
            var content = contentElement.InnerHtml;

            var documentElement = document.QuerySelector(".single_news + .panel");
            if (documentElement != null)
            {
                this.NormalizeUrlsRecursively(documentElement, "http://www.mh.government.bg/");
                content += documentElement.InnerHtml;
            }

            var timeAsString = document.QuerySelector(".newsdate li time").Attributes["datetime"].Value;
            var time = DateTime.Parse(timeAsString);

            var news = new RemoteNews
            {
                Title = title,
                OriginalUrl = url,
                ShortContent = null,
                Content = content,
                ImageUrl = imageUrl,
                PostDate = time,
                RemoteId = this.ExtractIdFromUrl(url),
            };

            return news;
        }

        internal string ExtractIdFromUrl(string url)
        {
            var uri = new Uri(url);
            var id = !string.IsNullOrWhiteSpace(uri.Segments[uri.Segments.Length - 1])
                         ? uri.Segments[uri.Segments.Length - 2] + uri.Segments[uri.Segments.Length - 1].Trim('/')
                         : uri.Segments[uri.Segments.Length - 3] + uri.Segments[uri.Segments.Length - 2].Trim('/');
            return id;
        }

        protected abstract string GetNewsListUrl();
    }
}
