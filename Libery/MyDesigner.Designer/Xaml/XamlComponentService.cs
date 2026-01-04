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

using System.Runtime.CompilerServices;
using System.Linq;
using Avalonia.Markup.Xaml.MarkupExtensions;
using MyDesigner.XamlDom;

namespace MyDesigner.Designer.Xaml;

public sealed class XamlComponentService : IComponentService
{
    private readonly XamlDesignContext _context;

    // TODO: this must not be a dictionary because there's no way to unregister components
    // however, this isn't critical because our design items will stay alive for the lifetime of the
    // designer anyway if we don't limit the Undo stack.
    private readonly Dictionary<object, XamlDesignItem> _sites = new(IdentityEqualityComparer.Instance);

    public XamlComponentService(XamlDesignContext context)
    {
        _context = context;
    }

    public event EventHandler<DesignItemPropertyChangedEventArgs> PropertyChanged;

    public event EventHandler<DesignItemEventArgs> ComponentRegisteredAndAddedToContainer;

    public event EventHandler<DesignItemEventArgs> ComponentRegistered;

    public event EventHandler<DesignItemEventArgs> ComponentRemoved;

    public DesignItem GetDesignItem(object component)
    {
        return GetDesignItem(component, false);
    }

    public DesignItem GetDesignItem(object component, bool findByView)
    {
        if (component == null)
            throw new ArgumentNullException("component");
        XamlDesignItem site;
        _sites.TryGetValue(component, out site);

        if (findByView) site = site ?? _sites.Values.FirstOrDefault(x => Equals(x.View, component));
        return site;
    }

    public void SetDefaultPropertyValues(DesignItem designItem)
    {
        var values = Metadata.GetDefaultPropertyValues(designItem.ComponentType);
        if (values != null)
            foreach (var value in values)
                designItem.Properties[value.Key].SetValue(value.Value);
    }
    
    public DesignItem RegisterComponentForDesigner(object component)
    {
        return RegisterComponentForDesigner(null, component);
    }

    public DesignItem RegisterComponentForDesigner(DesignItem parent, object component)
    {
        if (component == null)
            component = new NullExtension();
        else if (component is Type type) 
        {
            // TypeExtension is not available in Avalonia, create a simple object representation
            component = type;
        }

        XamlObject parentXamlObject = null;
        if (parent != null)
            parentXamlObject = ((XamlDesignItem)parent).XamlObject;

        var item = new XamlDesignItem(_context.Document.CreateObject(parentXamlObject, component), _context);
        _context.Services.ExtensionManager.ApplyDesignItemInitializers(item);

        if (!(component is string))
            _sites.Add(component, item);
        if (ComponentRegistered != null) ComponentRegistered(this, new DesignItemEventArgs(item));
        return item;
    }

    public DesignItem RegisterComponentForDesignerRecursiveUsingXaml(object component)
    {
        // XamlWriter is not available in Avalonia, use alternative approach
        // For now, register the component directly without XAML serialization
        return RegisterComponentForDesigner(component);
    }

    /// <summary>
    ///     registers components from an existing XAML tree
    /// </summary>
    internal void RaiseComponentRegisteredAndAddedToContainer(DesignItem obj)
    {
        if (ComponentRegisteredAndAddedToContainer != null)
            ComponentRegisteredAndAddedToContainer(this, new DesignItemEventArgs(obj));
    }


    /// <summary>
    ///     registers components from an existing XAML tree
    /// </summary>
    public XamlDesignItem RegisterXamlComponentRecursive(XamlObject obj)
    {
        if (obj == null) return null;

        foreach (var prop in obj.Properties)
        {
            RegisterXamlComponentRecursive(prop.PropertyValue as XamlObject);
            foreach (var val in prop.CollectionElements) RegisterXamlComponentRecursive(val as XamlObject);
        }

        var site = new XamlDesignItem(obj, _context);
        _context.Services.ExtensionManager.ApplyDesignItemInitializers(site);

        _sites.Add(site.Component, site);
        if (ComponentRegistered != null) ComponentRegistered(this, new DesignItemEventArgs(site));

        if (_context.RootItem != null && !string.IsNullOrEmpty(site.Name))
        {
            var nameScope = NameScopeHelper.GetNameScopeFromObject(((XamlDesignItem)_context.RootItem).XamlObject);

            if (nameScope != null)
            {
                // The object will be a part of the RootItem namescope, remove local namescope if set
                NameScopeHelper.ClearNameScopeProperty(obj.Instance);

                var newName = site.Name;
                var actualNameScope = NameScope.GetNameScope((StyledElement)nameScope);
                if (actualNameScope?.Find(newName) != null)
                {
                    var copyIndex = newName.LastIndexOf("_Copy", StringComparison.Ordinal);
                    if (copyIndex < 0)
                    {
                        newName += "_Copy";
                    }
                    else if (!newName.EndsWith("_Copy", StringComparison.Ordinal))
                    {
                        var copyEnd = newName.Substring(copyIndex + "_Copy".Length);
                        int copyEndValue;
                        if (int.TryParse(copyEnd, out copyEndValue))
                            newName = newName.Remove(copyIndex + "_Copy".Length);
                        else
                            newName += "_Copy";
                    }

                    var i = 1;
                    var newNameTemplate = newName;
                    while (actualNameScope?.Find(newName) != null) newName = newNameTemplate + i++;

                    site.Name = newName;
                }

                actualNameScope?.Register(newName, obj.Instance);
            }
        }

        return site;
    }

    /// <summary>
    ///     raises the Property changed Events
    /// </summary>
    internal void RaisePropertyChanged(XamlModelProperty property, object oldValue, object newValue)
    {
        var ev = PropertyChanged;
        if (ev != null)
            ev(this, new DesignItemPropertyChangedEventArgs(property.DesignItem, property, oldValue, newValue));
    }

    /// <summary>
    ///     raises the RaiseComponentRemoved Event
    /// </summary>
    internal void RaiseComponentRemoved(DesignItem item)
    {
        var ev = ComponentRemoved;
        if (ev != null) ev(this, new DesignItemEventArgs(item));
    }

    #region IdentityEqualityComparer

    private sealed class IdentityEqualityComparer : IEqualityComparer<object>
    {
        internal static readonly IdentityEqualityComparer Instance = new();

        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }

        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            return x == y;
        }
    }

    #endregion
}