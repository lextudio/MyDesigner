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
using Avalonia.Layout;
using MyDesigner.Design.Adorners;
using MyDesigner.Design.Extensions;
using MyDesigner.Designer.Controls;

namespace MyDesigner.Designer.Extensions;

[ExtensionFor(typeof(Control))]
[ExtensionServer(typeof(PrimarySelectionExtensionServer))]
public class MarginHandleExtension : AdornerProvider
{
    private Grid? _grid;
    private MarginHandle[]? _handles;
    private MarginHandle? _leftHandle, _topHandle, _rightHandle, _bottomHandle;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (ExtendedItem.Parent != null)
            if (ExtendedItem.Parent.ComponentType == typeof(Grid))
            {
                var extendedControl = (Control)ExtendedItem.Component;
                var adornerPanel = new AdornerPanel();

                // If the Element is rotated/skewed in the grid, then margin handles do not appear
                if (extendedControl.RenderTransform is MatrixTransform mt && mt.Matrix.IsIdentity)
                {
                    _grid = ExtendedItem.Parent.View as Grid;
                    _handles = new[]
                    {
                        _leftHandle = new MarginHandle(ExtendedItem, adornerPanel, HandleOrientation.Left),
                        _topHandle = new MarginHandle(ExtendedItem, adornerPanel, HandleOrientation.Top),
                        _rightHandle = new MarginHandle(ExtendedItem, adornerPanel, HandleOrientation.Right),
                        _bottomHandle = new MarginHandle(ExtendedItem, adornerPanel, HandleOrientation.Bottom)
                    };
                    foreach (var handle in _handles)
                    {
                        handle.PointerPressed += OnPointerPressed;
                        handle.Stub.PointerPressed += OnPointerPressed;
                    }
                }

                if (adornerPanel != null)
                    Adorners.Add(adornerPanel);
            }
    }

    public void HideHandles()
    {
        if (_handles != null)
            foreach (var handle in _handles)
            {
                handle.ShouldBeVisible = false;
                handle.IsVisible = false;
            }
    }

    public void ShowHandles()
    {
        if (_handles != null)
            foreach (var handle in _handles)
            {
                handle.ShouldBeVisible = true;
                handle.IsVisible = true;
                handle.DecideVisiblity(handle.HandleLength);
            }
    }

    #region Change margin through handle/stub

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            return;
            
        e.Handled = true;
        var row = ExtendedItem.Properties.GetAttachedProperty(Grid.RowProperty).GetConvertedValueOnInstance<int>();
        var rowSpan = ExtendedItem.Properties.GetAttachedProperty(Grid.RowSpanProperty)
            .GetConvertedValueOnInstance<int>();

        var column = ExtendedItem.Properties.GetAttachedProperty(Grid.ColumnProperty)
            .GetConvertedValueOnInstance<int>();
        var columnSpan = ExtendedItem.Properties.GetAttachedProperty(Grid.ColumnSpanProperty)
            .GetConvertedValueOnInstance<int>();

        var margin = ExtendedItem.Properties[Layoutable.MarginProperty].GetConvertedValueOnInstance<Thickness>();

        var point = ExtendedItem.View.TranslatePoint(new Point(), _grid) ?? new Point();
        var position = new Rect(point, PlacementOperation.GetRealElementSize(ExtendedItem.View));
        MarginHandle? handle = null;
        if (sender is MarginHandle)
            handle = sender as MarginHandle;
        if (sender is MarginStub)
            handle = ((MarginStub)sender).Handle;
        if (handle != null)
            switch (handle.Orientation)
            {
                case HandleOrientation.Left:
                    if (_rightHandle?.IsVisible == true)
                    {
                        if (_leftHandle?.IsVisible == true)
                        {
                            margin = margin.WithLeft(0);
                            ExtendedItem.Properties[Layoutable.WidthProperty].SetValue(position.Width);
                            ExtendedItem.Properties[Layoutable.HorizontalAlignmentProperty]
                                .SetValue(HorizontalAlignment.Right);
                        }
                        else
                        {
                            var leftMargin = position.Left - GetColumnOffset(column);
                            margin = margin.WithLeft(leftMargin);
                            ExtendedItem.Properties[Layoutable.HorizontalAlignmentProperty].Reset();
                            ExtendedItem.Properties[Layoutable.WidthProperty].Reset();
                        }
                    }
                    else
                    {
                        if (_leftHandle?.IsVisible == true)
                        {
                            margin = margin.WithLeft(0);
                            var rightMargin = GetColumnOffset(column + columnSpan) - position.Right;
                            margin = margin.WithRight(rightMargin);

                            ExtendedItem.Properties[Layoutable.WidthProperty].SetValue(position.Width);
                            ExtendedItem.Properties[Layoutable.HorizontalAlignmentProperty]
                                .SetValue(HorizontalAlignment.Right);
                        }
                        else
                        {
                            var leftMargin = position.Left - GetColumnOffset(column);
                            margin = margin.WithLeft(leftMargin);
                            ExtendedItem.Properties[Layoutable.HorizontalAlignmentProperty]
                                .SetValue(HorizontalAlignment.Left);
                        }
                    }

                    break;
                case HandleOrientation.Top:
                    if (_bottomHandle?.IsVisible == true)
                    {
                        if (_topHandle?.IsVisible == true)
                        {
                            margin = margin.WithTop(0);
                            ExtendedItem.Properties[Layoutable.HeightProperty].SetValue(position.Height);
                            ExtendedItem.Properties[Layoutable.VerticalAlignmentProperty]
                                .SetValue(VerticalAlignment.Bottom);
                        }
                        else
                        {
                            var topMargin = position.Top - GetRowOffset(row);
                            margin = margin.WithTop(topMargin);
                            ExtendedItem.Properties[Layoutable.VerticalAlignmentProperty].Reset();
                            ExtendedItem.Properties[Layoutable.HeightProperty].Reset();
                        }
                    }
                    else
                    {
                        if (_topHandle?.IsVisible == true)
                        {
                            margin = margin.WithTop(0);
                            var bottomMargin = GetRowOffset(row + rowSpan) - position.Bottom;
                            margin = margin.WithBottom(bottomMargin);

                            ExtendedItem.Properties[Layoutable.HeightProperty].SetValue(position.Height);
                            ExtendedItem.Properties[Layoutable.VerticalAlignmentProperty]
                                .SetValue(VerticalAlignment.Bottom);
                        }
                        else
                        {
                            var topMargin = position.Top - GetRowOffset(row);
                            margin = margin.WithTop(topMargin);
                            ExtendedItem.Properties[Layoutable.VerticalAlignmentProperty]
                                .SetValue(VerticalAlignment.Top);
                        }
                    }

                    break;
                case HandleOrientation.Right:
                    if (_leftHandle?.IsVisible == true)
                    {
                        if (_rightHandle?.IsVisible == true)
                        {
                            margin = margin.WithRight(0);
                            ExtendedItem.Properties[Layoutable.WidthProperty].SetValue(position.Width);
                            ExtendedItem.Properties[Layoutable.HorizontalAlignmentProperty]
                                .SetValue(HorizontalAlignment.Left);
                        }
                        else
                        {
                            var rightMargin = GetColumnOffset(column + columnSpan) - position.Right;
                            margin = margin.WithRight(rightMargin);
                            ExtendedItem.Properties[Layoutable.HorizontalAlignmentProperty].Reset();
                            ExtendedItem.Properties[Layoutable.WidthProperty].Reset();
                        }
                    }
                    else
                    {
                        if (_rightHandle?.IsVisible == true)
                        {
                            margin = margin.WithRight(0);
                            var leftMargin = position.Left - GetColumnOffset(column);
                            margin = margin.WithLeft(leftMargin);

                            ExtendedItem.Properties[Layoutable.WidthProperty].SetValue(position.Width);
                            ExtendedItem.Properties[Layoutable.HorizontalAlignmentProperty]
                                .SetValue(HorizontalAlignment.Left);
                        }
                        else
                        {
                            var rightMargin = GetColumnOffset(column + columnSpan) - position.Right;
                            margin = margin.WithRight(rightMargin);
                            ExtendedItem.Properties[Layoutable.HorizontalAlignmentProperty]
                                .SetValue(HorizontalAlignment.Right);
                        }
                    }

                    break;
                case HandleOrientation.Bottom:
                    if (_topHandle?.IsVisible == true)
                    {
                        if (_bottomHandle?.IsVisible == true)
                        {
                            margin = margin.WithBottom(0);
                            ExtendedItem.Properties[Layoutable.HeightProperty].SetValue(position.Height);
                            ExtendedItem.Properties[Layoutable.VerticalAlignmentProperty]
                                .SetValue(VerticalAlignment.Top);
                        }
                        else
                        {
                            var bottomMargin = GetRowOffset(row + rowSpan) - position.Bottom;
                            margin = margin.WithBottom(bottomMargin);
                            ExtendedItem.Properties[Layoutable.VerticalAlignmentProperty].Reset();
                            ExtendedItem.Properties[Layoutable.HeightProperty].Reset();
                        }
                    }
                    else
                    {
                        if (_bottomHandle?.IsVisible == true)
                        {
                            margin = margin.WithBottom(0);
                            var topMargin = position.Top - GetRowOffset(row);
                            margin = margin.WithTop(topMargin);

                            ExtendedItem.Properties[Layoutable.HeightProperty].SetValue(position.Height);
                            ExtendedItem.Properties[Layoutable.VerticalAlignmentProperty]
                                .SetValue(VerticalAlignment.Top);
                        }
                        else
                        {
                            var bottomMargin = GetRowOffset(row + rowSpan) - position.Bottom;
                            margin = margin.WithBottom(bottomMargin);
                            ExtendedItem.Properties[Layoutable.VerticalAlignmentProperty]
                                .SetValue(VerticalAlignment.Bottom);
                        }
                    }

                    break;
            }

        ExtendedItem.Properties[Layoutable.MarginProperty].SetValue(margin);
    }

    private double GetColumnOffset(int index)
    {
        if (_grid != null)
        {
            // when the grid has no columns, we still need to return 0 for index=0 and grid.Width for index=1
            if (index == 0)
                return 0;
            if (index < _grid.ColumnDefinitions.Count)
                return _grid.ColumnDefinitions[index].ActualWidth; // Note: Avalonia doesn't have Offset property
            return _grid.Bounds.Width;
        }

        return 0;
    }

    private double GetRowOffset(int index)
    {
        if (_grid != null)
        {
            if (index == 0)
                return 0;
            if (index < _grid.RowDefinitions.Count)
                return _grid.RowDefinitions[index].ActualHeight; // Note: Avalonia doesn't have Offset property
            return _grid.Bounds.Height;
        }

        return 0;
    }

    #endregion
}