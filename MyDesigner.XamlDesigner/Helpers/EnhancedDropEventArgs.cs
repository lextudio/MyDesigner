using System;
using Avalonia;

namespace MyDesigner.XamlDesigner.Helpers;

public class EnhancedDropEventArgs : EventArgs
{
    public DragData DragData { get; }
    public Point Position { get; }
    
    public EnhancedDropEventArgs(DragData dragData, Point position)
    {
        DragData = dragData;
        Position = position;
    }
}