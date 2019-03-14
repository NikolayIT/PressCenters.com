namespace PressCenters.Data.Models
{
    using System.Collections.Generic;

    using PressCenters.Data.Common.Models;

    public class MainNewsSource : BaseDeletableModel<int>
    {
        public MainNewsSource()
        {
            this.MainNews = new HashSet<MainNews>();
        }

        public string TypeName { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public virtual ICollection<MainNews> MainNews { get; set; }
    }
}
