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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using MyDesigner.Design.Adorners;
using MyDesigner.Design.Extensions;
using MyDesigner.Designer.Controls;

namespace MyDesigner.Designer.Extensions;

/// <summary>
///     The resize thumb around a component.
/// </summary>
[ExtensionServer(typeof(OnlyOneItemSelectedExtensionServer))]
[ExtensionFor(typeof(Control))]
public sealed class RotateThumbExtension : SelectionAdornerProvider
{
    private readonly AdornerPanel adornerPanel;

    /// <summary>An array containing this.ExtendedItem as only element</summary>
    private readonly DesignItem[] extendedItemArray = new DesignItem[1];

    private readonly Thumb thumb;
    private PlacementOperation operation;

    private IPlacementBehavior resizeBehavior;

    public RotateThumbExtension()
    {
        adornerPanel = new AdornerPanel();
        adornerPanel.Order = AdornerOrder.Foreground;
        Adorners.Add(adornerPanel);

        thumb = CreateRotateThumb();
    }

    private DesignerThumb CreateRotateThumb()
    {
        DesignerThumb rotateThumb = new RotateThumb();
        rotateThumb.Cursor = ZoomControl.GetCursor("avares://MyDesigner.Designer/Images/rotate.cur");
        // In Avalonia, we'll use a standard cursor instead of custom cursor file
        rotateThumb.Alignment = PlacementAlignment.Top;
        AdornerPanel.SetPlacement(rotateThumb,
            new RelativePlacement(HorizontalAlignment.Center, VerticalAlignment.Top)
                { WidthRelativeToContentWidth = 1, HeightOffset = 0 });
        adornerPanel.Children.Add(rotateThumb);

        var drag = new DragListener(rotateThumb);
        drag.Started += drag_Rotate_Started;
        drag.Changed += drag_Rotate_Changed;
        drag.Completed += drag_Rotate_Completed;
        return rotateThumb;
    }

    protected override void OnInitialized()
    {
        if (ExtendedItem.Component is WindowClone)
            return;
        base.OnInitialized();
        extendedItemArray[0] = ExtendedItem;
        ExtendedItem.PropertyChanged += OnPropertyChanged;
        Services.Selection.PrimarySelectionChanged += OnPrimarySelectionChanged;
        resizeBehavior = PlacementOperation.GetPlacementBehavior(extendedItemArray);
        OnPrimarySelectionChanged(null, null);

        var designerItem = ExtendedItem.Component as Control;
        rotateTransform = designerItem.RenderTransform as RotateTransform;
        if (rotateTransform == null)
        {
            var tg = designerItem.RenderTransform as TransformGroup;
            if (tg != null) rotateTransform = tg.Children.FirstOrDefault(x => x is RotateTransform) as RotateTransform;
        }
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
    }

    protected override void OnRemove()
    {
        ExtendedItem.PropertyChanged -= OnPropertyChanged;
        Services.Selection.PrimarySelectionChanged -= OnPrimarySelectionChanged;
        base.OnRemove();
    }

    private void OnPrimarySelectionChanged(object sender, EventArgs e)
    {
        var isPrimarySelection = Services.Selection.PrimarySelection == ExtendedItem;
        foreach (RotateThumb g in adornerPanel.Children) g.IsPrimarySelection = isPrimarySelection;
    }

    #region Rotate

    private Point centerPoint;
    private Visual parent;
    private Vector startVector;
    private RotateTransform rotateTransform;
    private double initialAngle;
    private DesignItem rtTransform;

    private void drag_Rotate_Started(DragListener drag)
    {
        var designerItem = ExtendedItem.Component as Control;
        parent = designerItem.GetVisualParent();
        
        var renderTransformOrigin = designerItem.RenderTransformOrigin;
        centerPoint = new Point(
            designerItem.Bounds.Width * renderTransformOrigin.Point.X,
            designerItem.Bounds.Height * renderTransformOrigin.Point.Y);

        // In Avalonia, we need to handle pointer position differently
        var startPoint = drag.StartPoint;
        startVector = startPoint - centerPoint;

        if (rotateTransform == null)
            initialAngle = 0;
        else
            initialAngle = rotateTransform.Angle;

        rtTransform = ExtendedItem.Properties[Visual.RenderTransformProperty].Value;

        operation = PlacementOperation.Start(extendedItemArray, PlacementType.Resize);
    }

    private void drag_Rotate_Changed(DragListener drag)
    {
        var currentPoint = drag.CurrentPoint;
        var deltaVector = currentPoint - centerPoint;

        var angle = VectorExtensions.AngleBetween(startVector, deltaVector);

        var destAngle = initialAngle + Math.Round(angle, 0);

        var keyModifiers = TopLevel.GetTopLevel(drag.Target as Visual)?.GetKeyModifiers() ?? KeyModifiers.None;
        if (!keyModifiers.HasFlag(KeyModifiers.Control))
            destAngle = (int)destAngle / 15 * 15;

        ModelTools.ApplyTransform(ExtendedItem, new RotateTransform { Angle = destAngle }, false);
    }

    private void drag_Rotate_Completed(DragListener drag)
    {
        operation.Commit();
    }

    #endregion
}