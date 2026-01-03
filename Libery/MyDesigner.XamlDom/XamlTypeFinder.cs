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
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MyDesigner.XamlDom;

/// <summary>
///     Allows finding types in a set of assemblies.
/// </summary>
public class XamlTypeFinder : ICloneable
{
    private readonly List<Assembly> _registeredAssemblies;

    private readonly Dictionary<string, XamlNamespace> namespaces = new();
    private readonly Dictionary<AssemblyNamespaceMapping, string> reverseDict = new();
    private readonly Dictionary<AssemblyNamespaceMapping, List<string>> reverseDictList = new();

    public XamlTypeFinder()
    {
        _registeredAssemblies = new List<Assembly>();
    }

    public ReadOnlyCollection<Assembly> RegisteredAssemblies => new(_registeredAssemblies);

    object ICloneable.Clone()
    {
        return Clone();
    }

    /// <summary>
    ///     Gets a type referenced in XAML.
    /// </summary>
    /// <param name="xmlNamespace">
    ///     The XML namespace to use to look up the type.
    ///     This can be a registered namespace or a 'clr-namespace' value.
    /// </param>
    /// <param name="localName">The local name of the type to find.</param>
    /// <returns>
    ///     The requested type, or null if it could not be found.
    /// </returns>
    public Type GetType(string xmlNamespace, string localName)
    {
        if (xmlNamespace == null)
            throw new ArgumentNullException("xmlNamespace");
        if (localName == null)
            throw new ArgumentNullException("localName");
        XamlNamespace ns;
        if (!namespaces.TryGetValue(xmlNamespace, out ns))
        {
            if (xmlNamespace.StartsWith("clr-namespace:", StringComparison.Ordinal))
                ns = namespaces[xmlNamespace] = ParseNamespace(xmlNamespace);
            else
                return null;
        }

        foreach (var mapping in ns.ClrNamespaces)
        {
            var type = mapping.Assembly.GetType(mapping.Namespace + "." + localName);
            if (type != null)
                return type;
        }

        return null;
    }

    /// <summary>
    ///     Gets the XML namespace that can be used for the specified assembly/namespace combination.
    /// </summary>
    public string GetXmlNamespaceFor(Assembly assembly, string @namespace, bool getClrNamespace = false)
    {
        var mapping = new AssemblyNamespaceMapping(assembly, @namespace);
        string xmlNamespace;
        if (!getClrNamespace && reverseDict.TryGetValue(mapping, out xmlNamespace)) return xmlNamespace;

        return "clr-namespace:" + mapping.Namespace + ";assembly=" + mapping.Assembly.GetName().Name;
    }

    /// <summary>
    ///     Gets the XML namespaces that can be used for the specified assembly/namespace combination.
    /// </summary>
    public List<string> GetXmlNamespacesFor(Assembly assembly, string @namespace, bool getClrNamespace = false)
    {
        var mapping = new AssemblyNamespaceMapping(assembly, @namespace);
        List<string> xmlNamespaces;
        if (!getClrNamespace && reverseDictList.TryGetValue(mapping, out xmlNamespaces)) return xmlNamespaces;

        return new List<string>
            { "clr-namespace:" + mapping.Namespace + ";assembly=" + mapping.Assembly.GetName().Name };
    }

    /// <summary>
    ///     Gets the prefix to use for the specified XML namespace,
    ///     or null if no suitable prefix could be found.
    /// </summary>
    public string GetPrefixForXmlNamespace(string xmlNamespace)
    {
        XamlNamespace ns;

        if (namespaces.TryGetValue(xmlNamespace, out ns)) return ns.XmlNamespacePrefix;

        return null;
    }

    private XamlNamespace ParseNamespace(string xmlNamespace)
    {
        var name = xmlNamespace;
        Debug.Assert(name.StartsWith("clr-namespace:", StringComparison.Ordinal));
        name = name.Substring("clr-namespace:".Length);
        string namespaceName, assembly;
        var pos = name.IndexOf(';');
        if (pos < 0)
        {
            namespaceName = name;
            assembly = string.Empty;
        }
        else
        {
            namespaceName = name.Substring(0, pos);
            name = name.Substring(pos + 1).Trim();
            if (!name.StartsWith("assembly=", StringComparison.Ordinal))
                throw new XamlLoadException("Expected: 'assembly='");
            assembly = name.Substring("assembly=".Length);
        }

        var ns = new XamlNamespace(null, xmlNamespace);

        var asm = LoadAssembly(assembly);

        if (asm == null && assembly == "mscorlib")
            asm = typeof(bool).Assembly;

        if (asm != null) AddMappingToNamespace(ns, new AssemblyNamespaceMapping(asm, namespaceName));
        return ns;
    }

    private void AddMappingToNamespace(XamlNamespace ns, AssemblyNamespaceMapping mapping)
    {
        ns.ClrNamespaces.Add(mapping);

        List<string> xmlNamespaceList;
        if (reverseDictList.TryGetValue(mapping, out xmlNamespaceList))
        {
            if (!xmlNamespaceList.Contains(ns.XmlNamespace))
                xmlNamespaceList.Add(ns.XmlNamespace);
        }
        else
        {
            reverseDictList.Add(mapping, new List<string> { ns.XmlNamespace });
        }

        string xmlNamespace;
        if (reverseDict.TryGetValue(mapping, out xmlNamespace))
            if (xmlNamespace == XamlConstants.PresentationNamespace)
                return;

        reverseDict[mapping] = ns.XmlNamespace;
    }

    /// <summary>
    ///     Registers XAML namespaces defined in the <paramref name="assembly" /> for lookup.
    /// </summary>
    public void RegisterAssembly(Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException("assembly");

        _registeredAssemblies.Add(assembly);

        var namespacePrefixes = new Dictionary<string, string>();
        foreach (XmlnsPrefixAttribute xmlnsPrefix in assembly.GetCustomAttributes(typeof(XmlnsPrefixAttribute), true))
            namespacePrefixes.Add(xmlnsPrefix.XmlNamespace, xmlnsPrefix.Prefix);

        foreach (XmlnsDefinitionAttribute xmlnsDef in assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute),
                     true))
        {
            XamlNamespace ns;
            if (!namespaces.TryGetValue(xmlnsDef.XmlNamespace, out ns))
            {
                string prefix;
                namespacePrefixes.TryGetValue(xmlnsDef.XmlNamespace, out prefix);
                ns = namespaces[xmlnsDef.XmlNamespace] = new XamlNamespace(prefix, xmlnsDef.XmlNamespace);
            }

            if (string.IsNullOrEmpty(assembly.FullName))
            {
                AddMappingToNamespace(ns, new AssemblyNamespaceMapping(assembly, xmlnsDef.ClrNamespace));
            }
            else
            {
                var asm = LoadAssembly(assembly.FullName);
                if (asm != null) AddMappingToNamespace(ns, new AssemblyNamespaceMapping(asm, xmlnsDef.ClrNamespace));
            }
        }
    }

    /// <summary>
    ///     Register the Namspaces not found in any Assembly, but used by VS and Expression Blend
    /// </summary>
    public void RegisterDesignerNamespaces()
    {
        var ns = namespaces[XamlConstants.DesignTimeNamespace] =
            new XamlNamespace("d", XamlConstants.DesignTimeNamespace);
        AddMappingToNamespace(ns,
            new AssemblyNamespaceMapping(typeof(DesignTimeProperties).Assembly,
                typeof(DesignTimeProperties).Namespace));
        ns = namespaces[XamlConstants.MarkupCompatibilityNamespace] =
            new XamlNamespace("mc", XamlConstants.MarkupCompatibilityNamespace);
        AddMappingToNamespace(ns,
            new AssemblyNamespaceMapping(typeof(MarkupCompatibilityProperties).Assembly,
                typeof(MarkupCompatibilityProperties).Namespace));
    }

    /// <summary>
    ///     Load the assembly with the specified name.
    ///     You can override this method to implement custom assembly lookup.
    /// </summary>
    public virtual Assembly LoadAssembly(string name)
    {
        return Assembly.Load(name);
    }

    /// <summary>
    ///     Clones this XamlTypeFinder.
    /// </summary>
    public virtual XamlTypeFinder Clone()
    {
        var copy = new XamlTypeFinder();
        copy.ImportFrom(this);
        return copy;
    }

    /// <summary>
    ///     Import information from another XamlTypeFinder.
    ///     Use this if you override Clone().
    /// </summary>
    protected void ImportFrom(XamlTypeFinder source)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        _registeredAssemblies.AddRange(source.RegisteredAssemblies);

        foreach (var pair in source.namespaces) namespaces.Add(pair.Key, pair.Value.Clone());
        foreach (var pair in source.reverseDict) reverseDict.Add(pair.Key, pair.Value);
        foreach (var pair in source.reverseDictList) reverseDictList.Add(pair.Key, pair.Value.ToList());
    }

    /// <summary>
    ///     Creates a new XamlTypeFinder where the Avalonia namespaces are registered.
    /// </summary>
    public static XamlTypeFinder CreateAvaloniaTypeFinder()
    {
        return AvaloniaTypeFinder.Instance.Clone();
    }

    /// <summary>
    ///     Converts the specified <see cref="Uri" /> to local.
    /// </summary>
    public virtual Uri ConvertUriToLocalUri(Uri uri)
    {
        return uri;
    }

    private sealed class AssemblyNamespaceMapping : IEquatable<AssemblyNamespaceMapping>
    {
        internal readonly Assembly Assembly;
        internal readonly string Namespace;

        internal AssemblyNamespaceMapping(Assembly assembly, string @namespace)
        {
            Assembly = assembly;
            Namespace = @namespace;
        }

        public bool Equals(AssemblyNamespaceMapping other)
        {
            return other != null && other.Assembly == Assembly && other.Namespace == Namespace;
        }

        public override int GetHashCode()
        {
            return Assembly.GetHashCode() ^ Namespace.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AssemblyNamespaceMapping);
        }
    }

    private sealed class XamlNamespace
    {
        internal readonly List<AssemblyNamespaceMapping> ClrNamespaces = new();
        internal readonly string XmlNamespace;
        internal readonly string XmlNamespacePrefix;

        internal XamlNamespace(string xmlNamespacePrefix, string xmlNamespace)
        {
            XmlNamespacePrefix = xmlNamespacePrefix;
            XmlNamespace = xmlNamespace;
        }

        internal XamlNamespace Clone()
        {
            var copy = new XamlNamespace(XmlNamespacePrefix, XmlNamespace);
            // AssemblyNamespaceMapping is immutable
            copy.ClrNamespaces.AddRange(ClrNamespaces);
            return copy;
        }
    }

    private static class AvaloniaTypeFinder
    {
        internal static readonly XamlTypeFinder Instance;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline",
            Justification = "We're using an explicit constructor to get it's lazy-loading semantics.")]
        static AvaloniaTypeFinder()
        {
            Instance = new XamlTypeFinder();
            Instance.RegisterDesignerNamespaces();
            Instance.RegisterAssembly(typeof(AvaloniaObject).Assembly); // Avalonia
            Instance.RegisterAssembly(typeof(Grid).Assembly); // Avalonia.Control
            Instance.RegisterAssembly(typeof(IMarkupExtension).Assembly); // Avalonia.Markup.Xaml
            Instance.RegisterAssembly(typeof(Type).Assembly); // mscorelib
        }
    }
}