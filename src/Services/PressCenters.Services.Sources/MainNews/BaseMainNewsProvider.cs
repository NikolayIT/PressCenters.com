namespace PressCenters.Worker.MainNewsProviders
{
    using AngleSharp;

    public abstract class BaseMainNewsProvider
    {
        protected BaseMainNewsProvider()
        {
            var configuration = Configuration.Default.WithDefaultLoader();
            this.BrowsingContext = AngleSharp.BrowsingContext.New(configuration);
        }

        protected IBrowsingContext BrowsingContext { get; }

        public abstract RemoteMainNews GetMainNews();
    }
}
