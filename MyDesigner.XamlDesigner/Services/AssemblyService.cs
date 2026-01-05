using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MyDesigner.XamlDesigner.Services
{
    /// <summary>
    /// Service for managing assembly loading and resolution
    /// </summary>
    public static class AssemblyService
    {
        private static readonly Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>();
        private static readonly List<string> AssemblySearchPaths = new List<string>();

        static AssemblyService()
        {
            // Add default search paths
            AssemblySearchPaths.Add(AppDomain.CurrentDomain.BaseDirectory);
            AssemblySearchPaths.Add(Environment.CurrentDirectory);
            
            // Add common .NET paths
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var dotnetPath = Path.Combine(programFiles, "dotnet", "shared");
            if (Directory.Exists(dotnetPath))
            {
                AssemblySearchPaths.Add(dotnetPath);
            }
        }

        public static Assembly LoadAssembly(string assemblyPath)
        {
            try
            {
                var fullPath = Path.GetFullPath(assemblyPath);
                
                if (LoadedAssemblies.ContainsKey(fullPath))
                {
                    return LoadedAssemblies[fullPath];
                }

                Assembly assembly;
                if (File.Exists(fullPath))
                {
                    assembly = Assembly.LoadFrom(fullPath);
                }
                else
                {
                    // Try to load by name
                    assembly = Assembly.Load(assemblyPath);
                }

                LoadedAssemblies[fullPath] = assembly;
                return assembly;
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
                return null;
            }
        }

        public static Assembly ResolveAssembly(string assemblyName)
        {
            try
            {
                // First try to find in already loaded assemblies
                var loaded = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));
                
                if (loaded != null)
                    return loaded;

                // Try to load from search paths
                foreach (var searchPath in AssemblySearchPaths)
                {
                    var dllPath = Path.Combine(searchPath, assemblyName + ".dll");
                    var exePath = Path.Combine(searchPath, assemblyName + ".exe");

                    if (File.Exists(dllPath))
                    {
                        return LoadAssembly(dllPath);
                    }
                    
                    if (File.Exists(exePath))
                    {
                        return LoadAssembly(exePath);
                    }
                }

                // Try to load by name (GAC, etc.)
                return Assembly.Load(assemblyName);
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
                return null;
            }
        }

        public static void AddSearchPath(string path)
        {
            if (Directory.Exists(path) && !AssemblySearchPaths.Contains(path))
            {
                AssemblySearchPaths.Add(path);
            }
        }

        public static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Return only the types that loaded successfully
                return ex.Types.Where(t => t != null);
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
                return Enumerable.Empty<Type>();
            }
        }

        public static IEnumerable<Assembly> GetLoadedAssemblies()
        {
            return LoadedAssemblies.Values;
        }
    }
}