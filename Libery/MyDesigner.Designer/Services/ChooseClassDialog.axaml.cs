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

using System.Collections.Specialized;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace MyDesigner.Designer.Services;

public partial class ChooseClassDialog : Window
{
 

    public ChooseClassDialog()
    {
        InitializeComponent();
    }

    public ChooseClassDialog(ChooseClass core) : this()
    {
        DataContext = core;
        
        uxFilter.Focus();
        uxList.DoubleTapped += uxList_DoubleTapped;
        uxOk.Click += delegate { Ok(); };
        uxCancel.Click += delegate { Close(); };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        uxFilter = this.FindControl<TextBox>("uxFilter");
        uxList = this.FindControl<ClassListBox>("uxList");
        uxOk = this.FindControl<Button>("uxOk");
        uxCancel = this.FindControl<Button>("uxCancel");
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Ok();
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            uxList.SelectedIndex = Math.Max(0, uxList.SelectedIndex - 1);
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            uxList.SelectedIndex++;
            e.Handled = true;
        }
        
        base.OnKeyDown(e);
    }

    private void uxList_DoubleTapped(object sender, TappedEventArgs e)
    {
        if (e.Source is Control control && control.DataContext is Type) 
            Ok();
    }

    private void Ok()
    {
        Close(true);
    }
}

internal class ClassListBox : ListBox
{
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if (change.Property == ItemsSourceProperty)
        {
            SelectedIndex = 0;
            ScrollIntoView(SelectedItem);
        }
    }
}

public class ClassNameConverter : IValueConverter
{
    public static ClassNameConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var c = value as Type;
        if (c == null) return value;
        return c.Name + " (" + c.Namespace + ")";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NullToBoolConverter : IValueConverter
{
    public static NullToBoolConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}