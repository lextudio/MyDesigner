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
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS or IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Windows.Input;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace MyDesigner.Designer;

public static class ExtensionMethods
{
    public static double Coerce(this double value, double min, double max)
    {
        return Math.Max(Math.Min(value, max), min);
    }

    public static void AddRange<T>(this ICollection<T> col, IEnumerable<T> items)
    {
        foreach (var item in items) col.Add(item);
    }

    private static bool IsVisual(AvaloniaObject d)
    {
        return d is Visual;
    }

    /// <summary>
    ///     Gets all ancestors in the visual tree (including <paramref name="visual" /> itself).
    ///     Returns an empty list if <paramref name="visual" /> is null or not a visual.
    /// </summary>
    public static IEnumerable<Visual> GetVisualAncestors(this Visual visual)
    {
        if (visual != null)
            while (visual != null)
            {
                yield return visual;
                visual = visual.GetVisualParent();
            }
    }

    public static void AddCommandHandler(this Control element, ICommand command, Action execute)
    {
        AddCommandHandler(element, command, execute, null);
    }

    public static void AddCommandHandler(this Control element, ICommand command, Action execute,
        Func<bool> canExecute)
    {
        // In Avalonia, we simulate WPF's CommandBinding behavior
        // Since Avalonia doesn't have CommandBinding, we use a custom implementation
        
        // Create a command wrapper that handles the execution
        var commandWrapper = new AvaloniaCommandWrapper(command, execute, canExecute);
        
        // Store the wrapper in the element's DataContext or use attached properties
        // For now, we'll use a simple approach with event handlers
        
        // Handle key gestures if the command has them
        // Note: In Avalonia, we don't have RoutedCommand, so this is simplified
        element.KeyDown += (sender, e) =>
        {
            // This is a simplified implementation for key handling
            // You may need to implement specific key gesture handling based on your needs
            if (canExecute?.Invoke() != false)
            {
                execute();
                e.Handled = true;
            }
        };
        
        // For controls that support Command property directly
        if (element is Button button)
        {
            button.Command = commandWrapper;
        }
        else if (element is MenuItem menuItem)
        {
            menuItem.Command = commandWrapper;
        }
    }
    
    private class AvaloniaCommandWrapper : ICommand
    {
        private readonly ICommand _originalCommand;
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public AvaloniaCommandWrapper(ICommand originalCommand, Action execute, Func<bool> canExecute)
        {
            _originalCommand = originalCommand;
            _execute = execute;
            _canExecute = canExecute;
            
            if (_originalCommand != null)
                _originalCommand.CanExecuteChanged += (s, e) => CanExecuteChanged?.Invoke(s, e);
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? _originalCommand?.CanExecute(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _execute?.Invoke();
                _originalCommand?.Execute(parameter);
            }
        }
        
        public bool CanExecute()
        {
            return CanExecute(null);
        }
        
        public void Execute()
        {
            Execute(null);
        }
    }
}