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

namespace MyDesigner.Designer.Extensions;

[ExtensionFor(typeof(Canvas))]
[ExtensionFor(typeof(Grid))]
public class DrawPolyLineExtension : BehaviorExtension, IDrawItemExtension
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

    private sealed class DrawPolylineMouseGesture : ClickOrDragMouseGesture
    {
        private readonly DesignItem newLine;
        private new readonly Point startPoint;
        private ChangeGroup changeGroup;
        private Point? lastAdded;
        private Matrix matrix;

        public DrawPolylineMouseGesture(DesignItem newLine, IInputElement relativeTo, ChangeGroup changeGroup,
            Transform transform)
        {
            this.newLine = newLine;
            positionRelativeTo = relativeTo;
            this.changeGroup = changeGroup;
            matrix = transform.Value;
            if (!matrix.TryInvert(out matrix))
                matrix = Matrix.Identity;

            startPoint = new Point(0, 0); // Will be set properly in actual implementation
        }

        protected override void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
            base.OnPointerPressed(sender, e);
        }

        protected override void OnPointerMoved(object sender, PointerEventArgs e)
        {
            if (changeGroup == null)
                return;
            var delta = matrix.Transform(e.GetPosition(null) - startPoint);
            var diff = new Vector(delta.X, delta.Y);
            if (lastAdded.HasValue) 
            {
                var lastPoint = lastAdded.Value;
                diff = new Vector(lastPoint.X - delta.X, lastPoint.Y - delta.Y);
            }
            
            var keyModifiers = e.KeyModifiers;
            if (keyModifiers.HasFlag(KeyModifiers.Alt))
            {
                if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                {
                    delta = delta.WithY(0);
                    if (newLine.View is Polyline polyline && polyline.Points.Count > 1)
                        delta = delta.WithY(polyline.Points.Reverse().Skip(1).First().Y);
                    else if (newLine.View is Polygon polygon && polygon.Points.Count > 1)
                        delta = delta.WithY(polygon.Points.Reverse().Skip(1).First().Y);
                }
                else
                {
                    delta = delta.WithX(0);
                    if (newLine.View is Polyline polyline && polyline.Points.Count > 1)
                        delta = delta.WithX(polyline.Points.Reverse().Skip(1).First().X);
                    else if (newLine.View is Polygon polygon && polygon.Points.Count > 1)
                        delta = delta.WithX(polygon.Points.Reverse().Skip(1).First().X);
                }
            }

            var point = new Point(delta.X, delta.Y);

            if (newLine.View is Polyline polylineView)
            {
                if (polylineView.Points.Count <= 1)
                    polylineView.Points.Add(point);
                if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
                    polylineView.Points.RemoveAt(polylineView.Points.Count - 1);
                if (polylineView.Points.Last() != point)
                    polylineView.Points.Add(point);
            }
            else if (newLine.View is Polygon polygonView)
            {
                if (polygonView.Points.Count <= 1)
                    polygonView.Points.Add(point);
                if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
                    polygonView.Points.RemoveAt(polygonView.Points.Count - 1);
                if (polygonView.Points.Last() != point)
                    polygonView.Points.Add(point);
            }
        }

        protected override void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (changeGroup == null)
                return;

            var delta = matrix.Transform(e.GetPosition(null) - startPoint);
            var diff = new Vector(delta.X, delta.Y);
            if (lastAdded.HasValue) 
            {
                var lastPoint = lastAdded.Value;
                diff = new Vector(lastPoint.X - delta.X, lastPoint.Y - delta.Y);
            }
            
            var keyModifiers = e.KeyModifiers;
            if (keyModifiers.HasFlag(KeyModifiers.Alt))
            {
                if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                {
                    delta = delta.WithY(0);
                    if (newLine.View is Polyline polyline && polyline.Points.Count > 1)
                        delta = delta.WithY(polyline.Points.Reverse().Skip(1).First().Y);
                    else if (newLine.View is Polygon polygon && polygon.Points.Count > 1)
                        delta = delta.WithY(polygon.Points.Reverse().Skip(1).First().Y);
                }
                else
                {
                    delta = delta.WithX(0);
                    if (newLine.View is Polyline polyline && polyline.Points.Count > 1)
                        delta = delta.WithX(polyline.Points.Reverse().Skip(1).First().X);
                    else if (newLine.View is Polygon polygon && polygon.Points.Count > 1)
                        delta = delta.WithX(polygon.Points.Reverse().Skip(1).First().X);
                }
            }

            var point = new Point(delta.X, delta.Y);
            lastAdded = point;

            if (newLine.View is Polyline polylineView)
                polylineView.Points.Add(point);
            else if (newLine.View is Polygon polygonView)
                polygonView.Points.Add(point);
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
        return createItemType == typeof(Polyline) || createItemType == typeof(Polygon);
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

        if (createItemType == typeof(Polyline))
            createdItem.Properties[Polyline.PointsProperty].CollectionElements
                .Add(createdItem.Services.Component.RegisterComponentForDesigner(new Point(0, 0)));
        else
            createdItem.Properties[Polygon.PointsProperty].CollectionElements
                .Add(createdItem.Services.Component.RegisterComponentForDesigner(new Point(0, 0)));

        new DrawPolylineMouseGesture(createdItem, clickedOn.View, changeGroup,
            ExtendedItem.GetCompleteAppliedTransformationToView()).Start(panel, (PointerPressedEventArgs)e);
    }

    #endregion
}