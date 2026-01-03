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
using Avalonia.Media;
using MyDesigner.Design.Adorners;

namespace MyDesigner.Designer.Controls;

/// <summary>
///     Description of MultiPointThumb.
/// </summary>
public class PointThumb : DesignerThumb
{

    protected override Type StyleKeyOverride => typeof(PointThumb);

    // Using a StyledProperty as the backing store for InnerRenderTransform.  This enables animation, styling, binding, etc...
    public static readonly StyledProperty<Transform> InnerRenderTransformProperty =
        AvaloniaProperty.Register<PointThumb, Transform>("InnerRenderTransform");

    // Using a StyledProperty as the backing store for IsEllipse.  This enables animation, styling, binding, etc...
    public static readonly StyledProperty<bool> IsEllipseProperty =
        AvaloniaProperty.Register<PointThumb, bool>("IsEllipse", false);

    // Using a StyledProperty as the backing store for Point.  This enables animation, styling, binding, etc...
    public static readonly StyledProperty<Point> PointProperty =
        AvaloniaProperty.Register<PointThumb, Point>("Point");

    // Using a StyledProperty as the backing store for RelativeToPoint.  This enables animation, styling, binding, etc...
    public static readonly StyledProperty<Point?> RelativeToPointProperty =
        AvaloniaProperty.Register<PointThumb, Point?>("RelativeToPoint");

    static PointThumb()
    {
        PointProperty.Changed.AddClassHandler<PointThumb>(OnPointChanged);
    }

    public PointThumb(Point point)
    {
        AdornerPlacement = new PointPlacementSupport(point);
        Point = point;
    }

    public PointThumb()
    {
        AdornerPlacement = new PointPlacementSupport(Point);
    }

    public Transform InnerRenderTransform
    {
        get => GetValue(InnerRenderTransformProperty);
        set => SetValue(InnerRenderTransformProperty, value);
    }

    public bool IsEllipse
    {
        get => GetValue(IsEllipseProperty);
        set => SetValue(IsEllipseProperty, value);
    }

    public Point Point
    {
        get => GetValue(PointProperty);
        set => SetValue(PointProperty, value);
    }

    public Point? RelativeToPoint
    {
        get => GetValue(RelativeToPointProperty);
        set => SetValue(RelativeToPointProperty, value);
    }

    public AdornerPlacement AdornerPlacement { get; }

    private static void OnPointChanged(PointThumb sender, AvaloniaPropertyChangedEventArgs e)
    {
        var pt = sender;
        ((PointPlacementSupport)pt.AdornerPlacement).p = (Point)e.NewValue;
        pt.ReDraw();
    }

    private class PointPlacementSupport : AdornerPlacement
    {
        public Point p;

        public PointPlacementSupport(Point point)
        {
            p = point;
        }

        public override void Arrange(AdornerPanel panel, Control adorner, Size adornedElementSize)
        {
            adorner.Arrange(new Rect(p.X, p.Y, adornedElementSize.Width, adornedElementSize.Height));
        }
    }
}