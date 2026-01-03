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

using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using MyDesigner.Designer.Controls;
using MyDesigner.Designer.Services;
using MyDesigner.Designer.Xaml;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using System.Xml;

namespace MyDesigner.Designer;

/// <summary>
///     Surface hosting the WPF designer.
/// </summary>
[TemplatePart(Name = "PART_DesignContent", Type = typeof(ContentControl))]
[TemplatePart(Name = "PART_Zoom", Type = typeof(ZoomControl))]
public class DesignSurface : ContentControl, INotifyPropertyChanged
{
    private readonly Border _sceneContainer;

    internal DesignPanel _designPanel;
    private FocusNavigator _focusNav;
    private ContentControl _partDesignContent;

    private bool enableBringIntoView;
    protected override Type StyleKeyOverride => typeof(DesignSurface);

    public DesignSurface()
    {
        //Propertygrid should show no inherited Datacontext!
        DataContext = null;

        // Using CommunityToolkit.Mvvm commands as replacement for WPF ApplicationCommands
        this.AddCommandHandler(ApplicationCommands.Undo, Undo, CanUndo);
        this.AddCommandHandler(ApplicationCommands.Redo, Redo, CanRedo);
        this.AddCommandHandler(ApplicationCommands.Copy, Copy, CanCopy);
        this.AddCommandHandler(ApplicationCommands.Cut, Cut, CanCut);
        this.AddCommandHandler(ApplicationCommands.Delete, Delete, CanDelete);
        this.AddCommandHandler(ApplicationCommands.Paste, Paste, CanPaste);
        this.AddCommandHandler(ApplicationCommands.SelectAll, SelectAll, CanSelectAll);

        this.AddCommandHandler(Commands.AlignTopCommand,
            () => ModelTools.ArrangeItems(DesignContext.Services.Selection.SelectedItems, ArrangeDirection.Top),
            () => DesignContext.Services.Selection.SelectedItems.Count() > 1);
        this.AddCommandHandler(Commands.AlignMiddleCommand,
            () => ModelTools.ArrangeItems(DesignContext.Services.Selection.SelectedItems,
                ArrangeDirection.VerticalMiddle), () => DesignContext.Services.Selection.SelectedItems.Count() > 1);
        this.AddCommandHandler(Commands.AlignBottomCommand,
            () => ModelTools.ArrangeItems(DesignContext.Services.Selection.SelectedItems, ArrangeDirection.Bottom),
            () => DesignContext.Services.Selection.SelectedItems.Count() > 1);
        this.AddCommandHandler(Commands.AlignLeftCommand,
            () => ModelTools.ArrangeItems(DesignContext.Services.Selection.SelectedItems, ArrangeDirection.Left),
            () => DesignContext.Services.Selection.SelectedItems.Count() > 1);
        this.AddCommandHandler(Commands.AlignCenterCommand,
            () => ModelTools.ArrangeItems(DesignContext.Services.Selection.SelectedItems,
                ArrangeDirection.HorizontalMiddle), () => DesignContext.Services.Selection.SelectedItems.Count() > 1);
        this.AddCommandHandler(Commands.AlignRightCommand,
            () => ModelTools.ArrangeItems(DesignContext.Services.Selection.SelectedItems, ArrangeDirection.Right),
            () => DesignContext.Services.Selection.SelectedItems.Count() > 1);

        this.AddCommandHandler(Commands.RotateLeftCommand,
            () => ModelTools.ApplyTransform(DesignContext.Services.Selection.PrimarySelection, new RotateTransform(-90),
                true,
                DesignContext.RootItem == DesignContext.Services.Selection.PrimarySelection
                    ? RenderTransformProperty
                    : RenderTransformProperty), () => DesignContext.Services.Selection.PrimarySelection != null);
        this.AddCommandHandler(Commands.RotateRightCommand,
            () => ModelTools.ApplyTransform(DesignContext.Services.Selection.PrimarySelection, new RotateTransform(90),
                true,
                DesignContext.RootItem == DesignContext.Services.Selection.PrimarySelection
                    ? RenderTransformProperty
                    : RenderTransformProperty), () => DesignContext.Services.Selection.PrimarySelection != null);

        this.AddCommandHandler(Commands.StretchToSameWidthCommand,
            () => ModelTools.StretchItems(DesignContext.Services.Selection.SelectedItems, StretchDirection.Width),
            () => DesignContext.Services.Selection.SelectedItems.Count() > 1);
        this.AddCommandHandler(Commands.StretchToSameHeightCommand,
            () => ModelTools.StretchItems(DesignContext.Services.Selection.SelectedItems, StretchDirection.Height),
            () => DesignContext.Services.Selection.SelectedItems.Count() > 1);
        
        this.AddCommandHandler(Commands.GridSettingsCommand, ShowGridSettings, () => true);

        _sceneContainer = new Border();
        DragDrop.SetAllowDrop(_sceneContainer, false);
        // Avalonia equivalent for UseLayoutRounding and TextFormattingMode will be different

        _designPanel = new DesignPanel { Child = _sceneContainer, DesignSurface = this };
    }

    public ZoomControl ZoomControl { get; private set; }

    /// <summary>
    ///     Gets the active design context.
    /// </summary>
    public DesignContext DesignContext { get; private set; }

    /// <summary>
    ///     Gets the DesignPanel
    /// </summary>
    public DesignPanel DesignPanel => _designPanel;

    /// <summary>
    ///     Gets the root design item.
    /// </summary>
    public DesignItem RootItem => DesignContext?.RootItem;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _partDesignContent = e.NameScope.Find("PART_DesignContent") as ContentControl;
        _partDesignContent.Content = _designPanel;
        // TODO: Implement Avalonia equivalent for RequestBringIntoView event

        ZoomControl = e.NameScope.Find("PART_Zoom") as ZoomControl;

        OnPropertyChanged("ZoomControl");

        base.OnApplyTemplate(e);
    }

    public void ScrollIntoView(DesignItem designItem)
    {
        enableBringIntoView = true;
        // Avalonia equivalent for BringIntoView
        designItem.View?.BringIntoView();
        enableBringIntoView = false;
    }

    // TODO: Implement Avalonia equivalent for _partDesignContent_RequestBringIntoView
    // private void _partDesignContent_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    // {
    //     if (!enableBringIntoView)
    //         e.Handled = true;
    // }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (ZoomControl != null && e.Source == ZoomControl && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) 
            UnselectAll();
        
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            Focus();
            
        base.OnPointerPressed(e);
    }

    /// <summary>
    ///     Initializes the designer content from the specified XmlReader.
    /// </summary>
    public void LoadDesigner(XmlReader xamlReader, XamlLoadSettings loadSettings)
    {
        UnloadDesigner();
        loadSettings = loadSettings ?? new XamlLoadSettings();
        loadSettings.CustomServiceRegisterFunctions.Add(context =>
            context.Services.AddService(typeof(IDesignPanel), _designPanel));
        InitializeDesigner(new XamlDesignContext(xamlReader, loadSettings));
    }

    /// <summary>
    ///     Saves the designer content into the specified XmlWriter.
    /// </summary>
    public void SaveDesigner(XmlWriter writer)
    {
        DesignContext.Save(writer);
    }

    private void InitializeDesigner(DesignContext context)
    {
        DesignContext = context;
        _designPanel.Context = context;
        _designPanel.ClearContextMenu();

        if (context.RootItem != null) _sceneContainer.Child = context.RootItem.View;

        context.Services.RunWhenAvailable<UndoService>(undoService =>
            undoService.UndoStackChanged += delegate { /* CommandManager equivalent in Avalonia */ }
        );
        context.Services.Selection.SelectionChanged += delegate { /* CommandManager equivalent in Avalonia */ };

        context.Services.AddService(typeof(IKeyBindingService), new DesignerKeyBindings(this));
        _focusNav = new FocusNavigator(this);
        _focusNav.Start();

        OnPropertyChanged("DesignContext");
    }

    /// <summary>
    ///     Unloads the designer content.
    /// </summary>
    public void UnloadDesigner()
    {
        if (DesignContext != null)
            foreach (var o in DesignContext.Services.AllServices)
            {
                var d = o as IDisposable;
                if (d != null) d.Dispose();
            }

        DesignContext = null;
        _designPanel.Context = null;
        _sceneContainer.Child = null;
        _designPanel.Adorners.Clear();
    }

    #region Commands

    public bool CanUndo()
    {
        var undoService = GetService<UndoService>();
        return undoService != null && undoService.CanUndo;
    }

    public void Undo()
    {
        var undoService = GetService<UndoService>();
        var action = undoService.UndoActions.First();
        Debug.WriteLine("Undo " + action.Title);
        undoService.Undo();
        DesignContext.Services.Selection.SetSelectedComponents(GetLiveElements(action.AffectedElements));
    }

    public bool CanRedo()
    {
        var undoService = GetService<UndoService>();
        return undoService != null && undoService.CanRedo;
    }

    public void Redo()
    {
        var undoService = GetService<UndoService>();
        var action = undoService.RedoActions.First();
        Debug.WriteLine("Redo " + action.Title);
        undoService.Redo();
        DesignContext.Services.Selection.SetSelectedComponents(GetLiveElements(action.AffectedElements));
    }

    public bool CanCopy()
    {
        return DesignContext?.Services?.CopyPasteService?.CanCopy(DesignContext) == true;
    }

    public void Copy()
    {
        DesignContext?.Services?.CopyPasteService?.Copy(DesignContext);
    }

    public bool CanCut()
    {
        return DesignContext?.Services?.CopyPasteService?.CanCut(DesignContext) == true;
    }

    public void Cut()
    {
        DesignContext?.Services?.CopyPasteService?.Cut(DesignContext);
    }

    public bool CanDelete()
    {
        return DesignContext?.Services?.CopyPasteService?.CanDelete(DesignContext) == true;
    }

    public void Delete()
    {
        DesignContext?.Services?.CopyPasteService?.Delete(DesignContext);
    }

    public bool CanPaste()
    {
        return DesignContext?.Services?.CopyPasteService?.CanPaste(DesignContext) == true;
    }

    public void Paste()
    {
        DesignContext?.Services?.CopyPasteService?.Paste(DesignContext);
    }

    public bool CanSelectAll()
    {
        return DesignContext != null;
    }

    //TODO: Do not select layout root
    public void SelectAll()
    {
        var items = Descendants(DesignContext.RootItem).Where(item => ModelTools.CanSelectComponent(item)).ToArray();
        DesignContext.Services.Selection.SetSelectedComponents(items);
    }

    public void UnselectAll()
    {
        DesignContext.Services.Selection.SetSelectedComponents(null);
    }

    //TODO: Share with Outline / PlacementBehavior
    public static IEnumerable<DesignItem> DescendantsAndSelf(DesignItem item)
    {
        yield return item;
        foreach (var child in Descendants(item)) yield return child;
    }

    public static IEnumerable<DesignItem> Descendants(DesignItem item)
    {
        if (item.ContentPropertyName != null)
        {
            var content = item.ContentProperty;
            if (content.IsCollection)
            {
                foreach (var child in content.CollectionElements)
                foreach (var child2 in DescendantsAndSelf(child))
                    yield return child2;
            }
            else
            {
                if (content.Value != null)
                    foreach (var child2 in DescendantsAndSelf(content.Value))
                        yield return child2;
            }
        }
    }

    // Filters an element list, dropping all elements that are not part of the xaml document
    // (e.g. because they were deleted).
    private static List<DesignItem> GetLiveElements(ICollection<DesignItem> items)
    {
        var result = new List<DesignItem>(items.Count);
        foreach (var item in items)
            if (ModelTools.IsInDocument(item) && ModelTools.CanSelectComponent(item))
                result.Add(item);

        return result;
    }

    private T GetService<T>() where T : class
    {
        if (DesignContext != null)
            return DesignContext.Services.GetService<T>();
        return null;
    }

    #endregion

    #region INotifyPropertyChanged implementation

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged(string propertyName)
    {
        var ev = PropertyChanged;
        if (ev != null)
            ev(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    private void ShowGridSettings()
    {
        var window = new Windows.GridSettingsWindow(_designPanel);
        // Avalonia window ownership is different
        window.Show();
    }
}