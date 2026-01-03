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

using System.Diagnostics;
using Avalonia.Input;

namespace MyDesigner.Designer.Services;

/// <summary>
///     Mouse gesture for moving elements inside a container or between containers.
///     Belongs to the PointerTool.
/// </summary>
public sealed class DragMoveMouseGesture : ClickOrDragMouseGesture
{
    private readonly bool isDoubleClick;
    private readonly MoveLogic moveLogic;
    private readonly bool setSelectionIfNotMoving;

    public DragMoveMouseGesture(DesignItem clickedOn, bool isDoubleClick, bool setSelectionIfNotMoving = false)
    {
        Debug.Assert(clickedOn != null);

        this.isDoubleClick = isDoubleClick;
        this.setSelectionIfNotMoving = setSelectionIfNotMoving;
        positionRelativeTo = clickedOn.Services.DesignPanel;

        moveLogic = new MoveLogic(clickedOn);
    }

    protected override void OnDragStarted(PointerEventArgs e)
    {
        moveLogic.Start(startPoint);
    }

    protected override void OnPointerMoved(object sender, PointerEventArgs e)
    {
        base.OnPointerMoved(sender, e); // call OnDragStarted if min. drag distance is reached
        moveLogic.Move(e.GetPosition(positionRelativeTo as Visual));
    }

    protected override void OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (!hasDragStarted)
        {
            if (isDoubleClick)
            {
                // user made a double-click
                Debug.Assert(moveLogic.Operation == null);
                moveLogic.HandleDoubleClick();
            }
            else if (setSelectionIfNotMoving)
            {
                services.Selection.SetSelectedComponents(new[] { moveLogic.ClickedOn }, SelectionTypes.Auto);
            }
        }

        moveLogic.Stop();
        Stop();
    }

    protected override void OnStopped()
    {
        moveLogic.Cancel();
    }
}