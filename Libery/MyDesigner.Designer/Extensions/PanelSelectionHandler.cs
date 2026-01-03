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
using Avalonia.Media;
using Avalonia.VisualTree;
using MyDesigner.Design.Adorners;
using MyDesigner.Design.Extensions;
using MyDesigner.Design.Services;
using MyDesigner.Designer.Controls;
using MyDesigner.Designer.Services;

namespace MyDesigner.Designer.Extensions;

/// <summary>
///     Handles selection multiple controls inside a Panel.
/// </summary>
[ExtensionFor(typeof(Panel))]
public class PanelSelectionHandler : BehaviorExtension, IHandlePointerToolMouseDown
{
    public void HandleSelectionMouseDown(IDesignPanel designPanel, PointerPressedEventArgs e,
        DesignPanelHitTestResult result)
    {
        if (e.GetCurrentPoint(null).Properties.IsLeftButtonPressed && MouseGestureBase.IsOnlyButtonPressed(e, PointerUpdateKind.LeftButtonPressed))
        {
            e.Handled = true;
            new RangeSelectionGesture(result.ModelHit).Start(designPanel, e);
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ExtendedItem.AddBehavior(typeof(IHandlePointerToolMouseDown), this);
    }
}

internal class RangeSelectionGesture : ClickOrDragMouseGesture
{
    protected AdornerPanel? adornerPanel;
    protected DesignItem container;

    protected GrayOutDesignerExceptActiveArea? grayOut;
    protected SelectionFrame? selectionFrame;

    public RangeSelectionGesture(DesignItem container)
    {
        this.container = container;
        positionRelativeTo = container.View;
    }

    protected override void OnDragStarted(PointerEventArgs e)
    {
        adornerPanel = new AdornerPanel();
        adornerPanel.SetAdornedElement(container.View, container);

        selectionFrame = new SelectionFrame();
        adornerPanel.Children.Add(selectionFrame);

        designPanel.Adorners.Add(adornerPanel);

        GrayOutDesignerExceptActiveArea.Start(ref grayOut, services, container.View);
    }

    protected override void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        base.OnPointerMoved(sender, e);
        if (hasDragStarted) SetPlacement(e.GetPosition(positionRelativeTo as Visual));
    }

    protected override void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!hasDragStarted)
        {
            services.Selection.SetSelectedComponents(new[] { container }, SelectionTypes.Auto);
        }
        else
        {
            var endPoint = e.GetPosition(positionRelativeTo as Visual);
            var frameRect = new Rect(
                Math.Min(startPoint.X, endPoint.X),
                Math.Min(startPoint.Y, endPoint.Y),
                Math.Abs(startPoint.X - endPoint.X),
                Math.Abs(startPoint.Y - endPoint.Y)
            );

            var items = GetChildDesignItemsInContainer(new RectangleGeometry(frameRect));
            if (items.Count == 0) items.Add(container);

            var filterService = services.GetService<ISelectionFilterService>();
            if (filterService != null) items = filterService.FilterSelectedElements(items);

            services.Selection.SetSelectedComponents(items, SelectionTypes.Auto);
        }

        Stop();
    }

    protected virtual ICollection<DesignItem> GetChildDesignItemsInContainer(Geometry geometry)
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

    private void SetPlacement(Point endPoint)
    {
        var p = new RelativePlacement();
        p.XOffset = Math.Min(startPoint.X, endPoint.X);
        p.YOffset = Math.Min(startPoint.Y, endPoint.Y);
        p.WidthOffset = Math.Max(startPoint.X, endPoint.X) - p.XOffset;
        p.HeightOffset = Math.Max(startPoint.Y, endPoint.Y) - p.YOffset;
        if (selectionFrame != null)
            AdornerPanel.SetPlacement(selectionFrame, p);
    }

    protected override void OnStopped()
    {
        if (adornerPanel != null)
        {
            designPanel.Adorners.Remove(adornerPanel);
            adornerPanel = null;
        }

        GrayOutDesignerExceptActiveArea.Stop(ref grayOut);
        selectionFrame = null;
        base.OnStopped();
    }
}