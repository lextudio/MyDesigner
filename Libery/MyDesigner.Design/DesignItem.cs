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
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MyDesigner.Design.Extensions;
using MyDesigner.Design.Interfaces;
using MyDesigner.Design.UIExtensions;

namespace MyDesigner.Design;

/// <summary>
///     The DesignItem connects a component with the service system and the designers.
///     Equivalent to Cider's ModelItem.
/// </summary>
public abstract class DesignItem : INotifyPropertyChanged
{
    /// <summary>
    ///     The initial Position after a Drag/Drop
    /// </summary>
    public Point Position { get; set; }

    /// <summary>
    ///     Gets the component this DesignSite was created for.
    /// </summary>
    public abstract object Component { get; }

    /// <summary>
    ///     Gets the component type of this design site.
    ///     This value may be different from Component.GetType() if a CustomInstanceFactory created
    ///     an object using a different type (e.g. ComponentType=Window but Component.GetType()=WindowClone).
    /// </summary>
    public abstract Type ComponentType { get; }

    /// <summary>
    ///     Gets the view used for the component.
    /// </summary>
    public abstract Control View { get; }

    /// <summary>
    ///     Gets the design context.
    /// </summary>
    public abstract DesignContext Context { get; }

    /// <summary>
    ///     Gets the parent design item.
    /// </summary>
    public abstract DesignItem Parent { get; }

    /// <summary>
    ///     Gets the property where this DesignItem is used as a value.
    /// </summary>
    public abstract DesignItemProperty ParentProperty { get; }

    /// <summary>
    ///     Gets properties set on the design item.
    /// </summary>
    public abstract DesignItemPropertyCollection Properties { get; }

    /// <summary>
    ///     Gets properties set on the design item.
    /// </summary>
    public abstract IEnumerable<DesignItemProperty> AllSetProperties { get; }

    /// <summary>
    ///     Gets/Sets the name of the design item.
    /// </summary>
    public abstract string Name { get; set; }

    /// <summary>
    ///     Gets/Sets the value of the "x:Key" attribute on the design item.
    /// </summary>
    public abstract string Key { get; set; }

    /// <summary>
    ///     Gets an instance that provides convenience properties for the most-used designers.
    /// </summary>
    public ServiceContainer Services
    {
        [DebuggerStepThrough] get => Context.Services;
    }

    /// <summary>
    ///     Gets the name of the content property (the property that contains the logical children)
    /// </summary>
    public abstract string ContentPropertyName { get; }

    /// <summary>
    ///     Gets the content property (the property that contains the logical children)
    /// </summary>
    public DesignItemProperty ContentProperty
    {
        get
        {
            if (ContentPropertyName == null) return null;
            return Properties[ContentPropertyName];
        }
    }

    /// <summary>
    ///     Gets the component this DesignSite was created for.
    /// </summary>
    public int DepthLevel
    {
        get
        {
            var j = 0;
            var x = Parent;
            while (x != null)
            {
                j++;
                x = x.Parent;
            }

            return j;
        }
    }

    /// <summary>
    ///     This event is raised whenever a model property on the DesignItem changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///     Set the View for a Component
    /// </summary>
    /// <param name="newView"></param>
    public abstract void SetView(Control newView);

    /// <summary>
    ///     Occurs when the parent of this design item changes.
    /// </summary>
    public abstract event EventHandler ParentChanged;

    /// <summary>
    ///     Is raised when the name of the design item changes.
    /// </summary>
    public abstract event EventHandler NameChanged;

    /// <summary>
    ///     Opens a new change group used to batch several changes.
    ///     ChangeGroups work as transactions and are used to support the Undo/Redo system.
    ///     Note: the ChangeGroup applies to the whole <see cref="DesignContext" />, not just to
    ///     this item!
    /// </summary>
    public ChangeGroup OpenGroup(string changeGroupTitle)
    {
        return Context.OpenGroup(changeGroupTitle, new[] { this });
    }

    /// <summary>
    ///     Raises the <see cref="PropertyChanged" /> event.
    /// </summary>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (PropertyChanged != null) PropertyChanged(this, e);
    }

    /// <summary>
    ///     Removes this design item from its parent property/collection.
    /// </summary>
    public void Remove()
    {
        if (ParentProperty != null)
        {
            if (ParentProperty.IsCollection)
                ParentProperty.CollectionElements.Remove(this);
            else
                ParentProperty.Reset();
        }
    }

    /// <summary>
    ///     Creates a copy of this design item.
    /// </summary>
    public abstract DesignItem Clone();

    /// <summary>
    ///     Gets a <see cref="IOutlineNode" /> for this this design item.
    /// </summary>
    public IOutlineNode CreateOutlineNode()
    {
        return Services.GetRequiredService<IOutlineNodeService>().Create(this);
    }

    /// <summary>
    ///     Gets a <see cref="Transform" /> that represents all transforms applied to the item's view.
    /// </summary>
    public Transform GetCompleteAppliedTransformationToView()
    {
        var retVal = new TransformGroup();
        var v = View as Visual;
        while (v != null)
        {
            var fe = v as Control;
            if (fe != null && fe.RenderTransform != null)
            {
                if (fe.RenderTransform is Transform transform)
                    retVal.Children.Add(transform);
            }
            v = v.TryFindParent<Visual>(true);
        }

        return retVal;
    }

    #region Extensions support

    [DebuggerDisplay("ExtensionEntry - {Extension} / {Server}")]
    private struct ExtensionEntry
    {
        internal readonly Extension Extension;
        internal readonly ExtensionServer Server;

        public ExtensionEntry(Extension extension, ExtensionServer server)
        {
            Extension = extension;
            Server = server;
        }
    }

    private ExtensionServer[] _extensionServers;
    private bool[] _extensionServerIsApplied;

    private readonly List<ExtensionEntry> _extensions = new();

    /// <summary>
    ///     Gets the extensions registered for this DesignItem.
    /// </summary>
    public IEnumerable<Extension> Extensions
    {
        get { return _extensions.Select(x => x.Extension).ToList(); }
    }

    internal void SetExtensionServers(ExtensionManager extensionManager, ExtensionServer[] extensionServers)
    {
        Debug.Assert(_extensionServers == null);
        Debug.Assert(extensionServers != null);

        _extensionServers = extensionServers;
        _extensionServerIsApplied = new bool[extensionServers.Length];

        for (var i = 0; i < _extensionServers.Length; i++)
        {
            var shouldApply = _extensionServers[i].ShouldApplyExtensions(this);
            if (shouldApply != _extensionServerIsApplied[i])
            {
                _extensionServerIsApplied[i] = shouldApply;
                ApplyUnapplyExtensionServer(extensionManager, shouldApply, _extensionServers[i]);
            }
        }
    }

    internal void ReapplyExtensionServer(ExtensionManager extensionManager, ExtensionServer server)
    {
        Debug.Assert(_extensionServers != null);

        for (var i = 0; i < _extensionServers.Length; i++)
            if (_extensionServers[i] == server)
            {
                var shouldApply = server.ShouldApplyExtensions(this);

                if (server.ShouldBeReApplied() && shouldApply && shouldApply == _extensionServerIsApplied[i])
                {
                    _extensionServerIsApplied[i] = false;
                    ApplyUnapplyExtensionServer(extensionManager, false, server);
                }

                if (shouldApply != _extensionServerIsApplied[i])
                {
                    _extensionServerIsApplied[i] = shouldApply;
                    ApplyUnapplyExtensionServer(extensionManager, shouldApply, server);
                }
            }
    }

    private void ApplyUnapplyExtensionServer(ExtensionManager extensionManager, bool shouldApply,
        ExtensionServer server)
    {
        if (shouldApply)
            // add extensions
            foreach (var ext in extensionManager.CreateExtensions(server, this))
                _extensions.Add(new ExtensionEntry(ext, server));
        else
            // remove extensions
            _extensions.RemoveAll(entry =>
            {
                if (entry.Server == server)
                {
                    server.RemoveExtension(entry.Extension);
                    return true;
                }

                return false;
            });
    }

    /// <summary>
    ///     Removes one specific Extension
    /// </summary>
    public void RemoveExtension(Extension extension)
    {
        var hasExtension = _extensions.Any(x => x.Extension.GetType() == extension.GetType());

        if (hasExtension)
        {
            var extensionEntry = _extensions.FirstOrDefault(x => x.Extension.GetType() == extension.GetType());
            //_extensions.Remove(extensionEntry);
            extensionEntry.Server.RemoveExtension(extensionEntry.Extension);
        }
    }

    /// <summary>
    ///     Reapplies all extensions.
    /// </summary>
    public void ReapplyAllExtensions()
    {
        var manager = Services.GetService<ExtensionManager>();
        var servers = _extensions.GroupBy(entry => entry.Server).Select(grp => grp.First().Server).ToList();

        foreach (var server in servers)
        {
            ApplyUnapplyExtensionServer(manager, false, server);
            ApplyUnapplyExtensionServer(manager, true, server);
        }
    }

    /// <summary>
    ///     Reapplies a specific extension.
    /// </summary>
    public void ReapplyExtension(Type extensionType)
    {
        var manager = Services.GetService<ExtensionManager>();
        var servers =
            _extensions.GroupBy(entry => entry.Server).Select(grp => grp.First().Server).ToList();

        foreach (var server in servers)
        {
            _extensions.RemoveAll(entry =>
            {
                if (entry.Server == server && entry.Extension.GetType() == extensionType)
                {
                    server.RemoveExtension(entry.Extension);
                    return true;
                }

                return false;
            });

            foreach (var ext in manager.CreateExtensions(server, this, extensionType))
                _extensions.Add(new ExtensionEntry(ext, server));
        }
    }

    #endregion

    #region Manage behavior

    private readonly Dictionary<Type, object> _behaviorObjects = new();

    /// <summary>
    ///     Adds a bevahior extension object to this design item.
    /// </summary>
    public void AddBehavior(Type behaviorInterface, object behaviorImplementation)
    {
        if (behaviorInterface == null)
            throw new ArgumentNullException("behaviorInterface");
        if (behaviorImplementation == null)
            throw new ArgumentNullException("behaviorImplementation");
        if (!behaviorInterface.IsInstanceOfType(behaviorImplementation))
            throw new ArgumentException("behaviorImplementation must implement bevahiorInterface",
                "behaviorImplementation");

        _behaviorObjects[behaviorInterface] = behaviorImplementation;
    }

    /// <summary>
    ///     Gets a bevahior extension object from the design item.
    /// </summary>
    /// <returns>The behavior object, or null if it was not found.</returns>
    [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
    [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    public T GetBehavior<T>() where T : class
    {
        object obj;
        _behaviorObjects.TryGetValue(typeof(T), out obj);
        return (T)obj;
    }

    #endregion
}