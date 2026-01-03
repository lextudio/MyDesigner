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
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using MyDesigner.Design;
using MyDesigner.Design.Adorners;
using MyDesigner.Design.Extensions;
using MyDesigner.Design.UIExtensions;
using MyDesigner.Designer.Controls;
using MyDesigner.Designer.Extensions;
using MyDesigner.Designer.Xaml;
using System.ComponentModel;
using System.Xml.Linq;
using AdornerLayerControl = MyDesigner.Designer.Controls.AdornerLayer;

namespace MyDesigner.Designer;

public sealed class DesignPanel : Decorator, IDesignPanel, INotifyPropertyChanged
{
    private readonly Dictionary<PlacementInformation, int> dx = new();
    private readonly Dictionary<PlacementInformation, int> dy = new();

    private PlacementOperation placementOp;

    public new event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///     If interface implementing class sets this to false defaultkeyaction will be
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    private bool InvokeDefaultKeyDownAction(Extension e)
    {
        var keyDown = e as IKeyDown;
        if (keyDown != null) return keyDown.InvokeDefaultAction;

        return true;
    }

    private void DesignPanel_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
        {
            e.Handled = true;

            if (placementOp != null)
            {
                placementOp.Commit();
                placementOp = null;
                dx.Clear();
                dy.Clear();
            }
        }

        //pass the key event to the underlying objects if they have implemented IKeyUp interface
        //OBS!!!! this call needs to be here, after the placementOp.Commit().
        //In case the underlying object has a operation of its own this operation needs to be commited first
        foreach (var di in Context.Services.Selection.SelectedItems.Reverse())
            foreach (var ext in di.Extensions)
            {
                var keyUp = ext as IKeyUp;
                if (keyUp != null) keyUp.KeyUpAction(sender, e);
            }
    }

    private void DesignPanel_KeyDown(object sender, KeyEventArgs e)
    {
        //pass the key event down to the underlying objects if they have implemented IKeyUp interface
        //OBS!!!! this call needs to be here, before the PlacementOperation.Start.
        //In case the underlying object has a operation of its own this operation needs to be set first
        foreach (var di in Context.Services.Selection.SelectedItems)
            foreach (var ext in di.Extensions)
            {
                var keyDown = ext as IKeyDown;
                if (keyDown != null) keyDown.KeyDownAction(sender, e);
            }

        if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
        {
            var initialEvent = false;

            e.Handled = true;

            var placementType = e.KeyModifiers.HasFlag(KeyModifiers.Control)
                ? PlacementType.Resize
                : PlacementType.MoveAndIgnoreOtherContainers;

            if (placementOp != null && placementOp.Type != placementType)
            {
                placementOp.Commit();
                placementOp = null;
                dx.Clear();
                dy.Clear();
            }

            if (placementOp == null)
            {
                //check if any objects don't want the default action to be invoked
                var placedItems = Context.Services.Selection.SelectedItems
                    .Where(x => x.Extensions.All(InvokeDefaultKeyDownAction)).ToList();

                //if no remaining objects, break
                if (placedItems.Count < 1) return;

                placementOp = PlacementOperation.Start(placedItems, placementType);

                dx.Clear();
                dy.Clear();
            }

            int odx = 0, ody = 0;
            switch (e.Key)
            {
                case Key.Left:
                    odx = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? -10 : -1;
                    break;
                case Key.Up:
                    ody = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? -10 : -1;
                    break;
                case Key.Right:
                    odx = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? 10 : 1;
                    break;
                case Key.Down:
                    ody = e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? 10 : 1;
                    break;
            }

            foreach (var info in placementOp.PlacedItems)
            {
                if (!dx.ContainsKey(info))
                {
                    dx[info] = 0;
                    dy[info] = 0;
                }

                var transform = Avalonia.VisualExtensions.TransformToVisual(info.Item.Parent.View, this);
                Matrix? mt = null;

                // In Avalonia 11.x, TransformToVisual may return different types
                if (transform != null)
                {
                    try
                    {
                        // Try to get matrix from transform
                        if (transform is Matrix matrix)
                        {
                            mt = matrix;
                        }
                        else if (transform.HasValue)
                        {
                            mt = transform.Value;
                        }
                    }
                    catch
                    {
                        // Fallback if transform extraction fails
                        mt = Matrix.Identity;
                    }
                }
                if (mt != null)
                {
                    var matrix = mt.Value;
                    var angle = Math.Atan2(matrix.M21, matrix.M11) * 180 / Math.PI;
                    if (angle > 45.0 && angle < 135.0)
                    {
                        dx[info] += ody * -1;
                        dy[info] += odx;
                    }
                    else if (angle < -45.0 && angle > -135.0)
                    {
                        dx[info] += ody;
                        dy[info] += odx * -1;
                    }
                    else if (angle > 135.0 || angle < -135.0)
                    {
                        dx[info] += odx * -1;
                        dy[info] += ody * -1;
                    }
                    else
                    {
                        dx[info] += odx;
                        dy[info] += ody;
                    }
                }

                var bounds = info.OriginalBounds;

                if (placementType == PlacementType.Move
                    || info.Operation.Type == PlacementType.MoveAndIgnoreOtherContainers)
                    info.Bounds = new Rect(bounds.Left + dx[info],
                        bounds.Top + dy[info],
                        bounds.Width,
                        bounds.Height);
                else if (placementType == PlacementType.Resize)
                    if (bounds.Width + dx[info] >= 0 && bounds.Height + dy[info] >= 0)
                        info.Bounds = new Rect(bounds.Left,
                            bounds.Top,
                            bounds.Width + dx[info],
                            bounds.Height + dy[info]);

                placementOp.CurrentContainerBehavior.SetPosition(info);
            }
        }
    }

    private static bool IsPropertySet(Control element, AvaloniaProperty property)
    {
        return element.IsSet(property);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (Context != null)
        {
            var cursor = Context.Services.Tool.CurrentTool.Cursor;
            if (cursor != null)
            {

                this.Cursor = cursor;

            }
            else
            {

                this.Cursor = Cursor.Default;
            }
        }
    }


    private void OnPropertyChanged(string propertyName)
    {
        var handler = PropertyChanged;
        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }

    #region Hit Testing

    private readonly List<DesignItem> hitTestElements = new();
    private readonly List<DesignItem> skippedHitTestElements = new();

    /// <summary>
    ///     this element is always hit (unless HitTestVisible is set to false)
    /// </summary>
    private sealed class EatAllHitTestRequests : Control
    {
        // In Avalonia, hit testing is handled differently
        // This class may need significant changes
    }

    private void RunHitTest(Visual reference, Point point, Func<Visual, bool> filterCallback,
        Func<Visual, bool> resultCallback)
    {
        if (!TopLevel.GetTopLevel(this)?.IsKeyDown(Key.LeftAlt) == true)
        {
            hitTestElements.Clear();
            skippedHitTestElements.Clear();
        }

        // Avalonia hit testing is different from WPF
        // This needs to be reimplemented using Avalonia's hit testing system
        var hitTest = reference.GetVisualsAt(point);
        foreach (var visual in hitTest)
        {
            if (!filterCallback(visual)) continue;
            if (!resultCallback(visual)) break;
        }
    }

    private bool FilterHitTestInvisibleElements(Visual potentialHitTestTarget, HitTestType hitTestType)
    {
        var element = potentialHitTestTarget as Control;

        if (element != null)
        {
            if (!(element.IsHitTestVisible && element.IsVisible))
                return false; // Skip this element and its children

            var designItem = Context.Services.Component.GetDesignItem(element) as XamlDesignItem;

            if (hitTestType == HitTestType.ElementSelection)
            {
                if (TopLevel.GetTopLevel(this)?.IsKeyDown(Key.LeftAlt) == true)
                    if (designItem != null)
                        if (skippedHitTestElements.LastOrDefault() == designItem ||
                            (hitTestElements.Contains(designItem) && !skippedHitTestElements.Contains(designItem)))
                        {
                            skippedHitTestElements.Remove(designItem);
                            return false; // Skip this element and its children
                        }
            }
            else
            {
                hitTestElements.Clear();
                skippedHitTestElements.Clear();
            }

            //if (designItem != null && designItem.IsDesignTimeLocked)
            //    return false; // Skip this element and its children

            if (designItem != null && !hitTestElements.Contains(designItem))
            {
                hitTestElements.Add(designItem);
                skippedHitTestElements.Add(designItem);
            }
        }

        return true; // Continue
    }

    /// <summary>
    ///     Performs a custom hit testing lookup for the specified mouse event args.
    /// </summary>
    public DesignPanelHitTestResult HitTest(Point mousePosition, bool testAdorners, bool testDesignSurface,
        HitTestType hitTestType)
    {
        var result = DesignPanelHitTestResult.NoHit;
        HitTest(mousePosition, testAdorners, testDesignSurface,
            delegate (DesignPanelHitTestResult r)
            {
                result = r;
                return false;
            }, hitTestType);

        return result;
    }

    /// <summary>
    ///     Performs a hit test on the design surface, raising <paramref name="callback" /> for each match.
    ///     Hit testing continues while the callback returns true.
    /// </summary>
    public void HitTest(Point mousePosition, bool testAdorners, bool testDesignSurface,
        Predicate<DesignPanelHitTestResult> callback, HitTestType hitTestType)
    {
        if (mousePosition.X < 0 || mousePosition.Y < 0 || mousePosition.X > Bounds.Width ||
            mousePosition.Y > Bounds.Height) return;

        // First try hit-testing on the adorner layer.
        var continueHitTest = true;

        var filterBehavior = CustomHitTestFilterBehavior ?? (x => FilterHitTestInvisibleElements(x, hitTestType));
        CustomHitTestFilterBehavior = null;

        if (testAdorners)
        {
            var adornerHitTest = AdornerLayer.GetVisualsAt(mousePosition);
            foreach (var visual in adornerHitTest)
            {
                if (!filterBehavior(visual)) continue;

                var customResult = new DesignPanelHitTestResult(visual);
                var obj = visual;
                while (obj != null && obj != AdornerLayer)
                {
                    var adorner = obj as AdornerPanel;
                    if (adorner != null) customResult.AdornerHit = adorner;
                    obj = Avalonia.VisualTree.VisualExtensions.GetVisualParent(obj);
                }

                continueHitTest = callback(customResult);
                if (!continueHitTest) break;
            }
        }

        if (continueHitTest && testDesignSurface && Child != null)
        {
            var surfaceHitTest = Child.GetVisualsAt(mousePosition);
            foreach (var visual in surfaceHitTest)
            {
                if (!filterBehavior(visual)) continue;

                var customResult = new DesignPanelHitTestResult(visual);

                var viewService = Context.Services.View;
                var obj = visual;

                while (obj != null)
                {
                    if ((customResult.ModelHit = viewService.GetModel(obj)) != null)
                        break;
                    obj = Avalonia.VisualTree.VisualExtensions.GetVisualParent(obj);
                }

                if (customResult.ModelHit == null) customResult.ModelHit = Context.RootItem;

                continueHitTest = callback(customResult);
                if (!continueHitTest) break;
            }
        }
    }

    #endregion

    #region Fields + Constructor

    private readonly EatAllHitTestRequests _eatAllHitTestRequests;

    public DesignPanel()
    {
        Focusable = true;
        //this.Margin = new Thickness(16);
        // DesignerProperties.SetIsInDesignMode equivalent in Avalonia
        // This might not be needed or handled differently

        _eatAllHitTestRequests = new EatAllHitTestRequests();
        //_eatAllHitTestRequests.PointerPressed += delegate
        //{
        //    // ensure the design panel has focus while the user is interacting with it
        //    Focus();
        //};
        // _eatAllHitTestRequests.AllowDrop = true; // Drag/drop in Avalonia is different
        AdornerLayer = new AdornerLayerControl(this);

        KeyUp += DesignPanel_KeyUp;
        KeyDown += DesignPanel_KeyDown;

        // Subscribe to drag and drop events
        DragDrop.SetAllowDrop(_eatAllHitTestRequests, true);
        AddHandler(DragDrop.DragEnterEvent, HandleDragEnter);
        AddHandler(DragDrop.DragOverEvent, HandleDragOver);
        AddHandler(DragDrop.DragLeaveEvent, HandleDragLeave);
        AddHandler(DragDrop.DropEvent, HandleDrop);
    }

    #endregion

    #region Properties

    public DesignSurface DesignSurface { get; internal set; }

    //Set custom HitTestFilterCallback - Avalonia version
    public Func<Visual, bool> CustomHitTestFilterBehavior { get; set; }

    public AdornerLayerControl AdornerLayer { get; }

    /// <summary>
    ///     Gets/Sets the design context.
    /// </summary>
    public DesignContext Context { get; set; }

    public ICollection<AdornerPanel> Adorners => AdornerLayer.Adorners;

    /// <summary>
    ///     Gets/Sets if the design content is visible for hit-testing purposes.
    /// </summary>
    public bool IsContentHitTestVisible
    {
        get => !_eatAllHitTestRequests.IsHitTestVisible;
        set => _eatAllHitTestRequests.IsHitTestVisible = !value;
    }

    /// <summary>
    ///     Gets/Sets if the adorner layer is visible for hit-testing purposes.
    /// </summary>
    public bool IsAdornerLayerHitTestVisible
    {
        get => AdornerLayer.IsHitTestVisible;
        set => AdornerLayer.IsHitTestVisible = value;
    }

    /// <summary>
    ///     Enables / Disables the Snapline Placement
    /// </summary>
    private bool _useSnaplinePlacement = true;

    public bool UseSnaplinePlacement
    {
        get => _useSnaplinePlacement;
        set
        {
            if (_useSnaplinePlacement != value)
            {
                _useSnaplinePlacement = value;
                OnPropertyChanged("UseSnaplinePlacement");
            }
        }
    }

    /// <summary>
    ///     Enables / Disables the Raster Placement
    /// </summary>
    private bool _useRasterPlacement;

    public bool UseRasterPlacement
    {
        get => _useRasterPlacement;
        set
        {
            if (_useRasterPlacement != value)
            {
                _useRasterPlacement = value;
                UpdateGridBackground();
                OnPropertyChanged("UseRasterPlacement");

            }
        }
    }

    private void UpdateGridBackground()
    {
        if (Context == null || Context.RootItem == null) return;

        var rootElement = Context.RootItem.View;
        if (rootElement == null) return;

        if (UseRasterPlacement)
        {
            // إظهار الشبكة
            ApplyGridBackground(rootElement);
        }
        else
        {
            // إخفاء الشبكة
            RemoveGridBackground(rootElement);
        }
    }

    private void ApplyGridBackground(Control element)
    {
        // استخدام RasterWidth من الإعدادات
        double gridSize = _rasterWidth > 0 ? _rasterWidth : 3;
        double smallGridSize = gridSize;
        double largeGridSize = gridSize * 4; // الشبكة الكبيرة = 4 أضعاف الصغيرة

        var drawingGroup = new DrawingGroup();

        // رسم الخلفية
        var backgroundDrawing = new GeometryDrawing
        {
            Brush = new SolidColorBrush(Color.FromRgb(250, 250, 250)),
            Geometry = new RectangleGeometry(new Rect(0, 0, largeGridSize, largeGridSize))
        };
        drawingGroup.Children.Add(backgroundDrawing);

        // رسم الشبكة الرئيسية فقط (بدون الشبكة الصغيرة)
        var gridPen = new Pen(new SolidColorBrush(Color.FromArgb(80, 150, 150, 150)), 1.0);
        var verticalLineDrawing = new GeometryDrawing
        {
            Pen = gridPen,
            Geometry = new LineGeometry(new Point(0, 0), new Point(0, largeGridSize))
        };
        drawingGroup.Children.Add(verticalLineDrawing);

        var horizontalLineDrawing = new GeometryDrawing
        {
            Pen = gridPen,
            Geometry = new LineGeometry(new Point(0, 0), new Point(largeGridSize, 0))
        };
        drawingGroup.Children.Add(horizontalLineDrawing);

        var drawingBrush = new DrawingBrush(drawingGroup)
        {
            TileMode = TileMode.Tile,
            DestinationRect = new RelativeRect(0, 0, largeGridSize, largeGridSize, RelativeUnit.Absolute)
        };

        // تطبيق الخلفية
        if (element is Window window)
        {
            // Store original background
            window.SetValue(TagProperty, window.Background);
            window.Background = drawingBrush;
        }
        else if (element is UserControl userControl)
        {
            userControl.SetValue(TagProperty, userControl.Background);
            userControl.Background = drawingBrush;
        }
        else if (element is Panel panel)
        {
            panel.SetValue(TagProperty, panel.Background);
            panel.Background = drawingBrush;
        }
        else if (element is Border border)
        {
            border.SetValue(TagProperty, border.Background);
            border.Background = drawingBrush;
        }
        // In Avalonia 11.x, Control doesn't have Background property
        // Only specific controls like Panel, Border, etc. have Background
    }


    private void RemoveGridBackground(Control element)
    {
        // استعادة الخلفية الأصلية
        if (element is Window window)
        {
            var originalBg = window.GetValue(TagProperty) as IBrush;
            window.Background = originalBg;
            window.ClearValue(TagProperty);
        }
        else if (element is UserControl userControl)
        {
            var originalBg = userControl.GetValue(TagProperty) as IBrush;
            userControl.Background = originalBg;
            userControl.ClearValue(TagProperty);
        }
        else if (element is Panel panel)
        {
            var originalBg = panel.GetValue(TagProperty) as IBrush;
            panel.Background = originalBg;
            panel.ClearValue(TagProperty);
        }
        else if (element is Border border)
        {
            var originalBg = border.GetValue(TagProperty) as IBrush;
            border.Background = originalBg;
            border.ClearValue(TagProperty);
        }
        // In Avalonia 11.x, Control doesn't have Background property
        // Only specific controls like Panel, Border, etc. have Background
    }




    /// <summary>
    /// Sets the with of the Raster when using Raster Placement
    /// </summary>
    private int _rasterWidth = 5;
    public int RasterWidth
    {
        get { return _rasterWidth; }
        set
        {
            if (_rasterWidth != value)
            {
                _rasterWidth = value;
                OnPropertyChanged("RasterWidth");
            }
        }
    }

    #endregion

    #region Visual Child Management
    public new Control? Child
    {
        get { return base.Child; }
        set
        {
            if (base.Child == value)
                return;
            if (value == null)
            {
                // Child is being set from some value to null

                // remove _adornerLayer and _eatAllHitTestRequests
                ((ISetLogicalParent)AdornerLayer).SetParent(null);
                LogicalChildren.Clear();
                VisualChildren.Remove(AdornerLayer);
                VisualChildren.Remove(_eatAllHitTestRequests);

            }
            else if (base.Child == null)
            {
                // Child is being set from null to some value
                ((ISetLogicalParent)AdornerLayer).SetParent(this);
                // إضافة _eatAllHitTestRequests أولاً ثم AdornerLayer لضمان ظهور الكنترولات فوق النافذة
                this.VisualChildren.Add(_eatAllHitTestRequests);
                this.VisualChildren.Add(AdornerLayer);
                this.LogicalChildren.Add(_eatAllHitTestRequests);
                this.LogicalChildren.Add(AdornerLayer);
            }
            base.Child = value;
        }
    }



    protected override Size MeasureOverride(Size constraint)
    {
        Size result = base.MeasureOverride(constraint);
        if (this.Child != null)
        {
            AdornerLayer.Measure(constraint);
            _eatAllHitTestRequests.Measure(constraint);
        }
        return result;
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        Size result = base.ArrangeOverride(arrangeSize);
        if (this.Child != null)
        {
            Rect r = new Rect(new Point(0, 0), arrangeSize);
            AdornerLayer.Arrange(r);
            _eatAllHitTestRequests.Arrange(r);
        }
        return result;
    }
    #endregion


    #region ContextMenu

    private readonly Dictionary<ContextMenu, Tuple<int, List<object>>> contextMenusAndEntries = new();

    public Action<ContextMenu> ContextMenuHandler { get; set; }

    public void AddContextMenu(ContextMenu contextMenu)
    {
        AddContextMenu(contextMenu, int.MaxValue);
    }

    public void AddContextMenu(ContextMenu contextMenu, int orderIndex)
    {
        contextMenusAndEntries.Add(contextMenu,
            new Tuple<int, List<object>>(orderIndex, new List<object>(contextMenu.Items.Cast<object>())));
        contextMenu.Items.Clear();

        UpdateContextMenu();
    }

    public void RemoveContextMenu(ContextMenu contextMenu)
    {
        contextMenusAndEntries.Remove(contextMenu);

        UpdateContextMenu();
    }

    public void ClearContextMenu()
    {
        contextMenusAndEntries.Clear();
        ContextMenu = null;

    }

    private void UpdateContextMenu()
    {
        if (ContextMenu != null)
        {
            ContextMenu.Items.Clear();
            ContextMenu = null;
        }

        var contextMenu = new ContextMenu();

        foreach (var entries in contextMenusAndEntries.Values.OrderBy(x => x.Item1).Select(x => x.Item2))
        {
            if (contextMenu.Items.Count > 0)
                contextMenu.Items.Add(new Separator());

            foreach (var entry in entries)
            {
                // Context menu item handling in Avalonia might be different
                contextMenu.Items.Add(entry);
            }
        }

        if (ContextMenuHandler != null)
            ContextMenuHandler(contextMenu);
        else
            ContextMenu = contextMenu;
    }

    #endregion

    #region Drag and Drop Events

    /// <summary>
    ///     Occurs when a drag operation enters the design panel.
    /// </summary>
    public event EventHandler<DragEventArgs> DragEnter;

    /// <summary>
    ///     Occurs when a drag operation is over the design panel.
    /// </summary>
    public event EventHandler<DragEventArgs> DragOver;

    /// <summary>
    ///     Occurs when a drag operation leaves the design panel.
    /// </summary>
    public event EventHandler<DragEventArgs> DragLeave;

    /// <summary>
    ///     Occurs when an element is dropped on the design panel.
    /// </summary>
    public event EventHandler<DragEventArgs> Drop;

    private void HandleDragEnter(object sender, DragEventArgs e)
    {
        DragEnter?.Invoke(this, e);
        e.DragEffects = DragDropEffects.Copy;
    }

    private void HandleDragOver(object sender, DragEventArgs e)
    {
        DragOver?.Invoke(this, e);
        e.DragEffects = DragDropEffects.Copy;
    }

    private void HandleDragLeave(object sender, DragEventArgs e)
    {
        DragLeave?.Invoke(this, e);
    }

    private void HandleDrop(object sender, DragEventArgs e)
    {
        Drop?.Invoke(this, e);
        e.DragEffects = DragDropEffects.Copy;
    }

    #endregion
}