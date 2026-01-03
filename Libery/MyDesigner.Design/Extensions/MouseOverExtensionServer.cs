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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using MyDesigner.Design.Adorners;

namespace MyDesigner.Design.Extensions;

/// <summary>
///     Applies an extension to the hovered components.
/// </summary>
public class MouseOverExtensionServer : DefaultExtensionServer
{
    private DesignItem _lastItem;

    /// <summary>
    ///     Is called after the extension server is initialized and the Context property has been set.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        var panel = Services.GetService<IDesignPanel>() as Control;
        if (panel != null)
        {
            panel.PointerMoved += MouseOverExtensionServer_PointerMoved;
            panel.PointerExited += MouseOverExtensionServer_PointerLeave;
            Services.Selection.SelectionChanged += OnSelectionChanged;
        }
    }

    private void OnSelectionChanged(object sender, DesignItemCollectionEventArgs e)
    {
        ReapplyExtensions(e.Items);
    }

    private void MouseOverExtensionServer_PointerLeave(object sender, PointerEventArgs e)
    {
        if (_lastItem != null)
        {
            var oldLastItem = _lastItem;
            _lastItem = null;
            ReapplyExtensions(new[] { oldLastItem });
        }
    }

    private void MouseOverExtensionServer_PointerMoved(object sender, PointerEventArgs e)
    {
        DesignItem element = null;
        var panel = (Control)Services.DesignPanel;
        var position = e.GetPosition(panel);
        
        // Simple hit testing in Avalonia
        var hitTest = panel.InputHitTest(position);
        if (hitTest is Visual visual)
        {
            // Walk up the visual tree to find design items
            var current = visual;
            while (current != null)
            {
                if (current is IAdornerLayer)
                {
                    current = current.GetVisualParent();
                    continue;
                }

                if (Extension.GetDisableMouseOverExtensions(current))
                {
                    current = current.GetVisualParent();
                    continue;
                }

                var item = Services.Component.GetDesignItem(current);
                if (item != null)
                {
                    element = item;
                    break;
                }

                current = current.GetVisualParent();
            }
        }

        var oldLastItem = _lastItem;
        _lastItem = element;
        if (oldLastItem != null && oldLastItem != element)
            ReapplyExtensions(new[] { oldLastItem, element });
        else
            ReapplyExtensions(new[] { element });
    }

    /// <summary>
    ///     Gets if the item is selected.
    /// </summary>
    public override bool ShouldApplyExtensions(DesignItem extendedItem)
    {
        return extendedItem == _lastItem && !Services.Selection.IsComponentSelected(extendedItem);
    }
}