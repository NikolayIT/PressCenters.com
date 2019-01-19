namespace PressCenters.Worker.Common
{
    using System.Reflection;

    public interface ITaskAssemblyProvider
    {
        Assembly GetAssembly();
    }
}
