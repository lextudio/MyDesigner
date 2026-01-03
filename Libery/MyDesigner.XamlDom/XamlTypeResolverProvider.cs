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

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Xml;

namespace MyDesigner.XamlDom;

public sealed class XamlTypeResolverProvider : IXamlTypeResolver, IServiceProvider
{
    private readonly XamlObject containingObject;
    private readonly XamlDocument document;

    public XamlTypeResolverProvider(XamlObject containingObject)
    {
        if (containingObject == null)
            throw new ArgumentNullException("containingObject");
        document = containingObject.OwnerDocument;
        this.containingObject = containingObject;
    }

    private XmlElement ContainingElement => containingObject.XmlElement;

    public object GetService(Type serviceType)
    {
        if (serviceType == typeof(IXamlTypeResolver) || serviceType == typeof(XamlTypeResolverProvider))
            return this;
        return document.ServiceProvider.GetService(serviceType);
    }

    public Type Resolve(string typeName)
    {
        string typeNamespaceUri;
        string typeLocalName;
        if (typeName.Contains(":"))
        {
            typeNamespaceUri = GetNamespaceOfPrefix(typeName.Substring(0, typeName.IndexOf(':')));
            typeLocalName = typeName.Substring(typeName.IndexOf(':') + 1);
        }
        else
        {
            typeNamespaceUri = GetNamespaceOfPrefix(string.Empty);
            typeLocalName = typeName;
        }

        if (string.IsNullOrEmpty(typeNamespaceUri))
        {
            var documentResolver = document.RootElement.ServiceProvider.Resolver;
            if (documentResolver != null && documentResolver != this) return documentResolver.Resolve(typeName);

            throw new XamlMarkupExtensionParseException("Unrecognized namespace prefix in type " + typeName);
        }

        return document.TypeFinder.GetType(typeNamespaceUri, typeLocalName);
    }

    private string GetNamespaceOfPrefix(string prefix)
    {
        var ns = ContainingElement.GetNamespaceOfPrefix(prefix);
        if (!string.IsNullOrEmpty(ns))
            return ns;
        var obj = containingObject;
        while (obj != null)
        {
            ns = obj.XmlElement.GetNamespaceOfPrefix(prefix);
            if (!string.IsNullOrEmpty(ns))
                return ns;
            obj = obj.ParentObject;
        }

        return null;
    }

    public XamlPropertyInfo ResolveProperty(string propertyName)
    {
        string propertyNamespace;
        if (propertyName.Contains(":"))
        {
            propertyNamespace =
                ContainingElement.GetNamespaceOfPrefix(propertyName.Substring(0, propertyName.IndexOf(':')));
            propertyName = propertyName.Substring(propertyName.IndexOf(':') + 1);
        }
        else
        {
            propertyNamespace = ContainingElement.GetNamespaceOfPrefix(string.Empty);
        }

        Type elementType = null;
        var obj = containingObject;
        while (obj != null)
        {
            var style = obj.Instance as Avalonia.Styling.Style;
            if (style != null && style.Selector != null)
            {
                // In Avalonia, we need to extract the target type from the selector
                // This is a simplified approach - in reality, selectors can be more complex
                var selectorString = style.Selector.ToString();
                if (selectorString != null && !selectorString.StartsWith(":"))
                {
                    var typeName = selectorString.Split('.', ':', '#', '[')[0];
                    elementType = document.TypeFinder.GetType(XamlConstants.PresentationNamespace, typeName);
                }
                break;
            }

            obj = obj.ParentObject;
        }

        if (propertyName.Contains("."))
        {
            var allPropertiesAllowed = containingObject is XamlObject &&
                                       (containingObject.ElementType == typeof(MyDesigner.XamlDom.Setter) ||
                                        containingObject.IsMarkupExtension);
            return XamlParser.GetPropertyInfo(document.TypeFinder, null, elementType, propertyNamespace, propertyName,
                allPropertiesAllowed);
        }

        if (elementType != null) return XamlParser.FindProperty(null, elementType, propertyName);

        return null;
    }

    public object FindResource(object key)
    {
        var obj = containingObject;
        while (obj != null)
        {
            var el = obj.Instance as Control;
            if (el != null && el.Resources != null)
            {
                if (el.Resources.TryGetResource(key, null, out var val))
                    return val;
            }

            obj = obj.ParentObject;
        }

        return null;
    }

    public object FindLocalResource(object key)
    {
        var el = containingObject.Instance as Control;
        if (el != null && el.Resources != null)
        {
            if (el.Resources.TryGetResource(key, null, out var val))
                return val;
        }
        return null;
    }
}