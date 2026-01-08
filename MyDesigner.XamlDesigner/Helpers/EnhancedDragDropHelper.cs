using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace MyDesigner.XamlDesigner.Helpers;

public static class EnhancedDragDropHelper
{
    private static readonly Dictionary<string, string> FileTypeIcons = new()
    {
        { ".axaml", "📄" },
        { ".xaml", "📄" },
        { ".cs", "📝" },
        { ".json", "🔧" },
        { ".xml", "📋" },
        { ".png", "🖼️" },
        { ".jpg", "🖼️" },
        { ".jpeg", "🖼️" },
        { ".gif", "🖼️" },
        { ".ico", "🎯" }
    };
    
    public static void EnableEnhancedDragDrop(Control element)
    {
        DragDrop.SetAllowDrop(element, true);
        element.AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
        element.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        element.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
        element.AddHandler(DragDrop.DropEvent, OnDrop);
    }
    
    private static void OnDragEnter(object sender, DragEventArgs e)
    {
        var element = sender as Control;
        if (element == null) return;
        
       
        var dragData = AnalyzeDragData(e.Data);
        
        if (dragData.IsValid)
        {
            
            ApplyDragEnterEffect(element);
            
            e.DragEffects = GetAppropriateEffect(dragData);
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
        
        e.Handled = true;
    }
    
    private static void OnDragOver(object sender, DragEventArgs e)
    {
        var element = sender as Control;
        if (element == null) return;
        
        var dragData = AnalyzeDragData(e.Data);
        
        e.DragEffects = dragData.IsValid ? GetAppropriateEffect(dragData) : DragDropEffects.None;
        e.Handled = true;
    }
    
    private static void OnDragLeave(object sender, DragEventArgs e)
    {
        var element = sender as Control;
        if (element == null) return;
        
        
        RemoveDragEffects(element);
    }
    
    private static void OnDrop(object sender, DragEventArgs e)
    {
        var element = sender as Control;
        if (element == null) return;
        
        var dragData = AnalyzeDragData(e.Data);
        
        if (dragData.IsValid)
        {
           
            ProcessDrop(element, dragData, e.GetPosition(element));
        }
        
       
        RemoveDragEffects(element);
        
        e.Handled = true;
    }
    
    private static DragData AnalyzeDragData(IDataObject data)
    {
        var dragData = new DragData();
        
       
        if (data.Contains(DataFormats.Files))
        {
            var files = data.GetFiles();
            if (files != null)
            {
                dragData.Files = files.Select(f => f.Path.LocalPath).ToList();
                dragData.FileTypes = dragData.Files.Select(Path.GetExtension).Distinct().ToList();
            }
        }
        
       
        if (data.Contains(DataFormats.Text))
        {
            dragData.Text = data.GetText();
        }
        
       
        if (dragData.Text?.Contains("<") == true && 
            (dragData.Text.Contains("xmlns") || dragData.Text.Contains("UserControl") || dragData.Text.Contains("Window")))
        {
            dragData.HasXaml = true;
        }
        
        dragData.IsValid = dragData.Files.Any() || !string.IsNullOrEmpty(dragData.Text) || dragData.HasXaml;
        
        return dragData;
    }
    
    private static DragDropEffects GetAppropriateEffect(DragData dragData)
    {
        if (dragData.HasXaml || dragData.FileTypes.Contains(".axaml") || dragData.FileTypes.Contains(".xaml"))
            return DragDropEffects.Copy | DragDropEffects.Move;
        
        if (dragData.FileTypes.Any(ext => new[] { ".png", ".jpg", ".jpeg", ".gif", ".ico" }.Contains(ext)))
            return DragDropEffects.Copy;
        
        return DragDropEffects.Copy;
    }
    
    private static void ApplyDragEnterEffect(Control element)
    {
       
        if (element is Panel panel)
        {
            panel.Background = new SolidColorBrush(Color.FromArgb(30, 0, 120, 215));
        }
        else if (element is Border border)
        {
            border.Background = new SolidColorBrush(Color.FromArgb(30, 0, 120, 215));
        }
        
        
        element.Opacity = 0.8;
    }
    
    private static void RemoveDragEffects(Control element)
    {
        if (element is Panel panel)
        {
            panel.Background = Brushes.Transparent;
        }
        else if (element is Border border)
        {
            border.Background = Brushes.Transparent;
        }
        
        element.Opacity = 1.0;
    }
    
    private static string GetFileIcon(string extension)
    {
        return FileTypeIcons.TryGetValue(extension.ToLower(), out string icon) ? icon : "📄";
    }
    
    private static void ProcessDrop(Control element, DragData dragData, Point position)
    {
        
        var args = new EnhancedDropEventArgs(dragData, position);
        
        foreach (var file in dragData.Files)
        {
            var extension = Path.GetExtension(file).ToLower();
            Console.WriteLine($"Processing file: {Path.GetFileName(file)} ({GetFileIcon(extension)})");
            
            switch (extension)
            {
                case ".axaml":
                case ".xaml":
                   
                    Shell.Instance?.Open(file);
                    break;
                    
                case ".cs":
                   
                    break;
                    
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".gif":
                case ".ico":
                   
                    break;
            }
        }
        
        
        if (!string.IsNullOrEmpty(dragData.Text))
        {
            if (dragData.HasXaml)
            {
                Console.WriteLine("Processing XAML content");
               
            }
            else
            {
                Console.WriteLine($"Processing text: {dragData.Text.Substring(0, Math.Min(50, dragData.Text.Length))}...");
            }
        }
    }
    
   
}
