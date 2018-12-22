namespace PressCenters.Data.Models
{
    using System.Collections.Generic;

    using PressCenters.Data.Common.Models;

    public class MainNewsSource : BaseDeletableModel<int>
    {
        public string ClassName { get; set; }

        public string Name { get; set; }

        public string Website { get; set; }

        public virtual ICollection<MainNews> MainNews { get; set; }
    }
}
