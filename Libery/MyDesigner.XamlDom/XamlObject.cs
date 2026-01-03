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
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Metadata;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Xml;

namespace MyDesigner.XamlDom;

/// <summary>
///     Represents a xaml object element.
/// </summary>
[DebuggerDisplay("XamlObject: {Instance}")]
public sealed class XamlObject : XamlPropertyValue
{
    private readonly List<XamlProperty> properties = new();

    private AvaloniaXamlType _systemXamlTypeForProperty;
    private XamlProperty nameProperty;

    private MarkupExtensionWrapper wrapper;

    private XmlAttribute xmlAttribute;

    /// <summary>For use by XamlParser only.</summary>
    internal XamlObject(XamlDocument document, XmlElement element, Type elementType, object instance)
    {
        OwnerDocument = document;
        XmlElement = element;
        ElementType = elementType;
        Instance = instance;

        ContentPropertyName = GetContentPropertyName(elementType);
        XamlSetTypeConverter = GetTypeConverterDelegate(elementType);

        ServiceProvider = new XamlObjectServiceProvider(this);
        CreateWrapper();

        var rnpAttrs =
            elementType.GetCustomAttributes(typeof(RuntimeNamePropertyAttribute), true) as RuntimeNamePropertyAttribute
                [];
        if (rnpAttrs != null && rnpAttrs.Length > 0 && !string.IsNullOrEmpty(rnpAttrs[0].Name))
            RuntimeNameProperty = rnpAttrs[0].Name;
    }

    /// <summary>
    ///     Gets the parent object.
    /// </summary>
    public XamlObject ParentObject { get; internal set; }

    public XmlElement XmlElement { get; private set; }

    public PositionXmlElement PositionXmlElement => XmlElement as PositionXmlElement;

    internal XmlAttribute XmlAttribute
    {
        get => xmlAttribute;
        set
        {
            xmlAttribute = value;
            XmlElement = VirtualAttachTo(XmlElement, value.OwnerElement);
        }
    }

    internal TypeConverterDelegate XamlSetTypeConverter { get; private set; }

    /// <summary>
    ///     Gets a <see cref="AvaloniaXamlType" /> representing the <see cref="XamlObject.ElementType" />.
    /// </summary>
    public AvaloniaXamlType SystemXamlTypeForProperty
    {
        get
        {
            if (_systemXamlTypeForProperty == null)
                _systemXamlTypeForProperty = new AvaloniaXamlType(ElementType);
            return _systemXamlTypeForProperty;
        }
    }

    /// <summary>
    ///     Gets the XamlDocument where this XamlObject is declared in.
    /// </summary>
    public XamlDocument OwnerDocument { get; }

    /// <summary>
    ///     Gets the instance created by this object element.
    /// </summary>
    public object Instance { get; }

    /// <summary>
    ///     Gets whether this instance represents a MarkupExtension.
    /// </summary>
    public bool IsMarkupExtension => Instance is IMarkupExtension;

    /// <summary>
    ///     Gets whether there were load errors for this object.
    /// </summary>
    public bool HasErrors { get; internal set; }

    /// <summary>
    ///     Gets the type of this object element.
    /// </summary>
    public Type ElementType { get; }

    /// <summary>
    ///     Gets a read-only collection of properties set on this XamlObject.
    ///     This includes both attribute and element properties.
    /// </summary>
    public IList<XamlProperty> Properties => properties.AsReadOnly();

    /// <summary>
    ///     Gets the name of the content property.
    /// </summary>
    public string ContentPropertyName { get; }

    /// <summary>
    ///     Gets which property name of the type maps to the XAML x:Name attribute.
    /// </summary>
    public string RuntimeNameProperty { get; }

    /// <summary>
    ///     Gets which property of the type maps to the XAML x:Name attribute.
    /// </summary>
    public XamlProperty NameProperty
    {
        get
        {
            if (nameProperty == null && RuntimeNameProperty != null)
                nameProperty = FindOrCreateProperty(RuntimeNameProperty);

            return nameProperty;
        }
    }

    /// <summary>
    ///     Gets/Sets the name of this XamlObject.
    /// </summary>
    public string Name
    {
        get
        {
            var name = GetXamlAttribute("Name");

            if (string.IsNullOrEmpty(name))
                if (NameProperty != null && NameProperty.IsSet)
                    name = (string)NameProperty.ValueOnInstance;

            if (name == string.Empty)
                name = null;

            return name;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
                SetXamlAttribute("Name", null);
            else
                SetXamlAttribute("Name", value);
        }
    }

    /// <summary>
    ///     Gets/Sets the <see cref="XamlObjectServiceProvider" /> associated with this XamlObject.
    /// </summary>
    public XamlObjectServiceProvider ServiceProvider { get; set; }

    /// <summary>For use by XamlParser only.</summary>
    internal void AddProperty(XamlProperty property)
    {
#if DEBUG
        if (!property.IsAttached)
            foreach (var p in properties)
                if (!p.IsAttached && p.PropertyName == property.PropertyName)
                    throw new XamlLoadException("duplicate property:" + property.PropertyName);
#endif
        properties.Add(property);
    }

    internal override void OnParentPropertyChanged()
    {
        ParentObject = ParentProperty != null ? ParentProperty.ParentObject : null;
        base.OnParentPropertyChanged();
    }

    private string GetPrefixOfNamespace(string ns, XmlElement target)
    {
        if (target.NamespaceURI == ns)
            return null;
        var prefix = target.GetPrefixOfNamespace(ns);
        if (!string.IsNullOrEmpty(prefix))
            return prefix;
        var obj = this;
        while (obj != null)
        {
            prefix = obj.XmlElement.GetPrefixOfNamespace(ns);
            if (!string.IsNullOrEmpty(prefix))
                return prefix;
            obj = obj.ParentObject;
        }

        return null;
    }

    private XmlElement VirtualAttachTo(XmlElement e, XmlElement target)
    {
        var prefix = GetPrefixOfNamespace(e.NamespaceURI, target);

        var newElement = e.OwnerDocument.CreateElement(prefix, e.LocalName, e.NamespaceURI);

        foreach (XmlAttribute a in target.Attributes)
            if (a.Prefix == "xmlns" || a.Name == "xmlns")
                newElement.Attributes.Append(a.Clone() as XmlAttribute);

        while (e.HasChildNodes) newElement.AppendChild(e.FirstChild);

        var ac = e.Attributes;
        while (ac.Count > 0) newElement.Attributes.Append(ac[0]);

        return newElement;
    }

    /// <summary>
    ///     Gets the name of the content property for the specified element type, or null if not available.
    /// </summary>
    /// <param name="elementType">The element type to get the content property name for.</param>
    /// <returns>The name of the content property for the specified element type, or null if not available.</returns>
    internal static string GetContentPropertyName(Type elementType)
    {
        return elementType.GetProperties()
                     .FirstOrDefault(p => p.GetCustomAttribute<ContentAttribute>() != null)
                     ?.Name;


        return null;
    }

    internal static TypeConverterDelegate GetTypeConverterDelegate(Type elementType)
    {
        var attrs =
            elementType.GetCustomAttributes(typeof(XamlSetTypeConverterAttribute), true) as
                XamlSetTypeConverterAttribute[];
        if (attrs != null && attrs.Length > 0)
        {
            var name = attrs[0].XamlSetTypeConverterHandler;
            var method =
                elementType.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            return (TypeConverterDelegate)TypeConverterDelegate.CreateDelegate(typeof(TypeConverterDelegate), method);
        }

        return null;
    }

    internal override void AddNodeTo(XamlProperty property)
    {
        XamlObject holder;
        if (!UpdateXmlAttribute(true, out holder)) property.AddChildNodeToProperty(XmlElement);
        UpdateMarkupExtensionChain();
    }

    internal override void RemoveNodeFromParent()
    {
        if (XmlAttribute != null)
        {
            XmlAttribute.OwnerElement.RemoveAttribute(XmlAttribute.Name);
            xmlAttribute = null;
        }
        else
        {
            XamlObject holder;
            if (!UpdateXmlAttribute(false, out holder)) XmlElement.ParentNode.RemoveChild(XmlElement);
        }
        //TODO: PropertyValue still there
        //UpdateMarkupExtensionChain();
    }

    //TODO: reseting path property for binding doesn't work in XamlProperty
    //use CanResetValue()
    internal void OnPropertyChanged(XamlProperty property)
    {
        XamlObject holder;
        if (!UpdateXmlAttribute(false, out holder))
            if (holder != null &&
                holder.XmlAttribute != null)
            {
                holder.XmlAttribute.OwnerElement.RemoveAttributeNode(holder.XmlAttribute);
                holder.xmlAttribute = null;
                holder.ParentProperty.AddChildNodeToProperty(holder.XmlElement);

                var isThisUpdated = false;
                foreach (var propXamlObject in holder.Properties.Where(prop => prop.IsSet)
                             .Select(prop => prop.PropertyValue).OfType<XamlObject>())
                {
                    XamlObject innerHolder;
                    var updateResult = propXamlObject.UpdateXmlAttribute(true, out innerHolder);
                    Debug.Assert(updateResult || innerHolder == null);

                    if (propXamlObject == this)
                        isThisUpdated = true;
                }

                if (!isThisUpdated)
                    UpdateXmlAttribute(true, out holder);
            }

        UpdateMarkupExtensionChain();

        if (!XmlElement.HasChildNodes && !XmlElement.IsEmpty) XmlElement.IsEmpty = true;

        if (property == NameProperty)
            if (NameChanged != null)
                NameChanged(this, EventArgs.Empty);
    }

    private void UpdateChildMarkupExtensions(XamlObject obj)
    {
        foreach (var prop in obj.Properties)
            if (prop.IsSet)
            {
                var propXamlObject = prop.PropertyValue as XamlObject;
                if (propXamlObject != null) UpdateChildMarkupExtensions(propXamlObject);
            }
            else if (prop.IsCollection)
            {
                foreach (var propXamlObject in prop.CollectionElements.OfType<XamlObject>())
                    UpdateChildMarkupExtensions(propXamlObject);
            }

        if (obj.IsMarkupExtension && obj.ParentProperty != null) obj.ParentProperty.UpdateValueOnInstance();
    }

    private void UpdateMarkupExtensionChain()
    {
        UpdateChildMarkupExtensions(this);

        var obj = ParentObject;
        while (obj != null && obj.IsMarkupExtension && obj.ParentProperty != null)
        {
            obj.ParentProperty.UpdateValueOnInstance();
            obj = obj.ParentObject;
        }
    }

    private bool UpdateXmlAttribute(bool force, out XamlObject holder)
    {
        holder = FindXmlAttributeHolder();
        if (holder == null && force && IsMarkupExtension) holder = this;
        if (holder != null && MarkupExtensionPrinter.CanPrint(holder))
        {
            var s = MarkupExtensionPrinter.Print(holder);
            holder.XmlAttribute = holder.ParentProperty.SetAttribute(s);
            return true;
        }

        return false;
    }

    private XamlObject FindXmlAttributeHolder()
    {
        var obj = this;
        while (obj != null && obj.IsMarkupExtension)
        {
            if (obj.XmlAttribute != null) return obj;
            obj = obj.ParentObject;
        }

        return null;
    }

    /// <summary>
    ///     Finds the specified property, or creates it if it doesn't exist.
    /// </summary>
    public XamlProperty FindOrCreateProperty(string propertyName)
    {
        if (propertyName == null)
            throw new ArgumentNullException("propertyName");

        //			if (propertyName == ContentPropertyName)
        //				return

        foreach (var p in properties)
            if (!p.IsAttached && p.PropertyName == propertyName)
                return p;
        var propertyDescriptors = TypeDescriptor.GetProperties(Instance);
        var propertyInfo = propertyDescriptors[propertyName];
        XamlProperty newProperty;

        if (propertyInfo == null)
        {
            propertyDescriptors = TypeDescriptor.GetProperties(ElementType);
            propertyInfo = propertyDescriptors[propertyName];
        }

        if (propertyInfo != null)
        {
            newProperty = new XamlProperty(this, new XamlNormalPropertyInfo(propertyInfo));
        }
        else
        {
            var events = TypeDescriptor.GetEvents(Instance);
            var eventInfo = events[propertyName];

            if (eventInfo == null)
            {
                events = TypeDescriptor.GetEvents(ElementType);
                eventInfo = events[propertyName];
            }

            if (eventInfo != null)
                newProperty = new XamlProperty(this, new XamlEventPropertyInfo(eventInfo));
            else
                throw new ArgumentException(
                    "The property '" + propertyName + "' doesn't exist on " + ElementType.FullName, "propertyName");
        }

        properties.Add(newProperty);
        return newProperty;
    }

    /// <summary>
    ///     Finds the specified property, or creates it if it doesn't exist.
    /// </summary>
    public XamlProperty FindOrCreateAttachedProperty(Type ownerType, string propertyName)
    {
        if (ownerType == null)
            throw new ArgumentNullException("ownerType");
        if (propertyName == null)
            throw new ArgumentNullException("propertyName");

        foreach (var p in properties)
            if (p.IsAttached && p.PropertyTargetType == ownerType && p.PropertyName == propertyName)
                return p;
        var info = XamlParser.TryFindAttachedProperty(ownerType, propertyName);
        if (info == null)
            throw new ArgumentException(
                "The attached property '" + propertyName + "' doesn't exist on " + ownerType.FullName, "propertyName");
        var newProperty = new XamlProperty(this, info);
        properties.Add(newProperty);
        return newProperty;
    }

    /// <summary>
    ///     Gets an attribute in the x:-namespace.
    /// </summary>
    public string GetXamlAttribute(string name)
    {
        var value = XmlElement.GetAttribute(name, XamlConstants.XamlNamespace);

        if (string.IsNullOrEmpty(value)) value = XmlElement.GetAttribute(name, XamlConstants.Xaml2009Namespace);

        return value;
    }

    /// <summary>
    ///     Sets an attribute in the x:-namespace.
    /// </summary>
    public void SetXamlAttribute(string name, string value)
    {
        XamlProperty runtimeNameProperty = null;
        var isNameChange = false;

        if (name == "Name")
        {
            isNameChange = true;
            var oldName = GetXamlAttribute("Name");

            if (string.IsNullOrEmpty(oldName))
            {
                runtimeNameProperty = NameProperty;
                if (runtimeNameProperty != null)
                {
                    if (runtimeNameProperty.IsSet)
                        oldName = (string)runtimeNameProperty.ValueOnInstance;
                    else
                        runtimeNameProperty = null;
                }
            }

            if (string.IsNullOrEmpty(oldName))
                oldName = null;

            NameScopeHelper.NameChanged(this, oldName, value);
        }

        if (value == null)
        {
            XmlElement.RemoveAttribute(name, XamlConstants.XamlNamespace);
        }
        else
        {
            var prefix = XmlElement.GetPrefixOfNamespace(XamlConstants.XamlNamespace);
            var prefix2009 = XmlElement.GetPrefixOfNamespace(XamlConstants.Xaml2009Namespace);

            if (!string.IsNullOrEmpty(prefix))
            {
                var attribute = XmlElement.OwnerDocument.CreateAttribute(prefix, name, XamlConstants.XamlNamespace);
                attribute.InnerText = value;
                XmlElement.SetAttributeNode(attribute);
            }
            else if (!string.IsNullOrEmpty(prefix2009))
            {
                var attribute = XmlElement.OwnerDocument.CreateAttribute(prefix, name, XamlConstants.Xaml2009Namespace);
                attribute.InnerText = value;
                XmlElement.SetAttributeNode(attribute);
            }
            else
            {
                XmlElement.SetAttribute(name, XamlConstants.XamlNamespace, value);
            }
        }

        if (isNameChange)
        {
            var nameChangedAlreadyRaised = false;
            if (runtimeNameProperty != null)
            {
                var handler = new EventHandler((sender, e) => nameChangedAlreadyRaised = true);
                NameChanged += handler;

                try
                {
                    runtimeNameProperty.Reset();
                }
                finally
                {
                    NameChanged -= handler;
                }
            }

            if (NameChanged != null && !nameChangedAlreadyRaised)
                NameChanged(this, EventArgs.Empty);
        }
    }

    private void CreateWrapper()
    {
        if (Instance is IBinding)
            wrapper = new BindingWrapper(this);
        else if (Instance is StaticResourceExtension) wrapper = new StaticResourceWrapper(this);

        if (wrapper == null && IsMarkupExtension)
        {
            var markupExtensionWrapperAttribute =
                Instance.GetType().GetCustomAttributes(typeof(MarkupExtensionWrapperAttribute), false).FirstOrDefault()
                    as MarkupExtensionWrapperAttribute;
            if (markupExtensionWrapperAttribute != null)
                wrapper = MarkupExtensionWrapper.CreateWrapper(
                    markupExtensionWrapperAttribute.MarkupExtensionWrapperType, this);
            else
                wrapper = MarkupExtensionWrapper.TryCreateWrapper(Instance.GetType(), this);
        }
    }

    private object ProvideValue()
    {
        if (wrapper != null) return wrapper.ProvideValue();
        if (ParentObject != null && ParentObject.ElementType == typeof(Setter) &&
            ElementType == typeof(DynamicResourceExtension))
            return Instance;
        
        // In Avalonia, IBinding doesn't have ProvideValue method
        // We need to handle this differently
        if (Instance is IBinding binding)
        {
            // For bindings, we return the binding itself
            return binding;
        }
        
        return (Instance as IMarkupExtension)?.ProvideValue(ServiceProvider) ?? Instance;
    }

    internal string GetNameForMarkupExtension()
    {
        var markupExtensionName = XmlElement.Name;

        // By convention a markup extension class name typically includes an "Extension" suffix.
        // When you reference the markup extension in XAML the "Extension" suffix is optional.
        // If present remove it to avoid bloating the XAML.
        if (markupExtensionName.EndsWith("Extension", StringComparison.Ordinal))
            markupExtensionName = markupExtensionName.Substring(0, markupExtensionName.Length - 9);

        return markupExtensionName;
    }

    /// <summary>
    ///     Is raised when the name of this XamlObject changes.
    /// </summary>
    public event EventHandler NameChanged;

    internal delegate void TypeConverterDelegate(object targetObject, AvaloniaXamlSetTypeConverterEventArgs eventArgs);

    #region XamlPropertyValue implementation

    internal override object GetValueFor(XamlPropertyInfo targetProperty)
    {
        if (IsMarkupExtension)
        {
            var value = ProvideValue();
            if (value is string && targetProperty != null && targetProperty.ReturnType != typeof(string))
                return XamlParser.CreateObjectFromAttributeText((string)value, targetProperty, this);
            return value;
        }

        return Instance;
    }

    internal override XmlNode GetNodeForCollection()
    {
        return XmlElement;
    }

    #endregion
}

internal class BindingWrapper : MarkupExtensionWrapper
{
    public BindingWrapper(XamlObject xamlObject)
        : base(xamlObject)
    {
    }

    public override object ProvideValue()
    {
        var target = XamlObject.Instance as IBinding;
        Debug.Assert(target != null);
        //TODO: XamlObject.Clone()
        var b = CopyBinding(target);
        // In Avalonia, IBinding doesn't have ProvideValue method
        // We return the binding itself
        return b;
    }

    private IBinding CopyBinding(IBinding target)
    {
        IBinding b;
        if (target != null)
            b = (IBinding)Activator.CreateInstance(target.GetType());
        else
            b = new Binding();

        foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(target))
        {
            if (pd.IsReadOnly)
            {
                if (pd.Name.Equals("Bindings", StringComparison.Ordinal))
                {
                    var bindings = (Collection<IBinding>)pd.GetValue(target);
                    var newBindings = (Collection<IBinding>)pd.GetValue(b);

                    foreach (var binding in bindings) newBindings.Add(CopyBinding(binding));
                }

                continue;
            }

            try
            {
                var val1 = pd.GetValue(b);
                var val2 = pd.GetValue(target);
                if (Equals(val1, val2)) continue;
                pd.SetValue(b, val2);
            }
            catch
            {
            }
        }

        return b;
    }
}

internal class StaticResourceWrapper : MarkupExtensionWrapper
{
    public StaticResourceWrapper(XamlObject xamlObject)
        : base(xamlObject)
    {
    }

    public override object ProvideValue()
    {
        var target = XamlObject.Instance as StaticResourceExtension;
        return XamlObject.ServiceProvider.Resolver.FindResource(target.ResourceKey);
    }
}