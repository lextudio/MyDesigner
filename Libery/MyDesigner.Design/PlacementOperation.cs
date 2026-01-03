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

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace MyDesigner.Design;

/// <summary>
///     Stores data about a placement operation.
/// </summary>
public sealed class PlacementOperation
{
    /// <summary>
    ///     Offset for inserted Components
    /// </summary>
    public const double PasteOffset = 10;

    private readonly ChangeGroup changeGroup;

    #region Delete Items

    /// <summary>
    ///     Deletes the items being placed, and commits the placement operation.
    /// </summary>
    public void DeleteItemsAndCommit()
    {
        if (IsAborted || IsCommitted)
            throw new PlacementOperationException("The operation is not running anymore.");
        if (!CurrentContainerBehavior.CanLeaveContainer(this))
            throw new PlacementOperationException("The items cannot be removed from their parent container.");

        CurrentContainerBehavior.LeaveContainer(this);
        Commit();
    }

    #endregion

    #region StartInsertNewComponents

    /// <summary>
    ///     Try to insert new components into the container.
    /// </summary>
    /// <param name="container">The container that should become the parent of the components.</param>
    /// <param name="placedItems">The components to add to the container.</param>
    /// <param name="positions">The rectangle specifying the position the element should get.</param>
    /// <param name="type">The type </param>
    /// <returns>The operation that inserts the new components, or null if inserting is not possible.</returns>
    public static PlacementOperation TryStartInsertNewComponents(DesignItem container, IList<DesignItem> placedItems,
        IList<Rect> positions, PlacementType type)
    {
        if (container == null)
            throw new ArgumentNullException("container");
        if (placedItems == null)
            throw new ArgumentNullException("placedItems");
        if (positions == null)
            throw new ArgumentNullException("positions");
        if (type == null)
            throw new ArgumentNullException("type");
        if (placedItems.Count == 0)
            throw new ArgumentException("placedItems.Count must be > 0");
        if (placedItems.Count != positions.Count)
            throw new ArgumentException("positions.Count must be = placedItems.Count");

        var items = placedItems.ToArray();

        var op = new PlacementOperation(items, type);
        try
        {
            for (var i = 0; i < items.Length; i++)
                op.PlacedItems[i].OriginalBounds = op.PlacedItems[i].Bounds = positions[i];
            op.CurrentContainer = container;
            op.CurrentContainerBehavior = container.GetBehavior<IPlacementBehavior>();
            if (op.CurrentContainerBehavior == null || !op.CurrentContainerBehavior.CanEnterContainer(op, true))
            {
                op.changeGroup.Abort();
                return null;
            }

            op.CurrentContainerBehavior.EnterContainer(op);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            op.changeGroup.Abort();
            throw;
        }

        return op;
    }

    #endregion

    /// <summary>
    ///     A exception wich can Happen during Placement
    /// </summary>
    public class PlacementOperationException : InvalidOperationException
    {
        /// <summary>
        ///     Constructor for Placement Exception
        /// </summary>
        /// <param name="message"></param>
        public PlacementOperationException(string message)
            : base(message)
        {
        }
    }

    #region Properties

    /// <summary>
    ///     The items being placed.
    /// </summary>
    public ReadOnlyCollection<PlacementInformation> PlacedItems { get; }

    /// <summary>
    ///     The type of the placement being done.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
    public PlacementType Type { get; }

    /// <summary>
    ///     Gets if the placement operation was aborted.
    /// </summary>
    public bool IsAborted { get; private set; }

    /// <summary>
    ///     Gets if the placement operation was committed.
    /// </summary>
    public bool IsCommitted { get; private set; }

    /// <summary>
    ///     Gets the current container for the placement operation.
    /// </summary>
    public DesignItem CurrentContainer { get; private set; }

    /// <summary>
    ///     Gets the placement behavior for the current container.
    /// </summary>
    public IPlacementBehavior CurrentContainerBehavior { get; private set; }

    #endregion

    #region Container changing

    /// <summary>
    ///     Make the placed item switch the container.
    ///     This method assumes that you already have checked if changing the container is possible.
    /// </summary>
    public void ChangeContainer(DesignItem newContainer)
    {
        if (newContainer == null)
            throw new ArgumentNullException("newContainer");
        if (IsAborted || IsCommitted)
            throw new PlacementOperationException("The operation is not running anymore.");
        if (CurrentContainer == newContainer)
            return;

        if (!CurrentContainerBehavior.CanLeaveContainer(this))
            throw new PlacementOperationException("The items cannot be removed from their parent container.");

        try
        {
            CurrentContainerBehavior.LeaveContainer(this);

            // Note: Avalonia uses different transform methods
            // This will need to be adapted based on the actual Avalonia transform system
            var transform = CurrentContainer.View.TransformToVisual(newContainer.View);

            foreach (var info in PlacedItems)
            {
                if (transform.HasValue)
                {
                    info.OriginalBounds = TransformRectByMiddlePoint(transform.Value, info.OriginalBounds);
                    info.Bounds = TransformRectByMiddlePoint(transform.Value, info.Bounds).Round();
                }
            }

            CurrentContainer = newContainer;
            CurrentContainerBehavior = newContainer.GetBehavior<IPlacementBehavior>();

            Debug.Assert(CurrentContainerBehavior != null);
            CurrentContainerBehavior.EnterContainer(this);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            Abort();
            throw;
        }
    }

    private static Rect TransformRectByMiddlePoint(Matrix transform, Rect r)
    {
        // we don't want to adjust the size of the control when moving it out of a scaled
        // container, we just want to move it correcly
        var p = new Point(r.Left + r.Width / 2, r.Top + r.Height / 2);
        var movement = transform.Transform(p) - p;
        return new Rect(r.TopLeft + movement, r.Size);
    }

    #endregion

    #region Start

    /// <summary>
    ///     Starts a new placement operation that changes the placement of <paramref name="placedItems" />.
    /// </summary>
    /// <param name="placedItems">The items to be placed.</param>
    /// <param name="type">The type of the placement.</param>
    /// <returns>A PlacementOperation object.</returns>
    /// <remarks>
    ///     You MUST call either <see cref="Abort" /> or <see cref="Commit" /> on the returned PlacementOperation
    ///     once you are done with it, otherwise a ChangeGroup will be left open and Undo/Redo will fail to work!
    /// </remarks>
    public static PlacementOperation Start(ICollection<DesignItem> placedItems, PlacementType type)
    {
        if (placedItems == null)
            throw new ArgumentNullException("placedItems");
        if (type == null)
            throw new ArgumentNullException("type");
        var items = placedItems.ToArray();
        if (items.Length == 0)
            throw new ArgumentException("placedItems.Length must be > 0");

        var op = new PlacementOperation(items, type);
        try
        {
            if (op.CurrentContainerBehavior == null)
                throw new PlacementOperationException("Starting the operation is not supported");

            op.CurrentContainerBehavior.BeginPlacement(op);
            foreach (var info in op.PlacedItems)
            {
                info.OriginalBounds = op.CurrentContainerBehavior.GetPosition(op, info.Item);
                info.Bounds = info.OriginalBounds;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            op.changeGroup.Abort();
            throw;
        }

        return op;
    }

    private PlacementOperation(DesignItem[] items, PlacementType type)
    {
        List<DesignItem> moveableItems;
        CurrentContainerBehavior = GetPlacementBehavior(items, out moveableItems, type);

        var information = new PlacementInformation[moveableItems.Count];
        for (var i = 0; i < information.Length; i++) information[i] = new PlacementInformation(moveableItems[i], this);
        PlacedItems = new ReadOnlyCollection<PlacementInformation>(information);
        Type = type;

        CurrentContainer = moveableItems[0].Parent;

        changeGroup = moveableItems[0].Context.OpenGroup(type.ToString(), moveableItems);
    }

    /// <summary>
    ///     The Size wich the Element really should have (even if its smaller Rendered (like emtpy Image!))
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static Size GetRealElementSize(Control element)
    {
        var size = element.Bounds.Size;
        if (!double.IsNaN(element.Width))
            size = size.WithWidth(element.Width);
        if (!double.IsNaN(element.Height))
            size = size.WithHeight(element.Height);

        if (size.Width < element.MinWidth)
            size = size.WithWidth(element.MinWidth);
        if (size.Height < element.MinHeight)
            size = size.WithHeight(element.MinHeight);

        return size;
    }

    /// <summary>
    ///     Gets the placement behavior associated with the specified items.
    /// </summary>
    public static IPlacementBehavior GetPlacementBehavior(ICollection<DesignItem> items)
    {
        List<DesignItem> moveableItems;
        return GetPlacementBehavior(items, out moveableItems, PlacementType.Move);
    }

    /// <summary>
    ///     Gets the placement behavior associated with the specified items.
    /// </summary>
    public static IPlacementBehavior GetPlacementBehavior(ICollection<DesignItem> items,
        out List<DesignItem> moveableItems, PlacementType placementType)
    {
        moveableItems = new List<DesignItem>();

        if (items == null)
            throw new ArgumentNullException("items");
        if (items.Count == 0)
            return null;

        var possibleItems = items;
        if (!items.Any(x => x.Parent == null))
        {
            var itemsPartentGroup = items.GroupBy(x => x.Parent);
            var parents = itemsPartentGroup.Select(x => x.Key).OrderBy(x => x.DepthLevel).First();
            possibleItems = itemsPartentGroup.Where(x => x.Key.DepthLevel == parents.DepthLevel).SelectMany(x => x)
                .ToList();
        }

        var first = possibleItems.First();
        var parent = first.Parent;
        moveableItems.Add(first);
        foreach (var item in possibleItems.Skip(1))
            if (item.Parent != parent)
            {
                if (placementType != PlacementType.MoveAndIgnoreOtherContainers) return null;
            }
            else
            {
                moveableItems.Add(item);
            }

        if (parent != null)
            return parent.GetBehavior<IPlacementBehavior>();
        if (possibleItems.Count == 1)
            return first.GetBehavior<IRootPlacementBehavior>();
        return null;
    }

    #endregion

    #region ChangeGroup handling

    /// <summary>
    ///     Gets/Sets the description of the underlying change group.
    /// </summary>
    public string Description
    {
        get => changeGroup.Title;
        set => changeGroup.Title = value;
    }

    /// <summary>
    ///     Aborts the operation.
    ///     This aborts the underlying change group, reverting all changes done while the operation was running.
    /// </summary>
    public void Abort()
    {
        if (!IsAborted)
        {
            if (IsCommitted)
                throw new PlacementOperationException("PlacementOperation is committed.");
            IsAborted = true;
            CurrentContainerBehavior.EndPlacement(this);
            changeGroup.Abort();
        }
    }

    /// <summary>
    ///     Commits the operation.
    ///     This commits the underlying change group.
    /// </summary>
    public void Commit()
    {
        if (IsAborted || IsCommitted)
            throw new PlacementOperationException("PlacementOperation is already aborted/committed.");
        IsCommitted = true;
        CurrentContainerBehavior.EndPlacement(this);
        changeGroup.Commit();
    }

    #endregion
}