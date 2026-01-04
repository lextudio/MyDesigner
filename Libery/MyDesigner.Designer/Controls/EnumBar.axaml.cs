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
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Diagnostics.CodeAnalysis;

namespace MyDesigner.Designer.Controls;

public partial class EnumBar : UserControl
{
    public static readonly StyledProperty<object> ValueProperty =
        AvaloniaProperty.Register<EnumBar, object>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<Panel> ContainerProperty =
        AvaloniaProperty.Register<EnumBar, Panel>(nameof(Container));

    public static readonly StyledProperty<ControlTheme> ButtonThemeProperty =
        AvaloniaProperty.Register<EnumBar, ControlTheme>(nameof(ButtonTheme));

    private Type currentEnumType;

   

    public EnumBar()
    {
        InitializeComponent();
        ValueProperty.Changed.AddClassHandler<EnumBar>((x, e) => x.OnValueChanged(e));
        ContainerProperty.Changed.AddClassHandler<EnumBar>((x, e) => x.OnContainerChanged(e));
        uxPanel = this.FindControl<StackPanel>("uxPanel");
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
    public object Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public Panel Container
    {
        get => GetValue(ContainerProperty);
        set => SetValue(ContainerProperty, value);
    }

    public ControlTheme ButtonTheme
    {
        get => GetValue(ButtonThemeProperty);
        set => SetValue(ButtonThemeProperty, value);
    }

  
   

    private void OnValueChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue != null)
        {
            var type = e.NewValue.GetType();

            if (currentEnumType != type)
            {
                currentEnumType = type;
                uxPanel.Children.Clear();
                foreach (var v in Enum.GetValues(type))
                {
                    var b = new EnumButton();
                    b.Value = v;
                    b.Content = Enum.GetName(type, v);
                    b.Bind(ButtonThemeProperty, new Binding("ButtonTheme") { Source = this });
                    b.Click += B_Click;
                    uxPanel.Children.Add(b);
                }
            }

            UpdateButtons();
            UpdateContainer();
        }
    }

    private void B_Click(object sender, RoutedEventArgs e)
    {
        Value = (sender as EnumButton).Value;
        e.Handled = true;
    }

    private void B_IsCheckedChanged(object sender, RoutedEventArgs e)
    {
       
    }

    private void OnContainerChanged(AvaloniaPropertyChangedEventArgs e)
    {
        UpdateContainer();
    }

    private void UpdateButtons()
    {
        foreach (EnumButton c in uxPanel.Children)
            if (c.Value.Equals(Value))
                c.IsChecked = true;
            else
                c.IsChecked = false;
    }

    private void UpdateContainer()
    {
        if (Container != null)
            for (var i = 0; i < uxPanel.Children.Count; i++)
            {
                var c = uxPanel.Children[i] as EnumButton;
                if (c.IsChecked == true)
                    Container.Children[i].IsVisible = true;
                else
                    Container.Children[i].IsVisible = false;
            }
    }

    private void button_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        Value = (sender as EnumButton).Value;
        e.Handled = true;
    }
}