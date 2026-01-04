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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using System.Xml;

namespace MyDesigner.XamlDom;

/// <summary>
///     Describes a property on a <see cref="XamlObject" />.
/// </summary>
[DebuggerDisplay("XamlProperty: {PropertyName}")]
public sealed class XamlProperty
{
    private static readonly IList<XamlPropertyValue> emptyCollectionElementsArray = new XamlPropertyValue[0];

    private readonly CollectionElementsCollection collectionElements;
    internal readonly XamlPropertyInfo propertyInfo;

    private XmlElement _propertyElement;

    private AvaloniaXamlMember _systemXamlMemberForProperty;

    private AvaloniaXamlType _systemXamlTypeForProperty;
    private XamlPropertyValue propertyValue;

    // for use by parser only
    internal XamlProperty(XamlObject parentObject, XamlPropertyInfo propertyInfo, XamlPropertyValue propertyValue)
        : this(parentObject, propertyInfo)
    {
        PossiblyNameChanged(null, propertyValue);

        this.propertyValue = propertyValue;
        if (propertyValue != null) propertyValue.ParentProperty = this;

        UpdateValueOnInstance();
    }

    internal XamlProperty(XamlObject parentObject, XamlPropertyInfo propertyInfo)
    {
        ParentObject = parentObject;
        this.propertyInfo = propertyInfo;

        if (propertyInfo.IsCollection)
        {
            IsCollection = true;
            collectionElements = new CollectionElementsCollection(this);
            collectionElements.CollectionChanged += OnCollectionChanged;

            if (propertyInfo.Name.Equals(XamlConstants.ResourcesPropertyName, StringComparison.Ordinal) &&
                propertyInfo.ReturnType == typeof(ResourceDictionary))
                IsResources = true;
        }
    }

    /// <summary>
    ///     Gets the parent object for which this property was declared.
    /// </summary>
    public XamlObject ParentObject { get; }

    /// <summary>
    ///     Gets the property name.
    /// </summary>
    public string PropertyName => propertyInfo.Name;

    /// <summary>
    ///     Gets the type the property is declared on.
    /// </summary>
    public Type PropertyTargetType => propertyInfo.TargetType;

    /// <summary>
    ///     Gets if this property is an attached property.
    /// </summary>
    public bool IsAttached => propertyInfo.IsAttached;

    /// <summary>
    ///     Gets if this property is an event.
    /// </summary>
    public bool IsEvent => propertyInfo.IsEvent;

    /// <summary>
    ///     Gets the return type of the property.
    /// </summary>
    public Type ReturnType => propertyInfo.ReturnType;

    /// <summary>
    ///     Gets the type converter used to convert property values to/from string.
    /// </summary>
    public TypeConverter TypeConverter => propertyInfo.TypeConverter;

    /// <summary>
    ///     Gets the category of the property.
    /// </summary>
    public string Category => propertyInfo.Category;

    /// <summary>
    ///     Gets the property info for this property.
    /// </summary>
    public XamlPropertyInfo PropertyInfo => propertyInfo;

    /// <summary>
    ///     Gets the value of the property. Can be null if the property is a collection property.
    /// </summary>
    public XamlPropertyValue PropertyValue
    {
        get => propertyValue;
        set => SetPropertyValue(value);
    }

    /// <summary>
    ///     Gets if the property represents the Control.Resources property that holds a locally-defined resource
    ///     dictionary.
    /// </summary>
    public bool IsResources { get; }

    /// <summary>
    ///     Gets if the property is a collection property.
    /// </summary>
    public bool IsCollection { get; }

    /// <summary>
    ///     Gets the collection elements of the property. Is empty if the property is not a collection.
    /// </summary>
    public IList<XamlPropertyValue> CollectionElements => collectionElements ?? emptyCollectionElementsArray;

    /// <summary>
    ///     Gets if the property is set.
    /// </summary>
    public bool IsSet =>
        propertyValue != null ||
        _propertyElement != null; // collection

    /// <summary>
    ///     Gets a <see cref="AvaloniaXamlMember" /> representing the property.
    /// </summary>
    public AvaloniaXamlMember SystemXamlMemberForProperty
    {
        get
        {
            if (_systemXamlMemberForProperty == null)
                _systemXamlMemberForProperty = new AvaloniaXamlMember(PropertyName, SystemXamlTypeForProperty, false);
            return _systemXamlMemberForProperty;
        }
    }

    /// <summary>
    ///     Gets a <see cref="AvaloniaXamlType" /> representing the type the property is declared on.
    /// </summary>
    public AvaloniaXamlType SystemXamlTypeForProperty
    {
        get
        {
            if (_systemXamlTypeForProperty == null)
                _systemXamlTypeForProperty = new AvaloniaXamlType(PropertyTargetType);
            return _systemXamlTypeForProperty;
        }
    }

    /// <summary>
    ///     Gets/Sets the value of the property on the instance without updating the XAML document.
    /// </summary>
    public object ValueOnInstance
    {
        get
        {
            if (IsEvent)
            {
                if (propertyValue != null)
                    return propertyValue.GetValueFor(null);
                return null;
            }

            var value = propertyInfo.GetValue(ParentObject.Instance);
            return value;
        }
        set
        {
            var setValue = value;
            if (propertyInfo.ReturnType == typeof(Uri))
                setValue = ParentObject.OwnerDocument.TypeFinder.ConvertUriToLocalUri((Uri)value);

            propertyInfo.SetValue(ParentObject.Instance, setValue);
            if (ValueOnInstanceChanged != null)
                ValueOnInstanceChanged(this, EventArgs.Empty);
        }
    }

    /// <summary>
    ///     Gets/Sets the value of the property in the designer without updating the XAML document.
    /// </summary>
    public object DesignerValue
    {
        get
        {
            if (IsEvent)
            {
                if (propertyValue != null)
                    return propertyValue.GetValueFor(null);
                return null;
            }

            try
            {
                if (propertyValue != null)
                {
                    var wr = propertyValue.GetValueFor(propertyInfo);
                    return wr;
                }
            }
            catch (Exception)
            {
            }

            var value = propertyInfo.GetValue(ParentObject.Instance);
            return value;
        }
        set => ValueOnInstance = value;
    }

    /// <summary>
    ///     Gets the value of the text property on the instance without updating the XAML document.
    /// </summary>
    public string TextValueOnInstance
    {
        get
        {
            if (propertyValue is XamlTextValue)
                return propertyValue.GetValueFor(null) as string;
            return null;
        }
    }

    /// <summary>
    ///     Gets if this property is considered "advanced" and should be hidden by default in a property grid.
    /// </summary>
    public bool IsAdvanced => propertyInfo.IsAdvanced;

    /// <summary>
    ///     Gets the avalonia property.
    /// </summary>
    public AvaloniaProperty DependencyProperty => propertyInfo.DependencyProperty;

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // If implicit collection that is now empty we remove markup for the property if still there.
        if (collectionElements.Count == 0 && propertyValue == null && _propertyElement != null)
        {
            _propertyElement.ParentNode.RemoveChild(_propertyElement);
            _propertyElement = null;

            ParentObject.OnPropertyChanged(this);

            if (IsSetChanged != null) IsSetChanged(this, EventArgs.Empty);
            if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
        }
    }

    /// <summary>
    ///     Occurs when the value of the IsSet property has changed.
    /// </summary>
    public event EventHandler IsSetChanged;

    /// <summary>
    ///     Occurs when the value of the property has changed.
    /// </summary>
    public event EventHandler ValueChanged;

    /// <summary>
    ///     Occurs when MarkupExtension evaluated PropertyValue dosn't changed but ValueOnInstance does.
    /// </summary>
    public event EventHandler ValueOnInstanceChanged;

    private void SetPropertyValue(XamlPropertyValue value)
    {
        // Binding...
        //if (IsCollection) {
        //    throw new InvalidOperationException("Cannot set the value of collection properties.");
        //}

        var wasSet = IsSet;

        PossiblyNameChanged(propertyValue, value);

        //reset expression
        var xamlObject = propertyValue as XamlObject;
        if (xamlObject != null && xamlObject.IsMarkupExtension)
            propertyInfo.ResetValue(ParentObject.Instance);

        ResetInternal();

        propertyValue = value;
        if (propertyValue != null)
        {
            propertyValue.ParentProperty = this;
            propertyValue.AddNodeTo(this);
        }

        UpdateValueOnInstance();

        ParentObject.OnPropertyChanged(this);

        if (!wasSet)
            if (IsSetChanged != null)
                IsSetChanged(this, EventArgs.Empty);

        if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
    }

    internal void UpdateValueOnInstance()
    {
        if (PropertyValue != null)
        {
            try
            {
                ValueOnInstance = PropertyValue.GetValueFor(propertyInfo);

                if (ParentObject.XamlSetTypeConverter != null && propertyValue is XamlTextValue)
                    ParentObject.XamlSetTypeConverter(ParentObject.Instance,
                        new AvaloniaXamlSetTypeConverterEventArgs(SystemXamlMemberForProperty, null,
                            ((XamlTextValue)propertyValue).Text,
                            ParentObject.OwnerDocument.GetTypeDescriptorContext(ParentObject)));

                if (propertyInfo.DependencyProperty == DesignTimeProperties.DesignWidthProperty)
                {
                    var widthProperty =
                        ParentObject.Properties.FirstOrDefault(x =>
                            x.DependencyProperty == Control.WidthProperty);
                    if (widthProperty == null || !widthProperty.IsSet)
                        ((Control)ParentObject.Instance).Width = (double)ValueOnInstance;
                }

                if (propertyInfo.DependencyProperty == DesignTimeProperties.DesignHeightProperty)
                {
                    var heightProperty =
                        ParentObject.Properties.FirstOrDefault(x =>
                            x.DependencyProperty == Control.HeightProperty);
                    if (heightProperty == null || !heightProperty.IsSet)
                        ((Control)ParentObject.Instance).Height = (double)ValueOnInstance;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdateValueOnInstance() failed - Exception:" + ex.Message);
            }
        }
        else if (IsCollection)
        {
            var list = ValueOnInstance as IList;
            if (list != null)
            {
                list.Clear();
                foreach (var item in CollectionElements)
                {
                    var newValue = item.GetValueFor(propertyInfo);
                    list.Add(newValue);
                }
            }
        }
    }
    
    /// <summary>
    ///     Resets the properties value.
    /// </summary>
    public void Reset()
    {
        if (IsSet)
        {
            propertyInfo.ResetValue(ParentObject.Instance);
            ResetInternal();

            ParentObject.OnPropertyChanged(this);

            if (IsSetChanged != null) IsSetChanged(this, EventArgs.Empty);
            if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
        }
    }

    private void ResetInternal()
    {
        var isExplicitCollection = false;

        if (propertyValue != null)
        {
            isExplicitCollection = IsCollection;

            propertyValue.RemoveNodeFromParent();
            propertyValue.ParentProperty = null;
            propertyValue = null;
        }

        if (_propertyElement != null)
        {
            Debug.Assert(!isExplicitCollection || _propertyElement.ParentNode == null);

            if (!isExplicitCollection) _propertyElement.ParentNode.RemoveChild(_propertyElement);
            _propertyElement = null;
        }
    }

    internal void ParserSetPropertyElement(XmlElement propertyElement)
    {
        var oldPropertyElement = _propertyElement;
        if (oldPropertyElement == propertyElement) return;

        _propertyElement = propertyElement;

        if (oldPropertyElement != null && IsCollection)
        {
            Debug.WriteLine("Property element for " + PropertyName + " already exists, merging..");
            foreach (var val in collectionElements)
            {
                val.RemoveNodeFromParent();
                val.AddNodeTo(this);
            }

            oldPropertyElement.ParentNode.RemoveChild(oldPropertyElement);
        }
    }

    private bool IsFirstChildResources(XamlObject obj)
    {
        return obj.XmlElement.FirstChild != null &&
               obj.XmlElement.FirstChild.Name.EndsWith("." + XamlConstants.ResourcesPropertyName) &&
               obj.Properties.Where(prop => prop.IsResources).FirstOrDefault() != null;
    }

    private XmlElement CreatePropertyElement()
    {
        var propertyElementType = GetPropertyElementType();
        var ns = ParentObject.OwnerDocument.GetNamespaceFor(propertyElementType);
        return ParentObject.OwnerDocument.XmlDocument.CreateElement(
            ParentObject.OwnerDocument.GetPrefixForNamespace(ns),
            propertyElementType.Name + "." + PropertyName,
            ns
        );
    }

    private Type GetPropertyElementType()
    {
        return IsAttached ? PropertyTargetType : ParentObject.ElementType;
    }

    private static XmlNode FindChildNode(XmlNode node, Type elementType, string propertyName, XamlDocument xamlDocument)
    {
        var localName = elementType.Name + "." + propertyName;
        var namespacesURI = xamlDocument.GetNamespacesFor(elementType);
        var clrNamespaceURI = xamlDocument.GetNamespaceFor(elementType, true);

        foreach (XmlNode childNode in node.ChildNodes)
            if (childNode.LocalName == localName && (namespacesURI.Contains(childNode.NamespaceURI) ||
                                                     childNode.NamespaceURI == clrNamespaceURI))
                return childNode;

        var type = elementType.BaseType;
        namespacesURI = xamlDocument.GetNamespacesFor(type);

        while (type != typeof(object))
        {
            if (type.GetProperty(propertyName) == null)
                break;

            localName = type.Name + "." + propertyName;

            foreach (XmlNode childNode in node.ChildNodes)
                if (childNode.LocalName == localName && namespacesURI.Contains(childNode.NamespaceURI))
                    return childNode;

            type = type.BaseType;
        }

        return null;
    }

    private bool IsNodeCollectionForThisProperty(XmlNode node)
    {
        //Remove the commented check! This is Possible: BeginStoryboard=>The COntent Property is Storyboard, and the Content Element is also Storyboard!
        return _propertyElement == null /* && this.PropertyName != this.ParentObject.ContentPropertyName */ &&
               ReturnType.IsAssignableFrom(
                   ParentObject.OwnerDocument.TypeFinder.GetType(node.NamespaceURI, node.LocalName));
    }

    internal void AddChildNodeToProperty(XmlNode newChildNode)
    {
        if (IsCollection)
        {
            if (IsNodeCollectionForThisProperty(newChildNode))
            {
                var propertyElementType = GetPropertyElementType();
                var parentNode = FindChildNode(ParentObject.XmlElement, propertyElementType, PropertyName,
                    ParentObject.OwnerDocument);

                if (parentNode == null)
                {
                    parentNode = CreatePropertyElement();

                    ParentObject.XmlElement.AppendChild(parentNode);
                }
                else if (parentNode.ChildNodes.Cast<XmlNode>().Where(x => !(x is XmlWhitespace)).Count() > 0)
                {
                    throw new XamlLoadException(
                        "Collection property node must have no children when adding collection element.");
                }

                parentNode.AppendChild(newChildNode);
                _propertyElement = (XmlElement)newChildNode;
            }
            else
            {
                // this is the default collection
                InsertNodeInCollection(newChildNode, collectionElements.Count);
            }

            return;
        }

        if (_propertyElement == null)
        {
            if (PropertyName == ParentObject.ContentPropertyName)
            {
                if (IsFirstChildResources(ParentObject))
                    // Resources element should always be first
                    ParentObject.XmlElement.InsertAfter(newChildNode, ParentObject.XmlElement.FirstChild);
                else
                    ParentObject.XmlElement.InsertBefore(newChildNode, ParentObject.XmlElement.FirstChild);
                return;
            }

            _propertyElement = CreatePropertyElement();

            if (IsFirstChildResources(ParentObject))
                // Resources element should always be first
                ParentObject.XmlElement.InsertAfter(_propertyElement, ParentObject.XmlElement.FirstChild);
            else
                ParentObject.XmlElement.InsertBefore(_propertyElement, ParentObject.XmlElement.FirstChild);
        }

        _propertyElement.AppendChild(newChildNode);
    }

    internal void InsertNodeInCollection(XmlNode newChildNode, int index)
    {
        Debug.Assert(index >= 0 && index <= collectionElements.Count);
        var collection = _propertyElement;
        if (collection == null)
        {
            if (collectionElements.Count == 0 && PropertyName != ParentObject.ContentPropertyName)
            {
                // we have to create the collection element
                _propertyElement = CreatePropertyElement();

                if (IsResources)
                    ParentObject.XmlElement.PrependChild(_propertyElement);
                else
                    ParentObject.XmlElement.AppendChild(_propertyElement);

                collection = _propertyElement;
            }
            else
            {
                // this is the default collection
                collection = ParentObject.XmlElement;
            }
        }

        if (collectionElements.Count == 0)
            // collection is empty -> we may insert anywhere
            collection.AppendChild(newChildNode);
        else if (index == collectionElements.Count)
            // insert after last element in collection
            collection.InsertAfter(newChildNode,
                collectionElements[collectionElements.Count - 1].GetNodeForCollection());
        else
            // insert before specified index
            collection.InsertBefore(newChildNode, collectionElements[index].GetNodeForCollection());
    }

    internal XmlAttribute SetAttribute(string value)
    {
        string name;
        var element = ParentObject.XmlElement;

        if (IsAttached)
        {
            if (PropertyTargetType == typeof(DesignTimeProperties)
                || PropertyTargetType == typeof(MarkupCompatibilityProperties)
                || PropertyTargetType == typeof(XamlNamespaceProperties))
                name = PropertyName;
            else
                name = PropertyTargetType.Name + "." + PropertyName;

            var ns = ParentObject.OwnerDocument.GetNamespaceFor(PropertyTargetType);
            var prefix = element.GetPrefixOfNamespace(ns);

            if (string.IsNullOrEmpty(prefix)) prefix = ParentObject.OwnerDocument.GetPrefixForNamespace(ns);
            var existingNameSpace = element.GetNamespaceOfPrefix(prefix);

            if (!string.IsNullOrEmpty(prefix) || existingNameSpace != ns)
            {
                element.SetAttribute(name, ns, value);
                return element.GetAttributeNode(name, ns);
            }
        }
        else
        {
            name = PropertyName;
        }

        element.SetAttribute(name, string.Empty, value);
        return element.GetAttributeNode(name);
    }

    internal string GetNameForMarkupExtension()
    {
        if (IsAttached)
        {
            var name = PropertyTargetType.Name + "." + PropertyName;

            var element = ParentObject.XmlElement;
            var ns = ParentObject.OwnerDocument.GetNamespaceFor(PropertyTargetType);
            var prefix = element.GetPrefixOfNamespace(ns);
            if (string.IsNullOrEmpty(prefix))
                return name;
            return prefix + ":" + name;
        }

        return PropertyName;
    }

    /// <summary>
    ///     used internally by the XamlParser.
    ///     Add a collection element that already is part of the XML DOM.
    /// </summary>
    internal void ParserAddCollectionElement(XmlElement collectionPropertyElement, XamlPropertyValue val)
    {
        if (collectionPropertyElement != null && _propertyElement == null)
            ParserSetPropertyElement(collectionPropertyElement);
        collectionElements.AddInternal(val);
        val.ParentProperty = this;
        if (collectionPropertyElement != _propertyElement)
        {
            val.RemoveNodeFromParent();
            val.AddNodeTo(this);
        }
    }

    private void PossiblyNameChanged(XamlPropertyValue oldValue, XamlPropertyValue newValue)
    {
        if (ParentObject.RuntimeNameProperty != null && PropertyName == ParentObject.RuntimeNameProperty)
        {
            if (!string.IsNullOrEmpty(ParentObject.GetXamlAttribute("Name")))
                throw new XamlLoadException("The property 'Name' is set more than once.");

            string oldName = null;
            string newName = null;

            var oldTextValue = oldValue as XamlTextValue;
            if (oldTextValue != null) oldName = oldTextValue.Text;

            var newTextValue = newValue as XamlTextValue;
            if (newTextValue != null) newName = newTextValue.Text;

            NameScopeHelper.NameChanged(ParentObject, oldName, newName);
        }
    }

    /*public bool IsAttributeSyntax {
        get {
            return attribute != null;
        }
    }

    public bool IsElementSyntax {
        get {
            return element != null;
        }
    }

    public bool IsImplicitDefaultProperty {
        get {
            return attribute == null && element == null;
        }
    }*/

    /// <inheritdoc />
    public T GetValueOnInstance<T>()
    {
        var obj = ValueOnInstance;

        if (obj == null) return default;

        if (obj is T typed) return typed;

        if (TypeConverter.CanConvertTo(typeof(T))) return (T)TypeConverter.ConvertTo(obj, typeof(T));

        return (T)obj;
    }
}