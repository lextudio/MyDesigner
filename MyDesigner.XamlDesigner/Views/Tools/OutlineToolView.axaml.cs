using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MyDesigner.Designer.OutlineView;
using MyDesigner.XamlDesigner.ViewModels.Tools;
using System.ComponentModel;

namespace MyDesigner.XamlDesigner.Views.Tools
{
    public partial class OutlineToolView : UserControl
    {
        private OutlineToolViewModel? _viewModel;
        private Outline? _outline;

        public OutlineToolView()
        {
            InitializeComponent();
            _viewModel = new OutlineToolViewModel();
            DataContext = _viewModel;
            
            _outline = this.FindControl<Outline>("uxOutline");
            if (_outline != null)
            {
                // Set initial value
                _outline.Root = _viewModel?.OutlineRoot;
                
                // Listen to ViewModel changes to update Outline
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OutlineToolViewModel.OutlineRoot) && _outline != null)
            {
                // Update Outline's Root when OutlineRoot changes
                _outline.Root = _viewModel?.OutlineRoot;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}