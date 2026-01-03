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
using Avalonia.Layout;

namespace MyDesigner.Design.Adorners;

/// <summary>
///     Placement class providing properties for different kinds of relative placements.
/// </summary>
public sealed class RelativePlacement : AdornerPlacement
{
    /// <summary>
    ///     Creates a new RelativePlacement instance. The default instance is a adorner with zero size, you
    ///     have to set some properties to define the placement.
    /// </summary>
    public RelativePlacement()
    {
    }

    /// <summary>
    ///     Creates a new RelativePlacement instance from the specified horizontal and vertical alignments.
    /// </summary>
    public RelativePlacement(HorizontalAlignment horizontal, VerticalAlignment vertical)
    {
        switch (horizontal)
        {
            case HorizontalAlignment.Left:
                WidthRelativeToDesiredWidth = 1;
                XRelativeToAdornerWidth = -1;
                break;
            case HorizontalAlignment.Right:
                WidthRelativeToDesiredWidth = 1;
                XRelativeToContentWidth = 1;
                break;
            case HorizontalAlignment.Center:
                WidthRelativeToDesiredWidth = 1;
                XRelativeToContentWidth = 0.5;
                XRelativeToAdornerWidth = -0.5;
                break;
            case HorizontalAlignment.Stretch:
                WidthRelativeToContentWidth = 1;
                break;
        }

        switch (vertical)
        {
            case VerticalAlignment.Top:
                HeightRelativeToDesiredHeight = 1;
                YRelativeToAdornerHeight = -1;
                break;
            case VerticalAlignment.Bottom:
                HeightRelativeToDesiredHeight = 1;
                YRelativeToContentHeight = 1;
                break;
            case VerticalAlignment.Center:
                HeightRelativeToDesiredHeight = 1;
                YRelativeToContentHeight = 0.5;
                YRelativeToAdornerHeight = -0.5;
                break;
            case VerticalAlignment.Stretch:
                HeightRelativeToContentHeight = 1;
                break;
        }
    }

    /// <summary>
    ///     Gets/Sets the width of the adorner as factor relative to the desired adorner width.
    /// </summary>
    public double WidthRelativeToDesiredWidth { get; set; }

    /// <summary>
    ///     Gets/Sets the height of the adorner as factor relative to the desired adorner height.
    /// </summary>
    public double HeightRelativeToDesiredHeight { get; set; }

    /// <summary>
    ///     Gets/Sets the width of the adorner as factor relative to the width of the adorned item.
    /// </summary>
    public double WidthRelativeToContentWidth { get; set; }

    /// <summary>
    ///     Gets/Sets the height of the adorner as factor relative to the height of the adorned item.
    /// </summary>
    public double HeightRelativeToContentHeight { get; set; }

    /// <summary>
    ///     Gets/Sets an offset that is added to the adorner width for the size calculation.
    /// </summary>
    public double WidthOffset { get; set; }

    /// <summary>
    ///     Gets/Sets an offset that is added to the adorner height for the size calculation.
    /// </summary>
    public double HeightOffset { get; set; }

    /// <summary>
    ///     Gets/Sets an offset that is added to the adorner position.
    /// </summary>
    public double XOffset { get; set; }

    /// <summary>
    ///     Gets/Sets an offset that is added to the adorner position.
    /// </summary>
    public double YOffset { get; set; }

    /// <summary>
    ///     Gets/Sets the left border of the adorner element as factor relative to the width of the adorner.
    /// </summary>
    public double XRelativeToAdornerWidth { get; set; }

    /// <summary>
    ///     Gets/Sets the top border of the adorner element as factor relative to the height of the adorner.
    /// </summary>
    public double YRelativeToAdornerHeight { get; set; }

    /// <summary>
    ///     Gets/Sets the left border of the adorner element as factor relative to the width of the adorned content.
    /// </summary>
    public double XRelativeToContentWidth { get; set; }

    /// <summary>
    ///     Gets/Sets the top border of the adorner element as factor relative to the height of the adorned content.
    /// </summary>
    public double YRelativeToContentHeight { get; set; }

    private Size CalculateSize(Control adorner, Size adornedElementSize)
    {
        return new Size(Math.Max(WidthOffset
                                 + WidthRelativeToDesiredWidth * adorner.DesiredSize.Width
                                 + WidthRelativeToContentWidth * adornedElementSize.Width, 0),
            Math.Max(HeightOffset
                     + HeightRelativeToDesiredHeight * adorner.DesiredSize.Height
                     + HeightRelativeToContentHeight * adornedElementSize.Height, 0));
    }

    private Point CalculatePosition(Size adornedElementSize, Size adornerSize)
    {
        return new Point(XOffset
                         + XRelativeToAdornerWidth * adornerSize.Width
                         + XRelativeToContentWidth * adornedElementSize.Width,
            YOffset
            + YRelativeToAdornerHeight * adornerSize.Height
            + YRelativeToContentHeight * adornedElementSize.Height);
    }

    /// <summary>
    ///     Arranges the adorner element on the specified adorner panel.
    /// </summary>
    public override void Arrange(AdornerPanel panel, Control adorner, Size adornedElementSize)
    {
        var adornerSize = CalculateSize(adorner, adornedElementSize);
        adorner.Arrange(new Rect(CalculatePosition(adornedElementSize, adornerSize), adornerSize));
    }
}