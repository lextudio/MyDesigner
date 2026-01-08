using System.Collections.Generic;

namespace MyDesigner.XamlDesigner.Helpers;

public class DragData
{
    public List<string> Files { get; set; } = new();
    public List<string> FileTypes { get; set; } = new();
    public string Text { get; set; }
    public bool HasXaml { get; set; }
    public bool IsValid { get; set; }
}
