using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MyDesigner.Design.Interfaces;

namespace MyDesigner.XamlDesigner.ViewModels.Tools
{
    public partial class OutlineToolViewModel : ObservableObject
    {
        [ObservableProperty]
        private IOutlineNode? outlineRoot;

        private Document? _currentDocument;

        public OutlineToolViewModel()
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
            // Unsubscribe from old document
            if (_currentDocument != null)
            {
                _currentDocument.PropertyChanged -= Document_PropertyChanged;
            }

            _currentDocument = Shell.Instance.CurrentDocument;
            var newOutlineRoot = _currentDocument?.OutlineRoot;
            
            OutlineRoot = newOutlineRoot;

            // Subscribe to new document changes
            if (_currentDocument != null)
            {
                _currentDocument.PropertyChanged += Document_PropertyChanged;
            }
        }

        private void Document_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Document.OutlineRoot))
            {
                OutlineRoot = _currentDocument?.OutlineRoot;
            }
        }
    }
}