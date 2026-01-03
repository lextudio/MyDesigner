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
using Avalonia.Layout;
using Avalonia.Threading;
using MyDesigner.Design.Adorners;
using MyDesigner.Design.Extensions;
using MyDesigner.Designer.Controls;

namespace MyDesigner.Designer.Extensions;

/// <summary>
///     Allows arranging the rows/column on a grid.
/// </summary>
[ExtensionFor(typeof(Grid))]
[ExtensionServer(
    typeof(LogicalOrExtensionServer<PrimarySelectionExtensionServer, PrimarySelectionParentExtensionServer>))]
public class GridAdornerProvider : AdornerProvider
{
    private readonly AdornerPanel adornerPanel = new();
    private readonly List<GridSplitterAdorner> splitterList = new();
    private bool requireSplitterRecreation;
    private GridRailAdorner topBar, leftBar;

    protected override void OnInitialized()
    {
        leftBar = new GridRailAdorner(ExtendedItem, adornerPanel, Orientation.Vertical);
        topBar = new GridRailAdorner(ExtendedItem, adornerPanel, Orientation.Horizontal);

        var rp = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Stretch);
        rp.XOffset -= GridRailAdorner.RailDistance;
        AdornerPanel.SetPlacement(leftBar, rp);
        rp = new RelativePlacement(HorizontalAlignment.Stretch, VerticalAlignment.Top);
        rp.YOffset -= GridRailAdorner.RailDistance;
        AdornerPanel.SetPlacement(topBar, rp);

        adornerPanel.Children.Add(leftBar);
        adornerPanel.Children.Add(topBar);
        Adorners.Add(adornerPanel);

        CreateSplitter();
        ExtendedItem.PropertyChanged += OnPropertyChanged;
        base.OnInitialized();
    }

    protected override void OnRemove()
    {
        ExtendedItem.PropertyChanged -= OnPropertyChanged;
        base.OnRemove();
    }
    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "RowDefinitions" || e.PropertyName == "ColumnDefinitions") CreateSplitter();
    }

    private void CreateSplitter()
    {
        if (requireSplitterRecreation) return;
        requireSplitterRecreation = true;

        // splitter creation is delayed to prevent unnecessary splitter re-creation when multiple
        // changes to the collection are done.
        // It also ensures that the Offset property of new rows/columns is initialized when the splitter
        // is added.
        Dispatcher.UIThread.InvokeAsync(
            (Action)delegate
            {
                requireSplitterRecreation = false;
                foreach (var splitter in splitterList) adornerPanel.Children.Remove(splitter);
                splitterList.Clear();
                var grid = (Grid)ExtendedItem.Component;
                IList<DesignItem> col = ExtendedItem.Properties["RowDefinitions"].CollectionElements;
                for (var i = 1; i < grid.RowDefinitions.Count; i++)
                {
                    var row = grid.RowDefinitions[i];
                    var splitter = new GridRowSplitterAdorner(leftBar, ExtendedItem, col[i - 1], col[i]);
                    AdornerPanel.SetPlacement(splitter, new RowSplitterPlacement(row));
                    adornerPanel.Children.Add(splitter);
                    splitterList.Add(splitter);
                }

                col = ExtendedItem.Properties["ColumnDefinitions"].CollectionElements;
                for (var i = 1; i < grid.ColumnDefinitions.Count; i++)
                {
                    var column = grid.ColumnDefinitions[i];
                    var splitter = new GridColumnSplitterAdorner(topBar, ExtendedItem, col[i - 1], col[i]);
                    AdornerPanel.SetPlacement(splitter, new ColumnSplitterPlacement(column));
                    adornerPanel.Children.Add(splitter);
                    splitterList.Add(splitter);
                }
            }, DispatcherPriority.Loaded);
    }

    private sealed class RowSplitterPlacement : AdornerPlacement
    {
        private readonly RowDefinition row;

        public RowSplitterPlacement(RowDefinition row)
        {
            this.row = row;
        }

        public override void Arrange(AdornerPanel panel, Control adorner, Size adornedElementSize)
        {
            adorner.Arrange(new Rect(-(GridRailAdorner.RailSize + GridRailAdorner.RailDistance),
                row.ActualHeight - GridRailAdorner.SplitterWidth / 2,
                GridRailAdorner.RailSize + GridRailAdorner.RailDistance + adornedElementSize.Width,
                GridRailAdorner.SplitterWidth));
        }
    }

    private sealed class ColumnSplitterPlacement : AdornerPlacement
    {
        private readonly ColumnDefinition column;

        public ColumnSplitterPlacement(ColumnDefinition column)
        {
            this.column = column;
        }

        public override void Arrange(AdornerPanel panel, Control adorner, Size adornedElementSize)
        {
            adorner.Arrange(new Rect(column.ActualWidth - GridRailAdorner.SplitterWidth / 2,
                -(GridRailAdorner.RailSize + GridRailAdorner.RailDistance),
                GridRailAdorner.SplitterWidth,
                GridRailAdorner.RailSize + GridRailAdorner.RailDistance + adornedElementSize.Height));
        }
    }
}