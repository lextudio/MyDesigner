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
using Avalonia.Media;
using MyDesigner.Design.Adorners;
using MyDesigner.Design.Extensions;

namespace MyDesigner.Designer.Extensions;

[ExtensionFor(typeof(Control))]
[ExtensionServer(typeof(MouseOverExtensionServer))]
public class BorderForMouseOver : AdornerProvider
{
    private readonly AdornerPanel adornerPanel;
    private readonly Border border = new();

    public BorderForMouseOver()
    {
        adornerPanel = new AdornerPanel();
        adornerPanel.Order = AdornerOrder.Background;
        Adorners.Add(adornerPanel);
        border.BorderThickness = new Thickness(1);
        border.BorderBrush = Brushes.DodgerBlue;
        border.Margin = new Thickness(-2);
        AdornerPanel.SetPlacement(border, AdornerPlacement.FillContent);
        adornerPanel.Children.Add(border);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (ExtendedItem.Component is Line line)
            // To display border of Line in correct position.
            border.Margin = new Thickness(
                line.EndPoint.X < 0 ? line.EndPoint.X : 0,  // left
                line.EndPoint.Y < 0 ? line.EndPoint.Y : 0,  // top
                0,  // right
                0   // bottom
            );
    }
}