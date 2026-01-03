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

using System.Collections;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using MyDesigner.Design.Adorners;
using MyDesigner.Designer.Controls;

namespace MyDesigner.Designer.Extensions;

/// <summary>
///     base class for the Line, Polyline and Polygon extension classes
/// </summary>
public class LineExtensionBase : SelectionAdornerProvider
{
    private readonly Canvas _surface;

    /// <summary>An array containing this.ExtendedItem as only element</summary>
    protected readonly DesignItem[] extendedItemArray = new DesignItem[1];

    protected bool _isResizing;
    private TextBlock _text;

    protected AdornerPanel adornerPanel;
    protected ChangeGroup changeGroup;
    protected PlacementOperation operation;

    protected IPlacementBehavior resizeBehavior;
    protected IEnumerable resizeThumbs;

    /// <summary>
    ///     on creation add adornerlayer
    /// </summary>
    public LineExtensionBase()
    {
        _surface = new Canvas();
        adornerPanel = new AdornerPanel { MinWidth = 10, MinHeight = 10 };
        adornerPanel.Order = AdornerOrder.Foreground;
        adornerPanel.Children.Add(_surface);
        Adorners.Add(adornerPanel);
    }

    /// <summary>
    ///     Gets whether this extension is resizing any element.
    /// </summary>
    public bool IsResizing => _isResizing;

    protected void UpdateAdornerVisibility()
    {
        var fe = ExtendedItem.View as Control;
        foreach (DesignerThumb r in resizeThumbs)
        {
            var isVisible = resizeBehavior != null &&
                            resizeBehavior.CanPlace(extendedItemArray, PlacementType.Resize, r.Alignment);
            r.IsVisible = isVisible;
        }
    }

    /// <summary>
    ///     Places resize thumbs at their respective positions
    ///     and streches out thumbs which are at the center of outline to extend resizability across the whole outline
    /// </summary>
    /// <param name="designerThumb"></param>
    /// <param name="alignment"></param>
    /// <param name="index">if using a polygon or multipoint adorner this is the index of the point in the Points array</param>
    /// <returns></returns>
    protected PointTrackerPlacementSupport Place(DesignerThumb designerThumb, PlacementAlignment alignment,
        int index = -1)
    {
        var placement = new PointTrackerPlacementSupport(ExtendedItem.View as Shape, alignment, index);
        return placement;
    }

    /// <summary>
    ///     forces redraw of shape
    /// </summary>
    protected void Invalidate()
    {
        var s = ExtendedItem.View as Shape;
        if (s != null)
        {
            s.InvalidateVisual();
        }
    }

    protected void SetSurfaceInfo(int x, int y, string s)
    {
        if (_text == null)
        {
            _text = new TextBlock { FontSize = 8, FontStyle = FontStyle.Italic };
            _surface.Children.Add(_text);
        }

        var ap = _surface.Parent as AdornerPanel;

        _surface.Width = ap.Width;
        _surface.Height = ap.Height;

        _text.Text = s;
        Canvas.SetLeft(_text, x);
        Canvas.SetTop(_text, y);
    }

    protected void HideSizeAndShowHandles()
    {
        SizeDisplayExtension sizeDisplay = null;
        MarginHandleExtension marginDisplay = null;
        foreach (var extension in ExtendedItem.Extensions)
        {
            if (extension is SizeDisplayExtension)
                sizeDisplay = extension as SizeDisplayExtension;
            if (extension is MarginHandleExtension)
                marginDisplay = extension as MarginHandleExtension;
        }

        if (sizeDisplay != null)
        {
            sizeDisplay.HeightDisplay.IsVisible = false;
            sizeDisplay.WidthDisplay.IsVisible = false;
        }

        if (marginDisplay != null) marginDisplay.ShowHandles();
    }

    protected void ResetWidthHeightProperties()
    {
        ExtendedItem.Properties.GetProperty(Control.HeightProperty).Reset();
        ExtendedItem.Properties.GetProperty(Control.WidthProperty).Reset();
    }

    /// <summary>
    ///     Used instead of Rect to allow negative values on "Width" and "Height" (here called X and Y).
    /// </summary>
    protected class Bounds
    {
        public double X, Y, Left, Top;
    }

    #region eventhandlers

    protected void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        UpdateAdornerVisibility();
    }

    protected override void OnRemove()
    {
        ExtendedItem.PropertyChanged -= OnPropertyChanged;
        base.OnRemove();
    }

    #endregion
}