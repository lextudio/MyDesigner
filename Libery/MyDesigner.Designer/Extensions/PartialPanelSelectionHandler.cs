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
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Controls;
using MyDesigner.Design.Extensions;
using MyDesigner.Designer.Services;

namespace MyDesigner.Designer.Extensions;

public class PartialPanelSelectionHandler : BehaviorExtension, IHandlePointerToolMouseDown
{
    #region IHandlePointerToolMouseDown

    public void HandleSelectionMouseDown(IDesignPanel designPanel, PointerPressedEventArgs e,
        DesignPanelHitTestResult result)
    {
        if (e.GetCurrentPoint(null).Properties.IsLeftButtonPressed && MouseGestureBase.IsOnlyButtonPressed(e, PointerUpdateKind.LeftButtonPressed))
        {
            e.Handled = true;
            new PartialRangeSelectionGesture(result.ModelHit).Start(designPanel, e);
        }
    }

    #endregion

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ExtendedItem.AddBehavior(typeof(IHandlePointerToolMouseDown), this);
    }
}

/// <summary>
/// </summary>
internal class PartialRangeSelectionGesture : RangeSelectionGesture
{
    public PartialRangeSelectionGesture(DesignItem container)
        : base(container)
    {
    }

    protected override ICollection<DesignItem> GetChildDesignItemsInContainer(Geometry geometry)
    {
        var resultItems = new HashSet<DesignItem>();
        var viewService = container.Services.View;

        // Note: Avalonia doesn't have HitTest with callbacks like WPF
        // This would need to be implemented differently using Avalonia's hit testing
        // For now, we'll use a simplified approach

        if (container.View is Visual containerVisual)
        {
            var bounds = geometry.Bounds;
            var children = containerVisual.GetVisualChildren().OfType<Control>();
            
            foreach (var child in children)
            {
                var childBounds = child.Bounds;
                // For partial selection, we also include intersecting items
                if (bounds.Contains(childBounds) || bounds.Intersects(childBounds))
                {
                    var model = viewService.GetModel(child);
                    if (model != null && model != container)
                    {
                        resultItems.Add(model);
                    }
                }
            }
        }

        return resultItems;
    }
}