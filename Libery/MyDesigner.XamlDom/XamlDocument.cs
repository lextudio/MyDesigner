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
using System.IO;
using System.Reflection;
using Avalonia.Media;
using Avalonia.Markup.Xaml;
using System.Xml;

namespace MyDesigner.XamlDom;

/// <summary>
///     Represents a .xaml document.
/// </summary>
public sealed class XamlDocument
{
    private static readonly Dictionary<Color, string> _colorBrushDictionary;

    private int namespacePrefixCounter;

    static XamlDocument()
    {
        _colorBrushDictionary = new Dictionary<Color, string>();
        foreach (var brushProp in typeof(Brushes).GetProperties(BindingFlags.Static | BindingFlags.Public)
                     .Where(p => p.PropertyType == typeof(SolidColorBrush)))
        {
            var brush = brushProp.GetValue(null, null) as SolidColorBrush;
            if (brush != null && !_colorBrushDictionary.ContainsKey(brush.Color)) 
                _colorBrushDictionary.Add(brush.Color, brushProp.Name);
        }
    }

    /// <summary>
    ///     Internal constructor, used by XamlParser.
    /// </summary>
    internal XamlDocument(XmlDocument xmlDoc, XamlParserSettings settings)
    {
        XmlDocument = xmlDoc;
        TypeFinder = settings.TypeFinder;
        ServiceProvider = settings.ServiceProvider;
        CurrentProjectAssemblyName = settings.CurrentProjectAssemblyName;
    }

    /// <summary>
    ///     Gets the underlying XML document.
    /// </summary>
    public XmlDocument XmlDocument { get; }

    /// <summary>
    ///     Gets the outer XML representation of the document.
    /// </summary>
    public string OuterXml => XmlDocument?.OuterXml ?? string.Empty;

    /// <summary>
    ///     Gets the type finder used for this XAML document.
    /// </summary>
    public XamlTypeFinder TypeFinder { get; }

    /// <summary>
    ///     Gets the service provider used for markup extensions in this document.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    ///     Gets the Current Projects Assembly Name (if it has any).
    /// </summary>
    public string CurrentProjectAssemblyName { get; }

    /// <summary>
    ///     Gets the root xaml object.
    /// </summary>
    public XamlObject RootElement { get; private set; }

    /// <summary>
    ///     Gets the object instance created by the root xaml object.
    /// </summary>
    public object RootInstance => RootElement != null ? RootElement.Instance : null;

    /// <summary>
    ///     Gets the type descriptor context used for type conversions.
    /// </summary>
    /// <param name="containingObject">
    ///     The containing object, used when the
    ///     type descriptor context needs to resolve an XML namespace.
    /// </param>
    internal ITypeDescriptorContext GetTypeDescriptorContext(XamlObject containingObject)
    {
        IServiceProvider serviceProvider;
        if (containingObject != null)
        {
            if (containingObject.OwnerDocument != this)
                throw new ArgumentException("Containing object must belong to the document!");
            serviceProvider = containingObject.ServiceProvider;
        }
        else
        {
            serviceProvider = ServiceProvider;
        }

        return new DummyTypeDescriptorContext(serviceProvider);
    }

    /// <summary>
    ///     Saves the xaml document into the <paramref name="writer" />.
    /// </summary>
    public void Save(XmlWriter writer)
    {
        if (writer == null)
            throw new ArgumentNullException("writer");
        XmlDocument.Save(writer);
    }

    /// <summary>
    ///     Called by XamlParser to finish initializing the document.
    /// </summary>
    internal void ParseComplete(XamlObject rootElement)
    {
        RootElement = rootElement;
    }

    /// <summary>
    ///     Create an XamlObject from the instance.
    /// </summary>
    public XamlObject CreateObject(object instance)
    {
        return (XamlObject)CreatePropertyValue(instance, null);
    }

    /// <summary>
    ///     Create an XamlObject from the instance.
    /// </summary>
    public XamlObject CreateObject(XamlObject parent, object instance)
    {
        return (XamlObject)CreatePropertyValue(parent, instance, null);
    }

    /// <summary>
    ///     Creates a value that represents {x:Null}
    /// </summary>
    public XamlPropertyValue CreateNullValue()
    {
        return CreateObject(new NullExtension());
    }

    /// <summary>
    ///     Create a XamlPropertyValue for the specified value instance.
    /// </summary>
    public XamlPropertyValue CreatePropertyValue(object instance, XamlProperty forProperty)
    {
        return CreatePropertyValue(null, instance, forProperty);
    }

    /// <summary>
    ///     Create a XamlPropertyValue for the specified value instance.
    /// </summary>
    public XamlPropertyValue CreatePropertyValue(XamlObject parent, object instance, XamlProperty forProperty)
    {
        if (instance == null)
            throw new ArgumentNullException("instance");

        var elementType = instance.GetType();
        var c = TypeDescriptor.GetConverter(instance);
        var ctx = new DummyTypeDescriptorContext(ServiceProvider);
        ctx.Instance = instance;
        var hasStringConverter = c.CanConvertTo(ctx, typeof(string)) && c.CanConvertFrom(typeof(string));
        if (forProperty != null && hasStringConverter)
        {
            if (instance is SolidColorBrush && _colorBrushDictionary.ContainsKey(((SolidColorBrush)instance).Color))
            {
                var name = _colorBrushDictionary[((SolidColorBrush)instance).Color];
                return new XamlTextValue(this, name);
            }

            return new XamlTextValue(this, c.ConvertToInvariantString(ctx, instance));
        }

        var ns = GetNamespaceFor(elementType);
        var prefix = GetPrefixForNamespace(ns);
        if (parent != null)
            prefix = GetPrefixForNamespace(parent.XmlElement, ns);

        var xml = XmlDocument.CreateElement(prefix, elementType.Name, ns);

        if (hasStringConverter && (XamlObject.GetContentPropertyName(elementType) != null || IsNativeType(instance)))
        {
            xml.InnerText = c.ConvertToInvariantString(instance);
        }
        else if (instance is IBrush && forProperty != null)
        {
            // TODO: this is a hacky fix, because Brush Editor doesn't
            // edit Design Items and so we have no XML, only the Brush 
            // object and we need to parse the Brush to XAML!
            // Note: AvaloniaXamlLoader.Save is not available in Avalonia
            // We'll use a simplified approach for now
            try
            {
                var brushString = instance.ToString();
                xml.InnerText = brushString;
            }
            catch
            {
                // Fallback to empty if conversion fails
                xml.InnerText = "";
            }
        }

        return new XamlObject(this, xml, elementType, instance);
    }

    internal string GetNamespaceFor(Type type, bool getClrNamespace = false)
    {
        if (type == typeof(DesignTimeProperties))
            return XamlConstants.DesignTimeNamespace;
        if (type == typeof(MarkupCompatibilityProperties))
            return XamlConstants.MarkupCompatibilityNamespace;
        if (type == typeof(XamlNamespaceProperties))
            return XamlConstants.Xaml2009Namespace;

        return TypeFinder.GetXmlNamespaceFor(type.Assembly, type.Namespace, getClrNamespace);
    }

    internal List<string> GetNamespacesFor(Type type, bool getClrNamespace = false)
    {
        if (type == typeof(DesignTimeProperties))
            return new List<string> { XamlConstants.DesignTimeNamespace };
        if (type == typeof(MarkupCompatibilityProperties))
            return new List<string> { XamlConstants.MarkupCompatibilityNamespace };
        if (type == typeof(XamlNamespaceProperties))
            return new List<string> { XamlConstants.Xaml2009Namespace, XamlConstants.XamlNamespace };

        return TypeFinder.GetXmlNamespacesFor(type.Assembly, type.Namespace, getClrNamespace);
    }

    internal string GetPrefixForNamespace(string @namespace)
    {
        return GetPrefixForNamespace(XmlDocument.DocumentElement, @namespace);
    }

    internal string GetPrefixForNamespace(XmlElement xmlElement, string @namespace)
    {
        //if (@namespace == XamlConstants.PresentationNamespace)
        //{
        //	return null;
        //}

        var prefix = xmlElement.GetPrefixOfNamespace(@namespace);

        if (xmlElement.NamespaceURI == @namespace && xmlElement.Prefix == string.Empty) return string.Empty;

        if (string.IsNullOrEmpty(prefix))
        {
            prefix = TypeFinder.GetPrefixForXmlNamespace(@namespace);

            string existingNamespaceForPrefix = null;
            if (!string.IsNullOrEmpty(prefix)) existingNamespaceForPrefix = xmlElement.GetNamespaceOfPrefix(prefix);

            if (string.IsNullOrEmpty(prefix) ||
                (!string.IsNullOrEmpty(existingNamespaceForPrefix) &&
                 existingNamespaceForPrefix != @namespace))
                do
                {
                    prefix = "Controls" + namespacePrefixCounter++;
                } while (!string.IsNullOrEmpty(xmlElement.GetNamespaceOfPrefix(prefix)));

            var xmlnsPrefix = xmlElement.GetPrefixOfNamespace(XamlConstants.XmlnsNamespace);
            Debug.Assert(!string.IsNullOrEmpty(xmlnsPrefix));

            xmlElement.SetAttribute(xmlnsPrefix + ":" + prefix, @namespace);

            if (@namespace == XamlConstants.DesignTimeNamespace)
            {
                var ignorableProp = new XamlProperty(RootElement,
                    new XamlDependencyPropertyInfo(MarkupCompatibilityProperties.IgnorableProperty, true, "Ignorable"));
                ignorableProp.SetAttribute(prefix);
            }
        }

        return prefix;
    }

    internal string GetNamespaceForPrefix(string prefix)
    {
        return XmlDocument.DocumentElement.GetNamespaceOfPrefix(prefix);
    }

    private bool IsNativeType(object instance)
    {
        return instance.GetType().Assembly == typeof(string).Assembly || instance.GetType().IsEnum;
    }

    private sealed class DummyTypeDescriptorContext : ITypeDescriptorContext
    {
        private readonly IServiceProvider baseServiceProvider;

        public DummyTypeDescriptorContext(IServiceProvider serviceProvider)
        {
            baseServiceProvider = serviceProvider;
        }

        public IContainer Container => null;

        public object Instance { get; set; }

        public PropertyDescriptor PropertyDescriptor => null;

        public bool OnComponentChanging()
        {
            return false;
        }

        public void OnComponentChanged()
        {
        }

        public object GetService(Type serviceType)
        {
            return baseServiceProvider.GetService(serviceType);
        }
    }
}