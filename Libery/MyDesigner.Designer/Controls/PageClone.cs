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
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using MyDesigner.Design.Extensions;

namespace MyDesigner.Designer.Controls;

/// <summary>
///     Description of PageClone.
/// </summary>
public class PageClone : TemplatedControl
{
    protected override Type StyleKeyOverride => typeof(PageClone);

    public static readonly StyledProperty<object> ContentProperty = 
        AvaloniaProperty.Register<PageClone, object>(nameof(Content));

    public static readonly StyledProperty<IBrush> BackgroundProperty =
        Panel.BackgroundProperty.AddOwner<PageClone>();

    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        TextBlock.FontFamilyProperty.AddOwner<PageClone>();

    public static readonly StyledProperty<double> FontSizeProperty =
        TextBlock.FontSizeProperty.AddOwner<PageClone>();

    public static readonly StyledProperty<IBrush> ForegroundProperty =
        TextBlock.ForegroundProperty.AddOwner<PageClone>();

    public static readonly StyledProperty<IControlTemplate> TemplateProperty =
        TemplatedControl.TemplateProperty.AddOwner<PageClone>();

    private readonly ContentPresenter content;

    static PageClone()
    {
        FocusableProperty.OverrideDefaultValue<PageClone>(false);
        // KeyboardNavigation properties are not available in Avalonia 11.x
        // These would need to be handled differently or omitted
        // KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue<PageClone>(KeyboardNavigationMode.Cycle);
        // KeyboardNavigation.DirectionalNavigationProperty.OverrideDefaultValue<PageClone>(KeyboardNavigationMode.Cycle);
        // KeyboardNavigation.ControlTabNavigationProperty.OverrideDefaultValue<PageClone>(KeyboardNavigationMode.Cycle);
    }

    public PageClone()
    {
        content = new ContentPresenter();
        content.Bind(ContentPresenter.ContentProperty, new Binding("Content") { Source = this });
        ((ISetLogicalParent)content).SetParent(this);
    }

    [Category("Appearance")]
    public IBrush Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    [Bindable(true)]
    [Category("Appearance")]
    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    [Bindable(true)]
    [Category("Appearance")]
    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    [Bindable(true)]
    [Category("Appearance")]
    public IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public bool KeepAlive { get; set; }

    public IControlTemplate Template
    {
        get => GetValue(TemplateProperty);
        set => SetValue(TemplateProperty, value);
    }

    public bool ShowsNavigationUI { get; set; }

    public string Title { get; set; }

    public string WindowTitle
    {
        get => Title;
        set => Title = value;
    }

    public double WindowWidth { get; set; }

    public double WindowHeight { get; set; }

    protected override Size ArrangeOverride(Size finalSize)
    {
        content.Arrange(new Rect(finalSize));
        return finalSize;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        content.Measure(availableSize);
        return content.DesiredSize;
    }
}

/// <summary>
///     A <see cref="CustomInstanceFactory" /> for <see cref="Page" />
///     (and derived classes, unless they specify their own <see cref="CustomInstanceFactory" />).
/// </summary>
[ExtensionFor(typeof(Page))]
public class PageCloneExtension : CustomInstanceFactory
{
    /// <summary>
    ///     Used to create instances of <see cref="PageClone" />.
    /// </summary>
    public override object CreateInstance(Type type, params object[] arguments)
    {
        Debug.Assert(arguments.Length == 0);
        return new PageClone();
    }
}