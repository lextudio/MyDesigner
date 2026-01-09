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

using System.IO;
using System.Reflection;
using System.Resources;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace MyDesigner.Designer.Controls;

public class ZoomControl : ZoomScrollViewer
{

    protected override Type StyleKeyOverride => typeof(ZoomControl);


    public static readonly StyledProperty<object> AdditionalControlsProperty =
        AvaloniaProperty.Register<ZoomControl, object>(nameof(AdditionalControls));

    private static readonly Cursor PanToolCursor;
    private static readonly Cursor PanToolCursorMouseDown;
    private bool isPointerPressed;
    private bool pan;

    private double startHorizontalOffset;
    private Point startPoint;
    private double startVerticalOffset;

    static ZoomControl()
    {
        PanToolCursor = GetCursor("avares://MyDesigner.Designer/Images/PanToolCursor.cur");
        PanToolCursorMouseDown = GetCursor("avares://MyDesigner.Designer/Images/PanToolCursorMouseDown.cur");
    }

    public object AdditionalControls
    {
        get => GetValue(AdditionalControlsProperty);
        set => SetValue(AdditionalControlsProperty, value);
    }

    internal static Cursor GetCursor(string path)
    {
        try
        {
            
            var bitmap = new Bitmap(AssetLoader.Open(new Uri(path)));
            {
                if (bitmap != null)
                {
                    
                    return new Cursor(bitmap,PixelPoint .Origin);
                }
            }
        }
        catch
        {
            // Fallback to default cursor if resource not found
        }
        
        return new Cursor(StandardCursorType.Arrow);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!pan && e.Key == Key.Space)
        {
            pan = true;
            // In Avalonia, cursor updating is handled differently
            InvalidateVisual();
        }

        base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            pan = false;
            InvalidateVisual();
        }

        base.OnKeyUp(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(this).Properties;
        
        if (!pan && properties.IsMiddleButtonPressed)
        {
            pan = true;
            InvalidateVisual();
        }

        if (pan && !e.Handled)
        {
            try
            {
                e.Pointer.Capture(this);
                isPointerPressed = true;
                e.Handled = true;
                startPoint = e.GetPosition(this);
                PanStart();
                InvalidateVisual();
            }
            catch
            {
                // Capture failed, ignore
            }
        }

        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (isPointerPressed)
        {
            var endPoint = e.GetPosition(this);
            PanContinue(endPoint - startPoint);
        }

        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        var properties = e.GetCurrentPoint(this).Properties;
        
        if (pan && !properties.IsMiddleButtonPressed && !IsKeyDown(Key.Space))
        {
            pan = false;
            InvalidateVisual();
        }

        if (isPointerPressed)
        {
            isPointerPressed = false;
            e.Pointer.Capture(null);
            InvalidateVisual();
        }

        base.OnPointerReleased(e);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (isPointerPressed)
        {
            isPointerPressed = false;
            InvalidateVisual();
        }

        base.OnPointerCaptureLost(e);
    }

    private bool IsKeyDown(Key key)
    {
        // In Avalonia 11.x, keyboard state checking is handled differently
        // This is a simplified implementation - may need platform-specific handling
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.PlatformImpl != null)
            {
                // Avalonia 11.x doesn't expose GetKeyboardDevice() directly
                // Use alternative approach or return false for now
                return false;
            }
        }
        catch
        {
            // Fallback if platform implementation is not available
        }
        return false;
    }

    private void PanStart()
    {
        startHorizontalOffset = Offset.X;
        startVerticalOffset = Offset.Y;
    }

    private void PanContinue(Vector delta)
    {
        // In Avalonia 11.x, ZoomFactor might be named differently or accessed differently
        // Using a default zoom factor for now - this needs to be connected to the actual zoom control
        double zoomFactor = 1.0; // This should be retrieved from the parent zoom control
        
        var newOffset = new Vector(
            startHorizontalOffset - delta.X / zoomFactor,
            startVerticalOffset - delta.Y / zoomFactor
        );
        
        Offset = newOffset;
    }

    // Override cursor handling
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        UpdateCursor();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        Cursor = Cursor.Default;
    }

    private void UpdateCursor()
    {
        if (pan || isPointerPressed)
        {
            Cursor = isPointerPressed ? PanToolCursorMouseDown : PanToolCursor;
        }
        else
        {
            Cursor = Cursor.Default;
        }
    }
}