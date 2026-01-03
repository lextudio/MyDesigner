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

using System.Windows.Input;

namespace MyDesigner.Designer.Controls;

/// <summary>
///     A command that invokes a delegate.
///     The command parameter must be of type T.
/// </summary>
public class RelayCommand<T> : ICommand
{
    private readonly Predicate<T> canExecute;
    private readonly Action<T> execute;

    public RelayCommand(Action<T> execute)
    {
        if (execute == null)
            throw new ArgumentNullException("execute");
        this.execute = execute;
    }

    public RelayCommand(Action<T> execute, Predicate<T> canExecute)
    {
        if (execute == null)
            throw new ArgumentNullException("execute");
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged
    {
        add
        {
            if (canExecute != null)
            {
                // In Avalonia, we don't have CommandManager.RequerySuggested
                // So we'll use a simple event handler approach
                _canExecuteChanged += value;
            }
        }
        remove
        {
            if (canExecute != null)
            {
                _canExecuteChanged -= value;
            }
        }
    }
    
    private event EventHandler _canExecuteChanged;

    public bool CanExecute(object parameter)
    {
        if (parameter != null && !(parameter is T))
            return false;
        return canExecute == null ? true : canExecute((T)parameter);
    }

    public void Execute(object parameter)
    {
        execute((T)parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        _canExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
///     A command that invokes a delegate.
///     This class does not provide the command parameter to the delegate -
///     if you need that, use the generic version of this class instead.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Func<bool> canExecute;
    private readonly Action execute;

    public RelayCommand(Action execute)
    {
        if (execute == null)
            throw new ArgumentNullException("execute");
        this.execute = execute;
    }

    public RelayCommand(Action execute, Func<bool> canExecute)
    {
        if (execute == null)
            throw new ArgumentNullException("execute");
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged
    {
        add
        {
            if (canExecute != null)
            {
                // In Avalonia, we don't have CommandManager.RequerySuggested
                // So we'll use a simple event handler approach
                _canExecuteChanged2 += value;
            }
        }
        remove
        {
            if (canExecute != null)
            {
                _canExecuteChanged2 -= value;
            }
        }
    }
    
    private event EventHandler _canExecuteChanged2;

    public bool CanExecute(object parameter)
    {
        return canExecute == null ? true : canExecute();
    }

    public void Execute(object parameter)
    {
        execute();
    }

    public void RaiseCanExecuteChanged()
    {
        _canExecuteChanged2?.Invoke(this, EventArgs.Empty);
    }
}