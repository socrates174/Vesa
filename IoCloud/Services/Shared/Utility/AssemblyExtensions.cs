using System.Reflection;

namespace IoCloud.Shared.Utility
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetTypes<T>(this Assembly assembly)
        {
            return assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && (type.IsSubclassOf(typeof(T)) || typeof(T).IsAssignableFrom(type)));
        }
    }
}
