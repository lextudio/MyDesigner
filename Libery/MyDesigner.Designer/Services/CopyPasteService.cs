using Avalonia.Input.Platform;
using MyDesigner.Design.Services;
using MyDesigner.Designer;
using MyDesigner.Designer.Xaml;

namespace MyDesigner.Designer.Services;

public class CopyPasteService : ICopyPasteService
{
    public virtual bool CanCopy(DesignContext designContext)
    {
        var selectionService = designContext.Services.GetService<ISelectionService>();
        if (selectionService != null)
        {
            if (selectionService.SelectedItems.Count == 0)
                return false;
            if (selectionService.SelectedItems.Contains(designContext.RootItem))
                return false;
        }

        return true;
    }

    public virtual void Copy(DesignContext designContext)
    {
        var xamlContext = designContext as XamlDesignContext;
        var selectionService = designContext.Services.GetService<ISelectionService>();
        if (xamlContext != null && selectionService != null &&
            !selectionService.SelectedItems.Contains(designContext.RootItem))
            xamlContext.XamlEditAction.Copy(selectionService.SelectedItems);
    }

    public virtual bool CanCut(DesignContext designContext)
    {
        return CanCopy(designContext);
    }

    public virtual void Cut(DesignContext designContext)
    {
        var xamlContext = designContext as XamlDesignContext;
        var selectionService = designContext.Services.GetService<ISelectionService>();
        if (xamlContext != null && selectionService != null)
            xamlContext.XamlEditAction.Cut(selectionService.SelectedItems);
    }

    public virtual bool CanDelete(DesignContext designContext)
    {
        if (designContext != null)
            return ModelTools.CanDeleteComponents(designContext.Services.Selection.SelectedItems);
        return false;
    }

    public virtual void Delete(DesignContext designContext)
    {
        if (designContext != null) ModelTools.DeleteComponents(designContext.Services.Selection.SelectedItems);
    }

    public virtual bool CanPaste(DesignContext designContext)
    {
        var selectionService = designContext.Services.GetService<ISelectionService>();
        if (selectionService != null && selectionService.SelectedItems.Count != 0)
            try
            {
                // In Avalonia, clipboard access is different
                var clipboard = TopLevel.GetTopLevel(designContext.RootItem.View as Control)?.Clipboard;
                if (clipboard != null)
                {
                    var task = clipboard.GetTextAsync();
                    task.Wait(1000); // Wait up to 1 second
                    var xaml = task.Result;
                    if (!string.IsNullOrWhiteSpace(xaml))
                        return true;
                }
            }
            catch (Exception)
            {
            }

        return false;
    }

    public virtual void Paste(DesignContext designContext)
    {
        var xamlContext = designContext as XamlDesignContext;
        if (xamlContext != null) xamlContext.XamlEditAction.Paste();
    }

    public void Copy(ICollection<DesignItem> items)
    {
        // Implementation for copying items
        // This would serialize the items to clipboard
    }

    public ICollection<DesignItem> Paste()
    {
        // Implementation for pasting items
        // This would deserialize items from clipboard
        return new List<DesignItem>();
    }
}