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
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using MyDesigner.Design.Extensions;

namespace MyDesigner.Designer.Controls;

/// <summary>
///     A custom control that imitates the properties of <see cref="Window" />, but is not a top-level control.
/// </summary>
public class WindowClone : ContentControl
{
    protected override Type StyleKeyOverride => typeof(WindowClone);

    public static readonly StyledProperty<bool> AllowsTransparencyProperty =
        AvaloniaProperty.Register<WindowClone, bool>(nameof(AllowsTransparency));

    public static readonly StyledProperty<IImage> IconProperty =
        AvaloniaProperty.Register<WindowClone, IImage>(nameof(Icon));

    public static readonly StyledProperty<double> LeftProperty =
        AvaloniaProperty.Register<WindowClone, double>(nameof(Left));

    public static readonly StyledProperty<bool> ShowActivatedProperty =
        AvaloniaProperty.Register<WindowClone, bool>(nameof(ShowActivated));

    public static readonly StyledProperty<bool> ShowInTaskbarProperty =
        AvaloniaProperty.Register<WindowClone, bool>(nameof(ShowInTaskbar));

    public static readonly StyledProperty<SizeToContent> SizeToContentProperty =
        AvaloniaProperty.Register<WindowClone, SizeToContent>(nameof(SizeToContent));

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<WindowClone, string>(nameof(Title));

    public static readonly StyledProperty<double> TopProperty =
        AvaloniaProperty.Register<WindowClone, double>(nameof(Top));

    public static readonly StyledProperty<bool> TopmostProperty =
        AvaloniaProperty.Register<WindowClone, bool>(nameof(Topmost));

    public static readonly StyledProperty<WindowState> WindowStateProperty =
        AvaloniaProperty.Register<WindowClone, WindowState>(nameof(WindowState));

    static WindowClone()
    {
        FocusableProperty.OverrideDefaultValue<WindowClone>(false);
        // KeyboardNavigation properties are not available in Avalonia 11.x
        // These would need to be handled differently or omitted
        // KeyboardNavigation.DirectionalNavigationProperty.OverrideDefaultValue<WindowClone>(KeyboardNavigationMode.Cycle);
        // KeyboardNavigation.TabNavigationProperty.OverrideDefaultValue<WindowClone>(KeyboardNavigationMode.Cycle);
        // KeyboardNavigation.ControlTabNavigationProperty.OverrideDefaultValue<WindowClone>(KeyboardNavigationMode.Cycle);
    }

    /// <summary>
    ///     This property has no effect. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    public bool AllowsTransparency
    {
        get => GetValue(AllowsTransparencyProperty);
        set => SetValue(AllowsTransparencyProperty, value);
    }

    /// <summary>
    ///     This property has no effect. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
    public bool? DialogResult
    {
        get => null;
        set { }
    }

    public IImage Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    ///     This property has no effect. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    public double Left
    {
        get => GetValue(LeftProperty);
        set => SetValue(LeftProperty, value);
    }

    /// <summary>
    ///     This property has no effect. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    public Window Owner { get; set; }

    /// <summary>
    ///     Gets or sets the resize mode.
    /// </summary>
    public ResizeMode ResizeMode { get; set; }

    /// <summary>
    ///     This property has no effect. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    public bool ShowActivated
    {
        get => GetValue(ShowActivatedProperty);
        set => SetValue(ShowActivatedProperty, value);
    }

    /// <summary>
    ///     This property has no effect. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    public bool ShowInTaskbar
    {
        get => GetValue(ShowInTaskbarProperty);
        set => SetValue(ShowInTaskbarProperty, value);
    }

    /// <summary>
    ///     Gets or sets a value that specifies whether a window will automatically size itself to fit the size of its content.
    /// </summary>
    public SizeToContent SizeToContent
    {
        get => GetValue(SizeToContentProperty);
        set => SetValue(SizeToContentProperty, value);
    }

    /// <summary>
    ///     The title to display in the Window's title bar.
    /// </summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    ///     This property has no effect. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    public double Top
    {
        get => GetValue(TopProperty);
        set => SetValue(TopProperty, value);
    }

    /// <summary>
    ///     This property has no effect. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    public bool Topmost
    {
        get => GetValue(TopmostProperty);
        set => SetValue(TopmostProperty, value);
    }

    /// <summary>
    ///     This property has no effect. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    public WindowStartupLocation WindowStartupLocation { get; set; }

    /// <summary>
    ///     This property has no effect. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    public WindowState WindowState
    {
        get => GetValue(WindowStateProperty);
        set => SetValue(WindowStateProperty, value);
    }

    /// <summary>
    ///     This property has no effect. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    public WindowStyle WindowStyle { get; set; }

    /// <summary>
    ///     This event is never raised. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
    public event EventHandler Activated
    {
        add { }
        remove { }
    }

    /// <summary>
    ///     This event is never raised. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
    public event EventHandler Closed
    {
        add { }
        remove { }
    }

    /// <summary>
    ///     This event is never raised. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
    public event EventHandler Closing
    {
        add { }
        remove { }
    }

    /// <summary>
    ///     This event is never raised. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
    public event EventHandler ContentRendered
    {
        add { }
        remove { }
    }

    /// <summary>
    ///     This event is never raised. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
    public event EventHandler Deactivated
    {
        add { }
        remove { }
    }

    /// <summary>
    ///     This event is never raised. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
    public event EventHandler LocationChanged
    {
        add { }
        remove { }
    }

    /// <summary>
    ///     This event is never raised. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
    public event EventHandler SourceInitialized
    {
        add { }
        remove { }
    }

    /// <summary>
    ///     This event is never raised. (for compatibility with <see cref="Window" /> only).
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
    public event EventHandler StateChanged
    {
        add { }
        remove { }
    }
}

/// <summary>
///     A <see cref="CustomInstanceFactory" /> for <see cref="Window" />
///     (and derived classes, unless they specify their own <see cref="CustomInstanceFactory" />).
/// </summary>
[ExtensionFor(typeof(Window))]
public class WindowCloneExtension : CustomInstanceFactory
{
    /// <summary>
    ///     Used to create instances of <see cref="WindowClone" />.
    /// </summary>
    public override object CreateInstance(Type type, params object[] arguments)
    {
        Debug.Assert(arguments.Length == 0);
        return new WindowClone();
    }
}