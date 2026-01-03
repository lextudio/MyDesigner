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
using Avalonia.Input;
using Avalonia.Interactivity;

namespace MyDesigner.Design.UIExtensions;

/// <summary>
/// Simplified horizontal wheel support for Avalonia.
/// Note: This is a simplified version as Avalonia handles pointer events differently than WPF.
/// Full horizontal wheel support would require platform-specific implementations.
/// </summary>
public static class MouseHorizontalWheelEnabler
{
    /// <summary>
    ///     When true it will try to enable Horizontal Wheel support automatically.
    ///     Defaults to true.
    /// </summary>
    public static bool AutoEnableMouseHorizontalWheelSupport = true;

    private static readonly HashSet<Control> _HookedControls = new();

    /// <summary>
    ///     Enable Horizontal Wheel support for the control.
    ///     In Avalonia, this is simplified as the platform handles most pointer events natively.
    /// </summary>
    /// <param name="control">Control to enable support for.</param>
    public static void EnableMouseHorizontalWheelSupport(Control control)
    {
        if (control == null) throw new ArgumentNullException(nameof(control));

        if (_HookedControls.Contains(control)) return;

        _HookedControls.Add(control);
        
        // In Avalonia, we can listen to PointerWheelChanged events
        control.AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged, RoutingStrategies.Bubble);
    }

    /// <summary>
    ///     Disable Horizontal Wheel support for the control.
    /// </summary>
    /// <param name="control">Control to disable support for.</param>
    public static void DisableMouseHorizontalWheelSupport(Control control)
    {
        if (control == null) throw new ArgumentNullException(nameof(control));

        if (!_HookedControls.Contains(control)) return;

        control.RemoveHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged);
        _HookedControls.Remove(control);
    }

    private static void OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    {
        // Check if this is a horizontal wheel event
        if (Math.Abs(e.Delta.X) > Math.Abs(e.Delta.Y))
        {
            // This is primarily a horizontal scroll
            var element = sender as Control;
            if (element == null) return;

            var horizontalWheelArgs = new MouseHorizontalWheelEventArgs((int)(e.Delta.X * 120))
            {
                RoutedEvent = PreviewMouseHorizontalWheelEvent,
                Source = element
            };

            // Raise preview event first
            element.RaiseEvent(horizontalWheelArgs);
            if (horizontalWheelArgs.Handled)
            {
                e.Handled = true;
                return;
            }

            // Then raise the bubble event
            horizontalWheelArgs.RoutedEvent = MouseHorizontalWheelEvent;
            element.RaiseEvent(horizontalWheelArgs);
            
            if (horizontalWheelArgs.Handled)
            {
                e.Handled = true;
            }
        }
    }

    #region MouseWheelHorizontal Event

    public static readonly RoutedEvent<MouseHorizontalWheelEventArgs> MouseHorizontalWheelEvent =
        RoutedEvent.Register<Control, MouseHorizontalWheelEventArgs>(
            "MouseHorizontalWheel", RoutingStrategies.Bubble);

    public static void AddMouseHorizontalWheelHandler(Control control, EventHandler<MouseHorizontalWheelEventArgs> handler)
    {
        if (control != null)
        {
            control.AddHandler(MouseHorizontalWheelEvent, handler);

            if (AutoEnableMouseHorizontalWheelSupport) 
                EnableMouseHorizontalWheelSupport(control);
        }
    }

    public static void RemoveMouseHorizontalWheelHandler(Control control, EventHandler<MouseHorizontalWheelEventArgs> handler)
    {
        control?.RemoveHandler(MouseHorizontalWheelEvent, handler);
    }

    #endregion

    #region PreviewMouseWheelHorizontal Event

    public static readonly RoutedEvent<MouseHorizontalWheelEventArgs> PreviewMouseHorizontalWheelEvent =
        RoutedEvent.Register<Control, MouseHorizontalWheelEventArgs>(
            "PreviewMouseHorizontalWheel", RoutingStrategies.Tunnel);

    public static void AddPreviewMouseHorizontalWheelHandler(Control control, EventHandler<MouseHorizontalWheelEventArgs> handler)
    {
        if (control != null)
        {
            control.AddHandler(PreviewMouseHorizontalWheelEvent, handler);

            if (AutoEnableMouseHorizontalWheelSupport) 
                EnableMouseHorizontalWheelSupport(control);
        }
    }

    public static void RemovePreviewMouseHorizontalWheelHandler(Control control, EventHandler<MouseHorizontalWheelEventArgs> handler)
    {
        control?.RemoveHandler(PreviewMouseHorizontalWheelEvent, handler);
    }

    #endregion
}

