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
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace MyDesigner.Designer.OutlineView;

public class DragTreeViewItem : TreeViewItem
{
    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<DragTreeViewItem>();

    public static readonly StyledProperty<bool> IsDragHoverProperty =
        AvaloniaProperty.Register<DragTreeViewItem, bool>(nameof(IsDragHover));

    public static readonly StyledProperty<int> LevelProperty =
        AvaloniaProperty.Register<DragTreeViewItem, int>(nameof(Level));

    private ContentPresenter part_header;

    public DragTreeViewItem()
    {
        Loaded += DragTreeViewItem_Loaded;
        Unloaded += DragTreeViewItem_Unloaded;
    }

    public new bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public bool IsDragHover
    {
        get => GetValue(IsDragHoverProperty);
        set => SetValue(IsDragHoverProperty, value);
    }

    internal ContentPresenter HeaderPresenter => this.FindNameScope()?.Find<ContentPresenter>("PART_Header");

    public int Level
    {
        get => GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    public DragTreeView ParentTree { get; private set; }

    private void DragTreeViewItem_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ParentTree = this.FindAncestorOfType<DragTreeView>();
        if (ParentTree != null)
        {
            ParentTree.ItemAttached(this);
            ParentTree.FilterChanged += ParentTree_FilterChanged;
        }
    }

    private void ParentTree_FilterChanged(string obj)
    {
        var v = ParentTree.ShouldItemBeVisible(this);
        if (part_header != null)
        {
            part_header.IsVisible = v;
        }
    }

    private void DragTreeViewItem_Unloaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (ParentTree != null)
        {
            ParentTree.ItemDetached(this);
            ParentTree.FilterChanged -= ParentTree_FilterChanged;
        }

        ParentTree = null;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        part_header = e.NameScope.Find<ContentPresenter>("PART_Header");
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if (change.Property == IsSelectedProperty)
        {
            if (IsSelected)
                this.BringIntoView();
            
            if (ParentTree != null)
                ParentTree.ItemIsSelectedChanged(this);
        }
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);

        var parentItem = this.GetLogicalParent() as DragTreeViewItem;
        if (parentItem != null) 
            Level = parentItem.Level + 1;
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

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.Source is ToggleButton) return;
        ParentTree?.ItemPointerPressed(this);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        ParentTree?.ItemPointerReleased(this);
    }
}