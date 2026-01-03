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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using MyDesigner.Design.Adorners;
using MyDesigner.Design.Extensions;
using MyDesigner.Design.UIExtensions;
using MyDesigner.Designer.Controls;

namespace MyDesigner.Designer.Extensions;

/// <summary>
///     Description of PolyLineHandlerExtension.
/// </summary>
[ExtensionFor(typeof(Polyline))]
[ExtensionFor(typeof(Polygon))]
public class PolyLineHandlerExtension : LineExtensionBase, IKeyDown, IKeyUp
{
    private readonly Dictionary<int, Point> _selectedPoints = new();
    private bool _isDragging;
    private ZoomControl _zoom;

    private IList<Point> GetPointCollection()
    {
        var pg = ExtendedItem.View as Polygon;
        var pl = ExtendedItem.View as Polyline;

        return pl == null ? pg.Points : pl.Points;
    }

    private IList<Point> MovePoints(IList<Point> pc, double displacementX, double displacementY, double theta,
        int? snapangle)
    {
        //iterate all selected points
        foreach (var i in _selectedPoints.Keys)
        {
            var p = pc[i];

            //x and y is calculated from the currentl point
            var x = _selectedPoints[i].X + displacementX;
            var y = _selectedPoints[i].Y + displacementY;

            //if snap is applied
            if (snapangle != null)
                if (_selectedPoints.Count > 0)
                {
                    //horizontal snap
                    if (Math.Abs(theta) < snapangle || 180 - Math.Abs(theta) < snapangle)
                        //if one point selected use point before as snap point, else snap to movement
                        y = _selectedPoints.Count == 1 ? pc[i - 1].Y : y - displacementY;
                    else if (Math.Abs(90 - Math.Abs(theta)) < snapangle) //vertical snap
                        //if one point selected use point before as snap point, else snap to movement
                        x = _selectedPoints.Count == 1 ? pc[i - 1].X : x - displacementX;
                }

            pc[i] = new Point(x, y);
        }

        return pc;
    }

    #region thumb methods

    protected DesignerThumb CreateThumb(PlacementAlignment alignment, Cursor cursor, int index)
    {
        DesignerThumb designerThumb = new MultiPointThumb
            { Index = index, Alignment = alignment, Cursor = cursor, IsPrimarySelection = true };
        AdornerPlacement ap = Place(designerThumb, alignment, index);
        (designerThumb as MultiPointThumb).AdornerPlacement = ap;

        AdornerPanel.SetPlacement(designerThumb, ap);
        adornerPanel.Children.Add(designerThumb);

        var drag = new DragListener(designerThumb);

        designerThumb.PointerPressed += ResizeThumbOnPointerPressed;

        drag.Started += drag_Started;
        drag.Changed += drag_Changed;
        drag.Completed += drag_Completed;
        return designerThumb;
    }

    private void ResetThumbs()
    {
        foreach (Control rt in adornerPanel.Children)
            if (rt is DesignerThumb)
                (rt as DesignerThumb).IsPrimarySelection = true;
        _selectedPoints.Clear();
    }

    private void SelectThumb(MultiPointThumb mprt)
    {
        var points = GetPointCollection();
        var p = points[mprt.Index];
        _selectedPoints.Add(mprt.Index, p);

        mprt.IsPrimarySelection = false;
    }

    #endregion

    #region eventhandlers

    private void ResizeThumbOnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        //get current thumb
        var mprt = sender as MultiPointThumb;
        if (mprt != null)
        {
            var keyModifiers = e.KeyModifiers;
            
            //shift+ctrl will remove selected point
            if (keyModifiers.HasFlag(KeyModifiers.Shift) && keyModifiers.HasFlag(KeyModifiers.Control))
            {
                //unselect all points
                ResetThumbs();
                var points = GetPointCollection();

                //iterate thumbs to lower index of remaining thumbs
                foreach (MultiPointThumb m in adornerPanel.Children)
                    if (m.Index > mprt.Index)
                        m.Index--;

                //remove point and thumb
                points.RemoveAt(mprt.Index);
                adornerPanel.Children.Remove(mprt);

                Invalidate();
            }
            else
            {
                //if not keyboard ctrl is pressed and selected point is not previously selected, clear selection
                if (!_selectedPoints.ContainsKey(mprt.Index) && !keyModifiers.HasFlag(KeyModifiers.Control))
                    ResetThumbs();
                //add selected thumb, if ctrl pressed this could be all points in poly
                if (!_selectedPoints.ContainsKey(mprt.Index))
                    SelectThumb(mprt);
                _isDragging = false;
            }
        }
    }

    // TODO : Remove all hide/show extensions from here.
    protected void drag_Started(DragListener drag)
    {
        //get current thumb
        var mprt = drag.Target as MultiPointThumb;
        if (mprt != null) SetOperation();
    }

    private void SetOperation()
    {
        var designPanel = ExtendedItem.Services.DesignPanel as DesignPanel;
        _zoom = designPanel.FindAncestorOfType<ZoomControl>();

        if (resizeBehavior != null)
            operation = PlacementOperation.Start(extendedItemArray, PlacementType.Resize);
        else
            changeGroup = ExtendedItem.Context.OpenGroup("Resize", extendedItemArray);
        _isResizing = true;
    }

    private void CommitOperation()
    {
        if (operation != null)
        {
            IList<Point> points;
            var pg = ExtendedItem.View as Polygon;
            var pl = ExtendedItem.View as Polyline;
            if (pl == null)
                points = pg.Points;
            else
                points = pl.Points;

            foreach (var i in _selectedPoints.Keys.ToList()) _selectedPoints[i] = points[i];
            ExtendedItem.Properties.GetProperty(pl != null ? Polyline.PointsProperty : Polygon.PointsProperty)
                .SetValue(points);
            operation.Commit();

            operation = null;
        }
        else
        {
            if (changeGroup != null)
                changeGroup.Commit();
            changeGroup = null;
        }

        _isResizing = false;

        Invalidate();
    }

    protected void drag_Changed(DragListener drag)
    {
        var points = GetPointCollection();

        var mprt = drag.Target as MultiPointThumb;
        if (mprt != null)
        {
            double dx = 0;
            double dy = 0;
            //if has zoomed
            if (_zoom != null)
            {
                dx = drag.Delta.X * (1 / _zoom.CurrentZoom);
                dy = drag.Delta.Y * (1 / _zoom.CurrentZoom);
            }

            double theta;
            //if one point selected snapping angle is calculated in relation to previous point
            if (_selectedPoints.Count == 1 && mprt.Index > 0)
                theta = 180 / Math.PI * Math.Atan2(_selectedPoints[mprt.Index].Y + dy - points[mprt.Index - 1].Y,
                    _selectedPoints[mprt.Index].X + dx - points[mprt.Index - 1].X);
            else
                //if multiple points snapping angle is calculated in relation to mouse dragging angle
                theta = 180 / Math.PI * Math.Atan2(dy, dx);

            //snappingAngle is used for snapping function to horizontal or vertical plane in line drawing, and is activated by pressing ctrl or shift button
            int? snapAngle = null;

            var keyModifiers = TopLevel.GetTopLevel(drag.Target as Visual)?.PlatformImpl?.GetKeyModifiers() ?? KeyModifiers.None;

            //shift+alt gives a new point
            if (keyModifiers.HasFlag(KeyModifiers.Shift) && keyModifiers.HasFlag(KeyModifiers.Alt))
            {
                //if dragging occurs on a point and that point is the only selected, a new node will be added.
                //_isCtrlDragging is needed since this method is called for every x pixel that the mouse moves
                //so it could be many thousands of times during a single dragging
                if (!_isDragging && _selectedPoints.Count == 1 && (Math.Abs(dx) > 0 || Math.Abs(dy) > 0))
                {
                    //duplicate point that is selected
                    var p = points[mprt.Index];

                    //insert duplicate
                    points.Insert(mprt.Index, p);

                    //create adorner marker
                    CreateThumb(PlacementAlignment.BottomRight, new Cursor(StandardCursorType.Cross), mprt.Index);

                    //set index of all points that had a higher index than selected to +1
                    foreach (Control rt in adornerPanel.Children)
                        if (rt is MultiPointThumb)
                        {
                            var t = rt as MultiPointThumb;
                            if (t.Index > mprt.Index)
                                t.Index++;
                        }

                    //set index of new point to old point index + 1
                    mprt.Index = mprt.Index + 1;
                    ResetThumbs();
                    SelectThumb(mprt);
                }

                snapAngle = 10;
            }

            //snapping occurs when mouse is within 10 degrees from horizontal or vertical plane if shift is pressed
            else if (keyModifiers.HasFlag(KeyModifiers.Shift))
            {
                snapAngle = 10;
            }
            //snapping occurs within 45 degree intervals that is line will always be horizontal or vertical if alt is pressed
            else if (keyModifiers.HasFlag(KeyModifiers.Alt))
            {
                snapAngle = 45;
            }

            _isDragging = true;
            points = MovePoints(points, dx, dy, theta, snapAngle);
        }

        (drag.Target as DesignerThumb).InvalidateArrange();
    }

    protected void drag_Completed(DragListener drag)
    {
        var mprt = drag.Target as MultiPointThumb;
        if (mprt != null)
        {
            if (operation != null && drag.IsCanceled)
                operation.Abort();
            else if (drag.IsCanceled) changeGroup.Abort();
            CommitOperation();
        }
    }


    protected override void OnInitialized()
    {
        base.OnInitialized();

        var points = GetPointCollection();

        resizeThumbs = new List<DesignerThumb>();
        for (var i = 0; i < points.Count; i++) CreateThumb(PlacementAlignment.BottomRight, new Cursor(StandardCursorType.Cross), i);

        Invalidate();

        ResetThumbs();
        _isDragging = false;

        extendedItemArray[0] = ExtendedItem;
        ExtendedItem.PropertyChanged += OnPropertyChanged;
        resizeBehavior = PlacementOperation.GetPlacementBehavior(extendedItemArray);
        UpdateAdornerVisibility();
    }
    #endregion
    #region IKeyDown

    public bool InvokeDefaultAction =>
        _selectedPoints.Count == 0 || _selectedPoints.Count == GetPointCollection().Count - 1;

    private int _movingDistance;

    public void KeyDownAction(object sender, KeyEventArgs e)
    {
        Debug.WriteLine("KeyDown");
        if (IsArrowKey(e.Key))
            if (operation == null)
            {
                SetOperation();
                _movingDistance = 0;
            }

        var keyModifiers = e.KeyModifiers;
        var dx1 = e.Key == Key.Left
            ? keyModifiers.HasFlag(KeyModifiers.Shift) ? _movingDistance - 10 : _movingDistance - 1
            : 0;
        var dy1 = e.Key == Key.Up ? keyModifiers.HasFlag(KeyModifiers.Shift) ? _movingDistance - 10 : _movingDistance - 1 : 0;
        var dx2 = e.Key == Key.Right
            ? keyModifiers.HasFlag(KeyModifiers.Shift) ? _movingDistance + 10 : _movingDistance + 1
            : 0;
        var dy2 = e.Key == Key.Down
            ? keyModifiers.HasFlag(KeyModifiers.Shift) ? _movingDistance + 10 : _movingDistance + 1
            : 0;

        _movingDistance = dx1 + dx2 + dy1 + dy2;
    }

    public void KeyUpAction(object sender, KeyEventArgs e)
    {
        Debug.WriteLine("Keyup");
        if (IsArrowKey(e.Key))
            CommitOperation();
    }

    private bool IsArrowKey(Key key)
    {
        return key == Key.Left || key == Key.Right || key == Key.Up || key == Key.Down;
    }

    #endregion
}