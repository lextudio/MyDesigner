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

namespace MyDesigner.XamlDom;

/// <summary>
///     A service provider that provides the IProvideValueTarget and IXamlTypeResolver services.
///     No other services (e.g. from the document's service provider) are offered.
/// </summary>
public class XamlObjectServiceProvider : IServiceProvider, IAvaloniaXamlNameResolver, IProvideValueTarget,
    IAvaloniaXamlSchemaContextProvider, IAvaloniaAmbientProvider, IUriContext
{
    /// <summary>
    ///     Creates a new XamlObjectServiceProvider.
    /// </summary>
    public XamlObjectServiceProvider(XamlObject obj)
    {
        XamlObject = obj ?? throw new ArgumentNullException("obj");
        Resolver = new XamlTypeResolverProvider(obj);
    }

    /// <summary>
    ///     Gets the XamlObject where this service provider is used.
    /// </summary>
    public XamlObject XamlObject { get; }

    /// <summary>
    ///     Gets the type resolver.
    /// </summary>
    public XamlTypeResolverProvider Resolver { get; }

    /// <inheritdoc />
    public object GetService(Type serviceType)
    {
        if (serviceType == typeof(IProvideValueTarget)) return this;
        if (serviceType == typeof(IXamlTypeResolver)) return Resolver;
        if (serviceType == typeof(XamlTypeResolverProvider)) return Resolver;
        if (serviceType == typeof(IAvaloniaXamlSchemaContextProvider)) return this;
        if (serviceType == typeof(IAvaloniaAmbientProvider)) return this;
        if (serviceType == typeof(IAvaloniaXamlNameResolver)) return this;
        if (serviceType == typeof(IUriContext)) return this;
        return XamlObject.OwnerDocument.ServiceProvider.GetService(serviceType);
    }

    #region IProvideValueTarget Members

    /// <inheritdoc />
    public object TargetObject => XamlObject.Instance;

    /// <inheritdoc />
    public object TargetProperty
    {
        get
        {
            if (XamlObject.ParentProperty != null)
            {
                var dependencyProperty = XamlObject.ParentProperty.DependencyProperty;
                if (dependencyProperty != null)
                    return dependencyProperty;
                return XamlObject.ParentProperty.propertyInfo;
            }
            return null;
        }
    }

    #endregion

    #region IXamlNameResolver Members

    /// <inheritdoc />
    public object Resolve(string name)
    {
        bool isFullyInitialized;
        return Resolve(name, out isFullyInitialized);
    }

    /// <inheritdoc />
    public object Resolve(string name, out bool isFullyInitialized)
    {
        isFullyInitialized = true;
        return NameScopeHelper.GetNamedObject(XamlObject, name);
    }

    /// <inheritdoc />
    public IEnumerable<KeyValuePair<string, object>> GetAllNamesAndValuesInScope()
    {
        return new KeyValuePair<string, object>[0];
    }

    /// <inheritdoc />
    public event EventHandler OnNameScopeInitializationComplete { add { } remove { } }

    /// <inheritdoc />
    public bool IsFixupTokenAvailable => false;

    /// <inheritdoc />
    public object GetFixupToken(IEnumerable<string> names)
    {
        return null;
    }

    /// <inheritdoc />
    public object GetFixupToken(IEnumerable<string> names, bool canAssignDirectly)
    {
        return null;
    }

    #endregion

    #region IAvaloniaXamlSchemaContextProvider Members

    private AvaloniaXamlSchemaContext avaloniaXamlSchemaContext;

    /// <inheritdoc />
    public AvaloniaXamlSchemaContext SchemaContext
    {
        get
        {
            return avaloniaXamlSchemaContext =
                avaloniaXamlSchemaContext ?? new AvaloniaXamlSchemaContext();
        }
    }

    #endregion

    #region IAvaloniaAmbientProvider Members

    /// <inheritdoc />
    public AvaloniaAmbientPropertyValue GetFirstAmbientValue(IEnumerable<AvaloniaXamlType> ceilingTypes, params AvaloniaXamlMember[] properties)
    {
        return GetAllAmbientValues(ceilingTypes, properties).FirstOrDefault();
    }

    /// <inheritdoc />
    public IEnumerable<AvaloniaAmbientPropertyValue> GetAllAmbientValues(IEnumerable<AvaloniaXamlType> ceilingTypes,
        params AvaloniaXamlMember[] properties)
    {
        var obj = XamlObject.ParentObject;
        while (obj != null)
        {
            if (ceilingTypes != null && ceilingTypes.Any(x => x.UnderlyingType.IsAssignableFrom(obj.ElementType)))
                yield break;
            if (properties != null)
                foreach (var pr in obj.Properties)
                    if (properties.Any(x => x.Name == pr.PropertyName))
                        yield return new AvaloniaAmbientPropertyValue(pr.SystemXamlMemberForProperty, pr.ValueOnInstance);

            obj = obj.ParentObject;
        }
    }

    /// <inheritdoc />
    public IEnumerable<AvaloniaAmbientPropertyValue> GetAllAmbientValues(IEnumerable<AvaloniaXamlType> ceilingTypes,
        bool searchLiveStackOnly, IEnumerable<AvaloniaXamlType> types, params AvaloniaXamlMember[] properties)
    {
        return new List<AvaloniaAmbientPropertyValue>();
    }

    #endregion

    #region IUriContext Members

    /// <inheritdoc />
    public Uri BaseUri { get; set; }

    #endregion
}