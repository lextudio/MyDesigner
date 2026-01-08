using System.Collections.Generic;

namespace MyDesigner.XamlDesigner.View;

/// <summary>
/// Project structure
/// </summary>
public class ProjectStructure
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string RootPath { get; set; }
    public List<ProjectFolder> Folders { get; set; }
    public List<ProjectFile> RootFiles { get; set; }
}
