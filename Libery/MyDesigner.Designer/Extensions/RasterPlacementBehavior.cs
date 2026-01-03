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
using Avalonia.Controls.Shapes;
using MyDesigner.Design.Adorners;

namespace MyDesigner.Designer.Extensions;

public class RasterPlacementBehavior : DefaultPlacementBehavior
{
    private AdornerPanel adornerPanel;
    private int raster = 5;
    private bool rasterDrawn;
    private Canvas surface;

    public override void BeginPlacement(PlacementOperation operation)
    {
        base.BeginPlacement(operation);

        var designPanel = ExtendedItem.Services.DesignPanel as DesignPanel;
        if (designPanel != null)
            raster = designPanel.RasterWidth;

        CreateSurface(operation);
    }

    public override void EndPlacement(PlacementOperation operation)
    {
        base.EndPlacement(operation);
        DeleteSurface();
    }

    public override void EnterContainer(PlacementOperation operation)
    {
        base.EnterContainer(operation);
        CreateSurface(operation);
    }

    public override void LeaveContainer(PlacementOperation operation)
    {
        base.LeaveContainer(operation);
        DeleteSurface();
    }

    private void CreateSurface(PlacementOperation operation)
    {
        if (ExtendedItem.Services.GetService<IDesignPanel>() != null)
        {
            surface = new Canvas();
            adornerPanel = new AdornerPanel();

            adornerPanel.SetAdornedElement(ExtendedItem.View, ExtendedItem);
            AdornerPanel.SetPlacement(surface, AdornerPlacement.FillContent);
            adornerPanel.Children.Add(surface);

            ExtendedItem.Services.DesignPanel.Adorners.Add(adornerPanel);
        }
    }

    private void DeleteSurface()
    {
        rasterDrawn = false;
        
        if (surface != null)
        {
            ExtendedItem.Services.DesignPanel.Adorners.Remove(adornerPanel);
            adornerPanel = null;
            surface = null;
        }
    }

    private bool IsKeyDown(Key key)
    {
        var topLevel = TopLevel.GetTopLevel(ExtendedItem.View as Visual);
        return topLevel?.IsKeyDown(key) == true;
    }

    public override void BeforeSetPosition(PlacementOperation operation)
    {
        base.BeforeSetPosition(operation);
        if (surface == null) return;

        var designPanel = ExtendedItem.Services.DesignPanel as DesignPanel;
        if (designPanel == null || !designPanel.UseRasterPlacement)
            return;

        if (IsKeyDown(Key.LeftCtrl))
        {
            surface.Children.Clear();
            rasterDrawn = false;
            return;
        }

        drawRaster();

        var bounds = operation.PlacedItems[0].Bounds;
        bounds = new Rect(
            (int)bounds.Y / raster * raster,
            (int)bounds.X / raster * raster,
            Convert.ToInt32(bounds.Width / raster) * raster,
            Convert.ToInt32(bounds.Height / raster) * raster
        );
        operation.PlacedItems[0].Bounds = bounds;
    }

    public override Point PlacePoint(Point point)
    {
        if (surface == null)
            return base.PlacePoint(point);

        var designPanel = ExtendedItem.Services.DesignPanel as DesignPanel;
        if (designPanel == null || !designPanel.UseRasterPlacement)
            return base.PlacePoint(point);

        if (IsKeyDown(Key.LeftCtrl))
        {
            surface.Children.Clear();
            rasterDrawn = false;
            return base.PlacePoint(point);
        }

        drawRaster();

        point = new Point(
            (int)point.X / raster * raster,
            (int)point.Y / raster * raster
        );

        return point;
    }

    private void drawRaster()
    {
        rasterDrawn = true;
    }
}