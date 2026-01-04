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

using MyDesigner.Designer.Xaml;
using MyDesigner.Designer.Extensions;

namespace MyDesigner.Designer;

/// <summary>
///     Static helper methods for working with the designer DOM.
/// </summary>
public static class ModelTools
{
    /// <summary>
    ///     Compares the positions of a and b in the model file.
    /// </summary>
    public static int ComparePositionInModelFile(DesignItem a, DesignItem b)
    {
        // first remember all parent properties of a
        var aProps = new HashSet<DesignItemProperty>();
        var tmp = a;
        while (tmp != null)
        {
            aProps.Add(tmp.ParentProperty);
            tmp = tmp.Parent;
        }

        // now walk up b's parent tree until a matching property is found
        tmp = b;
        while (tmp != null)
        {
            var prop = tmp.ParentProperty;
            if (aProps.Contains(prop))
            {
                if (prop.IsCollection)
                    return prop.CollectionElements.IndexOf(a).CompareTo(prop.CollectionElements.IndexOf(b));

                return 0;
            }

            tmp = tmp.Parent;
        }

        return 0;
    }

    /// <summary>
    ///     Gets if the specified design item is in the document it belongs to.
    /// </summary>
    /// <returns>True for live objects, false for deleted objects.</returns>
    public static bool IsInDocument(DesignItem item)
    {
        var rootItem = item.Context.RootItem;
        while (item != null)
        {
            if (item == rootItem) return true;
            item = item.Parent;
        }

        return false;
    }

    /// <summary>
    ///     Gets if the specified components can be deleted.
    /// </summary>
    public static bool CanDeleteComponents(ICollection<DesignItem> items)
    {
        var b = PlacementOperation.GetPlacementBehavior(items);
        return b != null
               && b.CanPlace(items, PlacementType.Delete, PlacementAlignment.Center);
    }

    public static bool CanSelectComponent(DesignItem item)
    {
        return item.View != null;
    }

    /// <summary>
    ///     Deletes the specified components from their parent containers.
    ///     If the deleted components are currently selected, they are deselected before they are deleted.
    /// </summary>
    public static void DeleteComponents(ICollection<DesignItem> deleteItems)
    {
        if (deleteItems.Count > 0)
        {
            var changeGroup = deleteItems.First().OpenGroup("Delete Items");
            try
            {
                var itemsGrpParent = deleteItems.GroupBy(x => x.Parent);
                foreach (var itemsList in itemsGrpParent)
                {
                    var items = itemsList.ToList();
                    var parent = items.First().Parent;
                    var operation = PlacementOperation.Start(items, PlacementType.Delete);
                    try
                    {
                        var selectionService = items.First().Services.Selection;
                        selectionService.SetSelectedComponents(items, SelectionTypes.Remove);
                        // if the selection is empty after deleting some components, select the parent of the deleted component
                        if (selectionService.SelectionCount == 0 && !items.Contains(parent))
                            selectionService.SetSelectedComponents(new[] { parent });
                        foreach (var designItem in items) designItem.Name = null;

                        var service = parent.Services.Component as XamlComponentService;
                        foreach (var item in items) service.RaiseComponentRemoved(item);

                        operation.DeleteItemsAndCommit();
                    }
                    catch
                    {
                        operation.Abort();
                        throw;
                    }
                }

                changeGroup.Commit();
            }
            catch
            {
                changeGroup.Abort();
                throw;
            }
        }
    }

    public static void CreateVisualTree(this Control element)
    {
        try
        {
            // Avalonia doesn't have FixedDocument equivalent
            // This is a simplified implementation for Avalonia
            element.Measure(Size.Infinity);
            element.Arrange(new Rect(element.DesiredSize));
        }
        catch (Exception)
        {
        }
    }

    internal static Size GetDefaultSize(DesignItem createdItem)
    {
        var defS = Metadata.GetDefaultSize(createdItem.ComponentType, false);
        if (defS != null)
            return defS.Value;

        CreateVisualTree(createdItem.View);

        var s = createdItem.View.DesiredSize;

        var newS = Metadata.GetDefaultSize(createdItem.ComponentType);

        if (newS.HasValue)
        {
            if (!(s.Width > 5) && newS.Value.Width > 0)
                s = s.WithWidth(newS.Value.Width);

            if (!(s.Height > 5) && newS.Value.Height > 0)
                s = s.WithHeight(newS.Value.Height);
        }

        if (double.IsNaN(s.Width) && GetWidth(createdItem.View) > 0) 
            s = s.WithWidth(GetWidth(createdItem.View));
        if (double.IsNaN(s.Height) && GetHeight(createdItem.View) > 0) 
            s = s.WithHeight(GetHeight(createdItem.View));

        return s;
    }

    public static double GetWidth(Control element)
    {
        var v = element.Width;
        if (double.IsNaN(v))
            return element.Bounds.Width;
        else
            return v;
    }

    public static double GetHeight(Control element)
    {
        var v = element.Height;
        if (double.IsNaN(v))
            return element.Bounds.Height;
        else
            return v;
    }

    public static void Resize(DesignItem item, double newWidth, double newHeight)
    {
        if (newWidth != GetWidth(item.View))
        {
            if (double.IsNaN(newWidth))
                item.Properties.GetProperty(Layoutable.WidthProperty).Reset();
            else
                item.Properties.GetProperty(Layoutable.WidthProperty).SetValue(newWidth);
        }

        if (newHeight != GetHeight(item.View))
        {
            if (double.IsNaN(newHeight))
                item.Properties.GetProperty(Layoutable.HeightProperty).Reset();
            else
                item.Properties.GetProperty(Layoutable.HeightProperty).SetValue(newHeight);
        }
    }
    
    private static ItemPos GetItemPos(PlacementOperation operation, DesignItem designItem)
    {
        var itemPos = new ItemPos { DesignItem = designItem };

        var pos = operation.CurrentContainerBehavior.GetPosition(operation, designItem);
        itemPos.Xmin = pos.X;
        itemPos.Xmax = pos.X + pos.Width;
        itemPos.Ymin = pos.Y;
        itemPos.Ymax = pos.Y + pos.Height;

        return itemPos;
    }

    public static Tuple<DesignItem, Rect> WrapItemsNewContainer(IEnumerable<DesignItem> items, Type containerType,
        bool doInsert = true)
    {
        var collection = items;

        var _context = collection.First().Context as XamlDesignContext;

        var container = collection.First().Parent;

        if (collection.Any(x => x.Parent != container))
            return null;

        //Change Code to use the Placement Operation!
        var placement = container.Extensions.OfType<IPlacementBehavior>().FirstOrDefault();
        if (placement == null)
            return null;

        var operation = PlacementOperation.Start(items.ToList(), PlacementType.Move);

        var newInstance =
            _context.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(containerType, null);
        var newPanel = _context.Services.Component.RegisterComponentForDesigner(newInstance);

        var itemList = new List<ItemPos>();

        int? firstIndex = null;

        foreach (var item in collection)
        {
            itemList.Add(GetItemPos(operation, item));
            
            if (container.Component is Canvas)
            {
                item.Properties.GetAttachedProperty(Canvas.RightProperty).Reset();
                item.Properties.GetAttachedProperty(Canvas.LeftProperty).Reset();
                item.Properties.GetAttachedProperty(Canvas.TopProperty).Reset();
                item.Properties.GetAttachedProperty(Canvas.BottomProperty).Reset();
            }
            else if (container.Component is Grid)
            {
                item.Properties.GetProperty(Layoutable.HorizontalAlignmentProperty).Reset();
                item.Properties.GetProperty(Layoutable.VerticalAlignmentProperty).Reset();
                item.Properties.GetProperty(Layoutable.MarginProperty).Reset();
            }

            if (item.ParentProperty.IsCollection)
            {
                var parCol = item.ParentProperty.CollectionElements;
                if (!firstIndex.HasValue)
                    firstIndex = parCol.IndexOf(item);
                parCol.Remove(item);
            }
            else
            {
                item.ParentProperty.Reset();
            }
        }

        var xmin = itemList.Min(x => x.Xmin);
        var xmax = itemList.Max(x => x.Xmax);
        var ymin = itemList.Min(x => x.Ymin);
        var ymax = itemList.Max(x => x.Ymax);

        foreach (var item in itemList)
            if (newPanel.Component is Canvas)
            {
                if (item.HorizontalAlignment == HorizontalAlignment.Right)
                    item.DesignItem.Properties.GetAttachedProperty(Canvas.RightProperty).SetValue(xmax - item.Xmax);
                else
                    item.DesignItem.Properties.GetAttachedProperty(Canvas.LeftProperty).SetValue(item.Xmin - xmin);

                if (item.VerticalAlignment == VerticalAlignment.Bottom)
                    item.DesignItem.Properties.GetAttachedProperty(Canvas.BottomProperty).SetValue(ymax - item.Ymax);
                else
                    item.DesignItem.Properties.GetAttachedProperty(Canvas.TopProperty).SetValue(item.Ymin - ymin);

                newPanel.ContentProperty.CollectionElements.Add(item.DesignItem);
            }
            else if (newPanel.Component is Grid)
            {
                var thickness = new Thickness(0);
                if (item.HorizontalAlignment == HorizontalAlignment.Right)
                {
                    item.DesignItem.Properties.GetProperty(Layoutable.HorizontalAlignmentProperty)
                        .SetValue(HorizontalAlignment.Right);
                    thickness = thickness.WithRight(xmax - item.Xmax);
                }
                else
                {
                    item.DesignItem.Properties.GetProperty(Layoutable.HorizontalAlignmentProperty)
                        .SetValue(HorizontalAlignment.Left);
                    thickness = thickness.WithLeft(item.Xmin - xmin);
                }

                if (item.VerticalAlignment == VerticalAlignment.Bottom)
                {
                    item.DesignItem.Properties.GetProperty(Layoutable.VerticalAlignmentProperty)
                        .SetValue(VerticalAlignment.Bottom);
                    thickness = thickness.WithBottom(ymax - item.Ymax);
                }
                else
                {
                    item.DesignItem.Properties.GetProperty(Layoutable.VerticalAlignmentProperty)
                        .SetValue(VerticalAlignment.Top);
                    thickness = thickness.WithTop(item.Ymin - ymin);
                }

                item.DesignItem.Properties.GetProperty(Layoutable.MarginProperty).SetValue(thickness);

                newPanel.ContentProperty.CollectionElements.Add(item.DesignItem);
            }
            else if (newPanel.Component is Viewbox)
            {
                newPanel.ContentProperty.SetValue(item.DesignItem);
            }
            else if (newPanel.Component is ContentControl)
            {
                newPanel.ContentProperty.SetValue(item.DesignItem);
            }

        if (doInsert)
        {
            var operation2 = PlacementOperation.TryStartInsertNewComponents(
                container,
                new[] { newPanel },
                new[] { new Rect(xmin, ymin, xmax - xmin, ymax - ymin).Round() },
                PlacementType.AddItem
            );

            if (items.Count() == 1 && container.ContentProperty != null && container.ContentProperty.IsCollection)
            {
                container.ContentProperty.CollectionElements.Remove(newPanel);
                container.ContentProperty.CollectionElements.Insert(firstIndex.Value, newPanel);
            }

            operation2.Commit();

            _context.Services.Selection.SetSelectedComponents(new[] { newPanel });
        }

        operation.Commit();

        return new Tuple<DesignItem, Rect>(newPanel, new Rect(xmin, ymin, xmax - xmin, ymax - ymin).Round());
    }

    public static void ApplyTransform(DesignItem designItem, Transform transform, bool relative = true,
        AvaloniaProperty transformProperty = null)
    {
        var changeGroup = designItem.OpenGroup("Apply Transform");

        transformProperty = transformProperty ?? Visual.RenderTransformProperty;
        Transform oldTransform = null;
        if (designItem.Properties.GetProperty(transformProperty).IsSet)
            oldTransform = designItem.Properties.GetProperty(transformProperty)
                .GetConvertedValueOnInstance<Transform>();

        if (oldTransform is MatrixTransform)
        {
            var mt = oldTransform as MatrixTransform;
            var tg = new TransformGroup();
            if (mt.Matrix.M31 != 0 && mt.Matrix.M32 != 0)
                tg.Children.Add(new TranslateTransform(mt.Matrix.M31, mt.Matrix.M32));
            if (mt.Matrix.M11 != 0 && mt.Matrix.M22 != 0)
                tg.Children.Add(new ScaleTransform(mt.Matrix.M11, mt.Matrix.M22));

            var angle = Math.Atan2(mt.Matrix.M21, mt.Matrix.M11) * 180 / Math.PI;
            if (angle != 0)
                tg.Children.Add(new RotateTransform(angle));
        }
        else if (oldTransform != null && oldTransform.GetType() != transform.GetType())
        {
            var tg = new TransformGroup();
            var tgDes = designItem.Services.Component.RegisterComponentForDesigner(tg);
            tgDes.ContentProperty.CollectionElements.Add(designItem.Services.Component.GetDesignItem(oldTransform));
            designItem.Properties.GetProperty(Visual.RenderTransformProperty).SetValue(tg);
            oldTransform = tg;
        }

        if (transform is RotateTransform)
        {
            var rotateTransform = transform as RotateTransform;

            if (oldTransform is RotateTransform || oldTransform == null)
            {
                if (rotateTransform.Angle != 0)
                {
                    designItem.Properties.GetProperty(transformProperty).SetValue(transform);
                    var angle = rotateTransform.Angle;
                    if (relative && oldTransform != null)
                        angle = rotateTransform.Angle + ((RotateTransform)oldTransform).Angle;
                    designItem.Properties.GetProperty(transformProperty).Value.Properties
                        .GetProperty(RotateTransform.AngleProperty).SetValue(angle);

                    if (oldTransform == null)
                        designItem.Properties.GetProperty(Visual.RenderTransformOriginProperty)
                            .SetValue(new RelativePoint(0.5, 0.5, RelativeUnit.Relative));
                }
                else
                {
                    designItem.Properties.GetProperty(transformProperty).Reset();
                    designItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).Reset();
                }
            }
            else if (oldTransform is TransformGroup)
            {
                var tg = oldTransform as TransformGroup;
                var rot = tg.Children.FirstOrDefault(x => x is RotateTransform);
                if (rot != null)
                    designItem.Services.Component.GetDesignItem(tg).ContentProperty.CollectionElements
                        .Remove(designItem.Services.Component.GetDesignItem(rot));
                if (rotateTransform.Angle != 0)
                {
                    var des = designItem.Services.Component.GetDesignItem(transform);
                    if (des == null)
                        des = designItem.Services.Component.RegisterComponentForDesigner(transform);
                    designItem.Services.Component.GetDesignItem(tg).ContentProperty.CollectionElements.Add(des);
                    if (oldTransform == null)
                        designItem.Properties.GetProperty(Visual.RenderTransformOriginProperty)
                            .SetValue(new RelativePoint(0.5, 0.5, RelativeUnit.Relative));
                }
            }
            else
            {
                if (rotateTransform.Angle != 0)
                {
                    designItem.Properties.GetProperty(transformProperty).SetValue(transform);
                    if (oldTransform == null)
                        designItem.Properties.GetProperty(Visual.RenderTransformOriginProperty)
                            .SetValue(new RelativePoint(0.5, 0.5, RelativeUnit.Relative));
                }
            }
        }

        ((DesignPanel)designItem.Services.DesignPanel).AdornerLayer.UpdateAdornersForElement(designItem.View, true);

        changeGroup.Commit();
    }

    public static void StretchItems(IEnumerable<DesignItem> items, StretchDirection stretchDirection)
    {
        var collection = items;

        var container = collection.First().Parent;

        if (collection.Any(x => x.Parent != container))
            return;

        var placement = container.Extensions.OfType<IPlacementBehavior>().FirstOrDefault();
        if (placement == null)
            return;

        var changeGroup = container.OpenGroup("StretchItems");

        var w = GetWidth(collection.First().View);
        var h = GetHeight(collection.First().View);

        foreach (var item in collection.Skip(1))
            switch (stretchDirection)
            {
                case StretchDirection.Width:
                {
                    if (!double.IsNaN(w))
                        item.Properties.GetProperty(Layoutable.WidthProperty).SetValue(w);
                }
                    break;
                case StretchDirection.Height:
                {
                    if (!double.IsNaN(h))
                        item.Properties.GetProperty(Layoutable.HeightProperty).SetValue(h);
                }
                    break;
            }

        changeGroup.Commit();
    }

    public static void ArrangeItems(IEnumerable<DesignItem> items, ArrangeDirection arrangeDirection)
    {
        var collection = items;

        var _context = collection.First().Context as XamlDesignContext;

        var container = collection.First().Parent;

        if (collection.Any(x => x.Parent != container))
            return;

        var placement = container.Extensions.OfType<IPlacementBehavior>().FirstOrDefault();
        if (placement == null)
            return;

        var operation = PlacementOperation.Start(items.ToList(), PlacementType.Move);

        var itemList = new List<ItemPos>();
        foreach (var item in collection) itemList.Add(GetItemPos(operation, item));

        var xmin = itemList.Min(x => x.Xmin);
        var xmax = itemList.Max(x => x.Xmax);
        var mpos = (xmax - xmin) / 2 + xmin;
        var ymin = itemList.Min(x => x.Ymin);
        var ymax = itemList.Max(x => x.Ymax);
        var ympos = (ymax - ymin) / 2 + ymin;

        foreach (var item in collection)
            switch (arrangeDirection)
            {
                case ArrangeDirection.Left:
                {
                    if (container.Component is Canvas)
                    {
                        if (!item.Properties.GetAttachedProperty(Canvas.RightProperty).IsSet)
                        {
                            item.Properties.GetAttachedProperty(Canvas.LeftProperty).SetValue(xmin);
                        }
                        else
                        {
                            var pos = ((Panel)item.Parent.Component).Bounds.Width -
                                      (xmin + ((Control)item.Component).Bounds.Width);
                            item.Properties.GetAttachedProperty(Canvas.RightProperty).SetValue(pos);
                        }
                    }
                    else if (container.Component is Grid)
                    {
                        if (item.Properties.GetProperty(Layoutable.HorizontalAlignmentProperty)
                                .GetConvertedValueOnInstance<HorizontalAlignment>() != HorizontalAlignment.Right)
                        {
                            var margin = item.Properties.GetProperty(Layoutable.MarginProperty)
                                .GetConvertedValueOnInstance<Thickness>();
                            margin = margin.WithLeft(xmin);
                            item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                        }
                        else
                        {
                            var pos = ((Panel)item.Parent.Component).Bounds.Width -
                                      (xmin + ((Control)item.Component).Bounds.Width);
                            var margin = item.Properties.GetProperty(Layoutable.MarginProperty)
                                .GetConvertedValueOnInstance<Thickness>();
                            margin = margin.WithRight(pos);
                            item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                        }
                    }
                }
                    break;
                case ArrangeDirection.HorizontalMiddle:
                {
                    if (container.Component is Canvas)
                    {
                        if (!item.Properties.GetAttachedProperty(Canvas.RightProperty).IsSet)
                        {
                            item.Properties.GetAttachedProperty(Canvas.LeftProperty)
                                .SetValue(mpos - ((Control)item.Component).Bounds.Width / 2);
                        }
                        else
                        {
                            var pp = mpos - ((Control)item.Component).Bounds.Width / 2;
                            var pos = ((Panel)item.Parent.Component).Bounds.Width - pp -
                                      ((Control)item.Component).Bounds.Width;
                            item.Properties.GetAttachedProperty(Canvas.RightProperty).SetValue(pos);
                        }
                    }
                    else if (container.Component is Grid)
                    {
                        if (item.Properties.GetProperty(Layoutable.HorizontalAlignmentProperty)
                                .GetConvertedValueOnInstance<HorizontalAlignment>() != HorizontalAlignment.Right)
                        {
                            var margin = item.Properties.GetProperty(Layoutable.MarginProperty)
                                .GetConvertedValueOnInstance<Thickness>();
                            margin = margin.WithLeft(mpos - ((Control)item.Component).Bounds.Width / 2);
                            item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                        }
                        else
                        {
                            var pp = mpos - ((Control)item.Component).Bounds.Width / 2;
                            var pos = ((Panel)item.Parent.Component).Bounds.Width - pp -
                                      ((Control)item.Component).Bounds.Width;
                            var margin = item.Properties.GetProperty(Layoutable.MarginProperty)
                                .GetConvertedValueOnInstance<Thickness>();
                            margin = margin.WithRight(pos);
                            item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                        }
                    }
                }
                    break;
                case ArrangeDirection.Right:
                {
                    if (container.Component is Canvas)
                    {
                        if (!item.Properties.GetAttachedProperty(Canvas.RightProperty).IsSet)
                        {
                            var pos = xmax - ((Control)item.Component).Bounds.Width;
                            item.Properties.GetAttachedProperty(Canvas.LeftProperty).SetValue(pos);
                        }
                        else
                        {
                            var pos = ((Panel)item.Parent.Component).Bounds.Width - xmax;
                            item.Properties.GetAttachedProperty(Canvas.RightProperty).SetValue(pos);
                        }
                    }
                    else if (container.Component is Grid)
                    {
                        if (item.Properties.GetProperty(Layoutable.HorizontalAlignmentProperty)
                                .GetConvertedValueOnInstance<HorizontalAlignment>() != HorizontalAlignment.Right)
                        {
                            var pos = xmax - ((Control)item.Component).Bounds.Width;
                            var margin = item.Properties.GetProperty(Layoutable.MarginProperty)
                                .GetConvertedValueOnInstance<Thickness>();
                            margin = margin.WithLeft(pos);
                            item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                        }
                        else
                        {
                            var pos = ((Panel)item.Parent.Component).Bounds.Width - xmax;
                            var margin = item.Properties.GetProperty(Layoutable.MarginProperty)
                                .GetConvertedValueOnInstance<Thickness>();
                            margin = margin.WithRight(pos);
                            item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                        }
                    }
                }
                    break;
                case ArrangeDirection.Top:
                {
                    if (container.Component is Canvas)
                    {
                        if (!item.Properties.GetAttachedProperty(Canvas.BottomProperty).IsSet)
                        {
                            item.Properties.GetAttachedProperty(Canvas.TopProperty).SetValue(ymin);
                        }
                        else
                        {
                            var pos = ((Panel)item.Parent.Component).Bounds.Height -
                                      (ymin + ((Control)item.Component).Bounds.Height);
                            item.Properties.GetAttachedProperty(Canvas.BottomProperty).SetValue(pos);
                        }
                    }
                    else if (container.Component is Grid)
                    {
                        if (item.Properties.GetProperty(Layoutable.VerticalAlignmentProperty)
                                .GetConvertedValueOnInstance<VerticalAlignment>() != VerticalAlignment.Bottom)
                        {
                            var margin = item.Properties.GetProperty(Layoutable.MarginProperty)
                                .GetConvertedValueOnInstance<Thickness>();
                            margin = margin.WithTop(ymin);
                            item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                        }
                        else
                        {
                            var pos = ((Panel)item.Parent.Component).Bounds.Height -
                                      (ymin + ((Control)item.Component).Bounds.Height);
                            var margin = item.Properties.GetProperty(Layoutable.MarginProperty)
                                .GetConvertedValueOnInstance<Thickness>();
                            margin = margin.WithBottom(pos);
                            item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                        }
                    }
                }
                    break;
                case ArrangeDirection.VerticalMiddle:
                {
                    if (container.Component is Canvas)
                    {
                        if (!item.Properties.GetAttachedProperty(Canvas.BottomProperty).IsSet)
                        {
                            item.Properties.GetAttachedProperty(Canvas.TopProperty)
                                .SetValue(ympos - ((Control)item.Component).Bounds.Height / 2);
                        }
                        else
                        {
                            var pp = ympos - ((Control)item.Component).Bounds.Height / 2;
                            var pos = ((Panel)item.Parent.Component).Bounds.Height - pp -
                                      ((Control)item.Component).Bounds.Height;
                            item.Properties.GetAttachedProperty(Canvas.BottomProperty).SetValue(pos);
                        }
                    }
                    else if (container.Component is Grid)
                    {
                        if (item.Properties.GetProperty(Layoutable.VerticalAlignmentProperty)
                                .GetConvertedValueOnInstance<VerticalAlignment>() != VerticalAlignment.Bottom)
                        {
                            var margin = item.Properties.GetProperty(Layoutable.MarginProperty)
                                .GetConvertedValueOnInstance<Thickness>();
                            margin = margin.WithTop(ympos - ((Control)item.Component).Bounds.Height / 2);
                            item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                        }
                        else
                        {
                            var pp = ympos - ((Control)item.Component).Bounds.Height / 2;
                            var pos = ((Panel)item.Parent.Component).Bounds.Height - pp -
                                      ((Control)item.Component).Bounds.Height;
                            var margin = item.Properties.GetProperty(Layoutable.MarginProperty)
                                .GetConvertedValueOnInstance<Thickness>();
                            margin = margin.WithBottom(pos);
                            item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                        }
                    }
                }
                    break;
                case ArrangeDirection.Bottom:
                {
                    if (container.Component is Canvas)
                    {
                        if (!item.Properties.GetAttachedProperty(Canvas.BottomProperty).IsSet)
                        {
                            var pos = ymax - ((Control)item.Component).Bounds.Height;
                            item.Properties.GetAttachedProperty(Canvas.TopProperty).SetValue(pos);
                        }
                        else
                        {
                            var pos = ((Panel)item.Parent.Component).Bounds.Height - ymax;
                            item.Properties.GetAttachedProperty(Canvas.BottomProperty).SetValue(pos);
                        }
                    }
                    else if (container.Component is Grid)
                    {
                        if (item.Properties.GetProperty(Layoutable.VerticalAlignmentProperty)
                                .GetConvertedValueOnInstance<VerticalAlignment>() != VerticalAlignment.Bottom)
                        {
                            var pos = ymax - ((Control)item.Component).Bounds.Height;
                            var margin = item.Properties.GetProperty(Layoutable.MarginProperty)
                                .GetConvertedValueOnInstance<Thickness>();
                            margin = margin.WithTop(pos);
                            item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                        }
                        else
                        {
                            var pos = ((Panel)item.Parent.Component).Bounds.Height - ymax;
                            var margin = item.Properties.GetProperty(Layoutable.MarginProperty)
                                .GetConvertedValueOnInstance<Thickness>();
                            margin = margin.WithBottom(pos);
                            item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                        }
                    }
                }
                    break;
            }

        operation.Commit();
    }

    private class ItemPos
    {
        public HorizontalAlignment HorizontalAlignment { get; set; }

        public VerticalAlignment VerticalAlignment { get; set; }

        public double Xmin { get; set; }

        public double Xmax { get; set; }

        public double Ymin { get; set; }

        public double Ymax { get; set; }

        public DesignItem DesignItem { get; set; }
    }

    public static void UnwrapItemsFromContainer(DesignItem container)
    {
        var collection = container.ContentProperty.CollectionElements.ToList();

        var newPanel = container.Parent;

        if (collection.Any(x => x.Parent != container))
            return;

        //Change Code to use the Placement Operation!
        var placement = container.Extensions.OfType<IPlacementBehavior>().FirstOrDefault();
        if (placement == null)
            return;

        var operation = PlacementOperation.Start(collection.ToList(), PlacementType.Move);

        var itemList = new List<ItemPos>();

        int? firstIndex = null;

        var containerPos = GetItemPos(operation, container);

        foreach (var item in collection)
        {
            itemList.Add(GetItemPos(operation, item));
            if (container.Component is Canvas)
            {
                item.Properties.GetAttachedProperty(Canvas.RightProperty).Reset();
                item.Properties.GetAttachedProperty(Canvas.LeftProperty).Reset();
                item.Properties.GetAttachedProperty(Canvas.TopProperty).Reset();
                item.Properties.GetAttachedProperty(Canvas.BottomProperty).Reset();
            }
            else if (container.Component is Grid)
            {
                item.Properties.GetProperty(Control.HorizontalAlignmentProperty).Reset();
                item.Properties.GetProperty(Control.VerticalAlignmentProperty).Reset();
                item.Properties.GetProperty(Layoutable.MarginProperty).Reset();
            }

            if (item.ParentProperty.IsCollection)
            {
                var parCol = item.ParentProperty.CollectionElements;
                if (!firstIndex.HasValue)
                    firstIndex = parCol.IndexOf(item);
                parCol.Remove(item);
            }
            else
            {
                item.ParentProperty.Reset();
            }
        }

        newPanel.ContentProperty.CollectionElements.Remove(container);

        foreach (var item in itemList)
            if (newPanel.Component is Canvas)
            {
                if (item.HorizontalAlignment == HorizontalAlignment.Right)
                    item.DesignItem.Properties.GetAttachedProperty(Canvas.RightProperty)
                        .SetValue(containerPos.Xmax - item.Xmax);
                else
                    item.DesignItem.Properties.GetAttachedProperty(Canvas.LeftProperty)
                        .SetValue(containerPos.Xmin + item.Xmin);

                if (item.VerticalAlignment == VerticalAlignment.Bottom)
                    item.DesignItem.Properties.GetAttachedProperty(Canvas.BottomProperty)
                        .SetValue(containerPos.Ymax - item.Ymax);
                else
                    item.DesignItem.Properties.GetAttachedProperty(Canvas.TopProperty)
                        .SetValue(containerPos.Ymin + item.Ymin);
            }
            else if (newPanel.Component is Grid)
            {
                item.DesignItem.Properties.GetProperty(Control.HorizontalAlignmentProperty)
                    .SetValue(item.HorizontalAlignment);
                item.DesignItem.Properties.GetProperty(Control.VerticalAlignmentProperty)
                    .SetValue(item.VerticalAlignment);
                item.DesignItem.Properties.GetProperty(Layoutable.MarginProperty)
                    .SetValue(new Thickness(containerPos.Xmin + item.Xmin, containerPos.Ymin + item.Ymin, 0, 0));
            }
            else
            {
                if (firstIndex.HasValue)
                    newPanel.ContentProperty.CollectionElements.Insert(firstIndex.Value, item.DesignItem);
                else
                    newPanel.ContentProperty.CollectionElements.Add(item.DesignItem);
                if (firstIndex.HasValue) firstIndex++;
            }

        operation.Commit();
    }
}