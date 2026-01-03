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

using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Input;

namespace MyDesigner.Designer.Services;

/// <summary>
///     Base class for classes handling mouse gestures on the design surface.
/// </summary>
public abstract class MouseGestureBase
{
    protected bool canAbortWithEscape = true;

    protected IDesignPanel designPanel;
    private bool isStarted;
    protected ServiceContainer services;

    /// <summary>
    ///     Checks if <paramref name="button" /> is the only button that is currently pressed.
    /// </summary>
    public static bool IsOnlyButtonPressed(PointerEventArgs e, PointerUpdateKind button)
    {
        var properties = e.GetCurrentPoint(null).Properties;
        return properties.IsLeftButtonPressed == (button == PointerUpdateKind.LeftButtonPressed)
               && properties.IsMiddleButtonPressed == (button == PointerUpdateKind.MiddleButtonPressed)
               && properties.IsRightButtonPressed == (button == PointerUpdateKind.RightButtonPressed);
    }

    public void Start(IDesignPanel designPanel, PointerPressedEventArgs e)
    {
        if (designPanel == null)
            throw new ArgumentNullException("designPanel");
        if (e == null)
            throw new ArgumentNullException("e");
        if (isStarted)
            throw new InvalidOperationException("Gesture already was started");

        isStarted = true;
        this.designPanel = designPanel;
        services = designPanel.Context.Services;
        e.Pointer.Capture(designPanel as Control);
        RegisterEvents();
        OnStarted(e);
    }

    private void RegisterEvents()
    {
        designPanel.PointerCaptureLost += OnPointerCaptureLost;
        designPanel.PointerPressed += OnPointerPressed;
        designPanel.PointerMoved += OnPointerMoved;
        designPanel.PointerReleased += OnPointerReleased;
        designPanel.KeyDown += OnKeyDown;
    }

    private void UnRegisterEvents()
    {
        designPanel.PointerCaptureLost -= OnPointerCaptureLost;
        designPanel.PointerPressed -= OnPointerPressed;
        designPanel.PointerMoved -= OnPointerMoved;
        designPanel.PointerReleased -= OnPointerReleased;
        designPanel.KeyDown -= OnKeyDown;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (canAbortWithEscape && e.Key == Key.Escape)
        {
            e.Handled = true;
            Stop();
        }
    }

    private void OnPointerCaptureLost(object sender, PointerCaptureLostEventArgs e)
    {
        Stop();
    }

    protected virtual void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (PointerHelper.IsDoubleClick(sender, e))
            OnPointerDoubleClick(sender, e);
    }

    protected virtual void OnPointerDoubleClick(object sender, PointerPressedEventArgs e)
    {
    }

    protected virtual void OnPointerMoved(object sender, PointerEventArgs e)
    {
    }

    protected virtual void OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        Stop();
    }

    protected void Stop()
    {
        if (!isStarted) return;
        isStarted = false;
        // In Avalonia, we release capture differently
        if (designPanel is Control control)
        {
            // Find the pointer that has capture and release it
            // This is a simplified approach - actual implementation may need more sophisticated logic
        }
        UnRegisterEvents();
        OnStopped();
    }

    protected virtual void OnStarted(PointerPressedEventArgs e)
    {
    }

    protected virtual void OnStopped()
    {
    }

    private static class PointerHelper
    {
        private const double k_MaxMoveDistance = 10;
        private const uint k_DoubleClickSpeed = 500; // Default double-click time in milliseconds

        private static long _LastClickTicks;
        private static Point _LastPosition;
        private static WeakReference _LastSender;

        internal static bool IsDoubleClick(object sender, PointerPressedEventArgs e)
        {
            var position = e.GetPosition(null);
            var clickTicks = DateTime.Now.Ticks;
            var elapsedTicks = clickTicks - _LastClickTicks;
            var elapsedTime = elapsedTicks / TimeSpan.TicksPerMillisecond;
            var quickClick = elapsedTime <= k_DoubleClickSpeed;
            var senderMatch = _LastSender != null && sender.Equals(_LastSender.Target);

            if (senderMatch && quickClick && Distance(position, _LastPosition) <= k_MaxMoveDistance)
            {
                // Double click!
                _LastClickTicks = 0;
                _LastSender = null;
                return true;
            }

            // Not a double click
            _LastClickTicks = clickTicks;
            _LastPosition = position;
            if (!quickClick)
                _LastSender = new WeakReference(sender);
            return false;
        }

        private static double Distance(Point pointA, Point pointB)
        {
            var x = pointA.X - pointB.X;
            var y = pointA.Y - pointB.Y;
            return Math.Sqrt(x * x + y * y);
        }
    }
}