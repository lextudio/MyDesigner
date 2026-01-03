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
using Avalonia.Input;
using Avalonia.Media;
using MyDesigner.Design.Extensions;
using MyDesigner.Designer.Services;
using Path = Avalonia.Controls.Shapes.Path;

namespace MyDesigner.Designer.Extensions;

[ExtensionFor(typeof(Canvas))]
[ExtensionFor(typeof(Grid))]
public class DrawPathExtension : BehaviorExtension, IDrawItemExtension
{
    private ChangeGroup changeGroup;

    private DesignItem CreateItem(DesignContext context, Type componentType)
    {
        var newInstance =
            context.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(componentType, null);
        var item = context.Services.Component.RegisterComponentForDesigner(newInstance);
        changeGroup = item.OpenGroup("Draw Line");
        context.Services.ExtensionManager.ApplyDefaultInitializers(item);
        return item;
    }

    private sealed class DrawPathMouseGesture : ClickOrDragMouseGesture
    {
        private readonly PathFigure figure;
        private readonly DesignItem geometry;
        private readonly DesignItem newLine;
        private readonly Point sP;
        private ChangeGroup changeGroup;
        private Matrix matrix;

        public DrawPathMouseGesture(PathFigure figure, DesignItem newLine, IInputElement relativeTo,
            ChangeGroup changeGroup, Transform transform)
        {
            this.newLine = newLine;
            positionRelativeTo = relativeTo;
            this.changeGroup = changeGroup;
            this.figure = figure;
            matrix = transform.Value;
            // Note: In Avalonia, Matrix.Invert() returns bool, need to handle differently
            if (!matrix.TryInvert(out matrix))
                matrix = Matrix.Identity;

            // In Avalonia, we need to get pointer position differently
            sP = new Point(0, 0); // Will be set properly in actual implementation

            geometry = newLine.Properties[Avalonia.Controls.Shapes.Path.DataProperty].Value;
        }

        protected override void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
            base.OnPointerPressed(sender, e);
        }

        protected override void OnPointerMoved(object sender, PointerEventArgs e)
        {
            var delta = matrix.Transform(e.GetPosition(null) - sP);
            var point = new Point(Math.Round(delta.X, 0), Math.Round(delta.Y, 0));

            var segment = figure.Segments.LastOrDefault() as LineSegment;
            if (e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
                if (segment == null || segment.Point != point)
                {
                    figure.Segments.Add(new LineSegment { Point = point });
                    segment = figure.Segments.Last() as LineSegment;
                }

            if (segment != null)
                segment.Point = point;
            
            var prop = geometry.Properties[PathGeometry.FiguresProperty];
            prop.SetValue(prop.TypeConverter.ConvertToInvariantString(figure));
        }

        protected override void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            var delta = matrix.Transform(e.GetPosition(null) - sP);
            var point = new Point(Math.Round(delta.X, 0), Math.Round(delta.Y, 0));

            figure.Segments.Add(new LineSegment { Point = point });
            var prop = geometry.Properties[PathGeometry.FiguresProperty];
            prop.SetValue(prop.TypeConverter.ConvertToInvariantString(figure));
        }

        protected override void OnStopped()
        {
            if (changeGroup != null)
            {
                changeGroup.Abort();
                changeGroup = null;
            }

            if (services.Tool.CurrentTool is CreateComponentTool) services.Tool.CurrentTool = services.Tool.PointerTool;

            newLine.ReapplyAllExtensions();

            base.OnStopped();
        }
    }

    #region IDrawItemBehavior implementation

    public bool CanItemBeDrawn(Type createItemType)
    {
        return createItemType == typeof(Path);
    }

    public void StartDrawItem(DesignItem clickedOn, Type createItemType, IDesignPanel panel, PointerEventArgs e,
        Action<DesignItem> drawItemCallback)
    {
        var createdItem = CreateItem(panel.Context, createItemType);

        var startPoint = e.GetPosition(clickedOn.View);
        var operation = PlacementOperation.TryStartInsertNewComponents(clickedOn,
            new[] { createdItem },
            new[] { new Rect(startPoint.X, startPoint.Y, double.NaN, double.NaN) },
            PlacementType.AddItem);
        if (operation != null)
        {
            createdItem.Services.Selection.SetSelectedComponents(new[] { createdItem });
            operation.Commit();
        }

        createdItem.Properties[Shape.StrokeProperty].SetValue(Brushes.Black);
        createdItem.Properties[Shape.StrokeThicknessProperty].SetValue(2d);
        createdItem.Properties[Shape.StretchProperty].SetValue(Stretch.None);
        if (drawItemCallback != null)
            drawItemCallback(createdItem);

        var figure = new PathFigure();
        var geometry = new PathGeometry();
        var geometryDesignItem = createdItem.Services.Component.RegisterComponentForDesigner(geometry);
        var figureDesignItem = createdItem.Services.Component.RegisterComponentForDesigner(figure);
        createdItem.Properties[Path.DataProperty].SetValue(geometry);
        figureDesignItem.Properties[PathFigure.StartPointProperty].SetValue(new Point(0, 0));

        new DrawPathMouseGesture(figure, createdItem, clickedOn.View, changeGroup,
            ExtendedItem.GetCompleteAppliedTransformationToView()).Start(panel, (PointerPressedEventArgs)e);
    }

    #endregion
}