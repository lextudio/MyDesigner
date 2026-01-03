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
using MyDesigner.Design.Extensions;
using MyDesigner.Designer.Controls;
using MyDesigner.XamlDom;

namespace MyDesigner.Designer.Extensions;

[ExtensionFor(typeof(Panel))]
[ExtensionFor(typeof(Control))]
[ExtensionFor(typeof(Border))]
[ExtensionFor(typeof(Viewbox))]
public class DefaultPlacementBehavior : BehaviorExtension, IPlacementBehavior
{
    private static InfoTextEnterArea infoTextEnterArea;

    static DefaultPlacementBehavior()
    {
    }

    public virtual bool CanPlace(IEnumerable<DesignItem> childItems, PlacementType type, PlacementAlignment position)
    {
        return true;
    }

    public virtual void BeginPlacement(PlacementOperation operation)
    {
    }

    public virtual void EndPlacement(PlacementOperation operation)
    {
        InfoTextEnterArea.Stop(ref infoTextEnterArea);
    }

    public virtual Rect GetPosition(PlacementOperation operation, DesignItem item)
    {
        return GetPositionRelativeToContainer(operation, item);
    }

    public virtual void BeforeSetPosition(PlacementOperation operation)
    {
    }

    public virtual void SetPosition(PlacementInformation info)
    {
        if (info.Operation.Type != PlacementType.Move
            && info.Operation.Type != PlacementType.MovePoint
            && info.Operation.Type != PlacementType.MoveAndIgnoreOtherContainers)
            ModelTools.Resize(info.Item, info.Bounds.Width, info.Bounds.Height);

        //if (info.Operation.Type == PlacementType.MovePoint)
        //	ModelTools.Resize(info.Item, info.Bounds.Width, info.Bounds.Height);
    }

    public virtual bool CanLeaveContainer(PlacementOperation operation)
    {
        return true;
    }

    public virtual void LeaveContainer(PlacementOperation operation)
    {
        foreach (var info in operation.PlacedItems)
        {
            var parentProperty = info.Item.ParentProperty;
            if (parentProperty.IsCollection)
                parentProperty.CollectionElements.Remove(info.Item);
            else
                parentProperty.Reset();
        }
    }

    public virtual bool CanEnterContainer(PlacementOperation operation, bool shouldAlwaysEnter)
    {
        var canEnter = internalCanEnterContainer(operation);

        if (canEnter && !shouldAlwaysEnter && !IsKeyDown(Key.LeftAlt) && !IsKeyDown(Key.RightAlt))
        {
            var element = ExtendedItem.View as Control;
            if (element != null)
            {
                var b = new Rect(0, 0, element.Bounds.Width, element.Bounds.Height);
                InfoTextEnterArea.Start(ref infoTextEnterArea, Services, ExtendedItem.View, b,
                    Translations.Instance.PressAltText);
            }

            return false;
        }

        return canEnter;
    }

    public virtual void EnterContainer(PlacementOperation operation)
    {
        if (ExtendedItem.ContentProperty.IsCollection)
            foreach (var info in operation.PlacedItems)
                ExtendedItem.ContentProperty.CollectionElements.Add(info.Item);
        else
            ExtendedItem.ContentProperty.SetValue(operation.PlacedItems[0].Item);

        if (operation.Type == PlacementType.AddItem)
            foreach (var info in operation.PlacedItems)
                SetPosition(info);
    }

    public virtual Point PlacePoint(Point point)
    {
        return new Point(Math.Round(point.X), Math.Round(point.Y));
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (ExtendedItem.ContentProperty == null ||
            Metadata.IsPlacementDisabled(ExtendedItem.ComponentType))
            return;
        ExtendedItem.AddBehavior(typeof(IPlacementBehavior), this);
    }

    public Rect GetPositionRelativeToContainer(PlacementOperation operation, DesignItem item)
    {
        if (item.View == null)
            return RectExtensions.Empty;
        
        var transform = item.View.TransformToVisual(operation.CurrentContainer.View);
        var p = transform?.Transform(new Point()) ?? new Point();

        return new Rect(p, PlacementOperation.GetRealElementSize(item.View));
    }

    public virtual bool CanPlaceItem(PlacementInformation info)
    {
        return true;
    }

    private bool IsKeyDown(Key key)
    {
        // In Avalonia, checking key state is different
        var topLevel = TopLevel.GetTopLevel(ExtendedItem.View as Visual);
        return topLevel?.IsKeyDown(key) == true;
    }

    private bool internalCanEnterContainer(PlacementOperation operation)
    {
        InfoTextEnterArea.Stop(ref infoTextEnterArea);

        if (ExtendedItem.Component is Expander)
            if (!((Expander)ExtendedItem.Component).IsExpanded)
                ((Expander)ExtendedItem.Component).IsExpanded = true;

        if (ExtendedItem.Component is UserControl && ExtendedItem.ComponentType != typeof(UserControl))
            return false;

        if (ExtendedItem.Component is Decorator)
            return ((Decorator)ExtendedItem.Component).Child == null;

        if (ExtendedItem.ContentProperty.IsCollection)
            return CollectionSupport.CanCollectionAdd(ExtendedItem.ContentProperty.ReturnType,
                operation.PlacedItems.Select(p => p.Item.Component));

        if (ExtendedItem.ContentProperty.ReturnType == typeof(string))
            return false;

        if (!ExtendedItem.ContentProperty.IsSet)
            return true;

        var value = ExtendedItem.ContentProperty.GetConvertedValueOnInstance<object>();
        // don't overwrite non-primitive values like bindings
        return ExtendedItem.ContentProperty.Value == null && value is string && string.IsNullOrEmpty(value as string);
    }
}