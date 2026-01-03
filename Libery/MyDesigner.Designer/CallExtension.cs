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

using System.Reflection;
using System.Windows.Input;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace MyDesigner.Designer;

public class CallExtension : MarkupExtension
{
    private readonly string methodName;

    public CallExtension(string methodName)
    {
        this.methodName = methodName;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var t = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        return new CallCommand(t?.TargetObject as Control, methodName);
    }
}

public class CallCommand : AvaloniaObject, ICommand
{
    public static readonly StyledProperty<bool> CanCallProperty =
        AvaloniaProperty.Register<CallCommand, bool>(nameof(CanCall), true);

    private readonly Control element;
    private readonly string methodName;
    private MethodInfo method;

    public CallCommand(Control element, string methodName)
    {
        this.element = element;
        this.methodName = methodName;
        
        if (element != null)
        {
            element.DataContextChanged += target_DataContextChanged;

            // Avalonia binding setup
            var binding = new Binding($"DataContext.Can{methodName}")
            {
                Source = element
            };
            this.Bind(CanCallProperty, binding);
        }

        GetMethod();
    }

    public bool CanCall
    {
        get => GetValue(CanCallProperty);
        set => SetValue(CanCallProperty, value);
    }

    public object DataContext => element?.DataContext;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CanCallProperty) 
            RaiseCanExecuteChanged();
    }

    private void GetMethod()
    {
        if (DataContext == null)
            method = null;
        else
            method = DataContext.GetType().GetMethod(methodName, Type.EmptyTypes);
    }

    private void target_DataContextChanged(object sender, EventArgs e)
    {
        GetMethod();
        RaiseCanExecuteChanged();
    }

    private void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    #region ICommand Members

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        return method != null && CanCall;
    }

    public void Execute(object parameter)
    {
        method?.Invoke(DataContext, null);
    }

    #endregion
}