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

// enable this define to test that event handlers are removed correctly
//#define EventHandlerDebugging

using System.ComponentModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using MyDesigner.Designer.Services;
using MyDesigner.XamlDom;

namespace MyDesigner.Designer.Xaml;

[DebuggerDisplay("XamlDesignItem: {ComponentType.Name}")]
public sealed class XamlDesignItem : DesignItem
{
    private readonly XamlDesignContext _designContext;
    private readonly XamlModelPropertyCollection _properties;
    private Control _view;

    public XamlDesignItem(XamlObject xamlObject, XamlDesignContext designContext)
    {
        XamlObject = xamlObject;
        _designContext = designContext;
        _properties = new XamlModelPropertyCollection(this);
    }

    internal XamlComponentService ComponentService => _designContext._componentService;

    public XamlObject XamlObject { get; }

    public override object Component => XamlObject.Instance;

    public override Type ComponentType => XamlObject.ElementType;

    private void SetNameInternal(string newName)
    {
        var oldName = Name;

        XamlObject.Name = newName;

        FixDesignItemReferencesOnNameChange(oldName, Name);
    }

    public override string Name
    {
        get => XamlObject.Name;
        set
        {
            var undoService = Services.GetService<UndoService>();
            if (undoService != null)
                undoService.Execute(new SetNameAction(this, value));
            else
                SetNameInternal(value);
        }
    }

    /// <summary>
    ///     Fixes {x:Reference and {Binding ElementName to this Element in XamlDocument
    /// </summary>
    /// <param name="oldName"></param>
    /// <param name="newName"></param>
    public void FixDesignItemReferencesOnNameChange(string oldName, string newName)
    {
        if (!string.IsNullOrEmpty(oldName) && !string.IsNullOrEmpty(newName))
        {
            var root = GetRootXamlObject(XamlObject);
            var references = GetAllChildXamlObjects(root).Where(x =>
                x.ElementType == typeof(Avalonia.Markup.Xaml.MarkupExtensions.StaticResourceExtension) &&
                Equals(x.FindOrCreateProperty("ResourceKey").GetValueOnInstance<string>(), oldName));
            foreach (var designItem in references)
            {
                var property = designItem.FindOrCreateProperty("Name");
                var propertyValue = designItem.OwnerDocument.CreatePropertyValue(newName, property);
                ComponentService.RegisterXamlComponentRecursive(propertyValue as XamlObject);
                property.PropertyValue = propertyValue;
            }

            root = GetRootXamlObject(XamlObject, true);
            var bindings = GetAllChildXamlObjects(root, true).Where(x =>
                x.ElementType == typeof(Binding) &&
                Equals(x.FindOrCreateProperty("ElementName").GetValueOnInstance<string>(), oldName));
            foreach (var designItem in bindings)
            {
                var property = designItem.FindOrCreateProperty("ElementName");
                var propertyValue = designItem.OwnerDocument.CreatePropertyValue(newName, property);
                ComponentService.RegisterXamlComponentRecursive(propertyValue as XamlObject);
                property.PropertyValue = propertyValue;
            }
        }
    }

    /// <summary>
    ///     Find's the Root XamlObject (real Root, or Root Object in Namescope)
    /// </summary>
    /// <param name="item"></param>
    /// <param name="onlyFromSameNamescope"></param>
    /// <returns></returns>
    internal static XamlObject GetRootXamlObject(XamlObject item, bool onlyFromSameNamescope = false)
    {
        var root = item;
        while (root.ParentObject != null)
        {
            if (onlyFromSameNamescope && NameScopeHelper.GetNameScopeFromObject(root) !=
                NameScopeHelper.GetNameScopeFromObject(root.ParentObject))
                break;
            root = root.ParentObject;
        }

        return root;
    }

    /// <summary>
    ///     Get's all Child XamlObject Instances
    /// </summary>
    /// <param name="item"></param>
    /// <param name="onlyFromSameNamescope"></param>
    /// <returns></returns>
    internal static IEnumerable<XamlObject> GetAllChildXamlObjects(XamlObject item, bool onlyFromSameNamescope = false)
    {
        foreach (var prop in item.Properties)
        {
            if (prop.PropertyValue as XamlObject != null)
            {
                if (!onlyFromSameNamescope || NameScopeHelper.GetNameScopeFromObject(item) ==
                    NameScopeHelper.GetNameScopeFromObject(prop.PropertyValue as XamlObject))
                    yield return prop.PropertyValue as XamlObject;

                foreach (var i in GetAllChildXamlObjects(prop.PropertyValue as XamlObject))
                    if (!onlyFromSameNamescope || NameScopeHelper.GetNameScopeFromObject(item) ==
                        NameScopeHelper.GetNameScopeFromObject(i))
                        yield return i;
            }

            if (prop.IsCollection)
                foreach (var collectionElement in prop.CollectionElements)
                    if (collectionElement as XamlObject != null)
                    {
                        if (!onlyFromSameNamescope || NameScopeHelper.GetNameScopeFromObject(item) ==
                            NameScopeHelper.GetNameScopeFromObject(collectionElement as XamlObject))
                            yield return collectionElement as XamlObject;

                        foreach (var i in GetAllChildXamlObjects(collectionElement as XamlObject))
                            if (!onlyFromSameNamescope || NameScopeHelper.GetNameScopeFromObject(item) ==
                                NameScopeHelper.GetNameScopeFromObject(i))
                                yield return i;
                    }
        }
    }

    public override string Key
    {
        get => XamlObject.GetXamlAttribute("Key");
        set => XamlObject.SetXamlAttribute("Key", value);
    }

#if EventHandlerDebugging
		static int totalEventHandlerCount;
#endif

    /// <summary>
    ///     Is raised when the name of the design item changes.
    /// </summary>
    public override event EventHandler NameChanged
    {
        add
        {
#if EventHandlerDebugging
				Debug.WriteLine("Add event handler to " + this.ComponentType.Name + " (handler count=" + (++totalEventHandlerCount) + ")");
#endif
            XamlObject.NameChanged += value;
        }
        remove
        {
#if EventHandlerDebugging
				Debug.WriteLine("Remove event handler from " + this.ComponentType.Name + " (handler count=" + (--totalEventHandlerCount) + ")");
#endif
            XamlObject.NameChanged -= value;
        }
    }

    public override DesignItem Parent
    {
        get
        {
            if (XamlObject.ParentProperty == null)
                return null;
            return ComponentService.GetDesignItem(XamlObject.ParentProperty.ParentObject.Instance);
        }
    }

    public override DesignItemProperty ParentProperty
    {
        get
        {
            var parent = Parent;
            if (parent == null)
                return null;
            var prop = XamlObject.ParentProperty;
            if (prop.IsAttached)
                return parent.Properties.GetAttachedProperty(prop.PropertyTargetType, prop.PropertyName);

            return parent.Properties.GetProperty(prop.PropertyName);
        }
    }

    /// <summary>
    ///     Occurs when the parent of this design item changes.
    /// </summary>
    public override event EventHandler ParentChanged
    {
        add => XamlObject.ParentPropertyChanged += value;
        remove => XamlObject.ParentPropertyChanged -= value;
    }

    public override Control View
    {
        get
        {
            if (_view != null)
                return _view;
            return Component as Control;
        }
    }

    public override void SetView(Control newView)
    {
        _view = newView;
    }

    public override DesignContext Context => _designContext;

    public override DesignItemPropertyCollection Properties => _properties;

    public override IEnumerable<DesignItemProperty> AllSetProperties
    {
        get { return XamlObject.Properties.Select(x => new XamlModelProperty(this, x)); }
    }

    internal void NotifyPropertyChanged(XamlModelProperty property, object oldValue, object newValue)
    {
        Debug.Assert(property != null);
        OnPropertyChanged(new PropertyChangedEventArgs(property.Name));

        ((XamlComponentService)Services.Component).RaisePropertyChanged(property, oldValue, newValue);
    }

    public override string ContentPropertyName => XamlObject.ContentPropertyName;

    ///// <summary>
    /////     Item is Locked at Design Time
    ///// </summary>
    //public bool IsDesignTimeLocked
    //{
    //    get
    //    {
    //        var locked = Properties.GetAttachedProperty(DesignTimeProperties.IsLockedProperty)
    //            .GetConvertedValueOnInstance<object>();
    //        return locked != null && (bool)locked;
    //    }
    //    set
    //    {
    //        if (value)
    //            Properties.GetAttachedProperty(DesignTimeProperties.IsLockedProperty).SetValue(true);
    //        else
    //            Properties.GetAttachedProperty(DesignTimeProperties.IsLockedProperty).Reset();
    //    }
    //}

    public override DesignItem Clone()
    {
        DesignItem item = null;
        var xaml = XamlStaticTools.GetXaml(XamlObject);
        var rootItem = Context.RootItem as XamlDesignItem;
        var obj = XamlParser.ParseSnippet(rootItem.XamlObject, xaml, ((XamlDesignContext)Context).ParserSettings);
        if (obj != null) item = ((XamlDesignContext)Context)._componentService.RegisterXamlComponentRecursive(obj);
        return item;
    }

    private sealed class SetNameAction : ITransactionItem
    {
        private readonly XamlDesignItem designItem;
        private readonly string oldName;
        private string newName;

        public SetNameAction(XamlDesignItem designItem, string newName)
        {
            this.designItem = designItem;
            this.newName = newName;

            oldName = designItem.Name;
        }

        public string Title => "Set name";

        public void Do()
        {
            designItem.SetNameInternal(newName);
        }

        public void Undo()
        {
            designItem.SetNameInternal(oldName);
        }

        public ICollection<DesignItem> AffectedElements
        {
            get { return new DesignItem[] { designItem }; }
        }

        public bool MergeWith(ITransactionItem other)
        {
            var o = other as SetNameAction;
            if (o != null && designItem == o.designItem)
            {
                newName = o.newName;
                return true;
            }

            return false;
        }
    }
}