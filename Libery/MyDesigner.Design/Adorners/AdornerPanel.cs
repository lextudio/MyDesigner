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

using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;

namespace MyDesigner.Design.Adorners;

/// <summary>
///     Manages display of adorners on the design surface.
/// </summary>
public sealed class AdornerPanel : Panel
{
    /// <summary>
    ///     Gets the element adorned by this AdornerPanel.
    /// </summary>
    public Control AdornedElement { get; private set; }

    /// <summary>
    ///     Gets the design item adorned by this AdornerPanel.
    /// </summary>
    public DesignItem AdornedDesignItem { get; private set; }

    /// <summary>
    ///     Gets/Sets the order used to display the AdornerPanel relative to other AdornerPanels.
    ///     Do not change this property after the panel was added to an AdornerLayer!
    /// </summary>
    public AdornerOrder Order { get; set; } = AdornerOrder.Content;

    /// <summary>
    ///     Sets the AdornedElement and AdornedDesignItem properties.
    ///     This method can be called only once.
    /// </summary>
    public void SetAdornedElement(Control adornedElement, DesignItem adornedDesignItem)
    {
        if (adornedElement == null)
            throw new ArgumentNullException("adornedElement");

        if (AdornedElement == adornedElement &&
            AdornedDesignItem == adornedDesignItem) return; // ignore calls when nothing was changed

        if (AdornedElement != null)
            throw new InvalidOperationException("AdornedElement is already set.");

        AdornedElement = adornedElement;
        AdornedDesignItem = adornedDesignItem;
    }

    /// <summary />
    protected override Size MeasureOverride(Size availableSize)
    {
        if (AdornedElement != null)
        {
            foreach (var child in Children)
            {
                if (child is Control control)
                    control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }

            return PlacementOperation.GetRealElementSize(AdornedElement);
        }

        return base.MeasureOverride(availableSize);
    }

    /// <summary />
    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var element in Children)
        {
            if (element is Control control)
                GetPlacement(control).Arrange(this, control, finalSize);
        }
        return finalSize;
    }

    #region Attached Property Placement

    /// <summary>
    ///     The attached property used to store the placement of adorner visuals.
    /// </summary>
    public static readonly AttachedProperty<AdornerPlacement> PlacementProperty =
        AvaloniaProperty.RegisterAttached<AdornerPanel, Control, AdornerPlacement>(
            "Placement", AdornerPlacement.FillContent);

    /// <summary>
    ///     Gets the placement of the specified adorner.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
    public static AdornerPlacement GetPlacement(Control adorner)
    {
        if (adorner == null)
            throw new ArgumentNullException("adorner");
        return adorner.GetValue(PlacementProperty);
    }

    /// <summary>
    ///     Converts an absolute vector to a vector relative to the element adorned by this <see cref="AdornerPanel" />.
    /// </summary>
    public Vector AbsoluteToRelative(Vector absolute)
    {
        return new Vector(absolute.X / AdornedElement.Bounds.Width,
            absolute.Y / AdornedElement.Bounds.Height);
    }

    /// <summary>
    ///     Converts a vector relative to the element adorned by this <see cref="AdornerPanel" /> to an absolute vector.
    /// </summary>
    public Vector RelativeToAbsolute(Vector relative)
    {
        return new Vector(relative.X * AdornedElement.Bounds.Width,
            relative.Y * AdornedElement.Bounds.Height);
    }

    /// <summary>
    ///     Sets the placement of the specified adorner.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
    public static void SetPlacement(Control adorner, AdornerPlacement placement)
    {
        if (adorner == null)
            throw new ArgumentNullException("adorner");
        if (placement == null)
            throw new ArgumentNullException("placement");
        adorner.SetValue(PlacementProperty, placement);
    }

    #endregion
}