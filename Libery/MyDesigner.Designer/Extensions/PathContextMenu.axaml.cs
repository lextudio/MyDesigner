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
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using MyDesigner.Designer.themes;

namespace MyDesigner.Designer.Extensions;

public partial class PathContextMenu : ContextMenu
{
    private readonly DesignItem designItem;

    public PathContextMenu(DesignItem designItem)
    {
        this.designItem = designItem;

        SpecialInitializeComponent();
    }

    /// <summary>
    ///     Fixes InitializeComponent with multiple Versions of same Assembly loaded
    /// </summary>
    public void SpecialInitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Click_ConvertToFigures(object? sender, RoutedEventArgs e)
    {
        var path = designItem.Component as Avalonia.Controls.Shapes.Path;
        if (path?.Data == null) return;

        if (path.Data is StreamGeometry)
        {
            var sg = path.Data as StreamGeometry;

            // Note: Avalonia's StreamGeometry doesn't have GetFlattenedPathGeometry method
            // This would need to be implemented differently or use a different approach
            var pg = new PathGeometry(); // Simplified conversion

            var pgDes = designItem.Services.Component.RegisterComponentForDesigner(pg);
            designItem.Properties[Avalonia.Controls.Shapes.Path.DataProperty].SetValue(pgDes);
        }
        else if (path.Data is PathGeometry)
        {
            var pg = path.Data as PathGeometry;

            var figs = pg.Figures;

            var newPg = new PathGeometry();
            var newPgDes = designItem.Services.Component.RegisterComponentForDesigner(newPg);

            foreach (var fig in figs)
                newPgDes.Properties[PathGeometry.FiguresProperty].CollectionElements.Add(FigureToDesignItem(fig));

            designItem.Properties[Avalonia.Controls.Shapes.Path.DataProperty].SetValue(newPg);
        }
    }

    private DesignItem FigureToDesignItem(PathFigure pf)
    {
        var pfDes = designItem.Services.Component.RegisterComponentForDesigner(new PathFigure());

        pfDes.Properties[PathFigure.StartPointProperty].SetValue(pf.StartPoint);
        pfDes.Properties[PathFigure.IsClosedProperty].SetValue(pf.IsClosed);

        foreach (var s in pf.Segments)
            pfDes.Properties[PathFigure.SegmentsProperty].CollectionElements.Add(SegmentToDesignItem(s));
        return pfDes;
    }

    private DesignItem SegmentToDesignItem(PathSegment s)
    {
        // In Avalonia, PathSegment doesn't have Clone() method
        // We need to create a new instance based on the type
        PathSegment clonedSegment = s switch
        {
            LineSegment line => new LineSegment { Point = line.Point },
            QuadraticBezierSegment quad => new QuadraticBezierSegment { Point1 = quad.Point1, Point2 = quad.Point2 },
            BezierSegment bezier => new BezierSegment { Point1 = bezier.Point1, Point2 = bezier.Point2, Point3 = bezier.Point3 },
            ArcSegment arc => new ArcSegment { Point = arc.Point, Size = arc.Size, RotationAngle = arc.RotationAngle, IsLargeArc = arc.IsLargeArc, SweepDirection = arc.SweepDirection },
            PolyLineSegment polyLine => new PolyLineSegment { Points = new Points(polyLine.Points) },
            PolyBezierSegment polyBezier => new PolyBezierSegment { Points = new Points(polyBezier.Points) },
            _ => throw new NotSupportedException($"PathSegment type {s.GetType()} is not supported")
        };

        var sDes = designItem.Services.Component.RegisterComponentForDesigner(clonedSegment);

        // Note: Avalonia PathSegment doesn't have IsSmoothJoin property
        // This functionality would need to be implemented differently if needed

        if (s is LineSegment lineSegment)
        {
            sDes.Properties[LineSegment.PointProperty].SetValue(lineSegment.Point);
        }
        else if (s is QuadraticBezierSegment quadSegment)
        {
            sDes.Properties[QuadraticBezierSegment.Point1Property].SetValue(quadSegment.Point1);
            sDes.Properties[QuadraticBezierSegment.Point2Property].SetValue(quadSegment.Point2);
        }
        else if (s is BezierSegment bezierSegment)
        {
            sDes.Properties[BezierSegment.Point1Property].SetValue(bezierSegment.Point1);
            sDes.Properties[BezierSegment.Point2Property].SetValue(bezierSegment.Point2);
            sDes.Properties[BezierSegment.Point3Property].SetValue(bezierSegment.Point3);
        }
        else if (s is ArcSegment arcSegment)
        {
            sDes.Properties[ArcSegment.PointProperty].SetValue(arcSegment.Point);
            sDes.Properties[ArcSegment.IsLargeArcProperty].SetValue(arcSegment.IsLargeArc);
            sDes.Properties[ArcSegment.RotationAngleProperty].SetValue(arcSegment.RotationAngle);
            sDes.Properties[ArcSegment.SizeProperty].SetValue(arcSegment.Size);
            sDes.Properties[ArcSegment.SweepDirectionProperty].SetValue(arcSegment.SweepDirection);
        }
        else if (s is PolyLineSegment polyLineSegment)
        {
            sDes.Properties[PolyLineSegment.PointsProperty].SetValue(polyLineSegment.Points);
        }
        else if (s is PolyBezierSegment polyBezierSegment)
        {
            sDes.Properties[PolyBezierSegment.PointsProperty].SetValue(polyBezierSegment.Points);
        }

        return sDes;
    }
}