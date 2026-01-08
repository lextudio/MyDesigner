using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace MyDesigner.XamlDesigner.Helpers;

/// <summary>
/// Helper class for loading references from project files
/// </summary>
public static class ProjectReferenceLoader
{
    /// <summary>
    /// Load all references from .csproj file recursively
    /// </summary>
    public static List<string> LoadProjectReferences(string xamlFilePath)
    {
        var references = new List<string>();
        var processedProjects = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        try
        {
           
            var projectFile = FindProjectFile(xamlFilePath);
            if (projectFile == null)
            {
               
                return references;
            }

            LoadProjectReferencesRecursive(projectFile, references, processedProjects);
            
          
            if (references.Count > 0)
            {
                
                foreach (var r in references)
                {
                    Console.WriteLine($"[ProjectReferenceLoader]   - {Path.GetFileName(r)}");
                }
            }
            Console.WriteLine($"[ProjectReferenceLoader] ========================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProjectReferenceLoader] : {ex.Message}");
        }
        
        return references;
    }
    
    /// <summary>
    /// Load references recursively from project file (public for use from SolutionReferenceLoader)
    /// </summary>
    public static void LoadProjectReferencesRecursivePublic(string projectFile, List<string> references, HashSet<string> processedProjects)
    {
        LoadProjectReferencesRecursive(projectFile, references, processedProjects);
    }
    
    /// <summary>
    /// Load references recursively from project file
    /// </summary>
    private static void LoadProjectReferencesRecursive(string projectFile, List<string> references, HashSet<string> processedProjects)
    {
       
        var normalizedPath = Path.GetFullPath(projectFile);
        if (processedProjects.Contains(normalizedPath))
        {
            return;
        }
        
        processedProjects.Add(normalizedPath);
        
        try
        {
           
            var doc = XDocument.Load(projectFile);
            var projectDir = Path.GetDirectoryName(projectFile);
            
            
            var projectReferences = doc.Descendants("ProjectReference")
                .Select(pr => pr.Attribute("Include")?.Value)
                .Where(path => !string.IsNullOrEmpty(path))
                .ToList();
            

            foreach (var projectRef in projectReferences)
            {
              
                var fullProjectPath = Path.GetFullPath(Path.Combine(projectDir, projectRef));
                
                if (!File.Exists(fullProjectPath))
                {
                 
                    continue;
                }
                
              
                LoadProjectReferencesRecursive(fullProjectPath, references, processedProjects);
                
              
                var dllPath = ResolveProjectReferenceToDll(projectRef, projectDir);
                if (dllPath != null && File.Exists(dllPath) && !references.Contains(dllPath))
                {
                    references.Add(dllPath);
                  
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProjectReferenceLoader]{Path.GetFileName(projectFile)}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Search for .csproj file in current folder or parent folders
    /// </summary>
    private static string FindProjectFile(string xamlFilePath)
    {
        var dir = new DirectoryInfo(Path.GetDirectoryName(xamlFilePath));
        
        while (dir != null)
        {
            var csprojFiles = dir.GetFiles("*.csproj");
            if (csprojFiles.Length > 0)
            {
                return csprojFiles[0].FullName;
            }
            
            dir = dir.Parent;
        }
        
        return null;
    }
    
    /// <summary>
    /// Convert ProjectReference path to DLL path
    /// </summary>
    private static string ResolveProjectReferenceToDll(string projectRefPath, string projectDir)
    {
        try
        {
          
            var fullProjectPath = Path.GetFullPath(Path.Combine(projectDir, projectRefPath));
            
            if (!File.Exists(fullProjectPath))
            {
                Console.WriteLine($"[ProjectReferenceLoader] : {fullProjectPath}");
                return null;
            }
            
          
            var projectName = Path.GetFileNameWithoutExtension(fullProjectPath);
            var projectRefDir = Path.GetDirectoryName(fullProjectPath);
            
          
            var possiblePaths = new[]
            {
                Path.Combine(projectRefDir, "bin", "Debug", "net10.0", $"{projectName}.dll"),
                Path.Combine(projectRefDir, "bin", "Debug", "net8.0-windows", $"{projectName}.dll"),
                Path.Combine(projectRefDir, "bin", "Debug", "net8.0", $"{projectName}.dll"),
                Path.Combine(projectRefDir, "bin", "Debug", "net7.0-windows", $"{projectName}.dll"),
                Path.Combine(projectRefDir, "bin", "Debug", "net7.0", $"{projectName}.dll"),
                Path.Combine(projectRefDir, "bin", "Debug", "net6.0-windows", $"{projectName}.dll"),
                Path.Combine(projectRefDir, "bin", "Debug", "net6.0", $"{projectName}.dll"),
                Path.Combine(projectRefDir, "bin", "Release", "net10.0", $"{projectName}.dll"),
                Path.Combine(projectRefDir, "bin", "Release", "net8.0-windows", $"{projectName}.dll"),
                Path.Combine(projectRefDir, "bin", "Release", "net8.0", $"{projectName}.dll"),
                Path.Combine(projectRefDir, "bin", "Release", "net7.0-windows", $"{projectName}.dll"),
                Path.Combine(projectRefDir, "bin", "Release", "net7.0", $"{projectName}.dll"),
                Path.Combine(projectRefDir, "bin", "Release", "net6.0-windows", $"{projectName}.dll"),
                Path.Combine(projectRefDir, "bin", "Release", "net6.0", $"{projectName}.dll")
            };
            
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
            
            Console.WriteLine($"[ProjectReferenceLoader]: {projectName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProjectReferenceLoader] : {ex.Message}");
        }
        
        return null;
    }
}