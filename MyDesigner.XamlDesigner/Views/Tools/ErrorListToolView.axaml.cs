using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MyDesigner.XamlDesigner.ViewModels.Tools;
using System.ComponentModel;

namespace MyDesigner.XamlDesigner.Views.Tools
{
    public partial class ErrorListToolView : UserControl
    {
        private ErrorListToolViewModel? _viewModel;
        private ListBox? _errorListBox;

        public ErrorListToolView()
        {
            InitializeComponent();
            _viewModel = new ErrorListToolViewModel();
            DataContext = _viewModel;
            
            _errorListBox = this.FindControl<ListBox>("uxErrorListBox");
            if (_errorListBox != null)
            {
                // Listen to ViewModel changes to update ErrorList
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ErrorListToolViewModel.Errors) && _errorListBox != null)
            {
                // Update ListBox's ItemsSource when Errors changes
                _errorListBox.ItemsSource = _viewModel?.Errors;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}