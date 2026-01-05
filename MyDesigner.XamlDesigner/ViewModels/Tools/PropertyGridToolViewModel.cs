using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MyDesigner.Design;
using MyDesigner.Design.Interfaces;

namespace MyDesigner.XamlDesigner.ViewModels.Tools
{
    public partial class PropertyGridToolViewModel : ObservableObject
    {
        [ObservableProperty]
        private ISelectionService? selectionService;

        private Document? _currentDocument;
        private ISelectionService? _previousSelectionService;

        public PropertyGridToolViewModel()
        {
            // Listen to Shell's CurrentDocument changes
            Shell.Instance.PropertyChanged += Shell_PropertyChanged;
            UpdateFromCurrentDocument();
        }

        private void Shell_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Shell.CurrentDocument))
            {
                UpdateFromCurrentDocument();
            }
        }

        private void UpdateFromCurrentDocument()
        {
            // Unsubscribe from old document and selection service
            if (_currentDocument != null)
            {
                _currentDocument.PropertyChanged -= Document_PropertyChanged;
            }
            
            if (_previousSelectionService != null)
            {
                _previousSelectionService.SelectionChanged -= SelectionService_SelectionChanged;
            }

            _currentDocument = Shell.Instance.CurrentDocument;
            SelectionService = _currentDocument?.SelectionService;
            _previousSelectionService = SelectionService;

            // Subscribe to new document changes
            if (_currentDocument != null)
            {
                _currentDocument.PropertyChanged += Document_PropertyChanged;
            }
            
            // Subscribe to selection changes
            if (SelectionService != null)
            {
                SelectionService.SelectionChanged += SelectionService_SelectionChanged;
            }
        }

        private void Document_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Document.SelectionService))
            {
                // Unsubscribe from old selection service
                if (_previousSelectionService != null)
                {
                    _previousSelectionService.SelectionChanged -= SelectionService_SelectionChanged;
                }
                
                SelectionService = _currentDocument?.SelectionService;
                _previousSelectionService = SelectionService;
                
                // Subscribe to new selection service
                if (SelectionService != null)
                {
                    SelectionService.SelectionChanged += SelectionService_SelectionChanged;
                }
            }
        }

        private void SelectionService_SelectionChanged(object? sender, DesignItemCollectionEventArgs e)
        {
            // Notify that selection has changed - this will trigger PropertyGrid update
            OnPropertyChanged(nameof(SelectionService));
        }
    }
}