namespace MyDesigner.XamlDesigner.View;

/// <summary>
/// File in project
/// </summary>
public class ProjectFile
{
    public string Name { get; set; }
    public string FullPath { get; set; }
    public string RelativePath { get; set; }
    public ProjectFileType Type { get; set; }
    public ProjectFile CodeBehindFile { get; set; } 
}
