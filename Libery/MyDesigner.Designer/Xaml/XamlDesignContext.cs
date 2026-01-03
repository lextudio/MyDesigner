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

using System.Xml;
using MyDesigner.Design.PropertyGrid;
using MyDesigner.Design.Services;
using MyDesigner.Designer.OutlineView;
using MyDesigner.Designer.PropertyGrid.Editors;
using MyDesigner.Designer.Services;
using MyDesigner.XamlDom;


namespace MyDesigner.Designer.Xaml;

/// <summary>
///     The design context implementation used when editing XAML.
/// </summary>
public sealed class XamlDesignContext : DesignContext
{
    internal readonly XamlComponentService _componentService;
    private readonly XamlDesignItem _rootItem;

    //set { _doc.RootElement.SetXamlAttribute("Class", value); }
    /// <summary>
    ///     Creates a new XamlDesignContext instance.
    /// </summary>
    public XamlDesignContext(XmlReader xamlReader, XamlLoadSettings loadSettings)
    {
        if (xamlReader == null)
            throw new ArgumentNullException("xamlReader");
        if (loadSettings == null)
            throw new ArgumentNullException("loadSettings");

        Services.AddService(typeof(ISelectionService), new DefaultSelectionService());
        Services.AddService(typeof(IComponentPropertyService), new ComponentPropertyService());
        Services.AddService(typeof(Design.IToolService), new DefaultToolService(this));
        Services.AddService(typeof(UndoService), new UndoService());
        Services.AddService(typeof(ICopyPasteService), new CopyPasteService());
        Services.AddService(typeof(IErrorService), new DefaultErrorService(this));
        Services.AddService(typeof(IOutlineNodeService), new OutlineNode.OutlineNodeService());
        Services.AddService(typeof(IOutlineNodeNameService), new OutlineNodeNameService());
        Services.AddService(typeof(ViewService), new DefaultViewService(this));
        Services.AddService(typeof(OptionService), new OptionService());

        var xamlErrorService = new XamlErrorService();
        Services.AddService(typeof(XamlErrorService), xamlErrorService);
        Services.AddService(typeof(IXamlErrorSink), xamlErrorService);

        _componentService = new XamlComponentService(this);
        Services.AddService(typeof(IComponentService), _componentService);

        foreach (var action in loadSettings.CustomServiceRegisterFunctions) action(this);

        // register default versions of overridable services:
        if (Services.GetService(typeof(ITopLevelWindowService)) == null)
            Services.AddService(typeof(ITopLevelWindowService), new AvaloniaTopLevelWindowService());

        EditorManager.SetDefaultTextBoxEditorType(typeof(TextBoxEditor));
        EditorManager.SetDefaultComboBoxEditorType(typeof(ComboBoxEditor));

        // register extensions from the designer assemblies:
        foreach (var designerAssembly in loadSettings.DesignerAssemblies)
        {
            Services.ExtensionManager.RegisterAssembly(designerAssembly);
            EditorManager.RegisterAssembly(designerAssembly);
        }

        ParserSettings = new XamlParserSettings();
        ParserSettings.TypeFinder = loadSettings.TypeFinder;
        ParserSettings.CurrentProjectAssemblyName = loadSettings.CurrentProjectAssemblyName;
        ParserSettings.CreateInstanceCallback = Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory;
        ParserSettings.ServiceProvider = Services;
        Document = XamlParser.Parse(xamlReader, ParserSettings);

        loadSettings.ReportErrors(xamlErrorService);

        if (Document == null)
        {
            string message;
            if (xamlErrorService != null && xamlErrorService.Errors.Count > 0)
                message = xamlErrorService.Errors[0].Message;
            else
                message = "Could not load document.";
            throw new XamlDom.XamlLoadException(message);
        }

        _rootItem = _componentService.RegisterXamlComponentRecursive(Document.RootElement);

        if (_rootItem != null)
        {
            var rootBehavior = new RootItemBehavior();
            rootBehavior.Initialize(this);
        }

        XamlEditAction = new XamlEditOperations(this, ParserSettings);
    }

    public XamlEditOperations XamlEditAction { get; }

    internal XamlDocument Document { get; }

    /// <summary>
    ///     Gets/Sets the value of the "x:class" property on the root item.
    /// </summary>
    public string ClassName => Document.RootElement.GetXamlAttribute("Class");

    /// <summary>
    ///     Gets the root item being designed.
    /// </summary>
    public override DesignItem RootItem => _rootItem;

    /// <summary>
    ///     Gets the parser Settings being used
    /// </summary>
    public XamlParserSettings ParserSettings { get; }


    /// <summary>
    ///     Saves the XAML DOM into the XML writer.
    /// </summary>
    public override void Save(XmlWriter writer)
    {
        Document.Save(writer);
    }

    /// <summary>
    ///     Opens a new change group used to batch several changes.
    ///     ChangeGroups work as transactions and are used to support the Undo/Redo system.
    /// </summary>
    public override ChangeGroup OpenGroup(string changeGroupTitle, ICollection<DesignItem> affectedItems)
    {
        if (affectedItems == null)
            throw new ArgumentNullException("affectedItems");

        var undoService = Services.GetRequiredService<UndoService>();
        var g = undoService.StartTransaction(affectedItems);
        g.Title = changeGroupTitle;
        return g;
    }
}