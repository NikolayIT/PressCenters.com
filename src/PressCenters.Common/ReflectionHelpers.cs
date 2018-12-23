namespace PressCenters.Common
{
    using System;

    public class ReflectionHelpers
    {
        public static T GetInstance<T>(string typeName)
        {
            var type = typeof(T).Assembly.GetType(typeName);
            if (type == null)
            {
                throw new Exception($"Type \"{typeName}\" not found!");
            }

            var instance = (T)Activator.CreateInstance(type);
            if (instance == null)
            {
                throw new Exception($"Unable to create {typeof(T).Name} instance from \"{typeName}\"!");
            }

            return instance;
        }
    }
}
