using System.Collections.Generic;

namespace MyDesigner.XamlDesigner.View;

/// <summary>
/// Folder in project
/// </summary>
public class ProjectFolder
{
    public string Name { get; set; }
    public string FullPath { get; set; }
    public List<ProjectFile> Files { get; set; }
    public List<ProjectFolder> SubFolders { get; set; }
}
