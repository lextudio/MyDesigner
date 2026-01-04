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
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using MyDesigner.Design.Adorners;
using MyDesigner.Designer.Services;

namespace MyDesigner.Designer.Controls;

/// <summary>
///     Gray out everything except a specific area.
/// </summary>
public sealed class GrayOutDesignerExceptActiveArea : Control
{
    private const double MaxOpacity = 0.3;

    private static readonly TimeSpan animationTime = new(2000000);
    private Geometry activeAreaGeometry;
    private AdornerPanel adornerPanel;
    private Geometry combinedGeometry;

    private Rect currentAnimateActiveAreaRectToTarget;
    private IDesignPanel designPanel;
    private Geometry designSurfaceRectangle;

    public GrayOutDesignerExceptActiveArea()
    {
        GrayOutBrush = new SolidColorBrush(Colors.Gray) { Opacity = MaxOpacity };
        IsHitTestVisible = false;
    }

    public IBrush GrayOutBrush { get; set; }

    public Geometry ActiveAreaGeometry
    {
        get => activeAreaGeometry;
        set
        {
            activeAreaGeometry = value;
            combinedGeometry =
                new CombinedGeometry(GeometryCombineMode.Exclude, designSurfaceRectangle, activeAreaGeometry);
        }
    }

    public override void Render(DrawingContext context)
    {
        if (combinedGeometry != null)
            context.DrawGeometry(GrayOutBrush, null, combinedGeometry);
    }

    public void AnimateActiveAreaRectTo(Rect newRect)
    {
        if (newRect.Equals(currentAnimateActiveAreaRectToTarget))
            return;
        
        // In Avalonia, we'll directly set the rectangle instead of animating
        // Animation support can be added later if needed
        if (activeAreaGeometry is RectangleGeometry rectGeometry)
        {
            rectGeometry.Rect = newRect;
        }
        currentAnimateActiveAreaRectToTarget = newRect;
    }

    public static void Start(ref GrayOutDesignerExceptActiveArea grayOut, ServiceContainer services,
        Control activeContainer)
    {
        Debug.Assert(activeContainer != null);
        Start(ref grayOut, services, activeContainer, new Rect(activeContainer.Bounds.Size));
    }

    public static void Start(ref GrayOutDesignerExceptActiveArea grayOut, ServiceContainer services,
        Control activeContainer, Rect activeRectInActiveContainer)
    {
        Debug.Assert(services != null);
        Debug.Assert(activeContainer != null);
        var designPanel = services.GetService<IDesignPanel>() as DesignPanel;
        var optionService = services.GetService<OptionService>();
        if (designPanel != null && grayOut == null && optionService != null &&
            optionService.GrayOutDesignSurfaceExceptParentContainerWhenDragging)
        {
            grayOut = new GrayOutDesignerExceptActiveArea();
            
            // In Avalonia, we need to handle this differently
            var childBorder = designPanel.Child as Border;
            var childSize = childBorder?.Child?.Bounds.Size ?? new Size(800, 600);
            
            grayOut.designSurfaceRectangle = new RectangleGeometry(new Rect(0, 0, childSize.Width, childSize.Height));
            grayOut.designPanel = designPanel;
            grayOut.adornerPanel = new AdornerPanel();
            grayOut.adornerPanel.Order = AdornerOrder.BehindForeground;
            grayOut.adornerPanel.SetAdornedElement(designPanel.Context.RootItem.View, null);
            grayOut.adornerPanel.Children.Add(grayOut);
            
            var transform = activeContainer.TransformToVisual(grayOut.adornerPanel.AdornedElement);
            var transformMatrix = transform != null ? transform.Value : Matrix.Identity;
            
            grayOut.ActiveAreaGeometry = new RectangleGeometry(activeRectInActiveContainer)
            {
                Transform = new MatrixTransform(transformMatrix)
            };
            
            AnimateBrush(grayOut.GrayOutBrush, MaxOpacity);
            designPanel.Adorners.Add(grayOut.adornerPanel);
        }
    }

    private static void AnimateBrush(IBrush brush, double to)
    {
        // Simplified animation - Avalonia animations work differently
        if (brush is SolidColorBrush solidBrush)
        {
            solidBrush.Opacity = to;
        }
    }

    public static void Stop(ref GrayOutDesignerExceptActiveArea grayOut)
    {
        if (grayOut != null)
        {
            AnimateBrush(grayOut.GrayOutBrush, 0);
            var designPanel = grayOut.designPanel;
            var adornerPanelToRemove = grayOut.adornerPanel;
            
            // Immediately remove the adorner panel instead of using a timer
            // This prevents the timer from keeping the application alive
            try
            {
                designPanel?.Adorners?.Remove(adornerPanelToRemove);
            }
            catch
            {
                // Ignore any exceptions during cleanup
            }
            
            grayOut = null;
        }
    }
}