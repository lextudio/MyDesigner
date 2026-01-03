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
using Avalonia.Input;
using Avalonia.Layout;
using MyDesigner.Design.Adorners;
using MyDesigner.Design.Extensions;
using MyDesigner.Designer.Controls;
using MyDesigner.Designer.Services;

namespace MyDesigner.Designer.Extensions;

/// <summary>
///     The drag handle displayed for Framework Elements
/// </summary>
[ExtensionServer(typeof(OnlyOneItemSelectedExtensionServer))]
[ExtensionFor(typeof(Control))]
public class TopLeftContainerDragHandle : AdornerProvider
{
    /// <summary />
    public TopLeftContainerDragHandle()
    {
        var rect = new ContainerDragHandle();

        rect.PointerPressed += delegate(object sender, PointerPressedEventArgs e)
        {
            //Services.Selection.SetSelectedComponents(new DesignItem[] { this.ExtendedItem }, SelectionTypes.Auto);
            new DragMoveMouseGesture(ExtendedItem, false).Start(ExtendedItem.Services.DesignPanel, e);
            e.Handled = true;
        };

        var p = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top);
        p.XOffset = -7;
        p.YOffset = -7;

        AddAdorner(p, AdornerOrder.Background, rect);
    }
}