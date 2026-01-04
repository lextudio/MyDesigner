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
using Avalonia.Input;
using MyDesigner.Designer.Xaml;

namespace MyDesigner.Designer.Services;

/// <summary>
///     A tool that creates a component.
/// </summary>
public class CreateComponentTool : ITool
{
    private readonly object[] arguments;

    protected ChangeGroup ChangeGroup;
    private Point createPoint;

    private MoveLogic moveLogic;

    /// <summary>
    ///     Creates a new CreateComponentTool instance.
    /// </summary>
    public CreateComponentTool(Type componentType) : this(componentType, null)
    {
    }

    /// <summary>
    ///     Creates a new CreateComponentTool instance.
    /// </summary>
    public CreateComponentTool(Type componentType, object[] arguments)
    {
        if (componentType == null)
            throw new ArgumentNullException(nameof(componentType));
        ComponentType = componentType;
        this.arguments = arguments;
    }

    /// <summary>
    ///     Gets the type of the component to be created.
    /// </summary>
    public Type ComponentType { get; }

    public Cursor Cursor => new Cursor(StandardCursorType.Cross);

    public void Activate(IDesignPanel designPanel)
    {
        designPanel.PointerPressed += OnPointerPressed;
        designPanel.DragOver += designPanel_DragOver;
        designPanel.Drop += designPanel_Drop;
        designPanel.DragLeave += designPanel_DragLeave;
    }

    public void Deactivate(IDesignPanel designPanel)
    {
        designPanel.PointerPressed -= OnPointerPressed;
        designPanel.DragOver -= designPanel_DragOver;
        designPanel.Drop -= designPanel_Drop;
        designPanel.DragLeave -= designPanel_DragLeave;
    }

    public event EventHandler<DesignItem> CreateComponentCompleted;

    private void designPanel_DragOver(object sender, DragEventArgs e)
    {
        try
        {
            var designPanel = (IDesignPanel)sender;
            e.DragEffects = DragDropEffects.Copy;
            e.Handled = true;
            var p = e.GetPosition(designPanel as Visual);

            if (moveLogic == null)
            {
                if (e.Data.Get(GetType().FullName) != this) return;
                // TODO: dropLayer in designPanel
                designPanel.IsAdornerLayerHitTestVisible = false;
                var result = designPanel.HitTest(p, false, true, HitTestType.Default);

                if (result.ModelHit != null)
                {
                    designPanel.Focus();
                    var items = CreateItemsWithPosition(designPanel.Context, e.GetPosition(result.ModelHit.View));
                    if (items != null)
                    {
                        if (AddItemsWithDefaultSize(result.ModelHit, items))
                        {
                            moveLogic = new MoveLogic(items[0]);

                            foreach (var designItem in items)
                                if (designPanel.Context.Services.Component is XamlComponentService)
                                    ((XamlComponentService)designPanel.Context.Services.Component)
                                        .RaiseComponentRegisteredAndAddedToContainer(designItem);

                            createPoint = p;
                            // We'll keep the ChangeGroup open as long as the moveLogic is active.
                        }
                        else
                        {
                            // Abort the ChangeGroup created by the CreateItem() call.
                            ChangeGroup.Abort();
                            ChangeGroup = null;
                        }
                    }
                    else
                    {
                        var createdItem =
                            CreateItemWithPosition(designPanel.Context, e.GetPosition(result.ModelHit.View));

                        CreateComponentCompleted?.Invoke(this, createdItem);

                        if (AddItemsWithDefaultSize(result.ModelHit, new[] { createdItem }))
                        {
                            moveLogic = new MoveLogic(createdItem);

                            if (designPanel.Context.Services.Component is XamlComponentService)
                                ((XamlComponentService)designPanel.Context.Services.Component)
                                    .RaiseComponentRegisteredAndAddedToContainer(createdItem);

                            createPoint = p;
                            // We'll keep the ChangeGroup open as long as the moveLogic is active.
                        }
                        else
                        {
                            // Abort the ChangeGroup created by the CreateItem() call.
                            ChangeGroup.Abort();
                            ChangeGroup = null;
                        }
                    }
                }
            }
            else if (moveLogic.ClickedOn.View.IsLoaded)
            {
                if (moveLogic.Operation == null)
                    moveLogic.Start(createPoint);
                else
                    moveLogic.Move(p);
            }
        }
        catch (Exception x)
        {
            DragDropExceptionHandler.RaiseUnhandledException(x);
        }
    }

    private void designPanel_Drop(object sender, DragEventArgs e)
    {
        try
        {
            if (moveLogic != null)
            {
                moveLogic.Stop();
                if (moveLogic.ClickedOn.Services.Tool.CurrentTool is CreateComponentTool)
                    moveLogic.ClickedOn.Services.Tool.CurrentTool = moveLogic.ClickedOn.Services.Tool.PointerTool;
                moveLogic.DesignPanel.IsAdornerLayerHitTestVisible = true;
                moveLogic = null;
                ChangeGroup.Commit();
                ChangeGroup = null;

                e.Handled = true;
            }
        }
        catch (Exception x)
        {
            DragDropExceptionHandler.RaiseUnhandledException(x);
        }
    }

    private void designPanel_DragLeave(object sender, DragEventArgs e)
    {
        try
        {
            if (moveLogic != null)
            {
                moveLogic.Cancel();
                moveLogic.ClickedOn.Services.Selection.SetSelectedComponents(null);
                moveLogic.DesignPanel.IsAdornerLayerHitTestVisible = true;
                moveLogic = null;
                ChangeGroup.Abort();
                ChangeGroup = null;
            }
        }
        catch (Exception x)
        {
            DragDropExceptionHandler.RaiseUnhandledException(x);
        }
    }

    /// <summary>
    ///     Is called to create the item used by the CreateComponentTool.
    /// </summary>
    protected virtual DesignItem CreateItemWithPosition(DesignContext context, Point position)
    {
        var item = CreateItem(context);
        item.Position = position;
        return item;
    }

    /// <summary>
    ///     Is called to create the item used by the CreateComponentTool.
    /// </summary>
    protected virtual DesignItem CreateItem(DesignContext context)
    {
        if (ChangeGroup == null)
            ChangeGroup = context.RootItem.OpenGroup("Add Control");

        var item = CreateItem(context, ComponentType, arguments);

        return item;
    }

    protected virtual DesignItem[] CreateItemsWithPosition(DesignContext context, Point position)
    {
        var items = CreateItems(context);
        if (items != null)
            foreach (var designItem in items)
                designItem.Position = position;

        return items;
    }

    protected virtual DesignItem[] CreateItems(DesignContext context)
    {
        return null;
    }

    /// <summary>
    ///     Is called to create the item used by the CreateComponentTool.
    /// </summary>
    public static DesignItem CreateItem(DesignContext context, Type type)
    {
        return CreateItem(context, type, null);
    }

    /// <summary>
    ///     Is called to create the item used by the CreateComponentTool.
    /// </summary>
    public static DesignItem CreateItem(DesignContext context, Type type, object[] arguments)
    {
        var newInstance = context.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(type, arguments);
        var item = context.Services.Component.RegisterComponentForDesigner(newInstance);
        context.Services.Component.SetDefaultPropertyValues(item);
        context.Services.ExtensionManager.ApplyDefaultInitializers(item);
        
        return item;
    }

    /// <summary>
    ///     Is called to set Properties of the Drawn Item
    /// </summary>
    protected virtual void SetPropertiesForDrawnItem(DesignItem designItem)
    {
    }

    public static bool AddItemWithCustomSizePosition(DesignItem container, Type createdItem, Size size, Point position)
    {
        return AddItemWithCustomSizePosition(container, createdItem, null, size, position);
    }

    public static bool AddItemWithCustomSizePosition(DesignItem container, Type createdItem, object[] arguments,
        Size size, Point position)
    {
        var cct = new CreateComponentTool(createdItem, arguments);
        return AddItemsWithCustomSize(container, new[] { cct.CreateItem(container.Context) },
            new[] { new Rect(position, size) });
    }

    public static bool AddItemWithDefaultSize(DesignItem container, Type createdItem, Size size)
    {
        return AddItemWithDefaultSize(container, createdItem, null, size);
    }

    public static bool AddItemWithDefaultSize(DesignItem container, Type createdItem, object[] arguments, Size size)
    {
        var cct = new CreateComponentTool(createdItem, arguments);
        return AddItemsWithCustomSize(container, new[] { cct.CreateItem(container.Context) },
            new[] { new Rect(new Point(0, 0), size) });
    }

    internal static bool AddItemsWithDefaultSize(DesignItem container, DesignItem[] createdItems)
    {
        return AddItemsWithCustomSize(container, createdItems,
            createdItems.Select(x => new Rect(x.Position, ModelTools.GetDefaultSize(x))).ToList());
    }

    internal static bool AddItemsWithCustomSize(DesignItem container, DesignItem[] createdItems, IList<Rect> positions)
    {
        PlacementOperation operation = null;

        while (operation == null && container != null)
        {
            operation = PlacementOperation.TryStartInsertNewComponents(
                container,
                createdItems,
                positions,
                PlacementType.AddItem
            );

            if (operation != null)
                break;

            try
            {
                if (container.Parent != null)
                {
                    var rel = container.View.TranslatePoint(new Point(0, 0), container.Parent.View);
                    for (var index = 0; index < positions.Count; index++)
                        positions[index] = new Rect(new Point(positions[index].X + rel.Value.X, positions[index].Y + rel.Value.Y),
                            positions[index].Size);
                }
            }
            catch (Exception)
            {
            }

            container = container.Parent;
        }

        if (operation != null)
        {
            container.Services.Selection.SetSelectedComponents(createdItems);
            operation.Commit();
            return true;
        }

        return false;
    }

    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
        {
            e.Handled = true;
            var designPanel = (IDesignPanel)sender;
            var result = designPanel.HitTest(e.GetPosition(designPanel as Visual), false, true, HitTestType.Default);
            if (result.ModelHit != null)
            {
                var darwItemBehaviors = result.ModelHit.Extensions.OfType<IDrawItemExtension>();
                var drawItembehavior = darwItemBehaviors.FirstOrDefault(x => x.CanItemBeDrawn(ComponentType));
                if (drawItembehavior != null && drawItembehavior.CanItemBeDrawn(ComponentType))
                {
                    drawItembehavior.StartDrawItem(result.ModelHit, ComponentType, designPanel, e,
                        SetPropertiesForDrawnItem);
                }
                else
                {
                    var placementBehavior = result.ModelHit.GetBehavior<IPlacementBehavior>();
                    if (placementBehavior != null)
                    {
                        var createdItem = CreateItem(designPanel.Context);

                        CreateComponentCompleted?.Invoke(this, createdItem);

                        new CreateComponentMouseGesture(result.ModelHit, createdItem, ChangeGroup)
                            .Start(designPanel, e);
                        // CreateComponentMouseGesture now is responsible for the changeGroup created by CreateItem()
                        ChangeGroup = null;
                    }
                }
            }
        }
    }
}

internal sealed class CreateComponentMouseGesture : ClickOrDragMouseGesture
{
    private readonly DesignItem container;
    private readonly DesignItem createdItem;
    private ChangeGroup changeGroup;
    private PlacementOperation operation;

    public CreateComponentMouseGesture(DesignItem clickedOn, DesignItem createdItem, ChangeGroup changeGroup)
    {
        container = clickedOn;
        this.createdItem = createdItem;
        positionRelativeTo = clickedOn.View;
        this.changeGroup = changeGroup;
    }

    private Rect GetStartToEndRect(PointerEventArgs e)
    {
        var endPoint = e.GetPosition(positionRelativeTo as Visual);
        return new Rect(
            Math.Min(startPoint.X, endPoint.X),
            Math.Min(startPoint.Y, endPoint.Y),
            Math.Abs(startPoint.X - endPoint.X),
            Math.Abs(startPoint.Y - endPoint.Y)
        );
    }

    protected override void OnDragStarted(PointerEventArgs e)
    {
        operation = PlacementOperation.TryStartInsertNewComponents(container,
            new[] { createdItem },
            new[] { GetStartToEndRect(e).Round() },
            PlacementType.Resize);
        if (operation != null) services.Selection.SetSelectedComponents(new[] { createdItem });
    }

    protected override void OnPointerMoved(object sender, PointerEventArgs e)
    {
        base.OnPointerMoved(sender, e);
        if (operation != null)
            foreach (var info in operation.PlacedItems)
            {
                info.Bounds = GetStartToEndRect(e).Round();
                operation.CurrentContainerBehavior.SetPosition(info);
            }
    }

    protected override void OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (hasDragStarted)
        {
            if (operation != null)
            {
                operation.Commit();
                operation = null;
            }
        }
        else
        {
            CreateComponentTool.AddItemsWithDefaultSize(container, new[] { createdItem });
        }

        if (changeGroup != null)
        {
            changeGroup.Commit();
            changeGroup = null;
        }

        if (designPanel.Context.Services.Component is XamlComponentService)
            ((XamlComponentService)designPanel.Context.Services.Component)
                .RaiseComponentRegisteredAndAddedToContainer(createdItem);

        base.OnPointerReleased(sender, e);
    }

    protected override void OnStopped()
    {
        if (operation != null)
        {
            operation.Abort();
            operation = null;
        }

        if (changeGroup != null)
        {
            changeGroup.Abort();
            changeGroup = null;
        }

        if (services.Tool.CurrentTool is CreateComponentTool) services.Tool.CurrentTool = services.Tool.PointerTool;
        base.OnStopped();
    }
}