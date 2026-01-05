using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MyDesigner.XamlDesigner.Tools;

namespace MyDesigner.XamlDesigner.Views
{
    public partial class EnumBar : UserControl
    {
      
        
        public EnumBar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            uxPanel = this.FindControl<StackPanel>("uxPanel");
        }

      
        private Type currentEnumType;

        public static readonly StyledProperty<object> ValueProperty =
            AvaloniaProperty.Register<EnumBar, object>(nameof(Value), 
                defaultBindingMode: BindingMode.TwoWay);

        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ValueProperty && change.NewValue != null)
            {
                var type = change.NewValue.GetType();

                if (currentEnumType != type)
                {
                    currentEnumType = type;
                    uxPanel?.Children.Clear();
                    foreach (var v in Enum.GetValues(type))
                    {
                        var b = new EnumButton();
                        b.Value = v;
                        b.Content = Enum.GetName(type, v);
                        b.Click += Button_Click;
                        uxPanel?.Children.Add(b);
                    }
                }

                if (uxPanel != null)
                {
                    foreach (EnumButton c in uxPanel.Children)
                    {
                        if (c.Value.Equals(Value))
                        {
                            c.IsChecked = true;
                        }
                        else
                        {
                            c.IsChecked = false;
                        }
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Value = (sender as EnumButton).Value;
            e.Handled = true;
        }
    }
}