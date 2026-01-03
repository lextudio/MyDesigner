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

namespace MyDesigner.Designer;

/// <summary>
///     Initializes different behaviors for the Root item.
///     <remarks>Could not be a extension since Root Item can be of any type</remarks>
/// </summary>
public class RootItemBehavior : IRootPlacementBehavior
{
    private DesignItem _rootItem;

    public bool CanPlace(IEnumerable<DesignItem> childItems, PlacementType type, PlacementAlignment position)
    {
        return type == PlacementType.Resize && (position == PlacementAlignment.Right ||
                                                position == PlacementAlignment.BottomRight ||
                                                position == PlacementAlignment.Bottom);
    }

    public void BeginPlacement(PlacementOperation operation)
    {
    }

    public void EndPlacement(PlacementOperation operation)
    {
    }

    public Rect GetPosition(PlacementOperation operation, DesignItem childItem)
    {
        var child = childItem.View;
        return new Rect(0, 0, ModelTools.GetWidth(child), ModelTools.GetHeight(child));
    }

    public void BeforeSetPosition(PlacementOperation operation)
    {
    }

    public void SetPosition(PlacementInformation info)
    {
        var element = info.Item.View;
        var newPosition = info.Bounds;
        if (newPosition.Right != ModelTools.GetWidth(element))
            info.Item.Properties[Layoutable.WidthProperty].SetValue(newPosition.Right);
        if (newPosition.Bottom != ModelTools.GetHeight(element))
            info.Item.Properties[Layoutable.HeightProperty].SetValue(newPosition.Bottom);
    }

    public bool CanLeaveContainer(PlacementOperation operation)
    {
        return false;
    }

    public void LeaveContainer(PlacementOperation operation)
    {
        throw new NotImplementedException();
    }

    public bool CanEnterContainer(PlacementOperation operation, bool shouldAlwaysEnter)
    {
        return false;
    }

    public void EnterContainer(PlacementOperation operation)
    {
        throw new NotImplementedException();
    }

    public Point PlacePoint(Point point)
    {
        return point;
    }

    public void Initialize(DesignContext context)
    {
        Debug.Assert(context.RootItem != null);
        _rootItem = context.RootItem;
        _rootItem.AddBehavior(typeof(IRootPlacementBehavior), this);
    }
}