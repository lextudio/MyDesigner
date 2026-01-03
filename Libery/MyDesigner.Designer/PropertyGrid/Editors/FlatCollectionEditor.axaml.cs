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

using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MyDesigner.Designer.PropertyGrid;

namespace MyDesigner.Designer.PropertyGrid.Editors;

public partial class FlatCollectionEditor : Window
{
    private static readonly Dictionary<Type, Type> TypeMappings = new();
    private IComponentService _componentService;

    private DesignItemProperty _itemProperty;
    private Type _type;
    
   

    static FlatCollectionEditor()
    {
        TypeMappings.Add(typeof(ListBox), typeof(ListBoxItem));
        //TypeMappings.Add(typeof(ListView), typeof(ListViewItem));
        TypeMappings.Add(typeof(ComboBox), typeof(ComboBoxItem));
        TypeMappings.Add(typeof(TabControl), typeof(TabItem));
        // Note: Avalonia doesn't have ColumnDefinitionCollection/RowDefinitionCollection in the same way as WPF
        // These would need to be handled differently in Avalonia
    }

    public FlatCollectionEditor()
    {
        InitializeComponent();
    }

    public FlatCollectionEditor(Window owner)
        : this()
    {
        // In Avalonia, we don't have Owner property, but we can use ShowDialog with parent
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        
        ListBox = this.FindControl<ListBox>("ListBox");
        ItemDataType = this.FindControl<ComboBox>("ItemDataType");
        ListBoxBorder = this.FindControl<Border>("ListBoxBorder");
        AddItem = this.FindControl<Button>("AddItem");
        RemoveItem = this.FindControl<Button>("RemoveItem");
        MoveUpItem = this.FindControl<Button>("MoveUpItem");
        MoveDownItem = this.FindControl<Button>("MoveDownItem");
        PropertyGridView = this.FindControl<PropertyGridView>("PropertyGridView");
    }

    public Type GetItemsSourceType(Type t)
    {
        // In Avalonia, we don't have UIElementCollection, but we can handle Controls collection
        if (t == typeof(Avalonia.Controls.Controls))
            return typeof(Control);

        var tp = t.GetInterfaces()
            .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>));

        return tp != null ? tp.GetGenericArguments()[0] : null;
    }

    public void LoadItemsCollection(DesignItemProperty itemProperty)
    {
        _itemProperty = itemProperty;
        _componentService = _itemProperty.DesignItem.Services.Component;
        TypeMappings.TryGetValue(_itemProperty.ReturnType, out _type);

        _type = _type ?? GetItemsSourceType(_itemProperty.ReturnType);

        if (_type == null) AddItem.IsEnabled = false;

        ListBox.ItemsSource = _itemProperty.CollectionElements;
        LoadItemsCombobox();
    }

    public void LoadItemsCombobox()
    {
        if (_type != null)
        {
            var types = new List<Type>();
            types.Add(_type);

            foreach (var items in GetInheritedClasses(_type))
                types.Add(items);
            ItemDataType.ItemsSource = types;
            ItemDataType.SelectedItem = types[0];

            if (types.Count < 2)
            {
                ItemDataType.IsVisible = false;
                ListBoxBorder.Margin = new Avalonia.Thickness(10);
            }
        }
        else
        {
            ItemDataType.IsVisible = false;
            ListBoxBorder.Margin = new Avalonia.Thickness(10);
        }
    }

    private IEnumerable<Type> GetInheritedClasses(Type type)
    {
        return AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic).SelectMany(x =>
            GetLoadableTypes(x).Where(y => y.IsClass && !y.IsAbstract && y.IsSubclassOf(type)));
    }

    private IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        if (assembly == null) throw new ArgumentNullException("assembly");
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null);
        }
    }

    private void OnAddItemClicked(object sender, RoutedEventArgs e)
    {
        var comboboxItem = ItemDataType.SelectedItem;
        var newItem = _componentService.RegisterComponentForDesigner(Activator.CreateInstance((Type)comboboxItem));
        _itemProperty.CollectionElements.Add(newItem);
    }

    private void OnRemoveItemClicked(object sender, RoutedEventArgs e)
    {
        var selItem = ListBox.SelectedItem as DesignItem;
        if (selItem != null)
            _itemProperty.CollectionElements.Remove(selItem);
    }

    private void OnMoveItemUpClicked(object sender, RoutedEventArgs e)
    {
        var selectedItem = ListBox.SelectedItem as DesignItem;
        if (selectedItem != null)
            if (_itemProperty.CollectionElements.Count != 1 &&
                _itemProperty.CollectionElements.IndexOf(selectedItem) != 0)
            {
                var moveToIndex = _itemProperty.CollectionElements.IndexOf(selectedItem) - 1;
                var itemAtMoveToIndex = _itemProperty.CollectionElements[moveToIndex];
                _itemProperty.CollectionElements.RemoveAt(moveToIndex);
                if (moveToIndex + 1 < _itemProperty.CollectionElements.Count + 1)
                    _itemProperty.CollectionElements.Insert(moveToIndex + 1, itemAtMoveToIndex);
            }
    }

    private void OnMoveItemDownClicked(object sender, RoutedEventArgs e)
    {
        var selectedItem = ListBox.SelectedItem as DesignItem;
        if (selectedItem != null)
        {
            var itemCount = _itemProperty.CollectionElements.Count;
            if (itemCount != 1 && _itemProperty.CollectionElements.IndexOf(selectedItem) != itemCount)
            {
                var moveToIndex = _itemProperty.CollectionElements.IndexOf(selectedItem) + 1;
                if (moveToIndex < itemCount)
                {
                    var itemAtMoveToIndex = _itemProperty.CollectionElements[moveToIndex];
                    _itemProperty.CollectionElements.RemoveAt(moveToIndex);
                    if (moveToIndex > 0)
                        _itemProperty.CollectionElements.Insert(moveToIndex - 1, itemAtMoveToIndex);
                }
            }
        }
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PropertyGridView.PropertyGrid.SelectedItems = ListBox.SelectedItems.Cast<DesignItem>();
    }
}