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

namespace MyDesigner.Designer.Services;

internal class MoveLogic
{
    private ICollection<DesignItem> selectedItems;
    private Point startPoint;

    public MoveLogic(DesignItem clickedOn)
    {
        ClickedOn = clickedOn;

        selectedItems = clickedOn.Services.Selection.SelectedItems;
        if (!selectedItems.Contains(clickedOn))
            selectedItems = SharedInstances.EmptyDesignItemArray;
    }

    public DesignItem ClickedOn { get; }

    public PlacementOperation Operation { get; private set; }

    public IDesignPanel DesignPanel => ClickedOn.Services.DesignPanel;

    public void Start(Point p)
    {
        startPoint = p;
        var b = PlacementOperation.GetPlacementBehavior(selectedItems);
        if (b != null && b.CanPlace(selectedItems, PlacementType.Move, PlacementAlignment.TopLeft))
        {
            var sortedSelectedItems = new List<DesignItem>(selectedItems);
            sortedSelectedItems.Sort(ModelTools.ComparePositionInModelFile);
            selectedItems = sortedSelectedItems;
            Operation = PlacementOperation.Start(selectedItems, PlacementType.Move);
        }
    }

    public void Move(Point p)
    {
        if (Operation != null)
        {
            // try to switch the container
            if (Operation.CurrentContainerBehavior.CanLeaveContainer(Operation)) ChangeContainerIfPossible(p);

            Vector v;
            var designPanel = DesignPanel as Visual;
            if (Operation.CurrentContainer.View != null && designPanel != null)
                v = designPanel.TranslatePoint(p, Operation.CurrentContainer.View).Value
                    - designPanel.TranslatePoint(startPoint, Operation.CurrentContainer.View).Value;
            else
                v = p - startPoint;

            foreach (var info in Operation.PlacedItems)
                info.Bounds = new Rect(info.OriginalBounds.Left + Math.Round(v.X, PlacementInformation.BoundsPrecision),
                    info.OriginalBounds.Top + Math.Round(v.Y, PlacementInformation.BoundsPrecision),
                    info.OriginalBounds.Width,
                    info.OriginalBounds.Height);
            Operation.CurrentContainerBehavior.BeforeSetPosition(Operation);
            foreach (var info in Operation.PlacedItems) Operation.CurrentContainerBehavior.SetPosition(info);
        }
    }

    public void Stop()
    {
        if (Operation != null)
        {
            Operation.Commit();
            Operation = null;
        }
    }

    public void Cancel()
    {
        if (Operation != null)
        {
            Operation.Abort();
            Operation = null;
        }
    }

    // Perform hit testing on the design panel and return the first model that is not selected
    private DesignPanelHitTestResult HitTestUnselectedModel(Point p)
    {
        var result = DesignPanelHitTestResult.NoHit;
        var selection = ClickedOn.Services.Selection;

        DesignPanel.HitTest(p, false, true, delegate(DesignPanelHitTestResult r)
        {
            if (r.ModelHit == null)
                return true; // continue hit testing
            if (selection.IsComponentSelected(r.ModelHit))
                return true; // continue hit testing
            result = r;
            return false; // finish hit testing
        }, HitTestType.Default);

        return result;
    }

    private bool ChangeContainerIfPossible(Point p)
    {
        var result = HitTestUnselectedModel(p);
        if (result.ModelHit == null) return false;
        if (result.ModelHit == Operation.CurrentContainer) return false;

        // check that we don't move an item into itself:
        var tmp = result.ModelHit;
        while (tmp != null)
        {
            if (tmp == ClickedOn) return false;
            tmp = tmp.Parent;
        }

        var b = result.ModelHit.GetBehavior<IPlacementBehavior>();
        if (b != null && b.CanEnterContainer(Operation, false))
        {
            Operation.ChangeContainer(result.ModelHit);
            return true;
        }

        return false;
    }

    public void HandleDoubleClick()
    {
        if (selectedItems.Count == 1)
        {
            var ehs = ClickedOn.Services.GetService<IEventHandlerService>();
            if (ehs != null)
            {
                var defaultEvent = ehs.GetDefaultEvent(ClickedOn);
                if (defaultEvent != null) ehs.CreateEventHandler(defaultEvent);
            }
        }
    }
}