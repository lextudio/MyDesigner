using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MyDesigner.XamlDesigner.Tools;
using MyDesigner.XamlDesigner.ViewModels;

namespace MyDesigner.XamlDesigner.Views
{
    public partial class FromErrorListView : ListBox
    {
        public ErrorListViewModel ViewModel { get; }

        public FromErrorListView()
        {
            ViewModel = new ErrorListViewModel();
            DataContext = ViewModel;
            
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnPointerPressed(Avalonia.Input.PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            
            if (e.ClickCount == 2)
            {
                try
                {
                    if (SelectedItem is XamlError error)
                    {
                        Shell.Instance.JumpToError(error);
                    }
                }
                catch (Exception ex)
                {
                    Shell.ReportException(ex);
                }
            }
        }
    }
}