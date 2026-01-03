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
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using MyDesigner.Design.UIExtensions;
using MyDesigner.Designer.Converters;
using MyDesigner.Designer.Services;

namespace MyDesigner.Designer.Controls;

public class PanelMoveAdorner : TemplatedControl
{
    private readonly DesignItem item;
    private readonly ScaleTransform scaleTransform;

    protected override Type StyleKeyOverride => typeof(PanelMoveAdorner);

    public PanelMoveAdorner(DesignItem item)
    {
        this.item = item;

        scaleTransform = new ScaleTransform(1.0, 1.0);
        RenderTransform = scaleTransform;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            e.Handled = true;
            //item.Services.Selection.SetSelectedComponents(new DesignItem [] { item }, SelectionTypes.Auto);
            new DragMoveMouseGesture(item, false, true).Start(item.Services.DesignPanel, e);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var bnd = new Binding("IsVisible") { Source = item.Component };
        bnd.Converter = CollapsedWhenFalse.Instance;
        this.Bind(IsVisibleProperty, bnd);

        var surface = this.TryFindParent<DesignSurface>();
        if (surface != null && surface.ZoomControl != null)
        {
            bnd = new Binding("CurrentZoom") { Source = surface.ZoomControl };
            bnd.Converter = InvertedZoomConverter.Instance;

            scaleTransform.Bind(ScaleTransform.ScaleXProperty, bnd);
            scaleTransform.Bind(ScaleTransform.ScaleYProperty, bnd);
        }
    }
}