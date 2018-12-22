namespace PressCenters.Web.ViewModels.Settings
{
    using PressCenters.Data.Models;
    using PressCenters.Services.Mapping;

    public class SettingViewModel : IMapFrom<Setting>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}
