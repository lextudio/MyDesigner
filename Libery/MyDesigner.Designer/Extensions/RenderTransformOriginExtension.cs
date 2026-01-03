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

using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using MyDesigner.Design.Adorners;
using MyDesigner.Design.Extensions;
using MyDesigner.Designer.Controls;

namespace MyDesigner.Designer.Extensions;

[ExtensionServer(typeof(OnlyOneItemSelectedExtensionServer))]
[ExtensionFor(typeof(Control))]
public class RenderTransformOriginExtension : SelectionAdornerProvider
{
    private readonly AdornerPanel adornerPanel;

    private Point renderTransformOrigin = new(0.5, 0.5);

    private RenderTransformOriginThumb renderTransformOriginThumb;

    //		IPlacementBehavior resizeBehavior;
    //		PlacementOperation operation;
    //		ChangeGroup changeGroup;

    public RenderTransformOriginExtension()
    {
        adornerPanel = new AdornerPanel();
        adornerPanel.Order = AdornerOrder.Foreground;
        Adorners.Add(adornerPanel);

        CreateRenderTransformOriginThumb();
    }

    private void CreateRenderTransformOriginThumb()
    {
        renderTransformOriginThumb = new RenderTransformOriginThumb();
        renderTransformOriginThumb.Cursor = new Cursor(StandardCursorType.Hand);

        AdornerPanel.SetPlacement(renderTransformOriginThumb,
            new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top)
            {
                XRelativeToContentWidth = renderTransformOrigin.X, YRelativeToContentHeight = renderTransformOrigin.Y
            });
        adornerPanel.Children.Add(renderTransformOriginThumb);

        renderTransformOriginThumb.DragDelta += renderTransformOriginThumb_DragDelta;
        renderTransformOriginThumb.DragCompleted += renderTransformOriginThumb_DragCompleted;
    }

    private void renderTransformOriginThumb_DragCompleted(object sender, VectorEventArgs e)
    {
        ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty)
            .SetValue(new RelativePoint(Math.Round(renderTransformOrigin.X, 4), Math.Round(renderTransformOrigin.Y, 4), RelativeUnit.Relative));
    }

    private void renderTransformOriginThumb_DragDelta(object sender, VectorEventArgs e)
    {
        var p = AdornerPanel.GetPlacement(renderTransformOriginThumb) as RelativePlacement;
        if (p == null) return;
        var pointAbs =
            adornerPanel.RelativeToAbsolute(new Vector(p.XRelativeToContentWidth, p.YRelativeToContentHeight));
        var pointAbsNew = pointAbs + new Vector(e.Vector.X, e.Vector.Y);
        var pRel = adornerPanel.AbsoluteToRelative(pointAbsNew);
        renderTransformOrigin = new Point(pRel.X, pRel.Y);

        ExtendedItem.View.SetValue(Visual.RenderTransformOriginProperty, new RelativePoint(renderTransformOrigin.X, renderTransformOrigin.Y, RelativeUnit.Relative));
        //this.ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).SetValue(new RelativePoint(Math.Round(pRel.X, 4), Math.Round(pRel.Y, 4), RelativeUnit.Relative));
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ExtendedItem.PropertyChanged += OnPropertyChanged;

        if (ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).IsSet)
        {
            var origin = ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty)
                .GetConvertedValueOnInstance<RelativePoint>();
            renderTransformOrigin = new Point(origin.Point.X, origin.Point.Y);
        }

        AdornerPanel.SetPlacement(renderTransformOriginThumb,
            new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top)
            {
                XRelativeToContentWidth = renderTransformOrigin.X, YRelativeToContentHeight = renderTransformOrigin.Y
            });

        // In Avalonia, we need to handle property changes differently
        ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).ValueChanged += OnRenderTransformOriginPropertyChanged;
    }

    private void OnRenderTransformOriginPropertyChanged(object sender, EventArgs e)
    {
        var pRel = renderTransformOrigin;
        if (ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).IsSet)
        {
            var origin = ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty)
                .GetConvertedValueOnInstance<RelativePoint>();
            pRel = new Point(origin.Point.X, origin.Point.Y);
        }

        AdornerPanel.SetPlacement(renderTransformOriginThumb,
            new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top)
                { XRelativeToContentWidth = pRel.X, YRelativeToContentHeight = pRel.Y });
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
    }

    protected override void OnRemove()
    {
        ExtendedItem.PropertyChanged -= OnPropertyChanged;
        ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).ValueChanged -= OnRenderTransformOriginPropertyChanged;

        base.OnRemove();
    }
}