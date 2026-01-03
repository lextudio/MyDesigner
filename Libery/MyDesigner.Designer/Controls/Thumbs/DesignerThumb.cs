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
using MyDesigner.Design.UIExtensions;

namespace MyDesigner.Designer.Controls;

/// <summary>
///     A thumb where the look can depend on the IsPrimarySelection property.
/// </summary>
public class DesignerThumb : Thumb
{
    protected override Type StyleKeyOverride => typeof(DesignerThumb);
    /// <summary>
    ///     Styled property for <see cref="IsPrimarySelection" />.
    /// </summary>
    public static readonly StyledProperty<bool> IsPrimarySelectionProperty
        = AvaloniaProperty.Register<DesignerThumb, bool>("IsPrimarySelection",false);

    /// <summary>
    ///     Styled property for <see cref="ThumbVisible" />.
    /// </summary>
    public static readonly StyledProperty<bool> ThumbVisibleProperty
        = AvaloniaProperty.Register<DesignerThumb, bool>("ThumbVisible", (bool)SharedInstances.BoxedTrue);

    /// <summary>
    ///     Styled property for <see cref="OperationMenu" />.
    /// </summary>
    public static readonly StyledProperty<Control[]> OperationMenuProperty =
        AvaloniaProperty.Register<DesignerThumb, Control[]>("OperationMenu");

    public PlacementAlignment Alignment;

    /// <summary>
    ///     Gets/Sets if the resize thumb is attached to the primary selection.
    /// </summary>
    public bool IsPrimarySelection
    {
        get => GetValue(IsPrimarySelectionProperty);
        set => SetValue(IsPrimarySelectionProperty, value);
    }

    /// <summary>
    ///     Gets/Sets if the resize thumb is visible.
    /// </summary>
    public bool ThumbVisible
    {
        get => GetValue(ThumbVisibleProperty);
        set => SetValue(ThumbVisibleProperty, value);
    }

    /// <summary>
    ///     Gets/Sets the OperationMenu.
    /// </summary>
    public Control[] OperationMenu
    {
        get => GetValue(OperationMenuProperty);
        set => SetValue(OperationMenuProperty, value);
    }

    public void ReDraw()
    {
        var parent = this.FindAncestorOfType<Control>();
        if (parent != null)
            parent.InvalidateArrange();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);    
    }
}