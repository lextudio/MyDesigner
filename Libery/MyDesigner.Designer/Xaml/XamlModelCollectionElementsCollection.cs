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

using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using MyDesigner.Design.Interfaces;
using MyDesigner.Designer.Services;
using MyDesigner.XamlDom;

namespace MyDesigner.Designer.Xaml;

internal sealed class XamlModelCollectionElementsCollection : IObservableList<DesignItem>, INotifyCollectionChanged, INotifyPropertyChanged
{
    private readonly XamlDesignContext context;
    private readonly XamlModelProperty modelProperty;
    private readonly XamlProperty property;

    public XamlModelCollectionElementsCollection(XamlModelProperty modelProperty, XamlProperty property)
    {
        this.modelProperty = modelProperty;
        this.property = property;
        context = (XamlDesignContext)modelProperty.DesignItem.Context;
    }

    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    public int Count => property.CollectionElements.Count;

    public bool IsReadOnly => false;

    public void Add(DesignItem item)
    {
        Insert(Count, item);
    }

    public void Clear()
    {
        while (Count > 0) RemoveAt(Count - 1);

        if (CollectionChanged != null)
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public bool Contains(DesignItem item)
    {
        var xitem = CheckItemNoException(item);
        if (xitem != null)
            return property.CollectionElements.Contains(xitem.XamlObject);
        return false;
    }

    public int IndexOf(DesignItem item)
    {
        var xitem = CheckItemNoException(item);
        if (xitem != null)
            return property.CollectionElements.IndexOf(xitem.XamlObject);
        return -1;
    }

    public void CopyTo(DesignItem[] array, int arrayIndex)
    {
        for (var i = 0; i < Count; i++) array[arrayIndex + i] = this[i];
    }

    public bool Remove(DesignItem item)
    {
        var index = IndexOf(item);
        if (index < 0)
            return false;

        RemoveAt(index);

        return true;
    }

    public IEnumerator<DesignItem> GetEnumerator()
    {
        foreach (var val in property.CollectionElements)
        {
            var item = GetItem(val);
            if (item != null)
                yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public DesignItem this[int index]
    {
        get => GetItem(property.CollectionElements[index]);
        set
        {
            RemoveAt(index);
            Insert(index, value);
        }
    }

    public void Insert(int index, DesignItem item)
    {
        Execute(new InsertAction(this, index, CheckItem(item)));
    }

    public void RemoveAt(int index)
    {
        Execute(new RemoveAtAction(this, index, (XamlDesignItem)this[index]));
    }

    private DesignItem GetItem(XamlPropertyValue val)
    {
        if (val is XamlObject) return context._componentService.GetDesignItem(((XamlObject)val).Instance);

        return null; //	throw new NotImplementedException();
    }

    private XamlDesignItem CheckItem(DesignItem item)
    {
        if (item == null)
            throw new ArgumentNullException("item");
        if (item.Context != modelProperty.DesignItem.Context)
            throw new ArgumentException("The item must belong to the same context as this collection", "item");
        var xitem = item as XamlDesignItem;
        Debug.Assert(xitem != null);
        return xitem;
    }

    private XamlDesignItem CheckItemNoException(DesignItem item)
    {
        return item as XamlDesignItem;
    }

    internal ITransactionItem CreateResetTransaction()
    {
        return new ResetAction(this);
    }

    private void Execute(ITransactionItem item)
    {
        var undoService = context.Services.GetService<UndoService>();
        if (undoService != null)
            undoService.Execute(item);
        else
            item.Do();
    }

    private void RemoveInternal(int index, XamlDesignItem item)
    {
        if (item != null)
            RemoveFromNamescopeRecursive(item);

        if (item != null) Debug.Assert(property.CollectionElements[index] == item.XamlObject);
        property.CollectionElements.RemoveAt(index);

        if (CollectionChanged != null)
            CollectionChanged(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
    }

    private void InsertInternal(int index, XamlDesignItem item)
    {
        property.CollectionElements.Insert(index, item.XamlObject);

        if (CollectionChanged != null)
            CollectionChanged(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));

        if (item != null)
            AddToNamescopeRecursive(item);
    }

    private static void RemoveFromNamescopeRecursive(XamlDesignItem designItem)
    {
        NameScopeHelper.NameChanged(designItem.XamlObject, designItem.Name, null);

        foreach (var p in designItem.Properties)
            if (p.Value != null)
                RemoveFromNamescopeRecursive((XamlDesignItem)p.Value);
            else if (p.IsCollection && p.CollectionElements != null)
                foreach (var c in p.CollectionElements)
                    RemoveFromNamescopeRecursive((XamlDesignItem)c);
    }

    private static void AddToNamescopeRecursive(XamlDesignItem designItem)
    {
        NameScopeHelper.NameChanged(designItem.XamlObject, null, designItem.Name);

        foreach (var p in designItem.Properties)
            if (p.Value != null)
                AddToNamescopeRecursive((XamlDesignItem)p.Value);
            else if (p.IsCollection && p.CollectionElements != null)
                foreach (var c in p.CollectionElements)
                    AddToNamescopeRecursive((XamlDesignItem)c);
    }

    private sealed class InsertAction : ITransactionItem
    {
        private readonly XamlModelCollectionElementsCollection collection;
        private readonly int index;
        private readonly XamlDesignItem item;

        public InsertAction(XamlModelCollectionElementsCollection collection, int index, XamlDesignItem item)
        {
            this.collection = collection;
            this.index = index;
            this.item = item;
        }

        public ICollection<DesignItem> AffectedElements
        {
            get { return new DesignItem[] { item }; }
        }

        public string Title => "Insert into collection";

        public void Do()
        {
            collection.InsertInternal(index, item);
            collection.modelProperty.XamlDesignItem.NotifyPropertyChanged(collection.modelProperty, null, item);
        }

        public void Undo()
        {
            collection.RemoveInternal(index, item);
            collection.modelProperty.XamlDesignItem.NotifyPropertyChanged(collection.modelProperty, item, null);
        }

        public bool MergeWith(ITransactionItem other)
        {
            return false;
        }
    }

    private sealed class RemoveAtAction : ITransactionItem
    {
        private readonly XamlModelCollectionElementsCollection collection;
        private readonly int index;
        private readonly XamlDesignItem item;

        public RemoveAtAction(XamlModelCollectionElementsCollection collection, int index, XamlDesignItem item)
        {
            this.collection = collection;
            this.index = index;
            this.item = item;
        }

        public ICollection<DesignItem> AffectedElements
        {
            get { return new[] { collection.modelProperty.DesignItem }; }
        }

        public string Title => "Remove from collection";

        public void Do()
        {
            collection.RemoveInternal(index, item);
            collection.modelProperty.XamlDesignItem.NotifyPropertyChanged(collection.modelProperty, item, null);
        }

        public void Undo()
        {
            collection.InsertInternal(index, item);
            collection.modelProperty.XamlDesignItem.NotifyPropertyChanged(collection.modelProperty, null, item);
        }

        public bool MergeWith(ITransactionItem other)
        {
            return false;
        }
    }

    private sealed class ResetAction : ITransactionItem
    {
        private readonly XamlModelCollectionElementsCollection collection;
        private readonly XamlDesignItem[] items;

        public ResetAction(XamlModelCollectionElementsCollection collection)
        {
            this.collection = collection;

            items = new XamlDesignItem[collection.Count];
            for (var i = 0; i < collection.Count; i++) items[i] = (XamlDesignItem)collection[i];
        }

        #region ITransactionItem implementation

        public void Do()
        {
            for (var i = items.Length - 1; i >= 0; i--) collection.RemoveInternal(i, items[i]);
            collection.modelProperty.XamlDesignItem.NotifyPropertyChanged(collection.modelProperty, items, null);
        }

        public void Undo()
        {
            for (var i = 0; i < items.Length; i++) collection.InsertInternal(i, items[i]);
            collection.modelProperty.XamlDesignItem.NotifyPropertyChanged(collection.modelProperty, null, items);
        }

        public bool MergeWith(ITransactionItem other)
        {
            return false;
        }

        #endregion

        #region IUndoAction implementation

        public ICollection<DesignItem> AffectedElements
        {
            get { return new[] { collection.modelProperty.DesignItem }; }
        }

        public string Title => "Reset collection";

        #endregion
    }
}