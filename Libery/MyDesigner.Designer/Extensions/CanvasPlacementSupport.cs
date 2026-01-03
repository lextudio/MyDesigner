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
using Avalonia.Layout;
using MyDesigner.Design.Extensions;
using MyDesigner.Designer.Controls;

namespace MyDesigner.Designer.Extensions;

/// <summary>
///     Provides <see cref="IPlacementBehavior" /> behavior for <see cref="Canvas" />.
/// </summary>
[ExtensionFor(typeof(Canvas), OverrideExtension = typeof(DefaultPlacementBehavior))]
public class CanvasPlacementSupport : SnaplinePlacementBehavior
{
    private Control extendedComponent;
    private Control extendedView;
    private GrayOutDesignerExceptActiveArea grayOut;

    private static double GetCanvasProperty(Control element, AvaloniaProperty property)
    {
        var v = element.GetValue(property);
        if (v is double d && double.IsNaN(d))
            return 0;
        return Convert.ToDouble(v);
    }

    private static bool IsPropertySet(Control element, AvaloniaProperty property)
    {
        return element.IsSet(property);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        extendedComponent = (Control)ExtendedItem.Component;
        extendedView = (Control)ExtendedItem.View;
    }

    public override Rect GetPosition(PlacementOperation operation, DesignItem item)
    {
        var child = item.View as Control;

        if (child == null)
            return RectExtensions.Empty;

        double x, y;

        if (IsPropertySet(child, Canvas.LeftProperty) || !IsPropertySet(child, Canvas.RightProperty))
            x = GetCanvasProperty(child, Canvas.LeftProperty);
        else
            x = extendedComponent.Bounds.Width - GetCanvasProperty(child, Canvas.RightProperty) -
                PlacementOperation.GetRealElementSize(child).Width;

        if (IsPropertySet(child, Canvas.TopProperty) || !IsPropertySet(child, Canvas.BottomProperty))
            y = GetCanvasProperty(child, Canvas.TopProperty);
        else
            y = extendedComponent.Bounds.Height - GetCanvasProperty(child, Canvas.BottomProperty) -
                PlacementOperation.GetRealElementSize(child).Height;

        var p = new Point(x, y);
        return new Rect(p, PlacementOperation.GetRealElementSize(item.View));
    }

    public override void SetPosition(PlacementInformation info)
    {
        base.SetPosition(info);
        info.Item.Properties[Layoutable.MarginProperty].Reset();

        var child = info.Item.View as Control;
        var newPosition = info.Bounds;

        if (IsPropertySet(child, Canvas.LeftProperty) || !IsPropertySet(child, Canvas.RightProperty))
        {
            if (newPosition.Left != GetCanvasProperty(child, Canvas.LeftProperty))
                info.Item.Properties.GetAttachedProperty(Canvas.LeftProperty).SetValue(newPosition.Left);
        }
        else
        {
            var newR = extendedComponent.Bounds.Width - newPosition.Right;
            if (newR != GetCanvasProperty(child, Canvas.RightProperty))
                info.Item.Properties.GetAttachedProperty(Canvas.RightProperty).SetValue(newR);
        }

        if (IsPropertySet(child, Canvas.TopProperty) || !IsPropertySet(child, Canvas.BottomProperty))
        {
            if (newPosition.Top != GetCanvasProperty(child, Canvas.TopProperty))
                info.Item.Properties.GetAttachedProperty(Canvas.TopProperty).SetValue(newPosition.Top);
        }
        else
        {
            var newB = extendedComponent.Bounds.Height - newPosition.Bottom;
            if (newB != GetCanvasProperty(child, Canvas.BottomProperty))
                info.Item.Properties.GetAttachedProperty(Canvas.BottomProperty).SetValue(newB);
        }

        if (info.Item == Services.Selection.PrimarySelection)
        {
            var b = new Rect(0, 0, extendedView.Bounds.Width, extendedView.Bounds.Height);
            // only for primary selection:
            if (grayOut != null)
                grayOut.AnimateActiveAreaRectTo(b);
            else
                GrayOutDesignerExceptActiveArea.Start(ref grayOut, Services, ExtendedItem.View, b);
        }
    }

    public override void LeaveContainer(PlacementOperation operation)
    {
        GrayOutDesignerExceptActiveArea.Stop(ref grayOut);
        base.LeaveContainer(operation);
        foreach (var info in operation.PlacedItems)
        {
            info.Item.Properties.GetAttachedProperty(Canvas.LeftProperty).Reset();
            info.Item.Properties.GetAttachedProperty(Canvas.TopProperty).Reset();
        }
    }

    public override void EnterContainer(PlacementOperation operation)
    {
        base.EnterContainer(operation);
        foreach (var info in operation.PlacedItems)
        {
            info.Item.Properties[Layoutable.HorizontalAlignmentProperty].Reset();
            info.Item.Properties[Layoutable.VerticalAlignmentProperty].Reset();
            info.Item.Properties[Layoutable.MarginProperty].Reset();

            if (operation.Type == PlacementType.PasteItem)
            {
                if (!double.IsNaN(info.Item.Properties.GetAttachedProperty(Canvas.LeftProperty)
                        .GetConvertedValueOnInstance<double>()))
                    info.Item.Properties.GetAttachedProperty(Canvas.LeftProperty)
                        .SetValue(info.Item.Properties.GetAttachedProperty(Canvas.LeftProperty)
                                      .GetConvertedValueOnInstance<double>() +
                                  PlacementOperation.PasteOffset);

                if (!double.IsNaN(info.Item.Properties.GetAttachedProperty(Canvas.TopProperty)
                        .GetConvertedValueOnInstance<double>()))
                    info.Item.Properties.GetAttachedProperty(Canvas.TopProperty)
                        .SetValue(info.Item.Properties.GetAttachedProperty(Canvas.TopProperty)
                                      .GetConvertedValueOnInstance<double>() +
                                  PlacementOperation.PasteOffset);
            }
        }
    }

    public override void EndPlacement(PlacementOperation operation)
    {
        GrayOutDesignerExceptActiveArea.Stop(ref grayOut);
        base.EndPlacement(operation);
    }
}