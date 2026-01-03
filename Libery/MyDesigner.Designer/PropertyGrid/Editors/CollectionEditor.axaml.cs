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

using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MyDesigner.Designer.OutlineView;
using MyDesigner.Designer.PropertyGrid;

namespace MyDesigner.Designer.PropertyGrid.Editors;

public partial class CollectionEditor : Window
{
    private static readonly Dictionary<Type, Type> TypeMappings = new();
    private IComponentService _componentService;

    private DesignItem _item;
    private Type _type;
    
   
    static CollectionEditor()
    {
        TypeMappings.Add(typeof(Menu), typeof(MenuItem));
        TypeMappings.Add(typeof(ListBox), typeof(ListBoxItem));
      //  TypeMappings.Add(typeof(ListView), typeof(ListViewItem));
        TypeMappings.Add(typeof(ComboBox), typeof(ComboBoxItem));
        TypeMappings.Add(typeof(TreeView), typeof(TreeViewItem));
        TypeMappings.Add(typeof(TabControl), typeof(TabItem));
    }

    public CollectionEditor()
    {
        InitializeComponent();
    }

    public CollectionEditor(Window owner)
        : this()
    {
        // In Avalonia, we don't have Owner property, but we can use ShowDialog with parent
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        
        Outline = this.FindControl<Outline>("Outline");
        PropertyGridView = this.FindControl<PropertyGridView>("PropertyGridView");
        AddItem = this.FindControl<Button>("AddItem");
        RemoveItem = this.FindControl<Button>("RemoveItem");
        MoveUpItem = this.FindControl<Button>("MoveUpItem");
        MoveDownItem = this.FindControl<Button>("MoveDownItem");
    }

    public void LoadItemsCollection(DesignItem item)
    {
        Debug.Assert(item.View is ItemsControl);
        _item = item;
        _componentService = item.Services.Component;
        item.Services.Selection.SelectionChanged += delegate
        {
            PropertyGridView.SelectedItems = item.Services.Selection.SelectedItems;
        };
        var control = item.View as ItemsControl;
        if (control != null)
        {
            TypeMappings.TryGetValue(control.GetType(), out _type);
            if (_type != null)
            {
                var node = item.CreateOutlineNode();
                Outline.Root = node;
                PropertyGridView.PropertyGrid.SelectedItems = item.Services.Selection.SelectedItems;
            }
            else
            {
                PropertyGridView.IsEnabled = false;
                Outline.IsEnabled = false;
                AddItem.IsEnabled = false;
                RemoveItem.IsEnabled = false;
                MoveUpItem.IsEnabled = false;
                MoveDownItem.IsEnabled = false;
            }
        }
    }

    private void OnAddItemClicked(object sender, RoutedEventArgs e)
    {
        var newItem = _componentService.RegisterComponentForDesigner(Activator.CreateInstance(_type));
        var selectedItem = _item.Services.Selection.PrimarySelection;
        if (selectedItem.ContentProperty.IsCollection)
            selectedItem.ContentProperty.CollectionElements.Add(newItem);
        else
            selectedItem.ContentProperty.SetValue(newItem);
        _item.Services.Selection.SetSelectedComponents(new[] { newItem });
    }

    private void OnRemoveItemClicked(object sender, RoutedEventArgs e)
    {
        var selectedItem = _item.Services.Selection.PrimarySelection;
        var parent = selectedItem.Parent;
        if (parent != null && selectedItem != _item)
        {
            if (parent.ContentProperty.IsCollection)
                parent.ContentProperty.CollectionElements.Remove(selectedItem);
            else
                parent.ContentProperty.SetValue(null);
            _item.Services.Selection.SetSelectedComponents(new[] { parent });
        }
    }

    private void OnMoveItemUpClicked(object sender, RoutedEventArgs e)
    {
        var selectedItem = _item.Services.Selection.PrimarySelection;
        var parent = selectedItem.Parent;
        if (parent != null && parent.ContentProperty.IsCollection)
            if (parent.ContentProperty.CollectionElements.Count != 1 &&
                parent.ContentProperty.CollectionElements.IndexOf(selectedItem) != 0)
            {
                var moveToIndex = parent.ContentProperty.CollectionElements.IndexOf(selectedItem) - 1;
                var itemAtMoveToIndex = parent.ContentProperty.CollectionElements[moveToIndex];
                parent.ContentProperty.CollectionElements.RemoveAt(moveToIndex);
                if (moveToIndex + 1 < parent.ContentProperty.CollectionElements.Count + 1)
                    parent.ContentProperty.CollectionElements.Insert(moveToIndex + 1, itemAtMoveToIndex);
            }
    }

    private void OnMoveItemDownClicked(object sender, RoutedEventArgs e)
    {
        var selectedItem = _item.Services.Selection.PrimarySelection;
        var parent = selectedItem.Parent;
        if (parent != null && parent.ContentProperty.IsCollection)
        {
            var itemCount = parent.ContentProperty.CollectionElements.Count;
            if (itemCount != 1 && parent.ContentProperty.CollectionElements.IndexOf(selectedItem) != itemCount)
            {
                var moveToIndex = parent.ContentProperty.CollectionElements.IndexOf(selectedItem) + 1;
                if (moveToIndex < itemCount)
                {
                    var itemAtMoveToIndex = parent.ContentProperty.CollectionElements[moveToIndex];
                    parent.ContentProperty.CollectionElements.RemoveAt(moveToIndex);
                    if (moveToIndex > 0)
                        parent.ContentProperty.CollectionElements.Insert(moveToIndex - 1, itemAtMoveToIndex);
                }
            }
        }
    }
}