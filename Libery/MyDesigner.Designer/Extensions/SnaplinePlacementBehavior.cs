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

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using MyDesigner.Design.Adorners;
using MyDesigner.Designer.Services;

namespace MyDesigner.Designer.Extensions;

public class SnaplinePlacementBehavior : RasterPlacementBehavior
{
    public static readonly AttachedProperty<bool> DisableSnaplinesProperty =
        AvaloniaProperty.RegisterAttached<SnaplinePlacementBehavior, Control, bool>("DisableSnaplines", false);

    private AdornerPanel adornerPanel;
    private double? baseline;
    private List<Snapline> horizontalMap;
    private Canvas surface;
    private List<Snapline> verticalMap;

    public static double SnaplineMargin { get; set; } = 8;

    public static double SnaplineAccuracy { get; set; } = 5;

    public static bool GetDisableSnaplines(Control obj)
    {
        return obj.GetValue(DisableSnaplinesProperty);
    }

    public static void SetDisableSnaplines(Control obj, bool value)
    {
        obj.SetValue(DisableSnaplinesProperty, value);
    }

    public override void BeginPlacement(PlacementOperation operation)
    {
        base.BeginPlacement(operation);
        CreateSurface(operation);
    }

    public override void EndPlacement(PlacementOperation operation)
    {
        base.EndPlacement(operation);
        DeleteSurface();
    }

    public override void EnterContainer(PlacementOperation operation)
    {
        base.EnterContainer(operation);
        CreateSurface(operation);
    }

    public override void LeaveContainer(PlacementOperation operation)
    {
        base.LeaveContainer(operation);
        DeleteSurface();
    }

    public override Point PlacePoint(Point point)
    {
        if (surface == null)
            return base.PlacePoint(point);

        var designPanel = ExtendedItem.Services.DesignPanel as DesignPanel;
        if (designPanel == null || !designPanel.UseSnaplinePlacement)
            return base.PlacePoint(point);

        surface.Children.Clear();
        if (IsKeyDown(Key.LeftCtrl))
            return base.PlacePoint(point);

        var bounds = new Rect(point.X, point.Y, 0, 0);

        var horizontalInput = new List<Snapline>();
        var verticalInput = new List<Snapline>();

        AddLines(bounds, 0, false, horizontalInput, verticalInput, null);
        if (baseline.HasValue)
        {
            var textOffset = bounds.Top + baseline.Value;
            horizontalInput.Add(
                new Snapline { Group = 1, Offset = textOffset, Start = bounds.Left, End = bounds.Right });
        }

        List<Snapline> drawLines;
        double delta;

        var newPoint = base.PlacePoint(point);
        if (Snap(horizontalInput, horizontalMap, SnaplineAccuracy, out drawLines, out delta))
        {
            foreach (var d in drawLines) DrawLine(d.Start, d.Offset + d.DrawOffset, d.End, d.Offset + d.DrawOffset);

            point = point.WithY(point.Y + delta);
        }
        else
        {
            point = point.WithY(newPoint.Y);
        }

        if (Snap(verticalInput, verticalMap, SnaplineAccuracy, out drawLines, out delta))
        {
            foreach (var d in drawLines) DrawLine(d.Offset + d.DrawOffset, d.Start, d.Offset + d.DrawOffset, d.End);

            point = point.WithX(point.X + delta);
        }
        else
        {
            point = point.WithX(newPoint.X);
        }

        return point;
    }

    private bool IsKeyDown(Key key)
    {
        var topLevel = TopLevel.GetTopLevel(ExtendedItem.View as Visual);
        return topLevel?.IsKeyDown(key) == true;
    }

    public override void BeforeSetPosition(PlacementOperation operation)
    {
        base.BeforeSetPosition(operation);
        if (surface == null) return;

        var designPanel = ExtendedItem.Services.DesignPanel as DesignPanel;
        if (designPanel == null || !designPanel.UseSnaplinePlacement)
            return;

        surface.Children.Clear();
        if (IsKeyDown(Key.LeftCtrl)) return;

        var bounds = RectExtensions.Empty;
        foreach (var item in operation.PlacedItems) bounds = bounds.Union(item.Bounds);

        var horizontalInput = new List<Snapline>();
        var verticalInput = new List<Snapline>();
        var info = operation.PlacedItems[0];

        if (operation.Type == PlacementType.Resize)
        {
            AddLines(bounds, 0, false, horizontalInput, verticalInput, info.ResizeThumbAlignment);
        }
        else
        {
            AddLines(bounds, 0, false, horizontalInput, verticalInput, null);
            if (baseline.HasValue)
            {
                var textOffset = bounds.Top + baseline.Value;
                horizontalInput.Add(new Snapline
                    { Group = 1, Offset = textOffset, Start = bounds.Left, End = bounds.Right });
            }
        }

        List<Snapline> drawLines;
        double delta;

        if (Snap(horizontalInput, horizontalMap, SnaplineAccuracy, out drawLines, out delta))
        {
            if (operation.Type == PlacementType.Resize)
            {
                if (info.ResizeThumbAlignment != null &&
                    info.ResizeThumbAlignment.Value.Vertical == VerticalAlignment.Top)
                {
                    bounds = bounds.WithY(bounds.Y + delta).WithHeight(Math.Max(0, bounds.Height - delta));
                    if (operation.CurrentContainer.Services.GetService<OptionService>().SnaplinePlacementRoundValues)
                    {
                        bounds = bounds.WithY(Math.Round(bounds.Y)).WithHeight(Math.Round(bounds.Height));
                    }
                }
                else
                {
                    bounds = bounds.WithHeight(Math.Max(0, bounds.Height + delta));

                    if (operation.CurrentContainer.Services.GetService<OptionService>().SnaplinePlacementRoundValues)
                        bounds = bounds.WithHeight(Math.Round(bounds.Height));
                }

                info.Bounds = bounds;
            }
            else
            {
                foreach (var item in operation.PlacedItems)
                {
                    var r = item.Bounds;
                    r = r.WithY(r.Y + delta);
                    if (operation.CurrentContainer.Services.GetService<OptionService>().SnaplinePlacementRoundValues)
                        r = r.WithY(Math.Round(r.Y));
                    item.Bounds = r;
                }
            }

            foreach (var d in drawLines) DrawLine(d.Start, d.Offset + d.DrawOffset, d.End, d.Offset + d.DrawOffset);
        }

        if (Snap(verticalInput, verticalMap, SnaplineAccuracy, out drawLines, out delta))
        {
            if (operation.Type == PlacementType.Resize)
            {
                if (info.ResizeThumbAlignment != null &&
                    info.ResizeThumbAlignment.Value.Horizontal == HorizontalAlignment.Left)
                {
                    bounds = bounds.WithX(bounds.X + delta).WithWidth(Math.Max(0, bounds.Width - delta));
                    if (operation.CurrentContainer.Services.GetService<OptionService>().SnaplinePlacementRoundValues)
                    {
                        bounds = bounds.WithX(Math.Round(bounds.X)).WithWidth(Math.Round(bounds.Width));
                    }
                }
                else
                {
                    bounds = bounds.WithWidth(Math.Max(0, bounds.Width + delta));
                    if (operation.CurrentContainer.Services.GetService<OptionService>().SnaplinePlacementRoundValues)
                        bounds = bounds.WithWidth(Math.Round(bounds.Width));
                }

                info.Bounds = bounds;
            }
            else
            {
                foreach (var item in operation.PlacedItems)
                {
                    var r = item.Bounds;
                    r = r.WithX(r.X + delta);
                    if (operation.CurrentContainer.Services.GetService<OptionService>().SnaplinePlacementRoundValues)
                        r = r.WithX(Math.Round(r.X));
                    item.Bounds = r;
                }
            }

            foreach (var d in drawLines) DrawLine(d.Offset + d.DrawOffset, d.Start, d.Offset + d.DrawOffset, d.End);
        }
    }

    private void CreateSurface(PlacementOperation operation)
    {
        if (ExtendedItem.Services.GetService<IDesignPanel>() != null)
        {
            surface = new Canvas();
            adornerPanel = new AdornerPanel();
            adornerPanel.SetAdornedElement(ExtendedItem.View, ExtendedItem);
            AdornerPanel.SetPlacement(surface, AdornerPlacement.FillContent);
            adornerPanel.Children.Add(surface);
            ExtendedItem.Services.DesignPanel.Adorners.Add(adornerPanel);

            BuildMaps(operation);

            if (operation.Type != PlacementType.Resize && operation.PlacedItems.Count == 1)
                baseline = GetBaseline(operation.PlacedItems[0].Item.View);
        }
    }

    private IEnumerable<DesignItem> AllDesignItems(DesignItem designItem = null)
    {
        if (designItem == null && ExtendedItem.Services.DesignPanel is DesignPanel)
            designItem = ExtendedItem.Services.DesignPanel.Context.RootItem;

        if (designItem?.ContentProperty != null)
        {
            if (designItem.ContentProperty.IsCollection)
            {
                foreach (var collectionElement in designItem.ContentProperty.CollectionElements)
                {
                    if (collectionElement != null)
                        yield return collectionElement;

                    foreach (var el in AllDesignItems(collectionElement))
                        if (el != null)
                            yield return el;
                }
            }
            else if (designItem.ContentProperty.Value != null)
            {
                yield return designItem.ContentProperty.Value;

                foreach (var el in AllDesignItems(designItem.ContentProperty.Value))
                    if (el != null)
                        yield return el;
            }
        }
    }

    protected IEnumerable<DesignItem> GetSnapToDesignItems(PlacementOperation operation)
    {
        return AllDesignItems();
    }

    private void BuildMaps(PlacementOperation operation)
    {
        horizontalMap = new List<Snapline>();
        verticalMap = new List<Snapline>();

        var containerRect = new Rect(0, 0, ModelTools.GetWidth(ExtendedItem.View),
            ModelTools.GetHeight(ExtendedItem.View));
        if (SnaplineMargin > 0) AddLines(containerRect, -SnaplineMargin, false);

        AddLines(containerRect, 0, false);

        AddContainerSnaplines(containerRect, horizontalMap, verticalMap);

        if (!CanPlace(operation.PlacedItems.Select(x => x.Item), operation.Type, PlacementAlignment.Center))
            return;

        foreach (var item in GetSnapToDesignItems(operation)
                     .Except(operation.PlacedItems.Select(f => f.Item))
                     .Where(x => x.View != null && !GetDisableSnaplines(x.View as Control)))
            if (item != null)
            {
                var bounds = GetPositionRelativeToContainer(operation, item);

                AddLines(bounds, 0, false);
                if (SnaplineMargin > 0) AddLines(bounds, SnaplineMargin, true);
                AddBaseline(item, bounds, horizontalMap);
            }
    }

    protected virtual void AddContainerSnaplines(Rect containerRect, List<Snapline> horizontalMap,
        List<Snapline> verticalMap)
    {
    }

    private void AddLines(Rect r, double inflate, bool requireOverlap)
    {
        AddLines(r, inflate, requireOverlap, horizontalMap, verticalMap, null);
    }

    private void AddLines(Rect r, double inflate, bool requireOverlap, List<Snapline> h, List<Snapline> v,
        PlacementAlignment? filter)
    {
        if (r != RectExtensions.Empty)
        {
            var r2 = r.Inflate(inflate);

            if (filter == null || filter.Value.Vertical == VerticalAlignment.Top)
                h.Add(new Snapline
                    { RequireOverlap = requireOverlap, Offset = r2.Top - 1, Start = r.Left, End = r.Right });
            if (filter == null || filter.Value.Vertical == VerticalAlignment.Bottom)
                h.Add(new Snapline
                    { RequireOverlap = requireOverlap, Offset = r2.Bottom - 1, Start = r.Left, End = r.Right });
            if (filter == null || filter.Value.Horizontal == HorizontalAlignment.Left)
                v.Add(new Snapline
                    { RequireOverlap = requireOverlap, Offset = r2.Left - 1, Start = r.Top, End = r.Bottom });
            if (filter == null || filter.Value.Horizontal == HorizontalAlignment.Right)
                v.Add(new Snapline
                    { RequireOverlap = requireOverlap, Offset = r2.Right - 1, Start = r.Top, End = r.Bottom });

            if (filter == null)
            {
                h.Add(new Snapline
                {
                    RequireOverlap = requireOverlap, Offset = r2.Top + Math.Abs((r2.Top - r2.Bottom) / 2) - 1,
                    DrawOffset = 1, Start = r.Left, End = r.Right
                });
                v.Add(new Snapline
                {
                    RequireOverlap = requireOverlap, Offset = r2.Left + Math.Abs((r2.Left - r2.Right) / 2) - 1,
                    DrawOffset = 1, Start = r.Top, End = r.Bottom
                });
            }
        }
    }

    private void AddBaseline(DesignItem item, Rect bounds, List<Snapline> list)
    {
        var baseline = GetBaseline(item.View);
        if (baseline.HasValue)
        {
            var transform = item.View.TransformToVisual(ExtendedItem.View);
            var textOffset = transform?.Transform(new Point(0, baseline.Value)).Y ?? baseline.Value;
            list.Add(new Snapline { Group = 1, Offset = textOffset, Start = bounds.Left, End = bounds.Right });
        }
    }

    private void DeleteSurface()
    {
        if (surface != null)
        {
            ExtendedItem.Services.DesignPanel.Adorners.Remove(adornerPanel);
            adornerPanel = null;
            surface = null;
            horizontalMap = null;
            verticalMap = null;
        }
    }

    private void DrawLine(double x1, double y1, double x2, double y2)
    {
        if (double.IsInfinity(x1) || double.IsNaN(x1) || double.IsInfinity(y1) || double.IsNaN(y1) ||
            double.IsInfinity(x2) || double.IsNaN(x2) || double.IsInfinity(y2) || double.IsNaN(y2))
            return;

        var line1 = new Line
        {
            StartPoint = new Point(x1, y1),
            EndPoint = new Point(x2, y2),
            StrokeThickness = 1,
            Stroke = Brushes.White
        };
        surface.Children.Add(line1);

        var line2 = new Line
        {
            StartPoint = new Point(x1, y1),
            EndPoint = new Point(x2, y2),
            StrokeThickness = 1,
            Stroke = Brushes.Orange,
            StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 5, 2 },
            StrokeDashOffset = x1 + y1 // fix dashes
        };
        surface.Children.Add(line2);
    }

    private static double? GetBaseline(object element)
    {
        var textBox = element as TextBox;
        if (textBox != null)
        {
            // In Avalonia, baseline calculation is different
            // This is a simplified implementation
            return textBox.Bounds.Height * 0.8; // Approximate baseline
        }

        var textBlock = element as TextBlock;
        if (textBlock != null)
            return textBlock.Bounds.Height * 0.8; // Approximate baseline

        return null;
    }

    private static bool Snap(List<Snapline> input, List<Snapline> map, double accuracy,
        out List<Snapline> drawLines, out double delta)
    {
        delta = double.MaxValue;
        drawLines = null;

        foreach (var inputLine in input)
        foreach (var mapLine in map)
            if (Math.Abs(mapLine.Offset - inputLine.Offset) <= accuracy)
                if ((!inputLine.RequireOverlap && !mapLine.RequireOverlap) ||
                    Math.Max(inputLine.Start, mapLine.Start) < Math.Min(inputLine.End, mapLine.End))
                    if (mapLine.Group == inputLine.Group)
                        delta = mapLine.Offset - inputLine.Offset;

        if (delta == double.MaxValue) return false;
        var offsetDict = new Dictionary<double, Snapline>();

        foreach (var inputLine in input)
        {
            inputLine.Offset += delta;
            foreach (var mapLine in map)
                if (inputLine.Offset == mapLine.Offset)
                {
                    var offset = mapLine.Offset;
                    Snapline drawLine;
                    if (!offsetDict.TryGetValue(offset, out drawLine))
                    {
                        drawLine = new Snapline();
                        drawLine.Start = double.MaxValue;
                        drawLine.End = double.MinValue;
                        drawLine.DrawOffset = mapLine.DrawOffset;
                        offsetDict[offset] = drawLine;
                    }

                    drawLine.Offset = offset;
                    drawLine.Start = Math.Min(drawLine.Start, Math.Min(inputLine.Start, mapLine.Start));
                    drawLine.End = Math.Max(drawLine.End, Math.Max(inputLine.End, mapLine.End));
                }
        }

        drawLines = offsetDict.Values.ToList();
        return true;
    }

    [DebuggerDisplay("Snapline: {Offset}")]
    protected class Snapline
    {
        public double DrawOffset;
        public double End;
        public int Group;
        public double Offset;
        public bool RequireOverlap;
        public double Start;
    }
}