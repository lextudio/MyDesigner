using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MyDesigner.Designer.Services;

namespace MyDesigner.XamlDesigner.ViewModels.Tools
{
    public partial class ErrorListToolViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<XamlError>? errors;

        private Document? _currentDocument;

        public ErrorListToolViewModel()
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
            Errors = _currentDocument?.XamlErrorService?.Errors;

            // Subscribe to new document changes
            if (_currentDocument != null)
            {
                _currentDocument.PropertyChanged += Document_PropertyChanged;
            }
        }

        private void Document_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Document.XamlErrorService))
            {
                Errors = _currentDocument?.XamlErrorService?.Errors;
            }
        }
    }
}