using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using System;

namespace MyDesigner.XamlDesigner.Tools
{
    public class SimpleCommand : IRelayCommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public SimpleCommand(string text, Action execute = null, Func<bool> canExecute = null)
        {
            Text = text;
            _execute = execute ?? (() => { });
            _canExecute = canExecute ?? (() => true);
        }

        public SimpleCommand(string text, KeyModifiers modifiers, Key key, Action execute = null, Func<bool> canExecute = null)
            : this(text, execute, canExecute)
        {
            KeyGesture = new KeyGesture(key, modifiers);
        }

        public string Text { get; }
        public KeyGesture KeyGesture { get; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}