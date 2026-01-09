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
using Avalonia.VisualTree;

namespace MyDesigner.Designer.OutlineView;

// limitations:
// - Do not use ItemsSource (use Root)
// - Do not use Items (use Root)
public class DragTreeView : TreeView
{
    
    
    protected override Type StyleKeyOverride => typeof(DragTreeView);
    
    public static readonly StyledProperty<object> RootProperty =
        AvaloniaProperty.Register<DragTreeView, object>(nameof(Root));

    private bool canDrop;
    private DragTreeViewItem dropAfter;
    private bool dropCopy;
    private bool dropInside;

    private DragTreeViewItem dropTarget;

    private Border insertLine;
    private int part;

    internal HashSet<DragTreeViewItem> Selection = new();
    private DragTreeViewItem treeItem;
    private DragTreeViewItem upSelection;

    public DragTreeView()
    {
        DragDrop.SetAllowDrop(this, true);
        new DragListener(this).DragStarted += DragTreeView_DragStarted;
    }

    public object Root
    {
        get => GetValue(RootProperty);
        set => SetValue(RootProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RootProperty) 
            ItemsSource = new[] { Root };
        else if (change.Property == FilterProperty)
        {
            var ev = FilterChanged;
            if (ev != null)
                ev(Filter);
        }
    }

    private void DragTreeView_DragStarted(object sender, PointerPressedEventArgs e)
    {
        // In Avalonia, drag and drop is handled differently
        // This would need to be implemented using Avalonia's DragDrop API
        // DragDrop.DoDragDrop(this, this, DragDropEffects.All);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        insertLine = e.NameScope.Find<Border>("PART_InsertLine");
    }

    protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
    {
        return new DragTreeViewItem();
    }

    protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
    {
        recycleKey = null;
        return !(item is DragTreeViewItem);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        ProcessDrag(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        ProcessDrag(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        HideDropMarker();
    }

    private void PrepareDropInfo(PointerEventArgs e)
    {
        dropTarget = null;
        dropAfter = null;
        treeItem = (e.Source as Visual)?.FindAncestorOfType<DragTreeViewItem>();

        if (treeItem != null)
        {
            var parent = treeItem.GetLogicalParent() as DragTreeViewItem;
            var header = treeItem.HeaderPresenter;
            if (header != null)
            {
                var p = e.GetPosition(header);
                part = (int)(p.Y / (header.Bounds.Height / 3));
                dropCopy = e.KeyModifiers.HasFlag(KeyModifiers.Control);
                dropInside = false;

                if (part == 1 || parent == null)
                {
                    dropTarget = treeItem;
                    dropInside = true;
                    if (treeItem.ItemCount > 0)
                        dropAfter = treeItem.ContainerFromIndex(treeItem.ItemCount - 1) as DragTreeViewItem;
                }
                else if (part == 0)
                {
                    dropTarget = parent;
                    var index = dropTarget?.IndexFromContainer(treeItem) ?? -1;
                    if (index > 0)
                        dropAfter = dropTarget?.ContainerFromIndex(index - 1) as DragTreeViewItem;
                }
                else
                {
                    dropTarget = parent;
                    dropAfter = treeItem;
                }
            }
        }
    }

    private void ProcessDrag(PointerEventArgs e)
    {
        canDrop = false;

        HideDropMarker();
        PrepareDropInfo(e);

        if (dropTarget != null && CanInsertInternal())
        {
            canDrop = true;
            DrawDropMarker();
        }
    }

    private void DrawDropMarker()
    {
        if (dropInside)
        {
            dropTarget.IsDragHover = true;
        }
        else if (insertLine != null && treeItem?.HeaderPresenter != null)
        {
            var header = treeItem.HeaderPresenter;
            var transform = header.TransformToVisual(this);
            var p = transform?.Transform(new Point(0, part == 0 ? 0 : header.Bounds.Height)) ?? new Point();

            insertLine.IsVisible = true;
            insertLine.Margin = new Thickness(p.X, p.Y, 0, 0);
        }
    }

    private void HideDropMarker()
    {
        if (insertLine != null)
            insertLine.IsVisible = false;
        if (dropTarget != null) 
            dropTarget.IsDragHover = false;
    }

    internal void ItemPointerPressed(DragTreeViewItem item)
    {
        upSelection = null;
        //var control = TopLevel.GetTopLevel(this)?.ke?.Modifiers.HasFlag(KeyModifiers.Control) ?? false;

        //if (Selection.Contains(item))
        //{
        //    if (control)
        //        Unselect(item);
        //    else
        //        upSelection = item;
        //}
        //else
        //{
        //    if (control)
        //        Select(item);
        //    else
        //        SelectOnly(item);
        //}
    }

    internal void ItemPointerReleased(DragTreeViewItem item)
    {
        if (upSelection == item) SelectOnly(item);
        upSelection = null;
    }

    internal void ItemAttached(DragTreeViewItem item)
    {
        if (item.IsSelected) Selection.Add(item);
    }

    internal void ItemDetached(DragTreeViewItem item)
    {
        if (item.IsSelected) Selection.Remove(item);
    }

    internal void ItemIsSelectedChanged(DragTreeViewItem item)
    {
        if (item.IsSelected)
            Selection.Add(item);
        else
            Selection.Remove(item);
    }

    private void Select(DragTreeViewItem item)
    {
        Selection.Add(item);
        item.IsSelected = true;
        OnSelectionChanged();
    }

    private void Unselect(DragTreeViewItem item)
    {
        Selection.Remove(item);
        item.IsSelected = false;
        OnSelectionChanged();
    }

    protected virtual void SelectOnly(DragTreeViewItem item)
    {
        ClearSelection();
        Select(item);
        OnSelectionChanged();
    }

    private void ClearSelection()
    {
        foreach (var treeItem in Selection.ToArray()) treeItem.IsSelected = false;
        Selection.Clear();
        OnSelectionChanged();
    }

    private void OnSelectionChanged()
    {
    }

    private bool CanInsertInternal()
    {
        if (!dropCopy)
        {
            var item = dropTarget;
            while (true)
            {
                if (Selection.Contains(item)) return false;
                item = item?.GetLogicalParent() as DragTreeViewItem;
                if (item == null) break;
            }

            if (Selection.Contains(dropAfter)) return false;
        }

        return CanInsert(dropTarget, Selection.ToArray(), dropAfter, dropCopy);
    }

    private void InsertInternal()
    {
        var selection = Selection.ToArray();

        if (!dropCopy)
            foreach (var item in Selection.ToArray())
            {
                var parent = item.GetLogicalParent() as DragTreeViewItem;
                if (parent != null) Remove(parent, item);
            }

        Insert(dropTarget, selection, dropAfter, dropCopy);
    }

    protected virtual bool CanInsert(DragTreeViewItem target, DragTreeViewItem[] items, DragTreeViewItem after,
        bool copy)
    {
        return true;
    }

    protected virtual void Insert(DragTreeViewItem target, DragTreeViewItem[] items, DragTreeViewItem after, bool copy)
    {
    }

    protected virtual void Remove(DragTreeViewItem target, DragTreeViewItem item)
    {
    }

    #region Filtering

    public static readonly StyledProperty<string> FilterProperty =
        AvaloniaProperty.Register<DragTreeView, string>(nameof(Filter));

    public string Filter
    {
        get => GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    public event Action<string> FilterChanged;

    public virtual bool ShouldItemBeVisible(DragTreeViewItem dragTreeViewitem)
    {
        return true;
    }

    #endregion
}