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
using Avalonia.Threading;
using MyDesigner.Design.PropertyGrid;

namespace MyDesigner.Designer.PropertyGrid;

public interface IPropertyGrid
{
    IEnumerable<DesignItem> SelectedItems { get; set; }
    Dictionary<MemberDescriptor, PropertyNode> NodeFromDescriptor { get; }
    DesignItem SingleItem { get; }
    string Name { get; set; }
    string OldName { get; }
    bool IsNameCorrect { get; set; }
    bool ReloadActive { get; }
    event EventHandler AggregatePropertiesUpdated;
    event PropertyChangedEventHandler PropertyChanged;
    void Refresh();
}

public class PropertyGrid : INotifyPropertyChanged, IPropertyGrid
{
    private readonly Category attachedCategory = new("Attached");

    private readonly Category otherCategory = new("Other");
    private readonly Category popularCategory = new("Popular");

    private readonly Category specialCategory = new("Scada");

    private PropertyGridGroupMode _groupMode;

    private PropertyGridTab currentTab;

    private string filter;

    private bool isNameCorrect = true;

    private volatile bool reloadActive;

    private IList<DesignItem> selectedItems;

    private DesignItem singleItem;

    public PropertyGrid()
    {
        Categories = new CategoriesCollection();
        Categories.Add(specialCategory);
        Categories.Add(popularCategory);
        Categories.Add(otherCategory);
        Categories.Add(attachedCategory);

        BasicMetadata.Register();

        Events = new PropertyNodeCollection();
    }

    public CategoriesCollection Categories { get; }
    public PropertyNodeCollection Events { get; }

    public PropertyGridGroupMode GroupMode
    {
        get => _groupMode;
        set
        {
            if (_groupMode != value)
            {
                _groupMode = value;

                RaisePropertyChanged("GroupMode");

                Reload();
            }
        }
    }

    public PropertyGridTab CurrentTab
    {
        get => currentTab;
        set
        {
            currentTab = value;
            RaisePropertyChanged("CurrentTab");
            RaisePropertyChanged("NameBackground");
        }
    }

    public string Filter
    {
        get => filter;
        set
        {
            filter = value;
            Reload();
            RaisePropertyChanged("Filter");
        }
    }

    public bool IsNameEnabled => SingleItem != null;
    public Dictionary<MemberDescriptor, PropertyNode> NodeFromDescriptor { get; } = new();

    public event EventHandler AggregatePropertiesUpdated;

    public DesignItem SingleItem
    {
        get => singleItem;
        private set
        {
            if (singleItem != null) singleItem.NameChanged -= singleItem_NameChanged;
            singleItem = value;
            if (singleItem != null) singleItem.NameChanged += singleItem_NameChanged;
            RaisePropertyChanged("SingleItem");
            RaisePropertyChanged("Name");
            RaisePropertyChanged("IsNameEnabled");
            IsNameCorrect = true;
        }
    }

    public string OldName { get; private set; }

    public string Name
    {
        get
        {
            if (SingleItem != null) return SingleItem.Name;
            return null;
        }
        set
        {
            if (SingleItem != null)
            {
                try
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        OldName = null;
                        SingleItem.Name = null;
                    }
                    else
                    {
                        OldName = SingleItem.Name;
                        SingleItem.Name = value;
                    }

                    IsNameCorrect = true;
                }
                catch
                {
                    IsNameCorrect = false;
                }

                RaisePropertyChanged("Name");
            }
        }
    }

    public bool IsNameCorrect
    {
        get => isNameCorrect;
        set
        {
            isNameCorrect = value;
            RaisePropertyChanged("IsNameCorrect");
        }
    }

    public IEnumerable<DesignItem> SelectedItems
    {
        get => selectedItems;
        set
        {
            if (value == null)
                selectedItems = null;
            else
                selectedItems = value.ToList();
            RaisePropertyChanged("SelectedItems");
            Dispatcher.UIThread.InvokeAsync(new Action(
                delegate { Reload(); }), DispatcherPriority.Background);
        }
    }

    public bool ReloadActive => reloadActive;

    private void singleItem_NameChanged(object sender, EventArgs e)
    {
        RaisePropertyChanged("Name");
    }

    public void ClearFilter()
    {
        Filter = null;
    }

    public void Refresh()
    {
        Reload();
    }

    private void Reload()
    {
        reloadActive = true;
        try
        {
            Clear();

            if (selectedItems == null || selectedItems.Count == 0) return;
            if (selectedItems.Count == 1) SingleItem = selectedItems[0];

            foreach (var md in GetDescriptors())
                if (PassesFilter(md.Name))
                    AddNode(md);
        }
        finally
        {
            reloadActive = false;
            if (AggregatePropertiesUpdated != null)
                AggregatePropertiesUpdated(this, EventArgs.Empty);
        }
    }

    private void Clear()
    {
        foreach (var c in Categories)
        {
            c.IsVisible = false;
            foreach (var p in c.Properties) p.IsVisible = false;
        }

        foreach (var e in Events) e.IsVisible = false;

        SingleItem = null;
    }

    private List<MemberDescriptor> GetDescriptors()
    {
        var list = new List<MemberDescriptor>();
        var service = (SingleItem ?? SelectedItems.First()).Services.GetService<IComponentPropertyService>();

        if (SelectedItems.Count() == 1)
        {
            list.AddRange(service.GetAvailableProperties(SingleItem));
            list.AddRange(service.GetAvailableEvents(SingleItem));
        }
        else
        {
            list.AddRange(service.GetCommonAvailableProperties(SelectedItems));
        }

        return list;
    }

    private bool PassesFilter(string name)
    {
        if (string.IsNullOrEmpty(Filter)) return true;
        for (var i = 0; i < name.Length; i++)
            if (i == 0 || char.IsUpper(name[i]))
                if (string.Compare(name, i, Filter, 0, Filter.Length, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;

        return false;
    }

    private void AddNode(MemberDescriptor md)
    {
        var designProperties = SelectedItems.Select(item => item.Properties.GetProperty(md)).ToArray();
        if (!Metadata.IsBrowsable(designProperties[0])) return;

        PropertyNode node;
        if (NodeFromDescriptor.TryGetValue(md, out node))
        {
            node.Load(designProperties);
        }
        else
        {
            node = new PropertyNode();
            node.Load(designProperties);
            if (node.IsEvent)
            {
                Events.AddSorted(node);
            }
            else
            {
                var cat = PickCategory(node);
                cat.Properties.AddSorted(node);
                node.Category = cat;
            }

            NodeFromDescriptor[md] = node;
        }

        node.IsVisible = true;
        if (node.Category != null)
            node.Category.IsVisible = true;
    }

    private Category PickCategory(PropertyNode node)
    {
        if (Metadata.IsPopularProperty(node.FirstProperty)) return popularCategory;
        if (node.FirstProperty.IsAttachedDependencyProperty()) return attachedCategory;
        var typeName = node.FirstProperty.DeclaringType.FullName;
        if (typeName.StartsWith("Avalonia.", StringComparison.Ordinal) ||
            typeName.StartsWith("MyDesigner.Designer.Controls.", StringComparison.Ordinal))
            return otherCategory;
        return specialCategory;
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void RaisePropertyChanged(string name)
    {
        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    #endregion
}

public class CategoriesCollection : SortedObservableCollection<Category, string>
{
    public CategoriesCollection()
        : base(n => n.Name)
    {
    }
}

public enum PropertyGridGroupMode
{
    GroupByPopularCategorys,
    GroupByCategorys,
    Ungrouped
}

public enum PropertyGridTab
{
    Properties,
    Events
}