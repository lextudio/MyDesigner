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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using MyDesigner.Design.Adorners;
using Path = Avalonia.Controls.Shapes.Path;

namespace MyDesigner.Designer.Controls;

/// <summary>
///     Adorner that displays the margin of a control in a Grid.
/// </summary>
public class CanvasPositionHandle : MarginHandle
{
    private readonly Control adornedControl;
    private readonly DesignItem adornedControlItem;
    private readonly AdornerPanel adornerPanel;
    private readonly INotifyPropertyChanged bottomDescriptor;

    private readonly Canvas canvas;
    private readonly INotifyPropertyChanged heightDescriptor;

    /// <summary> This grid contains the handle line and the endarrow.</summary>
    //		Grid lineArrow;
    private readonly INotifyPropertyChanged leftDescriptor;

    private readonly HandleOrientation orientation;
    private readonly INotifyPropertyChanged rightDescriptor;
    private readonly INotifyPropertyChanged topDescriptor;
    private readonly INotifyPropertyChanged widthDescriptor;

    private Path line1;
    private Path line2;

    static CanvasPositionHandle()
    {
        HandleLengthOffset = 2;
    }

    public CanvasPositionHandle(DesignItem adornedControlItem, AdornerPanel adornerPanel, HandleOrientation orientation)
    {
        Debug.Assert(adornedControlItem != null);
        this.adornedControlItem = adornedControlItem;
        this.adornerPanel = adornerPanel;
        this.orientation = orientation;

        Angle = (double)orientation;

        canvas = (Canvas)adornedControlItem.Parent.Component;
        adornedControl = (Control)adornedControlItem.Component;
        Stub = new MarginStub(this);
        ShouldBeVisible = true;

        // In Avalonia, we need to use property change notifications differently
        if (adornedControl is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
        }

        BindAndPlaceHandle();
    }

    /// <summary>
    ///     Gets/Sets the angle by which the Canvas display has to be rotated
    /// </summary>
    public override double TextTransform
    {
        get
        {
            if ((double)orientation == 90 || (double)orientation == 180)
                return 180;
            if ((double)orientation == 270)
                return 0;
            return (double)orientation;
        }
        set { }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        line1 = e.NameScope.Find<Path>("line1");
        line2 = e.NameScope.Find<Path>("line2");

        base.OnApplyTemplate(e);
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        BindAndPlaceHandle();
    }

    /// <summary>
    ///     Binds the <see cref="MarginHandle.HandleLength" /> to the margin and place the handles.
    /// </summary>
    private void BindAndPlaceHandle()
    {
        if (!adornerPanel.Children.Contains(this))
            adornerPanel.Children.Add(this);
        if (!adornerPanel.Children.Contains(Stub))
            adornerPanel.Children.Add(Stub);
        var placement = new RelativePlacement();
        switch (orientation)
        {
            case HandleOrientation.Left:
            {
                var wr = Canvas.GetLeft(adornedControl);
                if (double.IsNaN(wr))
                {
                    wr = Canvas.GetRight(adornedControl);
                    wr = canvas.Bounds.Width - (PlacementOperation.GetRealElementSize(adornedControl).Width + wr);
                }
                else
                {
                    if (line1 != null)
                    {
                        line1.StrokeDashArray = null;
                        line2.StrokeDashArray = null;
                    }
                }

                HandleLength = wr;
                placement = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Center);
                placement.XOffset = -HandleLengthOffset;
                break;
            }
            case HandleOrientation.Top:
            {
                var wr = Canvas.GetTop(adornedControl);
                if (double.IsNaN(wr))
                {
                    wr = Canvas.GetBottom(adornedControl);
                    wr = canvas.Bounds.Height - (PlacementOperation.GetRealElementSize(adornedControl).Height + wr);
                }
                else
                {
                    if (line1 != null)
                    {
                        line1.StrokeDashArray = null;
                        line2.StrokeDashArray = null;
                    }
                }

                HandleLength = wr;
                placement = new RelativePlacement(HorizontalAlignment.Center, VerticalAlignment.Top);
                placement.YOffset = -HandleLengthOffset;
                break;
            }
            case HandleOrientation.Right:
            {
                var wr = Canvas.GetRight(adornedControl);
                if (double.IsNaN(wr))
                {
                    wr = Canvas.GetLeft(adornedControl);
                    wr = canvas.Bounds.Width - (PlacementOperation.GetRealElementSize(adornedControl).Width + wr);
                }
                else
                {
                    if (line1 != null)
                    {
                        line1.StrokeDashArray = null;
                        line2.StrokeDashArray = null;
                    }
                }

                HandleLength = wr;
                placement = new RelativePlacement(HorizontalAlignment.Right, VerticalAlignment.Center);
                placement.XOffset = HandleLengthOffset;
                break;
            }
            case HandleOrientation.Bottom:
            {
                var wr = Canvas.GetBottom(adornedControl);
                if (double.IsNaN(wr))
                {
                    wr = Canvas.GetTop(adornedControl);
                    wr = canvas.Bounds.Height - (PlacementOperation.GetRealElementSize(adornedControl).Height + wr);
                }
                else
                {
                    if (line1 != null)
                    {
                        line1.StrokeDashArray = null;
                        line2.StrokeDashArray = null;
                    }
                }

                HandleLength = wr;
                placement = new RelativePlacement(HorizontalAlignment.Center, VerticalAlignment.Bottom);
                placement.YOffset = HandleLengthOffset;
                break;
            }
        }

        AdornerPanel.SetPlacement(this, placement);
        IsVisible = true;
    }
}