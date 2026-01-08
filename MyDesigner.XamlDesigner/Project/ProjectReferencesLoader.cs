using Avalonia.Controls;
using MyDesigner.XamlDesigner.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace MyDesigner.XamlDesigner.View;

/// <summary>
/// Class for loading external references and libraries from project
/// </summary>
public class ProjectReferencesLoader
{
    private string _projectPath;
    private string _csprojPath;
    private HashSet<string> _loadedAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase); 

    /// <summary>
    /// Load all references from project
    /// </summary>
    public void LoadAllReferences(string projectPath)
    {
        try
        {
            _projectPath = projectPath;
            _loadedAssemblies.Clear();

           
            var csprojFiles = Directory.GetFiles(projectPath, "*.csproj", SearchOption.TopDirectoryOnly);
            if (csprojFiles.Length == 0)
            {
              
                return;
            }

            _csprojPath = csprojFiles[0];
            var doc = XDocument.Load(_csprojPath);

         
            LoadProjectOutput();

    
            ScanXamlFilesForNamespaces();

        
            LoadProjectReferences(doc);

          
            LoadPackageReferences(doc);

         
            LoadDirectReferences(doc);

         
        }
        catch (Exception ex)
        {
         
        }
    }

    /// <summary>
    /// Scan XAML files and extract used namespaces
    /// </summary>
    private void ScanXamlFilesForNamespaces()
    {
        try
        {
           

            
            var xamlFiles = Directory.GetFiles(_projectPath, "*.xaml", SearchOption.AllDirectories)
                .Where(f => !f.Contains("\\bin\\") && !f.Contains("\\obj\\"))
                .ToList();

    

            var customNamespaces = new HashSet<string>();
            var projectName = Path.GetFileNameWithoutExtension(_csprojPath);

            foreach (var xamlFile in xamlFiles)
            {
                try
                {
                    var content = File.ReadAllText(xamlFile);

                  
                    var namespacePattern = @"xmlns:(\w+)\s*=\s*""clr-namespace:([^""]+)""";
                    var matches = System.Text.RegularExpressions.Regex.Matches(content, namespacePattern);

                    foreach (System.Text.RegularExpressions.Match match in matches)
                    {
                        var prefix = match.Groups[1].Value;
                        var clrNamespace = match.Groups[2].Value;

                      
                        if (!clrNamespace.StartsWith("System.") &&
                            !clrNamespace.StartsWith("Microsoft."))
                        {
                            customNamespaces.Add(clrNamespace);
                        }
                    }
                }
                catch (Exception ex)
                {
                  
                }
            }

            if (customNamespaces.Count > 0)
            {
             
                foreach (var ns in customNamespaces)
                {
                    Console.WriteLine($"      - {ns}");
                }

             
                LoadControlsFromNamespaces(customNamespaces, projectName);
            }
            else
            {
               
            }
        }
        catch (Exception ex)
        {
           
        }
    }

    /// <summary>
    /// Load Controls from specified namespaces
    /// </summary>
    private void LoadControlsFromNamespaces(HashSet<string> namespaces, string projectName)
    {
        try
        {
           

        
            var binFolder = Path.Combine(_projectPath, "bin");
            if (!Directory.Exists(binFolder))
            {
              
                return;
            }

          
            var outputFiles = new List<string>();

            try
            {
                var dllFiles = Directory.GetFiles(binFolder, "*.dll", SearchOption.AllDirectories)
                    .Where(f => !f.Contains("\\ref\\") && !f.Contains("\\resources\\"))
                    .ToList();
                var exeFiles = Directory.GetFiles(binFolder, "*.exe", SearchOption.AllDirectories)
                    .Where(f => !f.Contains("\\ref\\") && !f.Contains("\\resources\\"))
                    .ToList();

                outputFiles.AddRange(dllFiles);
                outputFiles.AddRange(exeFiles);

                
            }
            catch (Exception ex)
            {
              
                return;
            }

            if (outputFiles.Count == 0)
            {
               
                return;
            }

           
            var totalControlsLoaded = 0;

            foreach (var outputFile in outputFiles.OrderByDescending(f => File.GetLastWriteTime(f)))
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(outputFile);

                  
                    if (_loadedAssemblies.Contains(fileName))
                        continue;

                  
                    var assembly = System.Reflection.Assembly.LoadFrom(outputFile);
                    var assemblyName = assembly.GetName().Name;

                 
                    var alreadyExists = Toolbox.Instance.AssemblyNodes.Any(node =>
                        string.Equals(node.Assembly.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase)
                    );

                    if (alreadyExists)
                        continue;

                 
                    var types = assembly.GetExportedTypes();
                    var controlTypes = new List<Type>();

                    foreach (var type in types)
                    {
                       
                        if (!string.IsNullOrEmpty(type.Namespace) && namespaces.Contains(type.Namespace))
                        {
                            if (!type.IsAbstract &&
                                !type.IsGenericTypeDefinition &&
                                type.IsSubclassOf(typeof(Control)) &&
                                type.GetConstructor(
                                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                                    null, Type.EmptyTypes, null) != null)
                            {
                                controlTypes.Add(type);
                            }
                        }
                    }

                    if (controlTypes.Count > 0)
                    {
                       
                        MyTypeFinder.Instance.RegisterAssembly(assembly);

                       
                        var node = new AssemblyNode
                        {
                            Assembly = assembly,
                            Path = outputFile
                        };

                        foreach (var type in controlTypes)
                        {
                            node.Controls.Add(new ControlNode { Type = type });
                        }

                        node.Controls.Sort((c1, c2) => c1.Name.CompareTo(c2.Name));
                        Toolbox.Instance.AssemblyNodes.Add(node);

                      
                       

                        _loadedAssemblies.Add(assemblyName);
                        totalControlsLoaded += controlTypes.Count;
                    }
                }
                catch (Exception ex)
                {
                   
                    Console.WriteLine($"   ⚠ تخطي {Path.GetFileName(outputFile)}: {ex.Message}");
                }
            }

            if (totalControlsLoaded == 0)
            {
            
            }
        }
        catch (Exception ex)
        {
          
            Console.WriteLine($"   Stack: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Load project references (ProjectReference)
    /// </summary>
    private void LoadProjectReferences(XDocument doc)
    {
        var projectReferences = doc.Descendants("ProjectReference")
            .Select(x => x.Attribute("Include")?.Value)
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        if (projectReferences.Count == 0)
        {
          
            return;
        }

     

        foreach (var reference in projectReferences)
        {
            try
            {
                var referencedCsprojPath = Path.GetFullPath(
                    Path.Combine(Path.GetDirectoryName(_csprojPath), reference));

                if (!File.Exists(referencedCsprojPath))
                {
                   
                    continue;
                }

                var referencedProjectFolder = Path.GetDirectoryName(referencedCsprojPath);
                var referencedProjectName = Path.GetFileNameWithoutExtension(referencedCsprojPath);

              
                if (_loadedAssemblies.Contains(referencedProjectName))
                {
                  
                    continue;
                }

            
                var binDirectory = Path.Combine(referencedProjectFolder, "bin");
                if (Directory.Exists(binDirectory))
                {
                   
                    var dllFiles = Directory.GetFiles(binDirectory, $"{referencedProjectName}.dll",
                        SearchOption.AllDirectories)
                        .Where(f => !f.Contains("\\ref\\") && !f.Contains("\\resources\\"))
                        .ToList();

                    
                    var latestDll = dllFiles
                        .OrderByDescending(f => File.GetLastWriteTime(f))
                        .FirstOrDefault();

                    if (latestDll != null)
                    {
                        LoadAssembly(latestDll);
                    }
                    else
                    {
                       
                    }
                }
                else
                {
                    
                }

                
                LoadReferencedProjectDependencies(referencedCsprojPath);
            }
            catch (Exception ex)
            {
                
            }
        }
    }

    /// <summary>
    /// Load referenced project dependencies
    /// </summary>
    private void LoadReferencedProjectDependencies(string csprojPath)
    {
        try
        {
            var doc = XDocument.Load(csprojPath);
            var projectFolder = Path.GetDirectoryName(csprojPath);

           
            var packages = doc.Descendants("PackageReference")
                .Select(x => new
                {
                    Name = x.Attribute("Include")?.Value,
                    Version = x.Attribute("Version")?.Value ?? x.Element("Version")?.Value
                })
                .Where(x => !string.IsNullOrEmpty(x.Name));

            foreach (var package in packages)
            {
                LoadPackageFromNuGet(package.Name, package.Version, projectFolder);
            }
        }
        catch (Exception ex)
        {
           
        }
    }

    /// <summary>
    /// Load package references (PackageReference)
    /// </summary>
    private void LoadPackageReferences(XDocument doc)
    {
        var packageReferences = doc.Descendants("PackageReference")
            .Select(x => new
            {
                Name = x.Attribute("Include")?.Value,
                Version = x.Attribute("Version")?.Value ?? x.Element("Version")?.Value
            })
            .Where(x => !string.IsNullOrEmpty(x.Name))
            .ToList();

        if (packageReferences.Count == 0)
        {
           
            return;
        }

      

        foreach (var package in packageReferences)
        {
            try
            {
               
                LoadPackageFromNuGet(package.Name, package.Version, _projectPath);
            }
            catch (Exception ex)
            {
                
            }
        }
    }

    /// <summary>
    /// Load package from NuGet
    /// </summary>
    private void LoadPackageFromNuGet(string packageName, string version, string projectPath)
    {
       
        if (IsSystemAssembly(packageName))
        {
            
            return;
        }

      
        var packagesFolder = FindPackagesFolder(projectPath);
        if (packagesFolder != null)
        {
            var packageFolder = Directory.GetDirectories(packagesFolder, $"{packageName}*", SearchOption.TopDirectoryOnly)
                .OrderByDescending(d => d)
                .FirstOrDefault();

            if (packageFolder != null)
            {
                LoadDllsFromPackage(packageFolder);
                return;
            }
        }

       
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var nugetCache = Path.Combine(userProfile, ".nuget", "packages", packageName.ToLower());

        if (Directory.Exists(nugetCache))
        {
            var versionFolder = string.IsNullOrEmpty(version)
                ? Directory.GetDirectories(nugetCache).OrderByDescending(d => d).FirstOrDefault()
                : Path.Combine(nugetCache, version);

            if (versionFolder != null && Directory.Exists(versionFolder))
            {
                LoadDllsFromPackage(versionFolder);
            }
        }
    }

    /// <summary>
    /// Search for packages folder
    /// </summary>
    private string FindPackagesFolder(string startPath)
    {
        var currentPath = startPath;
        while (!string.IsNullOrEmpty(currentPath))
        {
            var packagesPath = Path.Combine(currentPath, "packages");
            if (Directory.Exists(packagesPath))
                return packagesPath;

            var parentPath = Directory.GetParent(currentPath)?.FullName;
            if (parentPath == currentPath)
                break;
            currentPath = parentPath;
        }
        return null;
    }

    /// <summary>
    /// Load DLLs from package
    /// </summary>
    private void LoadDllsFromPackage(string packageFolder)
    {
      
        var libFolder = Path.Combine(packageFolder, "lib");
        if (!Directory.Exists(libFolder))
            return;

      
        var frameworks = new[] {"net10.0-windows", "net8.0-windows", "net7.0-windows", "net6.0-windows",
                                "net5.0-windows", "netcoreapp3.1", "net48", "net472",
                                "net471", "net47", "net462", "net461", "net46", "net45" };

        string targetFolder = null;
        foreach (var framework in frameworks)
        {
            var fwFolder = Path.Combine(libFolder, framework);
            if (Directory.Exists(fwFolder))
            {
                targetFolder = fwFolder;
                break;
            }
        }

      
        if (targetFolder == null)
        {
            targetFolder = Directory.GetDirectories(libFolder)
                .OrderByDescending(d => d)
                .FirstOrDefault();
        }

        if (targetFolder != null && Directory.Exists(targetFolder))
        {
            
            var dllFiles = Directory.GetFiles(targetFolder, "*.dll", SearchOption.TopDirectoryOnly)
                .Where(f => !f.Contains("\\ref\\") && 
                           !f.Contains("\\resources\\") &&
                           !f.Contains("\\runtimes\\") &&
                           !Path.GetFileName(f).StartsWith("System.") &&
                           !Path.GetFileName(f).StartsWith("Microsoft."))
                .ToList();
            
            foreach (var dll in dllFiles)
            {
                LoadAssembly(dll);
            }
        }
    }

    /// <summary>
    /// Load direct references (Reference)
    /// </summary>
    private void LoadDirectReferences(XDocument doc)
    {
        var references = doc.Descendants("Reference")
            .Where(x => x.Attribute("Include") != null)
            .ToList();

        if (references.Count == 0)
        {
           
            return;
        }

      

        foreach (var reference in references)
        {
            try
            {
                var includeName = reference.Attribute("Include")?.Value;
                var hintPath = reference.Element("HintPath")?.Value;
                
                if (!string.IsNullOrEmpty(hintPath))
                {
                    var fullPath = Path.GetFullPath(
                        Path.Combine(Path.GetDirectoryName(_csprojPath), hintPath));

                    if (File.Exists(fullPath))
                    {
                      
                        LoadAssembly(fullPath);
                    }
                    else
                    {
                       
                    }
                }
            }
            catch (Exception ex)
            {
               
            }
        }
    }

    /// <summary>
    /// Load project output itself
    /// </summary>
    private void LoadProjectOutput()
    {
        try
        {
            var projectName = Path.GetFileNameWithoutExtension(_csprojPath);
           

            var binFolder = Path.Combine(_projectPath, "bin");
            bool loadedFromBin = false;

            if (Directory.Exists(binFolder))
            {
              
                var outputFiles = new List<string>();
                var dllFiles = Directory.GetFiles(binFolder, $"{projectName}.dll", SearchOption.AllDirectories);
                var exeFiles = Directory.GetFiles(binFolder, $"{projectName}.exe", SearchOption.AllDirectories);

                outputFiles.AddRange(dllFiles);
                outputFiles.AddRange(exeFiles);

             

                if (outputFiles.Count > 0)
                {
                  
                    var latestOutput = outputFiles
                        .OrderByDescending(f => File.GetLastWriteTime(f))
                        .FirstOrDefault();

                    if (latestOutput != null)
                    {
                      
                        LoadAssembly(latestOutput);
                        loadedFromBin = true;
                    }
                }
            }

          
            if (!loadedFromBin)
            {
               
                LoadCurrentAssemblyControls(projectName);
            }
        }
        catch (Exception ex)
        {
           
        }
    }

    /// <summary>
    /// Load Controls from current Assembly (for unbuilt projects)
    /// </summary>
    private void LoadCurrentAssemblyControls(string projectName)
    {
        try
        {
          
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

          
            var targetAssembly = loadedAssemblies.FirstOrDefault(a =>
                !a.IsDynamic &&
                string.Equals(a.GetName().Name, projectName, StringComparison.OrdinalIgnoreCase));

            if (targetAssembly == null)
            {
              
                var possiblePaths = new[]
                {
                    Path.Combine(_projectPath, "bin", "Debug", $"{projectName}.dll"),
                    Path.Combine(_projectPath, "bin", "Release", $"{projectName}.dll"),
                    Path.Combine(_projectPath, "bin", "Debug", $"{projectName}.exe"),
                    Path.Combine(_projectPath, "bin", "Release", $"{projectName}.exe")
                };

                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                      
                        LoadAssembly(path);
                        return;
                    }
                }

               
                return;
            }

            var assemblyName = targetAssembly.GetName().Name;
         

        
            var alreadyExists = Toolbox.Instance.AssemblyNodes.Any(node =>
                string.Equals(node.Assembly.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase)
            );

            if (alreadyExists)
            {
               
                return;
            }

            var types = targetAssembly.GetExportedTypes();
            var controlTypes = new List<Type>();

            foreach (var type in types)
            {
                if (!type.IsAbstract &&
                    !type.IsGenericTypeDefinition &&
                    type.IsSubclassOf(typeof(Control)) &&
                    type.GetConstructor(
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                        null, Type.EmptyTypes, null) != null)
                {
                    controlTypes.Add(type);
                }
            }

            if (controlTypes.Count == 0)
            {
              
                return;
            }

          
            MyTypeFinder.Instance.RegisterAssembly(targetAssembly);

          
            var node = new AssemblyNode
            {
                Assembly = targetAssembly,
                Path = targetAssembly.Location
            };

            foreach (var type in controlTypes)
            {
                node.Controls.Add(new ControlNode { Type = type });
            }

            node.Controls.Sort((c1, c2) => c1.Name.CompareTo(c2.Name));
            Toolbox.Instance.AssemblyNodes.Add(node);

         
            foreach (var ctrl in node.Controls.Take(5))
            {
                
            }
            if (node.Controls.Count > 5)
            {
               
            }

            _loadedAssemblies.Add(assemblyName);
        }
        catch (Exception ex)
        {
            
        }
    }



    /// <summary>
    /// Load Assembly to Toolbox
    /// </summary>
    private void LoadAssembly(string dllPath)
    {
        try
        {
            var fileName = Path.GetFileNameWithoutExtension(dllPath);

         
            if (IsSystemAssembly(fileName))
            {
                
                return;
            }

          
            if (dllPath.Contains("\\ref\\") || dllPath.Contains("\\resources\\") || dllPath.Contains("\\runtimes\\"))
            {
              
                return;
            }

            if (!File.Exists(dllPath))
            {
               
                return;
            }

           
            var alreadyExists = Toolbox.Instance.AssemblyNodes.Any(node =>
                string.Equals(node.Assembly.GetName().Name, fileName, StringComparison.OrdinalIgnoreCase)
            );

            if (alreadyExists)
            {
                
                return;
            }

           
            if (_loadedAssemblies.Contains(fileName))
            {
                
                return;
            }

           
            if (!HasUIControls(dllPath))
            {
                _loadedAssemblies.Add(fileName);
                return;
            }

        
            Toolbox.Instance.AddAssembly(dllPath);
            _loadedAssemblies.Add(fileName);
           
        }
        catch (Exception ex)
        {
           
        }
    }

    /// <summary>
    /// Check if library has UI Controls
    /// </summary>
    private bool HasUIControls(string dllPath)
    {
        try
        {
            var assembly = System.Reflection.Assembly.LoadFrom(dllPath);
            var types = assembly.GetExportedTypes();

         
            foreach (var type in types)
            {
                if (!type.IsAbstract &&
                    !type.IsGenericTypeDefinition &&
                    type.IsSubclassOf(typeof(Control)) &&
                    type.GetConstructor(
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                        null, Type.EmptyTypes, null) != null)
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            
            return false;
        }
    }

    /// <summary>
    /// Check if library is a system library
    /// </summary>
    private bool IsSystemAssembly(string assemblyName)
    {
        var systemPrefixes = new[]
        {
            "System.", "Microsoft.", "mscorlib", "netstandard",
            "WindowsBase", "PresentationCore", "PresentationFramework",
            "Newtonsoft.Json", "NuGet.", "NETStandard.Library",
            "AvalonEdit", "ICSharpCode.", "Mono.Cecil", "IKVM.",
            "Dirkster.", "AvalonDock", "WPFToolkit", "DynamicDataDisplay",
            "Windows.", "UIAutomation", "Accessibility", "ReachFramework",
            "System", "Microsoft", "api-ms-", "clr", "sni", "sos",
            "runtime.", "hostfxr", "hostpolicy", "coreclr", "clrjit",
            "dbgshim", "mscordaccore", "mscordbi", "mscorrc"
        };

        var exactMatches = new[]
        {
            "mscorlib", "netstandard", "WindowsBase", "PresentationCore",
            "PresentationFramework", "System", "Microsoft", "System.Runtime",
            "System.Core", "System.Xml", "System.Data", "System.Drawing",
            "System.Windows.Forms", "System.Configuration", "System.Net.Http"
        };

       
        if (exactMatches.Contains(assemblyName, StringComparer.OrdinalIgnoreCase))
            return true;

      
        return systemPrefixes.Any(prefix =>
            assemblyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Clear all loaded assemblies
    /// </summary>
    public void ClearLoadedAssemblies()
    {
        _loadedAssemblies.Clear();
    }
}
