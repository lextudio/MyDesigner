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
using Avalonia.Media;
using MyDesigner.Design.Adorners;
using MyDesigner.Designer.Services;

namespace MyDesigner.Designer.Controls;

/// <summary>
///     A Info text area.
/// </summary>
public sealed class InfoTextEnterArea : TemplatedControl
{
    protected override Type StyleKeyOverride => typeof(InfoTextEnterArea);
    private AdornerPanel adornerPanel;
    private IDesignPanel designPanel;

    public InfoTextEnterArea()
    {
        IsHitTestVisible = false;
    }

    public Geometry ActiveAreaGeometry { get; set; }

    public static void Start(ref InfoTextEnterArea grayOut, ServiceContainer services, Control activeContainer,
        string text)
    {
        Debug.Assert(activeContainer != null);
        Start(ref grayOut, services, activeContainer, new Rect(activeContainer.Bounds.Size), text);
    }

    public static void Start(ref InfoTextEnterArea grayOut, ServiceContainer services, Control activeContainer,
        Rect activeRectInActiveContainer, string text)
    {
        Debug.Assert(services != null);
        Debug.Assert(activeContainer != null);
        var designPanel = services.GetService<IDesignPanel>() as DesignPanel;
        var optionService = services.GetService<OptionService>();
        if (designPanel != null && grayOut == null && optionService != null &&
            optionService.GrayOutDesignSurfaceExceptParentContainerWhenDragging)
        {
            grayOut = new InfoTextEnterArea();
            grayOut.designPanel = designPanel;
            grayOut.adornerPanel = new AdornerPanel();
            grayOut.adornerPanel.Order = AdornerOrder.Background;
            grayOut.adornerPanel.SetAdornedElement(designPanel.Context.RootItem.View, null);
            
            var transform = activeContainer.TransformToVisual(grayOut.adornerPanel.AdornedElement);
            var transformMatrix = transform != null ? transform.Value : Matrix.Identity;
            
            grayOut.ActiveAreaGeometry = new RectangleGeometry(activeRectInActiveContainer)
            {
                Transform = new MatrixTransform(transformMatrix)
            };
            
            var tb = new TextBlock { Text = text };
            tb.FontSize = 10;
            tb.ClipToBounds = true;
            tb.Width = activeContainer.Bounds.Width;
            tb.Height = activeContainer.Bounds.Height;
            tb.VerticalAlignment = VerticalAlignment.Top;
            tb.HorizontalAlignment = HorizontalAlignment.Left;
            tb.RenderTransform = new MatrixTransform(transformMatrix);
            grayOut.adornerPanel.Children.Add(tb);

            designPanel.Adorners.Add(grayOut.adornerPanel);
        }
    }

    public static void Stop(ref InfoTextEnterArea grayOut)
    {
        if (grayOut != null)
        {
            var designPanel = grayOut.designPanel;
            var adornerPanelToRemove = grayOut.adornerPanel;
            designPanel.Adorners.Remove(adornerPanelToRemove);
            grayOut = null;
        }
    }
}