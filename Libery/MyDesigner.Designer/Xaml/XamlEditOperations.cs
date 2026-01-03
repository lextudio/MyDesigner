// Copyright (c) 2019 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using MyDesigner.XamlDom;

namespace MyDesigner.Designer.Xaml;

/// <summary>
///     Deals with operations on controls which also require access to internal XML properties of the XAML Document.
/// </summary>
public class XamlEditOperations
{
    private static readonly char _delimeter = Convert.ToChar(0x7F);
    private readonly XamlDesignContext _context;
    private readonly XamlParserSettings _settings;

    public XamlEditOperations(XamlDesignContext context, XamlParserSettings settings)
    {
        _context = context;
        _settings = settings;
    }

    /// <summary>
    ///     Delimet character to seperate different piece of Xaml's
    /// </summary>
    public char Delimeter => _delimeter;

    /// <summary>
    ///     Copy <paramref name="designItems" /> from the designer to clipboard.
    /// </summary>
    public async void Cut(ICollection<DesignItem> designItems)
    {
        var clipboard = TopLevel.GetTopLevel(_context.RootItem.View as Visual)?.Clipboard;
        if (clipboard == null) return;

        await clipboard.ClearAsync();

        var cutList = RemoveChildItemsWhenContainerIsInList(designItems);

        var cutXaml = string.Empty;
        var changeGroup = _context.OpenGroup("Cut " + cutList.Count + "/" + designItems.Count + " elements", cutList);
        foreach (var item in cutList)
            if (item != null && item != _context.RootItem)
            {
                var xamlItem = item as XamlDesignItem;
                if (xamlItem != null)
                {
                    cutXaml += XamlStaticTools.GetXaml(xamlItem.XamlObject);
                    cutXaml += _delimeter;
                }
            }

        ModelTools.DeleteComponents(cutList);
        await clipboard.SetTextAsync(cutXaml);
        changeGroup.Commit();
    }

    /// <summary>
    ///     Copy <paramref name="designItems" /> from the designer to clipboard.
    /// </summary>
    public async void Copy(ICollection<DesignItem> designItems)
    {
        var clipboard = TopLevel.GetTopLevel(_context.RootItem.View as Visual)?.Clipboard;
        if (clipboard == null) return;

        await clipboard.ClearAsync();

        var copyList = RemoveChildItemsWhenContainerIsInList(designItems);

        var copiedXaml = string.Empty;
        var changeGroup =
            _context.OpenGroup("Copy " + copyList.Count + "/" + designItems.Count + " elements", copyList);
        foreach (var item in copyList)
            if (item != null)
            {
                var xamlItem = item as XamlDesignItem;
                if (xamlItem != null)
                {
                    copiedXaml += XamlStaticTools.GetXaml(xamlItem.XamlObject);
                    copiedXaml += _delimeter;
                }
            }

        await clipboard.SetTextAsync(copiedXaml);
        changeGroup.Commit();
    }

    /// <summary>
    ///     Paste items from clipboard into the PrimarySelection.
    /// </summary>
    public void Paste()
    {
        Paste(_context.Services.Selection.PrimarySelection);
    }

    /// <summary>
    ///     Paste items from clipboard into the container.
    /// </summary>
    public async void Paste(DesignItem container)
    {
        var clipboard = TopLevel.GetTopLevel(_context.RootItem.View as Visual)?.Clipboard;
        if (clipboard == null) return;

        var parent = container;
        var child = container;

        var pasted = false;
        var combinedXaml = await clipboard.GetTextAsync();
        if (string.IsNullOrEmpty(combinedXaml)) return;

        IEnumerable<string> xamls = combinedXaml.Split(_delimeter);
        xamls = xamls.Where(xaml => xaml != string.Empty);


        var rootItem = parent.Services.DesignPanel.Context.RootItem as XamlDesignItem;
        var pastedItems = new Collection<DesignItem>();
        foreach (var xaml in xamls)
        {
            var obj = XamlParser.ParseSnippet(rootItem.XamlObject, xaml, _settings);
            if (obj != null)
            {
                DesignItem item = ((XamlComponentService)parent.Services.Component).RegisterXamlComponentRecursive(obj);
                if (item != null)
                    pastedItems.Add(item);
            }
        }

        if (pastedItems.Count != 0)
        {
            var changeGroup =
                parent.Services.DesignPanel.Context.OpenGroup("Paste " + pastedItems.Count + " elements", pastedItems);
            while (parent != null && !pasted)
                if (parent.ContentProperty != null)
                {
                    if (parent.ContentProperty.IsCollection)
                    {
                        if (CollectionSupport.CanCollectionAdd(parent.ContentProperty.ReturnType,
                                pastedItems.Select(item => item.Component)) &&
                            parent.GetBehavior<IPlacementBehavior>() != null)
                        {
                            AddInParent(parent, pastedItems);
                            pasted = true;
                        }
                    }
                    else if (pastedItems.Count == 1 && parent.ContentProperty.Value == null &&
                             parent.ContentProperty.ValueOnInstance == null && parent.View is ContentControl)
                    {
                        AddInParent(parent, pastedItems);
                        pasted = true;
                    }

                    if (!pasted)
                        parent = parent.Parent;
                }
                else
                {
                    parent = parent.Parent;
                }

            while (!pasted)
                if (child.ContentProperty != null)
                {
                    if (child.ContentProperty.IsCollection)
                    {
                        foreach (var col in child.ContentProperty.CollectionElements)
                            if (col.ContentProperty != null && col.ContentProperty.IsCollection)
                                if (CollectionSupport.CanCollectionAdd(col.ContentProperty.ReturnType,
                                        pastedItems.Select(item => item.Component)))
                                    pasted = true;

                        break;
                    }

                    if (child.ContentProperty.Value != null)
                    {
                        child = child.ContentProperty.Value;
                    }
                    else if (pastedItems.Count == 1)
                    {
                        child.ContentProperty.SetValue(pastedItems.First().Component);
                        pasted = true;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }

            foreach (var pastedItem in pastedItems)
                ((XamlComponentService)parent.Services.Component).RaiseComponentRegisteredAndAddedToContainer(
                    pastedItem);


            changeGroup.Commit();
        }
    }

    /// <summary>
    ///     Adds Items under a parent given that the content property is collection and can add types of
    ///     <paramref name="pastedItems" />
    /// </summary>
    /// <param name="parent">The Parent element</param>
    /// <param name="pastedItems">The list of elements to be added</param>
    private static void AddInParent(DesignItem parent, IList<DesignItem> pastedItems)
    {
        var rects = pastedItems.Select(i => new Rect(new Point(0, 0),
            new Point(i.Properties["Width"].GetConvertedValueOnInstance<double>(),
                i.Properties["Height"].GetConvertedValueOnInstance<double>())));
        var operation =
            PlacementOperation.TryStartInsertNewComponents(parent, pastedItems, rects.ToList(),
                PlacementType.PasteItem);
        var selection = parent.Services.DesignPanel.Context.Services.Selection;
        selection.SetSelectedComponents(pastedItems);
        if (operation != null)
            operation.Commit();
    }

    private List<DesignItem> RemoveChildItemsWhenContainerIsInList(ICollection<DesignItem> designItems)
    {
        var copyList = designItems.ToList();
        foreach (var designItem in designItems)
        {
            var parent = designItem.Parent;
            while (parent != null)
            {
                if (copyList.Contains(parent)) copyList.Remove(designItem);
                parent = parent.Parent;
            }
        }

        return copyList;
    }
}