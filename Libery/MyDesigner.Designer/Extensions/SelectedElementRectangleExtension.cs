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


using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using MyDesigner.Design.Adorners;
using MyDesigner.Design.Extensions;

namespace MyDesigner.Designer.Extensions;

/// <summary>
///     Draws a dotted line around selected UIElements.
/// </summary>
[ExtensionFor(typeof(Control))]
public class SelectedElementRectangleExtension : SelectionAdornerProvider
{
    /// <summary>
    ///     Creates a new SelectedElementRectangleExtension instance.
    /// </summary>
    public SelectedElementRectangleExtension()
    {
        var selectionRect = new Rectangle();
        selectionRect.Stroke = new SolidColorBrush(Color.FromRgb(0x47, 0x47, 0x47));
        selectionRect.StrokeThickness = 1.5;
        selectionRect.IsHitTestVisible = false;

        var placement = new RelativePlacement(HorizontalAlignment.Stretch, VerticalAlignment.Stretch);
        placement.XOffset = -1;
        placement.YOffset = -1;
        placement.WidthOffset = 2;
        placement.HeightOffset = 2;

        AddAdorners(placement, selectionRect);
    }
}