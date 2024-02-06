using System.Reflection;

namespace IoCloud.Shared.Utility
{
    /// <summary>
    /// Returns a Type from current assemblies given the type name
    /// </summary>
    public static class TypeUtils
    {
        static Assembly[] currentDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        public static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in currentDomainAssemblies)
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }
    }
}
