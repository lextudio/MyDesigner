using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MyDesigner.XamlDesigner.ViewModels;
using System;

namespace MyDesigner.XamlDesigner.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        
        // Ensure we have the correct DataContext
        if (DataContext == null && MainWindow.Instance?.ViewModel != null)
        {
            DataContext = MainWindow.Instance.ViewModel;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}