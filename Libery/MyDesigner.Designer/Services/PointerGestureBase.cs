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

using Avalonia.Input;
using MyDesigner.Design.Interfaces;
using MyDesigner.Design.Services;

namespace MyDesigner.Designer.Services;

/// <summary>
/// Base class for pointer-based gestures.
/// </summary>
public abstract class PointerGestureBase
{
    protected IDesignPanel designPanel;
    protected DesignContext services;
    protected bool IsPressed { get; private set; }

    public virtual void Start(IDesignPanel designPanel, PointerPressedEventArgs e)
    {
        this.designPanel = designPanel;
        this.services = designPanel.Context;
        
        IsPressed = true;
        
        // Subscribe to events
        if (designPanel is Control control)
        {
            control.PointerMoved += OnPointerMove;
            control.PointerReleased += OnPointerReleased;
            control.PointerCaptureLost += OnPointerCaptureLost;
        }
        
        OnPointerPressed(designPanel, e);
    }

    public virtual void Stop()
    {
        IsPressed = false;
        
        // Unsubscribe from events
        if (designPanel is Control control)
        {
            control.PointerMoved -= OnPointerMove;
            control.PointerReleased -= OnPointerReleased;
            control.PointerCaptureLost -= OnPointerCaptureLost;
        }
        
        OnStopped();
    }

    protected virtual void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        // Override in derived classes
    }

    protected virtual void OnPointerMove(object sender, PointerEventArgs e)
    {
        // Override in derived classes
    }

    protected virtual void OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        Stop();
    }

    protected virtual void OnPointerCaptureLost(object sender, PointerCaptureLostEventArgs e)
    {
        Stop();
    }

    protected virtual void OnStopped()
    {
        // Override in derived classes
    }
}