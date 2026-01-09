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
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using MyDesigner.Design.Adorners;
using MyDesigner.Designer.Extensions;

namespace MyDesigner.Designer.Controls;

/// <summary>
///     Adorner that displays the blue bar next to grids that can be used to create new rows/column.
/// </summary>
public class GridRailAdorner : Control
{
    protected override Type StyleKeyOverride => typeof(GridRailAdorner);

    public const double RailSize = 10;
    public const double RailDistance = 6;
    public const double SplitterWidth = 10;

    private static readonly SolidColorBrush bgBrush;
    private readonly AdornerPanel adornerPanel;
    private readonly Grid grid;

    private readonly DesignItem gridItem;
    private readonly Orientation orientation;
    private readonly GridSplitterAdorner previewAdorner;
    private readonly GridUnitSelector unitSelector;

    private bool displayUnitSelector; // Indicates whether Grid UnitSeletor should be displayed.

    static GridRailAdorner()
    {
        bgBrush = new SolidColorBrush(Color.FromArgb(0x35, 0x1E, 0x90, 0xff));
         
    }

    public GridRailAdorner(DesignItem gridItem, AdornerPanel adornerPanel, Orientation orientation)
    {
        Debug.Assert(gridItem != null);
        Debug.Assert(adornerPanel != null);

        this.gridItem = gridItem;
        grid = (Grid)gridItem.Component;
        this.adornerPanel = adornerPanel;
        this.orientation = orientation;
        displayUnitSelector = false;
        unitSelector = new GridUnitSelector(this);
        adornerPanel.Children.Add(unitSelector);

        if (orientation == Orientation.Horizontal)
        {
            Height = RailSize;
            previewAdorner = new GridColumnSplitterAdorner(this, gridItem, null, null);
        }
        else
        {
            // vertical
            Width = RailSize;
            previewAdorner = new GridRowSplitterAdorner(this, gridItem, null, null);
        }

        unitSelector.Orientation = orientation;
        previewAdorner.IsPreview = true;
        previewAdorner.IsHitTestVisible = false;
        unitSelector.IsVisible = false;
    }

    public override void Render(DrawingContext drawingContext)
    {
        base.Render(drawingContext);

        if (orientation == Orientation.Horizontal)
        {
            var bgRect = new Rect(0, 0, grid.Bounds.Width, RailSize);
            drawingContext.DrawRectangle(bgBrush, null, bgRect);

            var colCollection = gridItem.Properties["ColumnDefinitions"];
            foreach (var colItem in colCollection.CollectionElements)
            {
                var column = colItem.Component as ColumnDefinition;
                if (column.ActualWidth < 0) continue;
                var len = (GridLength)column.GetValue(ColumnDefinition.WidthProperty);

                var text = new FormattedText(GridLengthToText(len), CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, new Typeface("Segoe UI"), 10, Brushes.Black);
                text.TextAlignment = TextAlignment.Center;
                drawingContext.DrawText(text, new Point(column.Offset() + column.ActualWidth / 2, 0));
            }
        }
        else
        {
            var bgRect = new Rect(0, 0, RailSize, grid.Bounds.Height);
            drawingContext.DrawRectangle(bgBrush, null, bgRect);

            var rowCollection = gridItem.Properties["RowDefinitions"];
            foreach (var rowItem in rowCollection.CollectionElements)
            {
                var row = rowItem.Component as RowDefinition;
                if (row.ActualHeight < 0) continue;
                var len = (GridLength)row.GetValue(RowDefinition.HeightProperty);

                var text = new FormattedText(GridLengthToText(len), CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, new Typeface("Segoe UI"), 10, Brushes.Black);
                text.TextAlignment = TextAlignment.Center;
                using (drawingContext.PushTransform(Matrix.CreateRotation(-Math.PI / 2)))
                {
                    drawingContext.DrawText(text, new Point((row.Offset() + row.ActualHeight / 2) * -1, 0));
                }
            }
        }
    }

    private string GridLengthToText(GridLength len)
    {
        switch (len.GridUnitType)
        {
            case GridUnitType.Auto:
                return "Auto";
            case GridUnitType.Star:
                return len.Value == 1 ? "*" : Math.Round(len.Value, 2) + "*";
            case GridUnitType.Pixel:
                return Math.Round(len.Value, 2) + "px";
        }

        return string.Empty;
    }

    public void SetGridLengthUnit(GridUnitType unit)
    {
        var item = unitSelector.SelectedItem;
        grid.InvalidateArrange();

        Debug.Assert(item != null);

        if (orientation == Orientation.Vertical)
            SetGridLengthUnit(unit, item, RowDefinition.HeightProperty);
        else
            SetGridLengthUnit(unit, item, ColumnDefinition.WidthProperty);
        grid.InvalidateArrange();
        InvalidateVisual();
    }

    private void SetGridLengthUnit(GridUnitType unit, DesignItem item, AvaloniaProperty property)
    {
        var itemProperty = item.Properties[property];
        var oldValue = itemProperty.GetConvertedValueOnInstance<GridLength>();
        var value = GetNewGridLength(unit, oldValue);

        if (value != oldValue) itemProperty.SetValue(value);
    }

    private GridLength GetNewGridLength(GridUnitType unit, GridLength oldValue)
    {
        if (unit == GridUnitType.Auto) return GridLength.Auto;
        return new GridLength(oldValue.Value, unit);
    }

    #region Handle mouse events to add a new row/column

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        Cursor = new Cursor(StandardCursorType.Cross);
        var rpUnitSelector = new RelativePlacement();
        if (orientation == Orientation.Vertical)
        {
            var insertionPosition = e.GetPosition(this).Y;
            var current = grid.RowDefinitions
                .FirstOrDefault(r => insertionPosition >= r.Offset() &&
                                     insertionPosition <= r.Offset() + r.ActualHeight);
            if (current != null)
            {
                var component = gridItem.Services.Component.GetDesignItem(current);
                rpUnitSelector.XOffset = -(RailSize + RailDistance) * 2.75 - 6;
                rpUnitSelector.WidthOffset = RailSize + RailDistance;
                rpUnitSelector.WidthRelativeToContentWidth = 1;
                rpUnitSelector.HeightOffset = 55;
                rpUnitSelector.YOffset = current.Offset() + current.ActualHeight / 2 - 25;
                unitSelector.SelectedItem = component;
                unitSelector.Unit = component.Properties[RowDefinition.HeightProperty]
                    .GetConvertedValueOnInstance<GridLength>().GridUnitType;
                displayUnitSelector = true;
            }
            else
            {
                displayUnitSelector = false;
            }
        }
        else
        {
            var insertionPosition = e.GetPosition(this).X;
            var current = grid.ColumnDefinitions
                .FirstOrDefault(r => insertionPosition >= r.Offset() &&
                                     insertionPosition <= r.Offset() + r.ActualWidth);
            if (current != null)
            {
                var component = gridItem.Services.Component.GetDesignItem(current);
                Debug.Assert(component != null);
                rpUnitSelector.YOffset = -(RailSize + RailDistance) * 2.20 - 6;
                rpUnitSelector.HeightOffset = RailSize + RailDistance;
                rpUnitSelector.HeightRelativeToContentHeight = 1;
                rpUnitSelector.WidthOffset = 75;
                rpUnitSelector.XOffset = current.Offset() + current.ActualWidth / 2 - 35;
                unitSelector.SelectedItem = component;
                unitSelector.Unit = component.Properties[ColumnDefinition.WidthProperty]
                    .GetConvertedValueOnInstance<GridLength>().GridUnitType;
                displayUnitSelector = true;
            }
            else
            {
                displayUnitSelector = false;
            }
        }

        if (displayUnitSelector)
            unitSelector.IsVisible = true;
        if (!adornerPanel.Children.Contains(previewAdorner))
            adornerPanel.Children.Add(previewAdorner);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var rp = new RelativePlacement();
        var rpUnitSelector = new RelativePlacement();
        if (orientation == Orientation.Vertical)
        {
            var insertionPosition = e.GetPosition(this).Y;
            var current = grid.RowDefinitions
                .FirstOrDefault(r => insertionPosition >= r.Offset() &&
                                     insertionPosition <= r.Offset() + r.ActualHeight);

            rp.XOffset = -(RailSize + RailDistance);
            rp.WidthOffset = RailSize + RailDistance;
            rp.WidthRelativeToContentWidth = 1;
            rp.HeightOffset = SplitterWidth;
            rp.YOffset = e.GetPosition(this).Y - SplitterWidth / 2;
            if (current != null)
            {
                var component = gridItem.Services.Component.GetDesignItem(current);
                rpUnitSelector.XOffset = -(RailSize + RailDistance) * 2.75 - 6;
                rpUnitSelector.WidthOffset = RailSize + RailDistance;
                rpUnitSelector.WidthRelativeToContentWidth = 1;
                rpUnitSelector.HeightOffset = 55;
                rpUnitSelector.YOffset = current.Offset() + current.ActualHeight / 2 - 25;
                unitSelector.SelectedItem = component;
                unitSelector.Unit = component.Properties[RowDefinition.HeightProperty]
                    .GetConvertedValueOnInstance<GridLength>().GridUnitType;
                displayUnitSelector = true;
            }
            else
            {
                displayUnitSelector = false;
            }
        }
        else
        {
            var insertionPosition = e.GetPosition(this).X;
            var current = grid.ColumnDefinitions
                .FirstOrDefault(r => insertionPosition >= r.Offset() &&
                                     insertionPosition <= r.Offset() + r.ActualWidth);

            rp.YOffset = -(RailSize + RailDistance);
            rp.HeightOffset = RailSize + RailDistance;
            rp.HeightRelativeToContentHeight = 1;
            rp.WidthOffset = SplitterWidth;
            rp.XOffset = e.GetPosition(this).X - SplitterWidth / 2;

            if (current != null)
            {
                var component = gridItem.Services.Component.GetDesignItem(current);
                Debug.Assert(component != null);
                rpUnitSelector.YOffset = -(RailSize + RailDistance) * 2.20 - 6;
                rpUnitSelector.HeightOffset = RailSize + RailDistance;
                rpUnitSelector.HeightRelativeToContentHeight = 1;
                rpUnitSelector.WidthOffset = 75;
                rpUnitSelector.XOffset = current.Offset() + current.ActualWidth / 2 - 35;
                unitSelector.SelectedItem = component;
                unitSelector.Unit = component.Properties[ColumnDefinition.WidthProperty]
                    .GetConvertedValueOnInstance<GridLength>().GridUnitType;
                displayUnitSelector = true;
            }
            else
            {
                displayUnitSelector = false;
            }
        }

        AdornerPanel.SetPlacement(previewAdorner, rp);
        if (displayUnitSelector)
            AdornerPanel.SetPlacement(unitSelector, rpUnitSelector);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        Cursor = Cursor.Default;
        if (!unitSelector.IsPointerOver)
        {
            unitSelector.IsVisible = false;
            displayUnitSelector = false;
        }

        if (adornerPanel.Children.Contains(previewAdorner))
            adornerPanel.Children.Remove(previewAdorner);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Handled = true;
        Focus();
        adornerPanel.Children.Remove(previewAdorner);
        if (orientation == Orientation.Vertical)
        {
            var insertionPosition = e.GetPosition(this).Y;
            var rowCollection = gridItem.Properties["RowDefinitions"];

            DesignItem currentRow = null;

            using (var changeGroup = gridItem.OpenGroup("Split grid row"))
            {
                if (rowCollection.CollectionElements.Count == 0)
                {
                    var firstRow = gridItem.Services.Component.RegisterComponentForDesigner(new RowDefinition());
                    rowCollection.CollectionElements.Add(firstRow);
                    grid.InvalidateArrange(); // let Avalonia assign firstRow.ActualHeight

                    currentRow = firstRow;
                }
                else
                {
                    var current = grid.RowDefinitions
                        .FirstOrDefault(r => insertionPosition >= r.Offset() &&
                                             insertionPosition <= r.Offset() + r.ActualHeight);
                    if (current != null)
                        currentRow = gridItem.Services.Component.GetDesignItem(current);
                }

                if (currentRow == null)
                    currentRow = gridItem.Services.Component.GetDesignItem(grid.RowDefinitions.Last());

                unitSelector.SelectedItem = currentRow;
                for (var i = 0; i < grid.RowDefinitions.Count; i++)
                {
                    var row = grid.RowDefinitions[i];
                    if (row.Offset() > insertionPosition) continue;
                    if (row.Offset() + row.ActualHeight < insertionPosition) continue;

                    // split row
                    var oldLength = (GridLength)row.GetValue(RowDefinition.HeightProperty);
                    GridLength newLength1, newLength2;
                    SplitLength(oldLength, insertionPosition - row.Offset(), row.ActualHeight, out newLength1,
                        out newLength2);
                    var newRowDefinition =
                        gridItem.Services.Component.RegisterComponentForDesigner(new RowDefinition());
                    rowCollection.CollectionElements.Insert(i + 1, newRowDefinition);
                    rowCollection.CollectionElements[i].Properties[RowDefinition.HeightProperty].SetValue(newLength1);
                    newRowDefinition.Properties[RowDefinition.HeightProperty].SetValue(newLength2);
                    grid.InvalidateArrange();
                    FixIndicesAfterSplit(i, Grid.RowProperty, Grid.RowSpanProperty, insertionPosition);
                    grid.InvalidateArrange();
                    changeGroup.Commit();
                    break;
                }
            }
        }
        else
        {
            var insertionPosition = e.GetPosition(this).X;
            var columnCollection = gridItem.Properties["ColumnDefinitions"];

            DesignItem currentColumn = null;

            using (var changeGroup = gridItem.OpenGroup("Split grid column"))
            {
                if (columnCollection.CollectionElements.Count == 0)
                {
                    var firstColumn = gridItem.Services.Component.RegisterComponentForDesigner(new ColumnDefinition());
                    columnCollection.CollectionElements.Add(firstColumn);
                    grid.InvalidateArrange(); // let Avalonia assign firstColumn.ActualWidth

                    currentColumn = firstColumn;
                }
                else
                {
                    var current = grid.ColumnDefinitions
                        .FirstOrDefault(r => insertionPosition >= r.GetOffset() &&
                                             insertionPosition <= r.GetOffset() + r.ActualWidth);
                    if (current != null)
                        currentColumn = gridItem.Services.Component.GetDesignItem(current);
                }

                if (currentColumn == null)
                    currentColumn = gridItem.Services.Component.GetDesignItem(grid.ColumnDefinitions.Last());

                unitSelector.SelectedItem = currentColumn;
                for (var i = 0; i < grid.ColumnDefinitions.Count; i++)
                {
                    var column = grid.ColumnDefinitions[i];
                    if (column.GetOffset() > insertionPosition) continue;
                    if (column.GetOffset() + column.ActualWidth < insertionPosition) continue;

                    // split column
                    var oldLength = (GridLength)column.GetValue(ColumnDefinition.WidthProperty);
                    GridLength newLength1, newLength2;
                    SplitLength(oldLength, insertionPosition - column.GetOffset(), column.ActualWidth, out newLength1,
                        out newLength2);
                    var newColumnDefinition =
                        gridItem.Services.Component.RegisterComponentForDesigner(new ColumnDefinition());
                    columnCollection.CollectionElements.Insert(i + 1, newColumnDefinition);
                    columnCollection.CollectionElements[i].Properties[ColumnDefinition.WidthProperty]
                        .SetValue(newLength1);
                    newColumnDefinition.Properties[ColumnDefinition.WidthProperty].SetValue(newLength2);
                    grid.InvalidateArrange();
                    FixIndicesAfterSplit(i, Grid.ColumnProperty, Grid.ColumnSpanProperty, insertionPosition);
                    changeGroup.Commit();
                    grid.InvalidateArrange();
                    break;
                }
            }
        }

        InvalidateVisual();
    }

    private void FixIndicesAfterSplit(int splitIndex, AvaloniaProperty idxProperty, AvaloniaProperty spanProperty,
        double insertionPostion)
    {
        if (orientation == Orientation.Horizontal)
            // increment ColSpan of all controls in the split column, increment Column of all controls in later columns:
            foreach (var child in gridItem.Properties["Children"].CollectionElements)
            {
                var topLeft = child.View.TranslatePoint(new Point(0, 0), grid);
                var margin = child.Properties[Control.MarginProperty].GetConvertedValueOnInstance<Thickness>();
                var start = child.Properties.GetAttachedProperty(idxProperty).GetConvertedValueOnInstance<int>();
                var span = child.Properties.GetAttachedProperty(spanProperty).GetConvertedValueOnInstance<int>();
                if (start <= splitIndex && splitIndex < start + span)
                {
                    var width = child.Properties[Control.WidthProperty].GetConvertedValueOnInstance<double>();
                    if (insertionPostion >= topLeft.Value.X + width) continue;
                    if (insertionPostion > topLeft.Value.X)
                    {
                        child.Properties.GetAttachedProperty(spanProperty).SetValue(span + 1);
                    }
                    else
                    {
                        child.Properties.GetAttachedProperty(idxProperty).SetValue(start + 1);
                        margin = margin.WithLeft(topLeft.Value.X - insertionPostion);
                        child.Properties[Control.MarginProperty].SetValue(margin);
                    }
                }
                else if (start > splitIndex)
                {
                    child.Properties.GetAttachedProperty(idxProperty).SetValue(start + 1);
                }
            }
        else
            foreach (var child in gridItem.Properties["Children"].CollectionElements)
            {
                var topLeft = child.View.TranslatePoint(new Point(0, 0), grid);
                var margin = child.Properties[Control.MarginProperty].GetConvertedValueOnInstance<Thickness>();
                var start = child.Properties.GetAttachedProperty(idxProperty).GetConvertedValueOnInstance<int>();
                var span = child.Properties.GetAttachedProperty(spanProperty).GetConvertedValueOnInstance<int>();
                if (start <= splitIndex && splitIndex < start + span)
                {
                    var height = child.Properties[Control.HeightProperty].GetConvertedValueOnInstance<double>();
                    if (insertionPostion >= topLeft.Value.Y + height)
                        continue;
                    if (insertionPostion > topLeft.Value.Y)
                    {
                        child.Properties.GetAttachedProperty(spanProperty).SetValue(span + 1);
                    }
                    else
                    {
                        child.Properties.GetAttachedProperty(idxProperty).SetValue(start + 1);
                        margin = margin.WithTop(topLeft.Value.Y - insertionPostion);
                        child.Properties[Control.MarginProperty].SetValue(margin);
                    }
                }
                else if (start > splitIndex)
                {
                    child.Properties.GetAttachedProperty(idxProperty).SetValue(start + 1);
                }
            }
    }

    private static void SplitLength(GridLength oldLength, double insertionPosition, double oldActualValue,
        out GridLength newLength1, out GridLength newLength2)
    {
        if (oldLength.IsAuto) oldLength = new GridLength(oldActualValue);
        var percentage = insertionPosition / oldActualValue;
        newLength1 = new GridLength(oldLength.Value * percentage, oldLength.GridUnitType);
        newLength2 = new GridLength(oldLength.Value - newLength1.Value, oldLength.GridUnitType);
    }

    #endregion
}

public abstract class GridSplitterAdorner : TemplatedControl
{
    
    protected override Type StyleKeyOverride => typeof(GridSplitterAdorner);
    
    public static readonly StyledProperty<bool> IsPreviewProperty
        = AvaloniaProperty.Register<GridSplitterAdorner, bool>(nameof(IsPreview), false);

    protected readonly DesignItem firstRow, secondRow; // can also be columns

    protected readonly Grid grid;
    protected readonly DesignItem gridItem;
    protected readonly GridRailAdorner rail;

    private ChangeGroup activeChangeGroup;
    private bool mouseIsDown;
    private double mouseStartPos;

    protected GridLength original1, original2;
    protected double originalPixelSize1, originalPixelSize2;

    internal GridSplitterAdorner(GridRailAdorner rail, DesignItem gridItem, DesignItem firstRow, DesignItem secondRow)
    {
        Debug.Assert(gridItem != null);
        grid = (Grid)gridItem.Component;
        this.gridItem = gridItem;
        this.firstRow = firstRow;
        this.secondRow = secondRow;
        this.rail = rail;
    }

    public bool IsPreview
    {
        get => GetValue(IsPreviewProperty);
        set => SetValue(IsPreviewProperty, value);
    }

    protected abstract AvaloniaProperty RowColumnSizeProperty { get; }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        e.Handled = true;
        e.Pointer.Capture(this);
        Focus();
        mouseStartPos = GetCoordinate(e.GetPosition(grid));
        mouseIsDown = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (mouseIsDown)
        {
            var mousePos = GetCoordinate(e.GetPosition(grid));
            if (activeChangeGroup == null)
                if (Math.Abs(mousePos - mouseStartPos) >= 4) // Avalonia doesn't have SystemParameters
                {
                    activeChangeGroup = gridItem.OpenGroup("Change grid row/column size");
                    RememberOriginalSize();
                }

            if (activeChangeGroup != null) ChangeSize(mousePos - mouseStartPos);
        }
    }

    protected abstract double GetCoordinate(Point point);
    protected abstract void RememberOriginalSize();

    private void ChangeSize(double delta)
    {
        // delta = difference in pixels

        if (delta < -originalPixelSize1) delta = -originalPixelSize1;
        if (delta > originalPixelSize2) delta = originalPixelSize2;

        // replace Auto lengths with absolute lengths if necessary
        if (original1.IsAuto) original1 = new GridLength(originalPixelSize1);
        if (original2.IsAuto) original2 = new GridLength(originalPixelSize2);

        GridLength new1;
        if (original1.IsStar && originalPixelSize1 > 0)
            new1 = new GridLength(original1.Value * (originalPixelSize1 + delta) / originalPixelSize1,
                GridUnitType.Star);
        else
            new1 = new GridLength(originalPixelSize1 + delta);
        GridLength new2;
        if (original2.IsStar && originalPixelSize2 > 0)
            new2 = new GridLength(original2.Value * (originalPixelSize2 - delta) / originalPixelSize2,
                GridUnitType.Star);
        else
            new2 = new GridLength(originalPixelSize2 - delta);
        firstRow.Properties[RowColumnSizeProperty].SetValue(new1);
        secondRow.Properties[RowColumnSizeProperty].SetValue(new2);
        var e = (Control)this.GetVisualParent();
        e.InvalidateArrange();
        rail.InvalidateVisual();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (activeChangeGroup != null)
        {
            activeChangeGroup.Commit();
            activeChangeGroup = null;
        }

        Stop();
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        Stop();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Stop();
        }
    }

    protected void Stop()
    {
        if (mouseIsDown)
        {
            // Release pointer capture - need to get the current pointer
            // This is a simplified fix - may need refinement
            mouseIsDown = false;
        }
        
        if (activeChangeGroup != null)
        {
            activeChangeGroup.Abort();
            activeChangeGroup = null;
        }
    }
}

public class GridRowSplitterAdorner : GridSplitterAdorner
{
    protected override Type StyleKeyOverride => typeof(GridRowSplitterAdorner);
    static GridRowSplitterAdorner()
    {
        CursorProperty.OverrideDefaultValue<GridRowSplitterAdorner>(new Cursor(StandardCursorType.SizeNorthSouth));
    }

    internal GridRowSplitterAdorner(GridRailAdorner rail, DesignItem gridItem, DesignItem firstRow,
        DesignItem secondRow)
        : base(rail, gridItem, firstRow, secondRow)
    {
    }

    protected override AvaloniaProperty RowColumnSizeProperty => RowDefinition.HeightProperty;

    protected override double GetCoordinate(Point point)
    {
        return point.Y;
    }

    protected override void RememberOriginalSize()
    {
        var r1 = (RowDefinition)firstRow.Component;
        var r2 = (RowDefinition)secondRow.Component;
        original1 = (GridLength)r1.GetValue(RowDefinition.HeightProperty);
        original2 = (GridLength)r2.GetValue(RowDefinition.HeightProperty);
        originalPixelSize1 = r1.ActualHeight;
        originalPixelSize2 = r2.ActualHeight;
    }
}

public sealed class GridColumnSplitterAdorner : GridSplitterAdorner
{
    protected override Type StyleKeyOverride => typeof(GridColumnSplitterAdorner);
    static GridColumnSplitterAdorner()
    {
        CursorProperty.OverrideDefaultValue<GridColumnSplitterAdorner>(new Cursor(StandardCursorType.SizeWestEast));
    }

    internal GridColumnSplitterAdorner(GridRailAdorner rail, DesignItem gridItem, DesignItem firstRow,
        DesignItem secondRow)
        : base(rail, gridItem, firstRow, secondRow)
    {
    }

    protected override AvaloniaProperty RowColumnSizeProperty => ColumnDefinition.WidthProperty;

    protected override double GetCoordinate(Point point)
    {
        return point.X;
    }

    protected override void RememberOriginalSize()
    {
        var r1 = (ColumnDefinition)firstRow.Component;
        var r2 = (ColumnDefinition)secondRow.Component;
        original1 = (GridLength)r1.GetValue(ColumnDefinition.WidthProperty);
        original2 = (GridLength)r2.GetValue(ColumnDefinition.WidthProperty);
        originalPixelSize1 = r1.ActualWidth;
        originalPixelSize2 = r2.ActualWidth;
    }
}
