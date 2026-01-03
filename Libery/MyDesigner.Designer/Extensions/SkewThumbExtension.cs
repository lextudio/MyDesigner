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
using Avalonia.Media;
using Avalonia.VisualTree;
using MyDesigner.Design.Adorners;
using MyDesigner.Design.Extensions;
using MyDesigner.Design.UIExtensions;
using MyDesigner.Designer.Controls;

namespace MyDesigner.Designer.Extensions;

[ExtensionServer(typeof(OnlyOneItemSelectedExtensionServer))]
[ExtensionFor(typeof(Control))]
public sealed class SkewThumbExtension : SelectionAdornerProvider
{
    private readonly AdornerPanel adornerPanel;
    private readonly DesignItem[] extendedItemArray = new DesignItem[1];

    private Controls.AdornerLayer _adornerLayer;

    public SkewThumbExtension()
    {
        adornerPanel = new AdornerPanel();
        adornerPanel.Order = AdornerOrder.BeforeForeground;
        Adorners.Add(adornerPanel);
    }

    protected override void OnInitialized()
    {
        if (ExtendedItem.Component is WindowClone)
            return;
        base.OnInitialized();

        extendedItemArray[0] = ExtendedItem;
        ExtendedItem.PropertyChanged += OnPropertyChanged;

        var designerItem = ExtendedItem.Component as Control;
        skewTransform = designerItem.RenderTransform as SkewTransform;

        if (skewTransform != null)
        {
            skewX = skewTransform.AngleX;
            skewY = skewTransform.AngleY;
        }

        thumb1 = new Thumb { Cursor = new Cursor(StandardCursorType.SizeWestEast), Height = 14, Width = 4, Opacity = 1 };
        thumb2 = new Thumb { Cursor = new Cursor(StandardCursorType.SizeNorthSouth), Width = 14, Height = 4, Opacity = 1 };

        OnPropertyChanged(null, null);

        adornerPanel.Children.Add(thumb1);
        adornerPanel.Children.Add(thumb2);

        var drag1 = new DragListener(thumb1);
        drag1.Started += dragX_Started;
        drag1.Changed += dragX_Changed;
        drag1.Completed += dragX_Completed;
        var drag2 = new DragListener(thumb2);
        drag2.Started += dragY_Started;
        drag2.Changed += dragY_Changed;
        drag2.Completed += dragY_Completed;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (sender == null || e.PropertyName == "Width" || e.PropertyName == "Height")
        {
            AdornerPanel.SetPlacement(thumb1,
                new RelativePlacement(HorizontalAlignment.Center, VerticalAlignment.Top)
                {
                    YOffset = 0,
                    XOffset = -1 * PlacementOperation.GetRealElementSize(ExtendedItem.View).Width / 4
                });

            AdornerPanel.SetPlacement(thumb2,
                new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Center)
                {
                    YOffset = -1 * PlacementOperation.GetRealElementSize(ExtendedItem.View).Height / 4,
                    XOffset = 0
                });

            var designPanel = ExtendedItem.Services.DesignPanel as DesignPanel;
            if (designPanel != null)
                designPanel.AdornerLayer.UpdateAdornersForElement(ExtendedItem.View, true);
        }
    }

    protected override void OnRemove()
    {
        ExtendedItem.PropertyChanged -= OnPropertyChanged;
        base.OnRemove();
    }

    #region Skew

    private Point startPoint;
    private Visual parent;
    private SkewTransform skewTransform;
    private double skewX;
    private double skewY;
    private DesignItem rtTransform;
    private Thumb thumb1;
    private Thumb thumb2;
    private PlacementOperation operation;

    private void dragX_Started(DragListener drag)
    {
        _adornerLayer = adornerPanel.FindAncestorOfType<MyDesigner.Designer.Controls.AdornerLayer>();

        var designerItem = ExtendedItem.Component as Control;
        parent = Avalonia.VisualTree.VisualExtensions.GetVisualParent(designerItem);

        startPoint = drag.StartPoint;

        if (skewTransform == null)
        {
            skewX = 0;
            skewY = 0;
        }
        else
        {
            skewX = skewTransform.AngleX;
            skewY = skewTransform.AngleY;
        }

        rtTransform = ExtendedItem.Properties[Visual.RenderTransformProperty].Value;

        operation = PlacementOperation.Start(extendedItemArray, PlacementType.Resize);
    }

    private void dragX_Changed(DragListener drag)
    {
        var currentPoint = drag.CurrentPoint;
        var deltaVector = currentPoint - startPoint;

        var destAngle = -0.5 * deltaVector.X + skewX;

        if (destAngle == 0 && skewY == 0)
        {
            ExtendedItem.Properties.GetProperty(Visual.RenderTransformProperty).Reset();
            rtTransform = null;
            skewTransform = null;
        }
        else
        {
            if (rtTransform == null || !(rtTransform.Component is SkewTransform))
            {
                if (!ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).IsSet)
                    ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty)
                        .SetValue(new RelativePoint(0.5, 0.5, RelativeUnit.Relative));

                if (skewTransform == null)
                    skewTransform = new SkewTransform(0, 0);
                ExtendedItem.Properties.GetProperty(Visual.RenderTransformProperty).SetValue(skewTransform);
                rtTransform = ExtendedItem.Properties[Visual.RenderTransformProperty].Value;
            }

            rtTransform.Properties["AngleX"].SetValue(destAngle);
        }

        _adornerLayer.UpdateAdornersForElement(ExtendedItem.View, true);
    }

    private void dragX_Completed(DragListener drag)
    {
        operation.Commit();
    }

    private void dragY_Started(DragListener drag)
    {
        _adornerLayer = adornerPanel.FindAncestorOfType<MyDesigner.Designer.Controls.AdornerLayer>();

        var designerItem = ExtendedItem.Component as Control;
        parent = Avalonia.VisualTree.VisualExtensions.GetVisualParent(designerItem);

        startPoint = drag.StartPoint;

        if (skewTransform == null)
        {
            skewX = 0;
            skewY = 0;
        }
        else
        {
            skewX = skewTransform.AngleX;
            skewY = skewTransform.AngleY;
        }

        rtTransform = ExtendedItem.Properties[Visual.RenderTransformProperty].Value;

        operation = PlacementOperation.Start(extendedItemArray, PlacementType.Resize);
    }

    private void dragY_Changed(DragListener drag)
    {
        var currentPoint = drag.CurrentPoint;
        var deltaVector = currentPoint - startPoint;

        var destAngle = -0.5 * deltaVector.Y + skewY;

        if (destAngle == 0 && skewX == 0)
        {
            ExtendedItem.Properties.GetProperty(Visual.RenderTransformProperty).Reset();
            rtTransform = null;
            skewTransform = null;
        }
        else
        {
            if (rtTransform == null)
            {
                if (!ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).IsSet)
                    ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty)
                        .SetValue(new RelativePoint(0.5, 0.5, RelativeUnit.Relative));

                if (skewTransform == null)
                    skewTransform = new SkewTransform(0, 0);
                ExtendedItem.Properties.GetProperty(Visual.RenderTransformProperty).SetValue(skewTransform);
                rtTransform = ExtendedItem.Properties[Visual.RenderTransformProperty].Value;
            }

            rtTransform.Properties["AngleY"].SetValue(destAngle);
        }

        _adornerLayer.UpdateAdornersForElement(ExtendedItem.View, true);
    }

    private void dragY_Completed(DragListener drag)
    {
        operation.Commit();
    }

    #endregion
}