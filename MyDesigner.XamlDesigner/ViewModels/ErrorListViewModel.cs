using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MyDesigner.XamlDesigner.Tools;

namespace MyDesigner.XamlDesigner.ViewModels
{
    public partial class ErrorListViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<XamlError> errors;

        private Document currentDocument;

        public ErrorListViewModel()
        {
            Errors = new ObservableCollection<XamlError>();
            
            // Subscribe to Shell's CurrentDocument changes
            Shell.Instance.PropertyChanged += Shell_PropertyChanged;
            UpdateCurrentDocument(Shell.Instance.CurrentDocument);
        }

        private void Shell_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Shell.CurrentDocument))
            {
                UpdateCurrentDocument(Shell.Instance.CurrentDocument);
            }
        }

        private void UpdateCurrentDocument(Document newDocument)
        {
            // Unsubscribe from old document
            if (currentDocument != null)
            {
                currentDocument.PropertyChanged -= Document_PropertyChanged;
            }

            currentDocument = newDocument;

            // Subscribe to new document
            if (currentDocument != null)
            {
                currentDocument.PropertyChanged += Document_PropertyChanged;
                UpdateErrorsFromDocument(currentDocument);
            }
            else
            {
                Errors.Clear();
            }
        }

        private void Document_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Document.XamlErrorService))
            {
                UpdateErrorsFromDocument(currentDocument);
            }
        }

        private void UpdateErrorsFromDocument(Document document)
        {
            try
            {
                Errors.Clear();
                
                if (document?.XamlErrorService != null)
                {
                    foreach (var error in document.XamlErrorService.Errors)
                    {
                        // Convert from Designer XamlError to our XamlError
                        var localError = new XamlError
                        {
                            Line = error.Line,
                            Column = error.Column,
                            Message = error.Message
                        };
                        Errors.Add(localError);
                    }
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }
    }
}