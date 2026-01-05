using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MyDesigner.Designer.PropertyGrid;
using MyDesigner.XamlDesigner.ViewModels.Tools;
using System.ComponentModel;

namespace MyDesigner.XamlDesigner.Views.Tools
{
    public partial class PropertyGridToolView : UserControl
    {
        private PropertyGridToolViewModel? _viewModel;
        private PropertyGridView? _propertyGridView;

        public PropertyGridToolView()
        {
            InitializeComponent();
            _viewModel = new PropertyGridToolViewModel();
            DataContext = _viewModel;
            
            // Set up Shell reference
            _propertyGridView = this.FindControl<PropertyGridView>("uxPropertyGridView");
            if (_propertyGridView != null)
            {
                Shell.Instance.PropertyGridView = _propertyGridView;
                Shell.Instance.PropertyGrid = _propertyGridView.PropertyGrid;
                
                // Listen to ViewModel changes to update PropertyGrid
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PropertyGridToolViewModel.SelectionService) && _propertyGridView != null)
            {
                // Update PropertyGrid's SelectedItems when SelectionService changes
                if (_viewModel?.SelectionService != null)
                {
                    _propertyGridView.SelectedItems = _viewModel.SelectionService.SelectedItems;
                }
                else
                {
                    _propertyGridView.SelectedItems = null;
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}