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
public class DrawLineExtension : BehaviorExtension, IDrawItemExtension
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

    private sealed class DrawLinePointerGesture : ClickOrDragPointerGesture
    {
        private readonly LineHandlerExtension l;
        private ChangeGroup changeGroup;

        public DrawLinePointerGesture(LineHandlerExtension l, IInputElement relativeTo, ChangeGroup changeGroup)
        {
            this.l = l;
            positionRelativeTo = relativeTo as Control;
            this.changeGroup = changeGroup;
        }

        protected override void OnPointerMove(object sender, PointerEventArgs e)
        {
            base.OnPointerMove(sender, e);
            l.DragListener.ExternalPointerMove(e);
        }

        protected override void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            l.DragListener.ExternalStop();
            if (changeGroup != null)
            {
                changeGroup.Commit();
                changeGroup = null;
            }

            base.OnPointerReleased(sender, e);
        }

        protected override void OnStopped()
        {
            if (changeGroup != null)
            {
                changeGroup.Abort();
                changeGroup = null;
            }

            if (services.Services.Tool.CurrentTool is CreateComponentTool) 
                services.Services.Tool.CurrentTool = services.Services.Tool.PointerTool;
            base.OnStopped();
        }
    }

    #region IDrawItemBehavior implementation

    public bool CanItemBeDrawn(Type createItemType)
    {
        return createItemType == typeof(Line);
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

        var lineHandler = createdItem.Extensions.OfType<LineHandlerExtension>().First();
        lineHandler.DragListener.ExternalStart();

        new DrawLinePointerGesture(lineHandler, clickedOn.View, changeGroup).Start(panel, (PointerPressedEventArgs)e);
    }

    #endregion
}