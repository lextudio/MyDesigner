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
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using MyDesigner.Design.Adorners;
using MyDesigner.Design.Extensions;
using MyDesigner.Designer.Controls;
using MyDesigner.Designer.Services;

namespace MyDesigner.Designer.Extensions;

/// <summary>
///     The drag handle displayed for Framework Elements
/// </summary>
[ExtensionServer(typeof(PrimarySelectionButOnlyWhenMultipleSelectedExtensionServer))]
[ExtensionFor(typeof(Control))]
public class TopLeftContainerDragHandleMultipleItems : AdornerProvider
{
    /// <summary />
    public TopLeftContainerDragHandleMultipleItems()
    {
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        var rect = new ContainerDragHandle();

        rect.PointerPressed += delegate(object sender, PointerPressedEventArgs e)
        {
            //Services.Selection.SetSelectedComponents(new DesignItem[] { this.ExtendedItem }, SelectionTypes.Auto);
            new DragMoveMouseGesture(ExtendedItem, false).Start(ExtendedItem.Services.DesignPanel, e);
            e.Handled = true;
        };

        var items = ExtendedItem.Services.Selection.SelectedItems;

        double minX = 0;
        double minY = 0;
        double maxX = 0;
        double maxY = 0;

        foreach (var di in items)
        {
            var relativeLocation = di.View.TranslatePoint(new Point(0, 0), ExtendedItem.View);
            
            // Handle nullable Point in Avalonia 11.x
            if (relativeLocation.HasValue)
            {
                var location = relativeLocation.Value;
                minX = minX < location.X ? minX : location.X;
                minY = minY < location.Y ? minY : location.Y;
                maxX = maxX > location.X + ((Control)di.View).Bounds.Width
                    ? maxX
                    : location.X + ((Control)di.View).Bounds.Width;
                maxY = maxY > location.Y + ((Control)di.View).Bounds.Height
                    ? maxY
                    : location.Y + ((Control)di.View).Bounds.Height;
            }
        }

        var rect2 = new Rectangle
        {
            Width = maxX - minX + 4,
            Height = maxY - minY + 4,
            Stroke = Brushes.Black,
            StrokeThickness = 2,
            StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 2, 2 }
        };

        var p = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top);
        p.XOffset = minX - 3;
        p.YOffset = minY - 3;

        var p2 = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top);
        p2.XOffset = minX + rect2.Width - 2;
        p2.YOffset = minY + rect2.Height - 2;

        AddAdorner(p, AdornerOrder.Background, rect);
        AddAdorner(p2, AdornerOrder.Background, rect2);
    }
}