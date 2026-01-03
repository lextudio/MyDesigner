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
using Avalonia.Input;

namespace MyDesigner.Designer.Services;

/// <summary>
/// Base class for gestures that can be either a click or a drag operation.
/// </summary>
public abstract class ClickOrDragPointerGesture : PointerGestureBase
{
    protected Control positionRelativeTo;
    protected Point startPoint;
    protected bool hasDragStarted;

    protected override void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        base.OnPointerPressed(sender, e);
        startPoint = e.GetPosition(positionRelativeTo);
        hasDragStarted = false;
    }

    protected override void OnPointerMove(object sender, PointerEventArgs e)
    {
        base.OnPointerMove(sender, e);
        
        if (!hasDragStarted && IsPressed)
        {
            var currentPoint = e.GetPosition(positionRelativeTo);
            var delta = currentPoint - startPoint;
            
            // Start drag if moved more than threshold
            if (Math.Abs(delta.X) > 3 || Math.Abs(delta.Y) > 3)
            {
                hasDragStarted = true;
                OnDragStarted(e);
            }
        }
    }

    protected override void OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        OnPointerUp(sender, e);
        base.OnPointerReleased(sender, e);
    }

    protected virtual void OnDragStarted(PointerEventArgs e)
    {
        // Override in derived classes
    }

    protected virtual void OnPointerUp(object sender, PointerReleasedEventArgs e)
    {
        // Override in derived classes
    }
}