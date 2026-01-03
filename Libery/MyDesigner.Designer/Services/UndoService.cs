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
using System.Diagnostics.CodeAnalysis;

namespace MyDesigner.Designer.Services;

#region ITransactionItem

internal interface ITransactionItem : IUndoAction
{
    void Do();
    void Undo();
    bool MergeWith(ITransactionItem other);
}

#endregion

#region IUndoAction

/// <summary>
///     Describes an action available on the undo or redo stack.
/// </summary>
public interface IUndoAction
{
    /// <summary>
    ///     The list of elements affected by the action.
    /// </summary>
    ICollection<DesignItem> AffectedElements { get; }

    /// <summary>
    ///     The title of the action.
    /// </summary>
    string Title { get; }
}

#endregion

#region UndoTransaction

/// <summary>
///     Supports ChangeGroup transactions and undo behavior.
/// </summary>
internal sealed class UndoTransaction : ChangeGroup, ITransactionItem
{
    public enum TransactionState
    {
        Open,
        Completed,
        Undone,
        Failed
    }

    private readonly List<ITransactionItem> items = new();

    internal UndoTransaction(ICollection<DesignItem> affectedElements)
    {
        AffectedElements = affectedElements;
    }

    public TransactionState State { get; private set; }

    public ICollection<DesignItem> AffectedElements { get; }

    public void Undo()
    {
        AssertState(TransactionState.Completed);
        State = TransactionState.Undone;
        InternalRollback();
    }

    void ITransactionItem.Do()
    {
        if (State != TransactionState.Completed) Redo();
    }

    bool ITransactionItem.MergeWith(ITransactionItem other)
    {
        return false;
    }

    public void Execute(ITransactionItem item)
    {
        AssertState(TransactionState.Open);
        item.Do();

        foreach (var existingItem in items)
            if (existingItem.MergeWith(item))
                return;

        items.Add(item);
    }

    private void AssertState(TransactionState expectedState)
    {
        if (State != expectedState)
            throw new InvalidOperationException("Expected state " + expectedState + ", but state is " + State);
    }

    public event EventHandler Committed;
    public event EventHandler RolledBack;

    public override void Commit()
    {
        AssertState(TransactionState.Open);
        State = TransactionState.Completed;
        if (Committed != null)
            Committed(this, EventArgs.Empty);
    }

    public override void Abort()
    {
        AssertState(TransactionState.Open);
        State = TransactionState.Failed;
        InternalRollback();
        if (RolledBack != null)
            RolledBack(this, EventArgs.Empty);
    }

    private void InternalRollback()
    {
        try
        {
            for (var i = items.Count - 1; i >= 0; i--) items[i].Undo();
        }
        catch
        {
            State = TransactionState.Failed;
            throw;
        }
    }

    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
        Justification = "We rethrow the original exception, not the follow-up error.")]
    public void Redo()
    {
        AssertState(TransactionState.Undone);
        try
        {
            for (var i = 0; i < items.Count; i++) items[i].Do();
            State = TransactionState.Completed;
        }
        catch
        {
            State = TransactionState.Failed;
            try
            {
                InternalRollback();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception rolling back after Redo error:\n" + ex);
            }

            throw;
        }
    }

    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
        Justification = "We avoid throwing exceptions here because a disposed transaction" +
                        " indicates another exception happened earlier.")]
    protected override void Dispose()
    {
        if (State == TransactionState.Open)
            try
            {
                Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception rolling back after failure:\n" + ex);
            }
    }
}

#endregion

#region UndoService

/// <summary>
///     Service supporting Undo/Redo actions on the design surface.
/// </summary>
public sealed class UndoService
{
    private readonly Stack<ITransactionItem> _redoStack = new();
    private readonly Stack<UndoTransaction> _transactionStack = new();
    private readonly Stack<ITransactionItem> _undoStack = new();

    /// <summary>
    ///     Gets if undo actions are available.
    /// </summary>
    public bool CanUndo => _undoStack.Count > 0;

    /// <summary>
    ///     Gets the list of names of the available actions on the undo stack.
    /// </summary>
    public IEnumerable<IUndoAction> UndoActions => GetActions(_undoStack);

    /// <summary>
    ///     Gets the list of names of the available actions on the undo stack.
    /// </summary>
    public IEnumerable<IUndoAction> RedoActions => GetActions(_redoStack);

    /// <summary>
    ///     Gets if there are redo actions available.
    /// </summary>
    public bool CanRedo => _redoStack.Count > 0;

    internal UndoTransaction StartTransaction(ICollection<DesignItem> affectedItems)
    {
        var t = new UndoTransaction(affectedItems);
        _transactionStack.Push(t);
        t.Committed += TransactionFinished;
        t.RolledBack += TransactionFinished;
        t.Committed += delegate(object sender, EventArgs e) { Execute((UndoTransaction)sender); };
        return t;
    }

    private void TransactionFinished(object sender, EventArgs e)
    {
        if (sender != _transactionStack.Pop())
            throw new Exception("Invalid transaction finish, nested transactions must finish first");
    }

    internal void Execute(ITransactionItem item)
    {
        if (_transactionStack.Count == 0)
        {
            item.Do();
            _undoStack.Push(item);
            _redoStack.Clear();
            OnUndoStackChanged(EventArgs.Empty);
        }
        else
        {
            _transactionStack.Peek().Execute(item);
        }
    }

    /// <summary>
    ///     Is raised when the undo stack has changed.
    /// </summary>
    public event EventHandler UndoStackChanged;

    private void OnUndoStackChanged(EventArgs e)
    {
        if (UndoStackChanged != null) UndoStackChanged(this, e);
    }

    /// <summary>
    ///     Undoes the last action.
    /// </summary>
    public void Undo()
    {
        if (!CanUndo)
            throw new InvalidOperationException("Cannot Undo: undo stack is empty");
        if (_transactionStack.Count != 0)
            throw new InvalidOperationException("Cannot Undo while transaction is running");
        var item = _undoStack.Pop();
        try
        {
            item.Undo();
            _redoStack.Push(item);
            OnUndoStackChanged(EventArgs.Empty);
        }
        catch
        {
            // state might be invalid now, clear stacks to prevent getting more inconsistencies
            Clear();
            throw;
        }
    }

    private static IEnumerable<IUndoAction> GetActions(Stack<ITransactionItem> stack)
    {
        foreach (var item in stack)
            yield return item;
    }

    /// <summary>
    ///     Redoes a previously undone action.
    /// </summary>
    public void Redo()
    {
        if (!CanRedo)
            throw new InvalidOperationException("Cannot Redo: redo stack is empty");
        if (_transactionStack.Count != 0)
            throw new InvalidOperationException("Cannot Redo while transaction is running");
        var item = _redoStack.Pop();
        try
        {
            item.Do();
            _undoStack.Push(item);
            OnUndoStackChanged(EventArgs.Empty);
        }
        catch
        {
            // state might be invalid now, clear stacks to prevent getting more inconsistencies
            Clear();
            throw;
        }
    }

    /// <summary>
    ///     Clears saved actions (both undo and redo stack).
    /// </summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        OnUndoStackChanged(EventArgs.Empty);
    }
}

#endregion