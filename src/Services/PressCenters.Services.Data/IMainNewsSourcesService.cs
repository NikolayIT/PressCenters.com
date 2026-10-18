namespace PressCenters.Services.Data
{
    using System.Collections.Generic;

    public interface IMainNewsSourcesService
    {
        int Count();

        /// <summary>
        /// The id of the latest main news of every source that has any.
        /// </summary>
        IReadOnlyCollection<int> GetLatestMainNewsIds();
    }
}
