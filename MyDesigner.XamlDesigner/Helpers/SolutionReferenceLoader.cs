using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MyDesigner.XamlDesigner.Helpers;

/// <summary>
/// Helper class for loading all references from Solution file
/// </summary>
public static class SolutionReferenceLoader
{
    /// <summary>
    /// Load all references from .sln file
    /// </summary>
    public static List<string> LoadSolutionReferences(string solutionPath)
    {
        var references = new List<string>();
        var processedProjects = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        try
        {
            if (!File.Exists(solutionPath))
            {
             
                return references;
            }
            
        
            
          
            var solutionContent = File.ReadAllText(solutionPath);
            var solutionDir = Path.GetDirectoryName(solutionPath);
            
          
            var projectPattern = @"Project\(""\{[^}]+\}""\)\s*=\s*""[^""]+"",\s*""([^""]+\.csproj)""";
            var matches = Regex.Matches(solutionContent, projectPattern);
            
          
            
            foreach (Match match in matches)
            {
                var projectRelativePath = match.Groups[1].Value;
                var projectFullPath = Path.GetFullPath(Path.Combine(solutionDir, projectRelativePath));
                
                if (File.Exists(projectFullPath))
                {
                  
                    
                  
                    ProjectReferenceLoader.LoadProjectReferencesRecursivePublic(
                        projectFullPath, 
                        references, 
                        processedProjects
                    );
                }
                else
                {
                  
                }
            }
            
          
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SolutionReferenceLoader] : {ex.Message}");
        }
        
        return references;
    }
    
    /// <summary>
    /// Search for .sln file in specific folder
    /// </summary>
    public static string FindSolutionFile(string directory)
    {
        try
        {
            var dir = new DirectoryInfo(directory);
            
            while (dir != null)
            {
                var slnFiles = dir.GetFiles("*.sln");
                if (slnFiles.Length > 0)
                {
                    return slnFiles[0].FullName;
                }
                
                dir = dir.Parent;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SolutionReferenceLoader] Solution: {ex.Message}");
        }
        
        return null;
    }
}