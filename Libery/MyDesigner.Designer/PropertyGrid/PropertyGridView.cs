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
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using MyDesigner.Design.PropertyGrid;

namespace MyDesigner.Designer.PropertyGrid;

[TemplatePart(Name = "PART_Thumb", Type = typeof(Thumb))]
public class PropertyGridView : TemplatedControl
{
    private static PropertyContextMenu propertyContextMenu = new();

    public static readonly StyledProperty<double> FirstColumnWidthProperty =
        AvaloniaProperty.Register<PropertyGridView, double>(nameof(FirstColumnWidth), 120.0);

    public static readonly StyledProperty<IEnumerable<DesignItem>> SelectedItemsProperty =
        AvaloniaProperty.Register<PropertyGridView, IEnumerable<DesignItem>>(nameof(SelectedItems));

    private Thumb thumb;
    protected override Type StyleKeyOverride => typeof(PropertyGridView);
    
    public PropertyGridView() : this(null)
    {
    }

    public PropertyGridView(IPropertyGrid pg)
    {
        PropertyGrid = pg ?? new PropertyGrid();
        DataContext = PropertyGrid;
    }

    public IPropertyGrid PropertyGrid { get; }

    public double FirstColumnWidth
    {
        get => GetValue(FirstColumnWidthProperty);
        set => SetValue(FirstColumnWidthProperty, value);
    }

    public IEnumerable<DesignItem> SelectedItems
    {
        get => GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        thumb = e.NameScope.Find<Thumb>("PART_Thumb");

        if (thumb != null)
            thumb.DragDelta += thumb_DragDelta;

        base.OnApplyTemplate(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedItemsProperty) 
            PropertyGrid.SelectedItems = SelectedItems;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        
        if (e.InitialPressMouseButton != MouseButton.Right)
            return;

        var source = e.Source as Visual;
        if (source == null) return;

        var ancestors = source.GetVisualAncestors();
        var row = ancestors.OfType<Border>().FirstOrDefault(b => b.Name == "uxPropertyNodeRow");
        if (row == null) return;

        var node = row.DataContext as PropertyNode;
        if (node?.IsEvent == true) return;

        var contextMenu = new PropertyContextMenu();
        contextMenu.DataContext = node;
        contextMenu.PlacementMode = PlacementMode.Bottom;
        contextMenu.HorizontalOffset = -30;
        contextMenu.PlacementTarget = row;
        contextMenu.Open(row);
    }

    private void thumb_DragDelta(object sender, VectorEventArgs e)
    {
        FirstColumnWidth = Math.Max(0, FirstColumnWidth + e.Vector.X);
    }
}