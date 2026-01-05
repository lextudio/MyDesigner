using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MyDesigner.Designer;

namespace MyDesigner.XamlDesigner.ViewModels.Tools
{
    public partial class ThumbnailToolViewModel : ObservableObject
    {
        [ObservableProperty]
        private DesignSurface? designSurface;

        private Document? _currentDocument;

        public ThumbnailToolViewModel()
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
            DesignSurface = _currentDocument?.DesignSurface;

            // Subscribe to new document changes
            if (_currentDocument != null)
            {
                _currentDocument.PropertyChanged += Document_PropertyChanged;
            }
        }

        private void Document_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Document.DesignSurface))
            {
                DesignSurface = _currentDocument?.DesignSurface;
            }
        }
    }
}