using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using Avalonia.Input;
using Avalonia.Controls;
using MyDesigner.Design;
using MyDesigner.Designer;
using MyDesigner.Designer.Xaml;
using MyDesigner.XamlDom;

namespace MyDesigner.XamlDesigner.Commands
{
    /// <summary>
    /// Helper class for routing commands to the design surface
    /// </summary>
    public static class DesignSurfaceCommands
    {
        public static async System.Threading.Tasks.Task RouteToDesignSurfaceAsync(DesignSurface designSurface, string commandName)
        {
            if (designSurface?.DesignContext == null)
                return;

            try
            {
                switch (commandName.ToLower())
                {
                    case "undo":
                        var undoService = designSurface.DesignContext.Services.GetService<MyDesigner.Designer.Services.UndoService>();
                        undoService?.Undo();
                        break;

                    case "redo":
                        var redoService = designSurface.DesignContext.Services.GetService<MyDesigner.Designer.Services.UndoService>();
                        redoService?.Redo();
                        break;

                    case "cut":
                        var cutSelection = designSurface.DesignContext.Services.Selection;
                        if (cutSelection?.SelectedItems?.Count > 0)
                        {
                            // Copy to clipboard first
                            await CopyToClipboardAsync(cutSelection.SelectedItems);
                            
                            // Then delete the selected items
                            var selectedItems = cutSelection.SelectedItems.ToArray();
                            foreach (var item in selectedItems)
                            {
                                item.Remove();
                            }
                        }
                        break;

                    case "copy":
                        var copySelection = designSurface.DesignContext.Services.Selection;
                        if (copySelection?.SelectedItems?.Count > 0)
                        {
                            await CopyToClipboardAsync(copySelection.SelectedItems);
                        }
                        break;

                    case "paste":
                        await PasteFromClipboardAsync(designSurface);
                        break;

                    case "delete":
                        var deleteSelection = designSurface.DesignContext.Services.Selection;
                        if (deleteSelection?.SelectedItems?.Count > 0)
                        {
                            var selectedItems = deleteSelection.SelectedItems.ToArray();
                            foreach (var item in selectedItems)
                            {
                                item.Remove();
                            }
                        }
                        break;

                    case "selectall":
                        var rootItem = designSurface.DesignContext.RootItem;
                        if (rootItem != null)
                        {
                            var allItems = GetAllDesignItems(rootItem);
                            designSurface.DesignContext.Services.Selection.SetSelectedComponents(allItems);
                        }
                        break;

                    default:
                        System.Diagnostics.Debug.WriteLine($"Unknown command: {commandName}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        public static void RouteToDesignSurface(DesignSurface designSurface, string commandName)
        {
            // Synchronous wrapper for backward compatibility
            _ = RouteToDesignSurfaceAsync(designSurface, commandName);
        }

        public static bool CanExecuteOnDesignSurface(DesignSurface designSurface, string commandName)
        {
            if (designSurface?.DesignContext == null)
                return false;

            try
            {
                switch (commandName.ToLower())
                {
                    case "undo":
                        var undoService = designSurface.DesignContext.Services.GetService<MyDesigner.Designer.Services.UndoService>();
                        return undoService?.CanUndo ?? false;

                    case "redo":
                        var redoService = designSurface.DesignContext.Services.GetService<MyDesigner.Designer.Services.UndoService>();
                        return redoService?.CanRedo ?? false;

                    case "cut":
                    case "copy":
                    case "delete":
                        // Check if there are selected items
                        var selectionService = designSurface.DesignContext.Services.Selection;
                        return selectionService?.SelectedItems?.Count > 0;

                    case "paste":
                        // TODO: Check if clipboard has compatible content
                        return true;

                    case "selectall":
                        return true;

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
                return false;
            }
        }

        private static IList<DesignItem> GetAllDesignItems(DesignItem rootItem)
        {
            var items = new List<DesignItem>();
            CollectDesignItems(rootItem, items);
            return items;
        }

        private static void CollectDesignItems(DesignItem item, List<DesignItem> items)
        {
            if (item == null) return;
            
            items.Add(item);
            
            // Collect items from content property
            if (item.ContentProperty?.IsCollection == true)
            {
                foreach (var child in item.ContentProperty.CollectionElements)
                {
                    CollectDesignItems(child, items);
                }
            }
            else if (item.ContentProperty?.Value != null)
            {
                CollectDesignItems(item.ContentProperty.Value, items);
            }
            
            // Collect items from other properties
            foreach (var property in item.Properties)
            {
                if (property.IsCollection)
                {
                    foreach (var child in property.CollectionElements)
                    {
                        CollectDesignItems(child, items);
                    }
                }
                else if (property.Value != null)
                {
                    CollectDesignItems(property.Value, items);
                }
            }
        }

        private static async System.Threading.Tasks.Task CopyToClipboardAsync(ICollection<DesignItem> items)
        {
            try
            {
                if (items?.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("<DesignerClipboard>");
                    
                    foreach (var item in items)
                    {
                        using (var writer = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
                        {
                            // Use a simple XML serialization approach instead of XamlStaticTools
                            sb.AppendLine($"<{item.ComponentType.Name}>");
                            // Add properties here if needed
                            sb.AppendLine($"</{item.ComponentType.Name}>");
                        }
                    }
                    
                    sb.AppendLine("</DesignerClipboard>");
                    
                    // Get clipboard from main window
                    var mainWindow = MainWindow.Instance;
                    var clipboard = mainWindow?.Clipboard;
                    if (clipboard != null)
                    {
                        await clipboard.SetTextAsync(sb.ToString());
                        System.Diagnostics.Debug.WriteLine($"Copied {items.Count} items to clipboard");
                    }
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        private static async System.Threading.Tasks.Task PasteFromClipboardAsync(DesignSurface designSurface)
        {
            try
            {
                // Get clipboard from main window
                var mainWindow = MainWindow.Instance;
                var clipboard = mainWindow?.Clipboard;
                if (clipboard != null)
                {
                    var clipboardText = await clipboard.GetTextAsync();
                    if (!string.IsNullOrEmpty(clipboardText) && clipboardText.Contains("<DesignerClipboard>"))
                    {
                        // For now, just show a debug message
                        // Full clipboard parsing would require more complex implementation
                        System.Diagnostics.Debug.WriteLine("Paste operation - clipboard contains designer data");
                    }
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }
    }
}