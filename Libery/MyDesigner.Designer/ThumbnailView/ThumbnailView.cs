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
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using MyDesigner.Design.UIExtensions;
using MyDesigner.Designer.Controls;

namespace MyDesigner.Designer.ThumbnailView;

public class ThumbnailView : TemplatedControl, INotifyPropertyChanged
{
    public static readonly StyledProperty<DesignSurface> DesignSurfaceProperty =
        AvaloniaProperty.Register<ThumbnailView, DesignSurface>(nameof(DesignSurface));

    private DesignSurface oldSurface;
    private ZoomControl scrollViewer;
    private Canvas zoomCanvas;
    private Thumb zoomThumb;

    public DesignSurface DesignSurface
    {
        get => GetValue(DesignSurfaceProperty);
        set => SetValue(DesignSurfaceProperty, value);
    }

    public ScrollViewer ScrollViewer
    {
        get
        {
            if (DesignSurface != null && scrollViewer == null)
                scrollViewer = DesignSurface.FindDescendantOfType<ZoomControl>();

            return scrollViewer;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if (change.Property == DesignSurfaceProperty)
        {
            OnDesignSurfaceChanged();
        }
    }

    private void OnDesignSurfaceChanged()
    {
        if (oldSurface != null)
            oldSurface.LayoutUpdated -= DesignSurface_LayoutUpdated;

        oldSurface = DesignSurface;
        scrollViewer = null;

        if (DesignSurface != null) 
            DesignSurface.LayoutUpdated += DesignSurface_LayoutUpdated;

        OnPropertyChanged("ScrollViewer");
    }

    private void DesignSurface_LayoutUpdated(object sender, EventArgs e)
    {
        if (scrollViewer == null)
            OnPropertyChanged("ScrollViewer");

        if (scrollViewer != null)
        {
            double scale, xOffset, yOffset;
            InvalidateScale(out scale, out xOffset, out yOffset);

            zoomThumb.Width = scrollViewer.Viewport.Width * scale;
            zoomThumb.Height = scrollViewer.Viewport.Height * scale;
            Canvas.SetLeft(zoomThumb, xOffset + scrollViewer.Offset.X * scale);
            Canvas.SetTop(zoomThumb, yOffset + scrollViewer.Offset.Y * scale);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        zoomThumb = e.NameScope.Find<Thumb>("PART_ZoomThumb");
        zoomCanvas = e.NameScope.Find<Canvas>("PART_ZoomCanvas");

        if (zoomThumb != null)
            zoomThumb.DragDelta += Thumb_DragDelta;
        if (zoomCanvas != null)
            zoomCanvas.PointerPressed += Canvas_PointerPressed;
    }

    private void Canvas_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(zoomCanvas).Properties;
        if (!properties.IsLeftButtonPressed) return;
        
        var pos = e.GetPosition(zoomCanvas);
        var cl = Canvas.GetLeft(zoomThumb);
        var ct = Canvas.GetTop(zoomThumb);

        double scale, xOffset, yOffset;
        InvalidateScale(out scale, out xOffset, out yOffset);
        var dl = pos.X - cl - zoomThumb.Width / 2;
        var dt = pos.Y - ct - zoomThumb.Height / 2;

        var newOffset = new Vector(
            scrollViewer.Offset.X + dl / scale,
            scrollViewer.Offset.Y + dt / scale
        );
        scrollViewer.Offset = newOffset;
    }

    private void Thumb_DragDelta(object sender, VectorEventArgs e)
    {
        if (DesignSurface != null)
            if (scrollViewer != null)
            {
                double scale, xOffset, yOffset;
                InvalidateScale(out scale, out xOffset, out yOffset);

                var newOffset = new Vector(
                    scrollViewer.Offset.X + e.Vector.X / scale,
                    scrollViewer.Offset.Y + e.Vector.Y / scale
                );
                scrollViewer.Offset = newOffset;
            }
    }

    private void InvalidateScale(out double scale, out double xOffset, out double yOffset)
    {
        scale = 1;
        xOffset = 0;
        yOffset = 0;

        if (DesignSurface.DesignContext != null && DesignSurface.DesignContext.RootItem != null)
        {
            var designedElement = DesignSurface.DesignContext.RootItem.Component as Control;

            if (designedElement != null)
            {
                var fac1 = designedElement.DesiredSize.Width / zoomCanvas.Bounds.Width;
                var fac2 = designedElement.DesiredSize.Height / zoomCanvas.Bounds.Height;

                // zoom canvas size
                var x = zoomCanvas.Bounds.Width;
                var y = zoomCanvas.Bounds.Height;

                if (fac1 < fac2)
                {
                    x = designedElement.Bounds.Width / fac2;
                    xOffset = (zoomCanvas.Bounds.Width - x) / 2;
                    yOffset = 0;
                }
                else
                {
                    y = designedElement.Bounds.Height / fac1;
                    xOffset = 0;
                    yOffset = (zoomCanvas.Bounds.Height - y) / 2;
                }

                var w = designedElement.DesiredSize.Width;
                var h = designedElement.DesiredSize.Height;

                var scaleX = x / w;
                var scaleY = y / h;

                scale = scaleX < scaleY ? scaleX : scaleY;

                if (scrollViewer.Viewport.Height > h) yOffset -= (scrollViewer.Viewport.Height - h) / 2 * scale;
                if (scrollViewer.Viewport.Width > w) xOffset -= (scrollViewer.Viewport.Width - w) / 2 * scale;

                xOffset += (x - scale * w) / 2;
                yOffset += (y - scale * h) / 2;
            }
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        var handler = PropertyChanged;
        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
}