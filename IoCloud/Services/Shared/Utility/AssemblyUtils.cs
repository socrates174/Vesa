using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;

namespace IoCloud.Shared.Utility
{
    public static class AssemblyUtils
    {
        /// <summary>
        ///  Loads and gets an assembly by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Assembly GetAssemblyByName(string name)
        {
            var myAssembly = AppDomain.CurrentDomain.GetAssemblies().Single(assembly => assembly.GetName().FullName.Contains(name + ","));
            var loadedAssembly = Assembly.GetEntryAssembly()?.GetReferencedAssemblies().FirstOrDefault(a => a.Name == name);
            if (loadedAssembly == null) AssemblyLoadContext.Default.LoadFromAssemblyName(myAssembly.GetName());
            return myAssembly;
        }

        /// <summary>
        /// Gets a loaded assembly by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Assembly GetLoadedAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == name);
        }

        public static IEnumerable<Assembly> LoadAssemblies(string currentAssembliesPath, params string[] assemblyNamePatterns)
        {
            var assemblyFileNames = Directory.GetFiles(currentAssembliesPath, "*.dll");
            foreach (var assemblyFileName in assemblyFileNames)
            {
                var assemblyFileNameNoExtension = assemblyFileName
                                                    .Replace(".dll", "", StringComparison.CurrentCultureIgnoreCase)
                                                    .Substring(assemblyFileName.LastIndexOf("\\") + 1);
                if (assemblyNamePatterns.Where(pattern => Regex.IsMatch(assemblyFileNameNoExtension, pattern.Trim(), RegexOptions.IgnoreCase)).Any())
                {
                    yield return Assembly.LoadFrom(assemblyFileName.Trim());
                }
            }
        }
    }
}
