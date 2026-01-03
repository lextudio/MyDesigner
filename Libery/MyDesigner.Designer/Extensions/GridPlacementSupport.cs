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
using Avalonia.Layout;
using MyDesigner.Design.Extensions;
using MyDesigner.Designer.Controls;

namespace MyDesigner.Designer.Extensions;

/// <summary>
///     Provides <see cref="IPlacementBehavior" /> behavior for <see cref="Grid" />.
/// </summary>
[ExtensionFor(typeof(Grid), OverrideExtension = typeof(DefaultPlacementBehavior))]
public sealed class GridPlacementSupport : SnaplinePlacementBehavior
{
    private const double epsilon = 0.00000001;
    private bool enteredIntoNewContainer;

    private GrayOutDesignerExceptActiveArea grayOut;
    private Grid grid;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        grid = (Grid)ExtendedItem.Component;
    }

    private double GetColumnOffset(int index)
    {
        // when the grid has no columns, we still need to return 0 for index=0 and grid.Width for index=1
        if (index == 0)
            return 0;
        if (index < grid.ColumnDefinitions.Count)
        {
            // In Avalonia, we need to calculate offset differently
            double offset = 0;
            for (int i = 0; i < index && i < grid.ColumnDefinitions.Count; i++)
            {
                // This is a simplified calculation - actual implementation may need more complex logic
                offset += grid.ColumnDefinitions[i].ActualWidth;
            }
            return offset;
        }
        return grid.Bounds.Width;
    }

    private double GetRowOffset(int index)
    {
        if (index == 0)
            return 0;
        if (index < grid.RowDefinitions.Count)
        {
            // In Avalonia, we need to calculate offset differently
            double offset = 0;
            for (int i = 0; i < index && i < grid.RowDefinitions.Count; i++)
            {
                // This is a simplified calculation - actual implementation may need more complex logic
                offset += grid.RowDefinitions[i].ActualHeight;
            }
            return offset;
        }
        return grid.Bounds.Height;
    }

    private int GetColumnIndex(double x)
    {
        if (grid.ColumnDefinitions.Count == 0)
            return 0;
        
        double currentOffset = 0;
        for (var i = 0; i < grid.ColumnDefinitions.Count; i++)
        {
            currentOffset += grid.ColumnDefinitions[i].ActualWidth;
            if (x < currentOffset - epsilon)
                return i;
        }
        return grid.ColumnDefinitions.Count - 1;
    }

    private int GetRowIndex(double y)
    {
        if (grid.RowDefinitions.Count == 0)
            return 0;
        
        double currentOffset = 0;
        for (var i = 0; i < grid.RowDefinitions.Count; i++)
        {
            currentOffset += grid.RowDefinitions[i].ActualHeight;
            if (y < currentOffset - epsilon)
                return i;
        }
        return grid.RowDefinitions.Count - 1;
    }

    private int GetEndColumnIndex(double x)
    {
        if (grid.ColumnDefinitions.Count == 0)
            return 0;
        
        double currentOffset = 0;
        for (var i = 0; i < grid.ColumnDefinitions.Count; i++)
        {
            currentOffset += grid.ColumnDefinitions[i].ActualWidth;
            if (x <= currentOffset + epsilon)
                return i;
        }
        return grid.ColumnDefinitions.Count - 1;
    }

    private int GetEndRowIndex(double y)
    {
        if (grid.RowDefinitions.Count == 0)
            return 0;
        
        double currentOffset = 0;
        for (var i = 0; i < grid.RowDefinitions.Count; i++)
        {
            currentOffset += grid.RowDefinitions[i].ActualHeight;
            if (y <= currentOffset + epsilon)
                return i;
        }
        return grid.RowDefinitions.Count - 1;
    }

    protected override void AddContainerSnaplines(Rect containerRect, List<Snapline> horizontalMap,
        List<Snapline> verticalMap)
    {
        var grid = (Grid)ExtendedItem.View;
        double offset = 0;
        foreach (var r in grid.RowDefinitions)
        {
            offset += r.ActualHeight;
            horizontalMap.Add(new Snapline
                { RequireOverlap = false, Offset = offset, Start = offset, End = containerRect.Right });
            if (SnaplineMargin > 0)
            {
                horizontalMap.Add(new Snapline
                {
                    RequireOverlap = false, Offset = offset - SnaplineMargin, Start = offset, End = containerRect.Right
                });
                horizontalMap.Add(new Snapline
                {
                    RequireOverlap = false, Offset = offset + SnaplineMargin, Start = offset, End = containerRect.Right
                });
            }
        }

        offset = 0;
        foreach (var c in grid.ColumnDefinitions)
        {
            offset += c.ActualWidth;
            verticalMap.Add(new Snapline
                { RequireOverlap = false, Offset = offset, Start = containerRect.Top, End = containerRect.Bottom });
            if (SnaplineMargin > 0)
            {
                verticalMap.Add(new Snapline
                {
                    RequireOverlap = false, Offset = offset - SnaplineMargin, Start = containerRect.Top,
                    End = containerRect.Bottom
                });
                verticalMap.Add(new Snapline
                {
                    RequireOverlap = false, Offset = offset + SnaplineMargin, Start = containerRect.Top,
                    End = containerRect.Bottom
                });
            }
        }
    }

    private static void SetColumn(DesignItem item, int column, int columnSpan)
    {
        Debug.Assert(item != null && column >= 0 && columnSpan > 0);
        item.Properties.GetAttachedProperty(Grid.ColumnProperty).SetValue(column);
        if (columnSpan == 1)
            item.Properties.GetAttachedProperty(Grid.ColumnSpanProperty).Reset();
        else
            item.Properties.GetAttachedProperty(Grid.ColumnSpanProperty).SetValue(columnSpan);
    }

    private static void SetRow(DesignItem item, int row, int rowSpan)
    {
        Debug.Assert(item != null && row >= 0 && rowSpan > 0);
        item.Properties.GetAttachedProperty(Grid.RowProperty).SetValue(row);
        if (rowSpan == 1)
            item.Properties.GetAttachedProperty(Grid.RowSpanProperty).Reset();
        else
            item.Properties.GetAttachedProperty(Grid.RowSpanProperty).SetValue(rowSpan);
    }

    private static HorizontalAlignment SuggestHorizontalAlignment(Rect itemBounds, Rect availableSpaceRect)
    {
        var isLeft = itemBounds.Left < availableSpaceRect.Left + availableSpaceRect.Width / 4;
        var isRight = itemBounds.Right > availableSpaceRect.Right - availableSpaceRect.Width / 4;
        if (isLeft && isRight)
            return HorizontalAlignment.Stretch;
        if (isRight)
            return HorizontalAlignment.Right;
        return HorizontalAlignment.Left;
    }

    private static VerticalAlignment SuggestVerticalAlignment(Rect itemBounds, Rect availableSpaceRect)
    {
        var isTop = itemBounds.Top < availableSpaceRect.Top + availableSpaceRect.Height / 4;
        var isBottom = itemBounds.Bottom > availableSpaceRect.Bottom - availableSpaceRect.Height / 4;
        if (isTop && isBottom)
            return VerticalAlignment.Stretch;
        if (isBottom)
            return VerticalAlignment.Bottom;
        return VerticalAlignment.Top;
    }

    public override void EnterContainer(PlacementOperation operation)
    {
        enteredIntoNewContainer = true;
        grid.UpdateLayout();
        base.EnterContainer(operation);

        if (operation.Type == PlacementType.PasteItem)
            foreach (var info in operation.PlacedItems)
            {
                var margin = info.Item.Properties.GetProperty(Layoutable.MarginProperty)
                    .GetConvertedValueOnInstance<Thickness>();
                var horizontalAlignment = info.Item.Properties.GetProperty(Layoutable.HorizontalAlignmentProperty)
                    .GetConvertedValueOnInstance<HorizontalAlignment>();
                var verticalAlignment = info.Item.Properties.GetProperty(Layoutable.VerticalAlignmentProperty)
                    .GetConvertedValueOnInstance<VerticalAlignment>();

                if (horizontalAlignment == HorizontalAlignment.Left)
                    margin = margin.WithLeft(margin.Left + PlacementOperation.PasteOffset);
                else if (horizontalAlignment == HorizontalAlignment.Right)
                    margin = margin.WithRight(margin.Right - PlacementOperation.PasteOffset);

                if (verticalAlignment == VerticalAlignment.Top)
                    margin = margin.WithTop(margin.Top + PlacementOperation.PasteOffset);
                else if (verticalAlignment == VerticalAlignment.Bottom)
                    margin = margin.WithBottom(margin.Bottom - PlacementOperation.PasteOffset);

                info.Item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
            }
    }

    public override void EndPlacement(PlacementOperation operation)
    {
        GrayOutDesignerExceptActiveArea.Stop(ref grayOut);
        enteredIntoNewContainer = false;
        base.EndPlacement(operation);
    }

    public override void SetPosition(PlacementInformation info)
    {
        base.SetPosition(info);
        var leftColumnIndex = GetColumnIndex(info.Bounds.Left);
        var rightColumnIndex = GetEndColumnIndex(info.Bounds.Right);
        if (rightColumnIndex < leftColumnIndex) rightColumnIndex = leftColumnIndex;
        SetColumn(info.Item, leftColumnIndex, rightColumnIndex - leftColumnIndex + 1);
        var topRowIndex = GetRowIndex(info.Bounds.Top);
        var bottomRowIndex = GetEndRowIndex(info.Bounds.Bottom);
        if (bottomRowIndex < topRowIndex) bottomRowIndex = topRowIndex;
        SetRow(info.Item, topRowIndex, bottomRowIndex - topRowIndex + 1);

        var availableSpaceRect = new Rect(
            new Point(GetColumnOffset(leftColumnIndex), GetRowOffset(topRowIndex)),
            new Point(GetColumnOffset(rightColumnIndex + 1), GetRowOffset(bottomRowIndex + 1))
        );
        if (info.Item == Services.Selection.PrimarySelection)
        {
            // only for primary selection:
            if (grayOut != null)
                grayOut.AnimateActiveAreaRectTo(availableSpaceRect);
            else
                GrayOutDesignerExceptActiveArea.Start(ref grayOut, Services, ExtendedItem.View, availableSpaceRect);
        }

        var ha = info.Item.Properties[Layoutable.HorizontalAlignmentProperty]
            .GetConvertedValueOnInstance<HorizontalAlignment>();
        var va = info.Item.Properties[Layoutable.VerticalAlignmentProperty]
            .GetConvertedValueOnInstance<VerticalAlignment>();
        if (enteredIntoNewContainer)
        {
            ha = SuggestHorizontalAlignment(info.Bounds, availableSpaceRect);
            va = SuggestVerticalAlignment(info.Bounds, availableSpaceRect);
        }

        info.Item.Properties[Layoutable.HorizontalAlignmentProperty].SetValue(ha);
        info.Item.Properties[Layoutable.VerticalAlignmentProperty].SetValue(va);

        var margin = new Thickness(0, 0, 0, 0);
        if (ha == HorizontalAlignment.Left || ha == HorizontalAlignment.Stretch)
            margin = margin.WithLeft(info.Bounds.Left - GetColumnOffset(leftColumnIndex));
        if (va == VerticalAlignment.Top || va == VerticalAlignment.Stretch)
            margin = margin.WithTop(info.Bounds.Top - GetRowOffset(topRowIndex));
        if (ha == HorizontalAlignment.Right || ha == HorizontalAlignment.Stretch)
            margin = margin.WithRight(GetColumnOffset(rightColumnIndex + 1) - info.Bounds.Right);
        if (va == VerticalAlignment.Bottom || va == VerticalAlignment.Stretch)
            margin = margin.WithBottom(GetRowOffset(bottomRowIndex + 1) - info.Bounds.Bottom);
        info.Item.Properties[Layoutable.MarginProperty].SetValue(margin);

        if (ha == HorizontalAlignment.Stretch)
            info.Item.Properties[Layoutable.WidthProperty].Reset();
        //else
        //    info.Item.Properties[Layoutable.WidthProperty].SetValue(info.Bounds.Width);

        if (va == VerticalAlignment.Stretch)
            info.Item.Properties[Layoutable.HeightProperty].Reset();
        //else
        //    info.Item.Properties[Layoutable.HeightProperty].SetValue(info.Bounds.Height);
    }

    public override void LeaveContainer(PlacementOperation operation)
    {
        GrayOutDesignerExceptActiveArea.Stop(ref grayOut);
        base.LeaveContainer(operation);
        foreach (var info in operation.PlacedItems)
            if (info.Item.ComponentType == typeof(ColumnDefinition))
            {
                // TODO: combine the width of the deleted column with the previous column
                ExtendedItem.Properties["ColumnDefinitions"].CollectionElements.Remove(info.Item);
            }
            else if (info.Item.ComponentType == typeof(RowDefinition))
            {
                ExtendedItem.Properties["RowDefinitions"].CollectionElements.Remove(info.Item);
            }
            else
            {
                info.Item.Properties.GetAttachedProperty(Grid.RowProperty).Reset();
                info.Item.Properties.GetAttachedProperty(Grid.ColumnProperty).Reset();
                info.Item.Properties.GetAttachedProperty(Grid.RowSpanProperty).Reset();
                info.Item.Properties.GetAttachedProperty(Grid.ColumnSpanProperty).Reset();

                var ha = info.Item.Properties[Layoutable.HorizontalAlignmentProperty]
                    .GetConvertedValueOnInstance<HorizontalAlignment>();
                var va = info.Item.Properties[Layoutable.VerticalAlignmentProperty]
                    .GetConvertedValueOnInstance<VerticalAlignment>();

                if (ha == HorizontalAlignment.Stretch)
                    info.Item.Properties[Layoutable.WidthProperty].SetValue(info.Bounds.Width);
                if (va == VerticalAlignment.Stretch)
                    info.Item.Properties[Layoutable.HeightProperty].SetValue(info.Bounds.Height);
            }
    }
}