namespace PressCenters.Services.Sources
{
    using System.Collections.Generic;

    public interface ISource
    {
        string BaseUrl { get; }

        IEnumerable<RemoteNews> GetLatestPublications();

        IEnumerable<RemoteNews> GetAllPublications();

        RemoteNews GetPublication(string url);
    }
}
