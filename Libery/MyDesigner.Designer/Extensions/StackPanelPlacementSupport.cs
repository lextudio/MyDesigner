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
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using MyDesigner.Design.Adorners;
using MyDesigner.Design.Extensions;

namespace MyDesigner.Designer.Extensions;

/// <summary>
///     Provides <see cref="IPlacementBehavior" /> for <see cref="StackPanel" />.
/// </summary>
[ExtensionFor(typeof(StackPanel), OverrideExtension = typeof(DefaultPlacementBehavior))]
public class StackPanelPlacementSupport : DefaultPlacementBehavior
{
    private readonly List<Rect> _rects = new(); // Contains the Rect of all the children of StackPanel.
    private AdornerPanel _adornerPanel;
    private int _indexToInsert; // Postion where to insert the element.


    private bool _isItemGettingResized; // Indicates whether any children is getting resized.
    private Rectangle _rectangle = new(); // Draws a rectangle to indicate the position of insertion.
    private StackPanel _stackPanel;


    protected override void OnInitialized()
    {
        base.OnInitialized();
        _stackPanel = ExtendedItem.View as StackPanel;
        var children = ExtendedItem.ContentProperty.CollectionElements;
        foreach (var child in children)
        {
            var p = child.View.TranslatePoint(new Point(0, 0), ExtendedItem.View) ?? new Point(0, 0);
            _rects.Add(new Rect(p, child.View.Bounds.Size));
        }
    }

    public override void BeginPlacement(PlacementOperation operation)
    {
        base.BeginPlacement(operation);
        if (_rects.Count > 0)
            _rects.Clear();

        /* Add Rect of all children to _rects */
        var children = ExtendedItem.ContentProperty.CollectionElements;
        foreach (var child in children)
        {
            var p = child.View.TranslatePoint(new Point(0, 0), ExtendedItem.View) ?? new Point(0, 0);
            _rects.Add(new Rect(p, child.View.Bounds.Size));
        }

        if (_adornerPanel != null && ExtendedItem.Services.DesignPanel.Adorners.Contains(_adornerPanel))
            ExtendedItem.Services.DesignPanel.Adorners.Remove(_adornerPanel);

        /* Place the Rectangle */
        _adornerPanel = new AdornerPanel();
        _rectangle = new Rectangle();
        _adornerPanel.SetAdornedElement(ExtendedItem.View, ExtendedItem);
        _adornerPanel.Children.Add(_rectangle);
        ExtendedItem.Services.DesignPanel.Adorners.Add(_adornerPanel);
    }

    public override void EndPlacement(PlacementOperation operation)
    {
        base.EndPlacement(operation);
        if (_adornerPanel != null && ExtendedItem.Services.DesignPanel.Adorners.Contains(_adornerPanel))
            ExtendedItem.Services.DesignPanel.Adorners.Remove(_adornerPanel);
    }

    public override void EnterContainer(PlacementOperation operation)
    {
        base.EnterContainer(operation);

        _rectangle.IsVisible = true;
    }

    public override void LeaveContainer(PlacementOperation operation)
    {
        base.LeaveContainer(operation);
        /* Hide the rectangle in case switching to the other container
         *  otherwise it will show up intersecting with the container */
        _rectangle.IsVisible = false;
    }

    public override void SetPosition(PlacementInformation info)
    {
        base.SetPosition(info);

        var resizeExtensions = info.Item.Extensions.OfType<ResizeThumbExtension>();
        if (resizeExtensions != null && resizeExtensions.Count() != 0)
        {
            var resizeExtension = resizeExtensions.First();
            _isItemGettingResized = resizeExtension.IsResizing;
        }

        if (_stackPanel != null && !_isItemGettingResized)
        {
            if (_stackPanel.Orientation == Orientation.Vertical)
            {
                var offset = FindHorizontalRectanglePlacementOffset(info.Bounds);
                DrawHorizontalRectangle(offset);
            }
            else
            {
                var offset = FindVerticalRectanglePlacementOffset(info.Bounds);
                DrawVerticalRectangle(offset);
            }

            ChangePositionTo(info.Item, _indexToInsert);
        }
    }

    private void ChangePositionTo(DesignItem element, int index)
    {
        if (ExtendedItem.ContentProperty == null || !ExtendedItem.ContentProperty.IsCollection)
            return;
        var elements = ExtendedItem.ContentProperty.CollectionElements;
        var elementIndex = elements.IndexOf(element);
        if (elementIndex >= 0 && index > elementIndex)
            index--;
        elements.Remove(element);
        elements.Insert(index, element);
    }

    private double FindHorizontalRectanglePlacementOffset(Rect rect)
    {
        _rects.Sort((r1, r2) => r1.Top.CompareTo(r2.Top));
        var itemCenter = (rect.Top + rect.Bottom) / 2;
        for (var i = 0; i < _rects.Count; i++)
        {
            var rectCenter = (_rects[i].Top + _rects[i].Bottom) / 2;
            if (rectCenter >= itemCenter)
            {
                _indexToInsert = i;
                return _rects[i].Top;
            }
        }

        _indexToInsert = _rects.Count;
        return _rects.Count > 0 ? _rects.Last().Bottom : 0;
    }

    private double FindVerticalRectanglePlacementOffset(Rect rect)
    {
        _rects.Sort((r1, r2) => r1.Left.CompareTo(r2.Left));
        var itemCenter = (rect.Left + rect.Right) / 2;
        for (var i = 0; i < _rects.Count; i++)
        {
            var rectCenter = (_rects[i].Left + _rects[i].Top) / 2;
            if (rectCenter >= itemCenter)
            {
                _indexToInsert = i;
                return _rects[i].Left;
            }
        }

        _indexToInsert = _rects.Count;
        return _rects.Count > 0 ? _rects.Last().Right : 0;
    }

    private void DrawHorizontalRectangle(double offset)
    {
        _rectangle.Height = 2;
        _rectangle.Fill = Brushes.Black;
        var placement = new RelativePlacement(HorizontalAlignment.Stretch, VerticalAlignment.Top) { YOffset = offset };
        AdornerPanel.SetPlacement(_rectangle, placement);
    }

    private void DrawVerticalRectangle(double offset)
    {
        _rectangle.Width = 2;
        _rectangle.Fill = Brushes.Black;
        var placement = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Stretch) { XOffset = offset };
        AdornerPanel.SetPlacement(_rectangle, placement);
    }
}
