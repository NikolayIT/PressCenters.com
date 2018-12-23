namespace PressCenters.Sources
{
    using System.Collections.Generic;

    public class RemoteDataResult
    {
        public RemoteDataResult()
        {
            this.News = new List<RemoteNews>();
        }

        public string LastNewsIdentifier { get; set; }

        public IEnumerable<RemoteNews> News { get; set; }
    }
}
