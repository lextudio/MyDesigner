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
using Avalonia.Media;

namespace MyDesigner.Designer.Controls;

public delegate void DragHandler(DragListener drag);

public class DragListener
{
    private static DragListener CurrentListener;

    public DragListener(IInputElement target)
    {
        Target = target;

        Target.AddHandler(InputElement.PointerPressedEvent, Target_PointerPressed, handledEventsToo: true);
        Target.AddHandler(InputElement.PointerMovedEvent, Target_PointerMoved, handledEventsToo: true);
        Target.AddHandler(InputElement.PointerReleasedEvent, Target_PointerReleased, handledEventsToo: true);
        Target.AddHandler(InputElement.KeyDownEvent, Target_KeyDown, handledEventsToo: true);
    }

    public Transform Transform { get; set; }

    public IInputElement Target { get; }
    public Point StartPoint { get; private set; }
    public Point CurrentPoint { get; private set; }
    public Vector DeltaDelta { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDown { get; private set; }
    public bool IsCanceled { get; private set; }

    public Vector Delta
    {
        get
        {
            if (Transform != null)
            {
                var matrix = Transform.Value;
                if (matrix.HasInverse)
                {
                    matrix = matrix.Invert();
                    return matrix.Transform(CurrentPoint - StartPoint);
                }
            }

            return CurrentPoint - StartPoint;
        }
    }

    public void ExternalStart()
    {
        Target_PointerPressed(null, null);
    }

    public void ExternalPointerMove(PointerEventArgs e)
    {
        Target_PointerMoved(null, e);
    }

    public void ExternalStop()
    {
        Target_PointerReleased(null, null);
    }

    private void Target_KeyDown(object sender, KeyEventArgs e)
    {
        if (CurrentListener != null && e.Key == Key.Escape)
        {
            if (Target is Control control)
            {
                // Release pointer capture - in Avalonia, we need to use Pointer.Capture(null)
                // But we don't have access to the pointer here, so we'll skip this
                // The pointer capture will be released automatically when the gesture ends
            }
            CurrentListener.IsDown = false;
            CurrentListener.IsCanceled = true;
            CurrentListener.Complete();
        }
    }

    private void Target_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e?.GetCurrentPoint(null) != null)
        {
            StartPoint = e.GetCurrentPoint(null).Position;
        }
        else
        {
            StartPoint = new Point(0, 0); // Fallback for external calls
        }
        CurrentPoint = StartPoint;
        DeltaDelta = new Vector();
        IsDown = true;
        IsCanceled = false;
        if (MouseDown != null)
            MouseDown(this);
    }

    private void Target_PointerMoved(object sender, PointerEventArgs e)
    {
        if (IsDown)
        {
            var currentPoint = e.GetCurrentPoint(Target as Visual);
            if (currentPoint != null)
            {
                var newPoint = currentPoint.Position;
                DeltaDelta = newPoint - CurrentPoint;
                CurrentPoint = newPoint;
            }

            if (!IsActive)
            {
                // Avalonia doesn't have SystemParameters, use reasonable defaults
                const double MinDragDistance = 4.0;
                if (Math.Abs(Delta.X) >= MinDragDistance || Math.Abs(Delta.Y) >= MinDragDistance)
                {
                    IsActive = true;
                    CurrentListener = this;

                    if (Started != null) Started(this);
                }
            }

            if (IsActive && Changed != null) Changed(this);
        }
    }

    private void Target_PointerReleased(object sender, PointerReleasedEventArgs e)
    {
        IsDown = false;
        if (IsActive) Complete();
    }

    private void Complete()
    {
        IsActive = false;
        CurrentListener = null;

        if (Completed != null) Completed(this);
    }

    public event DragHandler MouseDown;
    public event DragHandler Started;
    public event DragHandler Changed;
    public event DragHandler Completed;
}