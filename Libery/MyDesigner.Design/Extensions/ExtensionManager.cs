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

using System.Diagnostics;
using System.Reflection;

namespace MyDesigner.Design.Extensions;

/// <summary>
///     Manages extension creation for a design context.
/// </summary>
public sealed class ExtensionManager
{
    private readonly DesignContext _context;

    internal ExtensionManager(DesignContext context)
    {
        Debug.Assert(context != null);
        _context = context;

        context.Services.RunWhenAvailable(
            delegate(IComponentService componentService)
            {
                componentService.ComponentRegistered += OnComponentRegistered;
            });
    }

    private void OnComponentRegistered(object sender, DesignItemEventArgs e)
    {
        e.Item.SetExtensionServers(this, GetExtensionServersForItem(e.Item));
    }

    /// <summary>
    ///     Re-applies extensions from the ExtensionServer to the specified design items.
    /// </summary>
    private void ReapplyExtensions(IEnumerable<DesignItem> items, ExtensionServer server)
    {
        foreach (var item in items)
            if (item != null)
                item.ReapplyExtensionServer(this, server);
    }

    #region Manage ExtensionEntries

    private sealed class ExtensionEntry
    {
        internal readonly Type ExtensionType;
        internal readonly int Order;
        internal readonly List<Type> OverriddenExtensionTypes = new();
        internal readonly ExtensionServer Server;

        public ExtensionEntry(Type extensionType, ExtensionServer server, Type overriddenExtensionType, int order)
        {
            ExtensionType = extensionType;
            Server = server;
            OverriddenExtensionTypes.Add(overriddenExtensionType);
            Order = order;
        }

        public ExtensionEntry(Type extensionType, ExtensionServer server, List<Type> overriddenExtensionTypes,
            int order)
        {
            ExtensionType = extensionType;
            Server = server;
            OverriddenExtensionTypes = overriddenExtensionTypes;
            Order = order;
        }
    }

    private readonly Dictionary<Type, List<ExtensionEntry>> _extensions = new();

    private void AddExtensionEntry(Type extendedItemType, ExtensionEntry entry)
    {
        List<ExtensionEntry> list;
        if (!_extensions.TryGetValue(extendedItemType, out list))
            list = _extensions[extendedItemType] = new List<ExtensionEntry>();
        list.Add(entry);
    }

    /// <summary>
    ///     Remove a Extension form a Type, so it is not used!
    /// </summary>
    /// <param name="extendedItemType"></param>
    /// <param name="extensionType"></param>
    public void RemoveExtension(Type extendedItemType, Type extensionType)
    {
        List<ExtensionEntry> list;
        if (!_extensions.TryGetValue(extendedItemType, out list))
            list = _extensions[extendedItemType] = new List<ExtensionEntry>();
        list.RemoveAll(x => x.ExtensionType == extensionType);
    }

    private List<ExtensionEntry> GetExtensionEntries(Type extendedItemType)
    {
        var result = new List<ExtensionEntry>();
        var overriddenExtensions = new List<Type>();
        var ie = _extensions.Where(x => x.Key.IsAssignableFrom(extendedItemType)).SelectMany(x => x.Value);
        foreach (var entry in ie)
            if (!overriddenExtensions.Contains(entry.ExtensionType))
            {
                overriddenExtensions.AddRange(entry.OverriddenExtensionTypes);

                result.RemoveAll(x => overriddenExtensions.Contains(x.ExtensionType));
                result.Add(entry);
            }

        return result.OrderBy(x => x.Order).ToList();
    }

    /// <summary>
    ///     Gets all the types of all extensions that are applied to the specified item type.
    /// </summary>
    public IEnumerable<Type> GetExtensionTypes(Type extendedItemType)
    {
        if (extendedItemType == null)
            throw new ArgumentNullException("extendedItemType");
        foreach (var entry in GetExtensionEntries(extendedItemType)) yield return entry.ExtensionType;
    }

    #endregion

    #region Create Extensions

    private static readonly ExtensionEntry[] emptyExtensionEntryArray = new ExtensionEntry[0];

    private IEnumerable<ExtensionEntry> GetExtensionEntries(DesignItem extendedItem)
    {
        if (extendedItem.Component == null)
            return emptyExtensionEntryArray;
        return GetExtensionEntries(extendedItem.ComponentType);
    }

    private ExtensionServer[] GetExtensionServersForItem(DesignItem item)
    {
        Debug.Assert(item != null);

        var servers = new HashSet<ExtensionServer>();
        foreach (var entry in GetExtensionEntries(item)) servers.Add(entry.Server);
        return servers.ToArray();
    }

    internal IEnumerable<Extension> CreateExtensions(ExtensionServer server, DesignItem item, Type extensionType = null)
    {
        Debug.Assert(server != null);
        Debug.Assert(item != null);

        foreach (var entry in GetExtensionEntries(item))
            if (entry.Server == server && (extensionType == null || entry.ExtensionType == extensionType))
            {
                var disabledExtensions = Extension.GetDisabledExtensions(item.View);
                if (string.IsNullOrEmpty(disabledExtensions) ||
                    !disabledExtensions.Split(';').Contains(entry.ExtensionType.Name))
                    yield return server.CreateExtension(entry.ExtensionType, item);
            }
    }

    #endregion

    #region RegisterAssembly

    private readonly HashSet<Assembly> _registeredAssemblies = new();

    /// <summary>
    ///     Registers extensions from the specified assembly.
    /// </summary>
    public void RegisterAssembly(Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException("assembly");

        if (!_registeredAssemblies.Add(assembly))
            // the assembly already is registered, don't try to register it again.
            return;

        foreach (Type type in assembly.GetTypes())
        {
            object[] extensionForAttributes = type.GetCustomAttributes(typeof(ExtensionForAttribute), false);
            if (extensionForAttributes.Length == 0)
                continue;

            foreach (ExtensionForAttribute designerFor in extensionForAttributes)
            {
                ExtensionServer server = GetServerForExtension(type);
                ExtensionAttribute extensionAttribute = type.GetCustomAttributes(typeof(ExtensionAttribute), false).FirstOrDefault() as ExtensionAttribute;
                AddExtensionEntry(designerFor.DesignedItemType, new ExtensionEntry(type, server, designerFor.OverrideExtensions.ToList(), extensionAttribute != null ? extensionAttribute.Order : 0));
            }
        }
    }

    #endregion

    #region Extension Server Creation

    // extension server type => extension server instance
    private readonly Dictionary<Type, ExtensionServer> _extensionServers = new();

    private ExtensionServer GetServerForExtension(Type extensionType)
    {
        Debug.Assert(extensionType != null);

        var extensionServerAttributes = extensionType.GetCustomAttributes(typeof(ExtensionServerAttribute), true);
        if (extensionServerAttributes.Length != 1)
            throw new DesignerException("Extension types must have exactly one [ExtensionServer] attribute.");

        return GetExtensionServer((ExtensionServerAttribute)extensionServerAttributes[0]);
    }

    /// <summary>
    ///     Gets the extension server for the specified extension server attribute.
    /// </summary>
    public ExtensionServer GetExtensionServer(ExtensionServerAttribute attribute)
    {
        if (attribute == null)
            throw new ArgumentNullException("attribute");

        var extensionServerType = attribute.ExtensionServerType;

        ExtensionServer server;
        if (_extensionServers.TryGetValue(extensionServerType, out server))
            return server;

        server = (ExtensionServer)Activator.CreateInstance(extensionServerType);
        server.InitializeExtensionServer(_context);
        _extensionServers[extensionServerType] = server;
        server.ShouldApplyExtensionsInvalidated += delegate(object sender, DesignItemCollectionEventArgs e)
        {
            ReapplyExtensions(e.Items, (ExtensionServer)sender);
        };
        return server;
    }

    #endregion

    #region Special extensions (CustomInstanceFactory and DefaultInitializer)

    private static readonly object[] emptyObjectArray = new object[0];

    /// <summary>
    ///     Create an instance of the specified type using the specified arguments.
    ///     The instance is created using a CustomInstanceFactory registered for the type,
    ///     or using reflection if no instance factory is found.
    /// </summary>
    public object CreateInstanceWithCustomInstanceFactory(Type instanceType, object[] arguments)
    {
        if (instanceType == null)
            throw new ArgumentNullException("instanceType");
        if (arguments == null)
            arguments = emptyObjectArray;

        foreach (var extensionType in GetExtensionTypes(instanceType))
            if (typeof(CustomInstanceFactory).IsAssignableFrom(extensionType))
            {
                var factory = (CustomInstanceFactory)Activator.CreateInstance(extensionType);
                return factory.CreateInstance(instanceType, arguments);
            }

        return CustomInstanceFactory.DefaultInstanceFactory.CreateInstance(instanceType, arguments);
    }

    /// <summary>
    ///     Applies all DefaultInitializer extensions on the design item.
    /// </summary>
    public void ApplyDefaultInitializers(DesignItem item)
    {
        if (item == null)
            throw new ArgumentNullException("item");

        foreach (var entry in GetExtensionEntries(item))
            if (typeof(DefaultInitializer).IsAssignableFrom(entry.ExtensionType))
            {
                var initializer = (DefaultInitializer)Activator.CreateInstance(entry.ExtensionType);
                initializer.InitializeDefaults(item);
            }
    }

    /// <summary>
    ///     Applies all DefaultInitializer extensions on the design item.
    /// </summary>
    public void ApplyDesignItemInitializers(DesignItem item)
    {
        if (item == null)
            throw new ArgumentNullException("item");

        foreach (var entry in GetExtensionEntries(item))
            if (typeof(DesignItemInitializer).IsAssignableFrom(entry.ExtensionType))
            {
                var initializer = (DesignItemInitializer)Activator.CreateInstance(entry.ExtensionType);
                initializer.InitializeDesignItem(item);
            }
    }

    #endregion
}