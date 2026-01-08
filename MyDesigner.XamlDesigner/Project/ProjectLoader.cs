using MyDesigner.XamlDesigner.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace MyDesigner.XamlDesigner.View;

/// <summary>
/// Class for parsing and loading project structure from .csproj file
/// </summary>
public class ProjectLoader
{
    public string ProjectPath { get; private set; }
    public string ProjectName { get; private set; }
    public string ProjectType { get; private set; }
    public ProjectStructure Structure { get; private set; }

    /// <summary>
    /// Load project from specified path
    /// </summary>
    public bool LoadProject(string projectPath)
    {
        try
        {
            ProjectPath = projectPath;

          
            var csprojFiles = Directory.GetFiles(projectPath, "*.csproj", SearchOption.TopDirectoryOnly);
            var slnFiles = Directory.GetFiles(projectPath, "*.sln", SearchOption.TopDirectoryOnly);
            
          
            if (csprojFiles.Length == 0 && slnFiles.Length == 0)
                return false;

          
            string projectFile;
            string actualProjectPath;
            
            if (csprojFiles.Length > 0)
            {
                projectFile = csprojFiles[0];
                actualProjectPath = Path.GetDirectoryName(projectFile);
            }
            else
            {
              
                var allCsprojFiles = Directory.GetFiles(projectPath, "*.csproj", SearchOption.AllDirectories);
                if (allCsprojFiles.Length > 0)
                {
                    projectFile = allCsprojFiles[0];
                    actualProjectPath = Path.GetDirectoryName(projectFile);
                }
                else
                {
                   
                    ProjectName = Path.GetFileNameWithoutExtension(slnFiles[0]);
                    projectFile = slnFiles[0];
                    
                   
                    return LoadFromSolutionFile(slnFiles[0], projectPath);
                }
            }

            ProjectName = Path.GetFileNameWithoutExtension(projectFile);
            ProjectPath = actualProjectPath;

           
            var doc = XDocument.Load(projectFile);

          
            ProjectType = DetermineProjectType(doc, actualProjectPath);

          
            Structure = BuildProjectStructure(doc, actualProjectPath);

      
            Settings.Default.ProjectType = ProjectType;
            Settings.Default.Save();

            return true;
        }
        catch (Exception ex)
        {
           
            return false;
        }
    }

    /// <summary>
    /// Determine project type from .csproj content
    /// </summary>
    private string DetermineProjectType(XDocument doc, string projectPath)
    {
        var root = doc.Root;
        if (root == null) return "Unknown";

        // البحث عن TargetFramework
        var targetFramework = root.Descendants("TargetFramework").FirstOrDefault()?.Value ??
                            root.Descendants("TargetFrameworks").FirstOrDefault()?.Value ?? "";

        // فحص PackageReference
        var packageRefs = root.Descendants("PackageReference")
            .Select(x => x.Attribute("Include")?.Value ?? "")
            .ToList();

        if (packageRefs.Any(p => p.Contains("Avalonia")))
            return "Avalonia";

        if (packageRefs.Any(p => p.Contains("Microsoft.Maui")))
            return "Maui";
      
        if (targetFramework.Contains("net") && targetFramework.Contains("android"))
            return "Maui";

        if (targetFramework.Contains("net") && targetFramework.Contains("ios"))
            return "Maui";

        // SDK
        var sdk = root.Attribute("Sdk")?.Value ?? "";
        if (sdk.Contains("Microsoft.NET.Sdk.Maui"))
            return "Maui";

      

     
        var xamlFiles = Directory.GetFiles(projectPath, "*.xaml", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".xaml.cs", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var axamlFiles = Directory.GetFiles(projectPath, "*.axaml", SearchOption.AllDirectories).ToArray();

        if (axamlFiles.Any())
            return "Avalonia";

      
        foreach (var xamlFile in xamlFiles.Take(3))
        {
            try
            {
                var content = File.ReadAllText(xamlFile);
                if (content.Contains("xmlns=\"http://schemas.microsoft.com/dotnet/2021/maui\""))
                    return "Maui";
            }
            catch { }
        }

        return "WPF";
    }

    /// <summary>
    /// Build project structure from .csproj file
    /// </summary>
    private ProjectStructure BuildProjectStructure(XDocument doc, string projectPath)
    {
        var structure = new ProjectStructure
        {
            Name = ProjectName,
            Type = ProjectType,
            RootPath = projectPath,
            Folders = new List<ProjectFolder>()
        };

      
        var root = doc.Root;
        if (root == null) return structure;

      
        var isSdkStyle = root.Attribute("Sdk") != null;

     

        //  ItemGroup
        var itemGroups = root.Descendants("ItemGroup");
        var includedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var excludedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var itemGroup in itemGroups)
        {
            //  (Remove)
            var removedItems = itemGroup.Elements()
                .Where(e => e.Name.LocalName == "Compile" ||
                           e.Name.LocalName == "Page" ||
                           e.Name.LocalName == "None")
                .Where(e => e.Attribute("Remove") != null)
                .Select(e => e.Attribute("Remove")?.Value)
                .Where(v => !string.IsNullOrEmpty(v));

            foreach (var item in removedItems)
            {
                //  wildcards  "Visual Studio\**\*.cs"
                if (item.Contains("**"))
                {
                    var pattern = item.Replace("**\\", "").Replace("**", "");
                    excludedFiles.Add(pattern);
                }
                else
                {
                    excludedFiles.Add(item.Replace("\\", "/"));
                }
            }

            //   Page, Compile, None, Content  
            var items = itemGroup.Elements()
                .Where(e => e.Name.LocalName == "Page" ||
                           e.Name.LocalName == "Compile" ||
                           e.Name.LocalName == "None" ||
                           e.Name.LocalName == "Content" ||
                           e.Name.LocalName == "ApplicationDefinition")
                .Where(e => e.Attribute("Include") != null)
                .Select(e => e.Attribute("Include")?.Value)
                .Where(v => !string.IsNullOrEmpty(v));

            foreach (var item in items)
            {
                //  wildcards  "View\**\*.cs"
                if (item.Contains("**"))
                {
                    var expandedFiles = ExpandWildcard(item, projectPath);
                    foreach (var file in expandedFiles)
                    {
                        includedFiles.Add(file);
                    }
                }
                else
                {
                    includedFiles.Add(item.Replace("\\", "/"));
                }
            }
        }

        //  SDK-style، 
        if (isSdkStyle)
        {
            var allFiles = GetAllProjectFiles(projectPath);

          
            foreach (var file in allFiles)
            {
                var shouldExclude = false;

               
                foreach (var excludePattern in excludedFiles)
                {
                    if (file.Contains(excludePattern.Replace("\\", "/")))
                    {
                        shouldExclude = true;
                        break;
                    }
                }

                if (!shouldExclude)
                {
                    includedFiles.Add(file);
                }
            }
        }
        else if (includedFiles.Count == 0)
        {
          
            includedFiles = GetAllProjectFiles(projectPath);
        }

     

     
        BuildFolderStructure(structure, includedFiles, projectPath);

       

        return structure;
    }

    /// <summary>
    /// Expand wildcards like "View\**\*.cs"
    /// </summary>
    private List<string> ExpandWildcard(string pattern, string projectPath)
    {
        var files = new List<string>();

        try
        {
           
            var parts = pattern.Split(new[] { "**" }, StringSplitOptions.None);
            var baseFolder = parts[0].Replace("\\", "").Replace("/", "");
            var filePattern = parts.Length > 1 ? parts[1].Replace("\\", "") : "*.*";

            var searchPath = string.IsNullOrEmpty(baseFolder)
                ? projectPath
                : Path.Combine(projectPath, baseFolder);

            if (Directory.Exists(searchPath))
            {
                var foundFiles = Directory.GetFiles(searchPath, filePattern, SearchOption.AllDirectories)
                    .Select(f => f.Substring(projectPath.Length + 1).Replace("\\", "/"));

                files.AddRange(foundFiles);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"wildcard {pattern}: {ex.Message}");
        }

        return files;
    }

    /// <summary>
    /// Get all project files
    /// </summary>
    private HashSet<string> GetAllProjectFiles(string projectPath)
    {
        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var extensions = new[] { ".xaml", ".axaml", ".cs", ".config", ".json", ".xml", ".resx", ".settings" };

     
        var excludeFolders = new[] { "bin", "obj", ".vs", "packages", "node_modules", ".git" };

     
        var excludeFiles = new[] { "App.g.i.cs", "App.g.cs" };

      

        try
        {
          
            var allFiles = Directory.GetFiles(projectPath, "*.*", SearchOption.AllDirectories)
                .Where(f =>
                {
                  
                    var relativePath = f.Substring(projectPath.Length + 1);
                    if (excludeFolders.Any(folder => relativePath.StartsWith(folder + "\\") || relativePath.Contains("\\" + folder + "\\")))
                        return false;

                  
                    var fileName = Path.GetFileName(f);
                    if (excludeFiles.Any(ef => fileName.Equals(ef, StringComparison.OrdinalIgnoreCase)))
                        return false;

                 
                    if (fileName.EndsWith(".g.cs") || fileName.EndsWith(".g.i.cs"))
                        return false;

                  
                    var ext = Path.GetExtension(f).ToLower();
                    return extensions.Contains(ext);
                })
                .Select(f => f.Substring(projectPath.Length + 1).Replace("\\", "/"));

            foreach (var file in allFiles)
            {
                files.Add(file);
            }

          

         
            var sampleFiles = files.Take(10).ToList();
            if (sampleFiles.Any())
            {
              
                foreach (var file in sampleFiles)
                {
                    Console.WriteLine($"  - {file}");
                }
            }
        }
        catch (Exception ex)
        {
          
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }

        return files;
    }

    /// <summary>
    /// Build folder structure
    /// </summary>
    private void BuildFolderStructure(ProjectStructure structure, HashSet<string> files, string projectPath)
    {
        var folderDict = new Dictionary<string, ProjectFolder>(StringComparer.OrdinalIgnoreCase);

       
        var rootFolder = new ProjectFolder
        {
            Name = "",
            FullPath = projectPath,
            Files = new List<ProjectFile>(),
            SubFolders = new List<ProjectFolder>()
        };
        folderDict[""] = rootFolder;

      
        var codeBehindFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

     
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);

            if (fileName.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase) &&
                !fileName.EndsWith(".xaml.cs", StringComparison.OrdinalIgnoreCase))
            {
            
                var csFile = file + ".cs";
                if (files.Contains(csFile))
                {
                    codeBehindFiles.Add(csFile);
                }
            }
            else if (fileName.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase) &&
                     !fileName.EndsWith(".axaml.cs", StringComparison.OrdinalIgnoreCase))
            {
              
                var csFile = file + ".cs";
                if (files.Contains(csFile))
                {
                    codeBehindFiles.Add(csFile);
                }
            }
        }

     

        var processedFiles = 0;
        var skippedFiles = 0;

        foreach (var file in files)
        {
            var fullPath = Path.Combine(projectPath, file.Replace("/", "\\"));

         
            if (!File.Exists(fullPath))
            {
             
                skippedFiles++;
                continue;
            }

            var fileName = Path.GetFileName(file);
            var directory = Path.GetDirectoryName(file)?.Replace("\\", "/") ?? "";

          
            if (ShouldSkipFile(fileName))
            {
                skippedFiles++;
                continue;
            }

           
            if (codeBehindFiles.Contains(file))
            {
                skippedFiles++;
                continue;
            }

            EnsureFolderExists(folderDict, directory, projectPath);

         
            var folder = folderDict[directory];
            var projectFile = new ProjectFile
            {
                Name = fileName,
                FullPath = fullPath,
                RelativePath = file,
                Type = GetFileType(fileName)
            };

          
            if (projectFile.Type == ProjectFileType.Xaml)
            {
                var csFile = file + ".cs";
                if (files.Contains(csFile))
                {
                    var csFullPath = Path.Combine(projectPath, csFile.Replace("/", "\\"));
                    if (File.Exists(csFullPath))
                    {
                        projectFile.CodeBehindFile = new ProjectFile
                        {
                            Name = Path.GetFileName(csFile),
                            FullPath = csFullPath,
                            RelativePath = csFile,
                            Type = ProjectFileType.CSharp
                        };
                    }
                }
            }

            folder.Files.Add(projectFile);
            processedFiles++;
        }

      
        structure.Folders = rootFolder.SubFolders;
        structure.RootFiles = rootFolder.Files;

     
    }

    /// <summary>
    /// Ensure folder exists in dictionary
    /// </summary>
    private void EnsureFolderExists(Dictionary<string, ProjectFolder> folderDict, string path, string projectPath)
    {
        if (string.IsNullOrEmpty(path) || folderDict.ContainsKey(path))
            return;

        var parts = path.Split('/');
        var currentPath = "";

        for (int i = 0; i < parts.Length; i++)
        {
            var parentPath = currentPath;
            currentPath = i == 0 ? parts[i] : $"{currentPath}/{parts[i]}";

            if (!folderDict.ContainsKey(currentPath))
            {
                var folder = new ProjectFolder
                {
                    Name = parts[i],
                    FullPath = Path.Combine(projectPath, currentPath.Replace("/", "\\")),
                    Files = new List<ProjectFile>(),
                    SubFolders = new List<ProjectFolder>()
                };

                folderDict[currentPath] = folder;

              
                if (folderDict.ContainsKey(parentPath))
                {
                    folderDict[parentPath].SubFolders.Add(folder);
                }
            }
        }
    }

    /// <summary>
    /// Determine file type
    /// </summary>
    private ProjectFileType GetFileType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLower();

        if (ext == ".xaml" || ext == ".axaml")
            return ProjectFileType.Xaml;
        if (ext == ".cs")
            return ProjectFileType.CSharp;
        if (ext == ".xml" || ext == ".config" || ext == ".resx" || ext == ".settings")
            return ProjectFileType.Config;
        if (ext == ".json")
            return ProjectFileType.Json;

        return ProjectFileType.Other;
    }

    /// <summary>
    /// Determine files to skip
    /// </summary>
    private bool ShouldSkipFile(string fileName)
    {
      
        if (fileName.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".g.i.cs", StringComparison.OrdinalIgnoreCase))
            return true;

      
        if (fileName.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    /// <summary>
    /// Load project from .sln file
    /// </summary>
    private bool LoadFromSolutionFile(string slnPath, string projectPath)
    {
        try
        {
           
            var slnContent = File.ReadAllText(slnPath);
            var lines = slnContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

          
            foreach (var line in lines)
            {
                if (line.StartsWith("Project("))
                {
                   
                    //  Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ProjectName", "ProjectName\ProjectName.csproj", "{GUID}"
                    var parts = line.Split(new[] { '"' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 5)
                    {
                        var csprojRelativePath = parts[3];
                        if (csprojRelativePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                        {
                            var csprojFullPath = Path.Combine(projectPath, csprojRelativePath);
                            if (File.Exists(csprojFullPath))
                            {
                            
                                ProjectName = Path.GetFileNameWithoutExtension(csprojFullPath);
                                var doc = XDocument.Load(csprojFullPath);
                                ProjectType = DetermineProjectType(doc, Path.GetDirectoryName(csprojFullPath));
                                Structure = BuildProjectStructure(doc, Path.GetDirectoryName(csprojFullPath));

                             
                                Settings.Default.ProjectName = ProjectName;
                                Settings.Default.ProjectPath = projectPath;
                                Settings.Default.ProjectType = ProjectType;
                                Settings.Default.Save();

                                return true;
                            }
                        }
                    }
                }
            }

          
            var allCsprojFiles = Directory.GetFiles(projectPath, "*.csproj", SearchOption.AllDirectories);
            if (allCsprojFiles.Length > 0)
            {
                var csprojFile = allCsprojFiles[0];
                ProjectName = Path.GetFileNameWithoutExtension(csprojFile);
                var doc = XDocument.Load(csprojFile);
                ProjectType = DetermineProjectType(doc, Path.GetDirectoryName(csprojFile));
                Structure = BuildProjectStructure(doc, Path.GetDirectoryName(csprojFile));

                // حفظ الإعدادات
                Settings.Default.ProjectName = ProjectName;
                Settings.Default.ProjectPath = projectPath;
                Settings.Default.ProjectType = ProjectType;
                Settings.Default.Save();

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
          
            return false;
        }
    }
}
