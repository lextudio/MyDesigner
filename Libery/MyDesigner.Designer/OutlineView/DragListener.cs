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

namespace MyDesigner.Designer.OutlineView;

public class DragListener
{
    private readonly Control target;
    private PointerPressedEventArgs args;
    private bool ready;
    private Point startPoint;

    public DragListener(Control target)
    {
        this.target = target;
        target.AddHandler(InputElement.PointerPressedEvent, PointerPressed, handledEventsToo: true);
        target.AddHandler(InputElement.PointerMovedEvent, PointerMoved, handledEventsToo: true);
        target.AddHandler(InputElement.PointerReleasedEvent, PointerReleased, handledEventsToo: true);
    }

    public event EventHandler<PointerPressedEventArgs> DragStarted;

    private void PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(target).Properties.IsLeftButtonPressed && e.Pointer.Captured == null)
        {
            ready = true;
            startPoint = e.GetPosition(target);
            args = e;
            e.Pointer.Capture(target);
        }
    }

    private void PointerMoved(object sender, PointerEventArgs e)
    {
        if (ready)
        {
            var currentPoint = e.GetPosition(target);
            const double MinDragDistance = 4.0; // Avalonia doesn't have SystemParameters
            if (Math.Abs(currentPoint.X - startPoint.X) >= MinDragDistance ||
                Math.Abs(currentPoint.Y - startPoint.Y) >= MinDragDistance)
            {
                ready = false;
                if (DragStarted != null) DragStarted(this, args);
            }
        }
    }

    private void PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        ready = false;
        e.Pointer.Capture(null);
    }
}