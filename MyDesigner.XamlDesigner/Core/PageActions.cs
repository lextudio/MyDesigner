using System;
using System.Threading.Tasks;

namespace MyDesigner.XamlDesigner.Core
{
    /// <summary>
    ///
    /// Direct functions to access different page functions
    /// </summary>
    public static class PageActions
    {
        #region ProjectExplorer Actions 

        /// <summary>
        /// OpenProjectFolder
        /// </summary>
        public static void OpenProjectFolder()
        {
            try
            {
                PageRegistry.ProjectExplorer?.OpenFolder();
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }



        /// <summary>
        /// OpenProject
        /// </summary>
        public static async Task OpenProject()
        {
            try
            {
                if (PageRegistry.ProjectExplorer != null)
                {
                    await PageRegistry.ProjectExplorer.OpenProject();
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        /// <summary>
        /// OpenFolderDialog
        /// </summary>
        public static async Task OpenFolderDialog()
        {
            try
            {
                if (PageRegistry.ProjectExplorer != null)
                {
                    await PageRegistry.ProjectExplorer.OpenFolderDialog();
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        /// <summary>
        /// OpenFile
        /// </summary>
        public static async Task OpenFile()
        {
            try
            {
                if (PageRegistry.ProjectExplorer != null)
                {
                   await PageRegistry.ProjectExplorer.OpenFile();
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }


        /// <summary>
        /// CloseAllDocuments
        /// </summary>
        public static void CloseAllDocuments()
        {
            try
            {
                PageRegistry.ProjectExplorer?.CloseAllDocuments();
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

      

        #endregion

        #region PropertyGrid Actions 

        /// <summary>
        /// RefreshPropertyGrid
        /// </summary>
        public static void RefreshPropertyGrid()
        {
            try
            {
              //  PageRegistry.PropertyGrid?.RefreshProperties();
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        /// <summary>
        /// ClearPropertyGrid
        /// </summary>
        public static void ClearPropertyGrid()
        {
            try
            {
              //  PageRegistry.PropertyGrid?.ClearProperties();
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        #endregion

        #region ErrorList Actions - 

        /// <summary>
        /// RefreshErrorList
        /// </summary>
        public static void RefreshErrorList()
        {
            try
            {
               // PageRegistry.ErrorList?.RefreshErrors();
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        /// <summary>
        /// ClearErrorList
        /// </summary>
        public static void ClearErrorList()
        {
            try
            {
               // PageRegistry.ErrorList?.ClearErrors();
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        /// <summary>
        /// GoToError
        /// </summary>
        public static void GoToError(int errorIndex)
        {
            try
            {
               // PageRegistry.ErrorList?.GoToError(errorIndex);
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        #endregion

        #region Toolbox Actions 

        /// <summary>
        /// RefreshToolbox
        /// </summary>
        public static void RefreshToolbox()
        {
            try
            {
               // PageRegistry.Toolbox?.RefreshToolbox();
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        /// <summary>
        /// AddAssemblyToToolbox
        /// </summary>
        public static void AddAssemblyToToolbox(string assemblyPath)
        {
            try
            {
                if (!string.IsNullOrEmpty(assemblyPath))
                {
                   // PageRegistry.Toolbox?.AddAssembly(assemblyPath);
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        #endregion

        #region Outline Actions 

        /// <summary>
        /// RefreshOutline
        /// </summary>
        public static void RefreshOutline()
        {
            try
            {
              //  PageRegistry.Outline?.RefreshOutline();
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        /// <summary>
        /// ExpandAllOutlineNodes
        /// </summary>
        public static void ExpandAllOutlineNodes()
        {
            try
            {
               // PageRegistry.Outline?.ExpandAll();
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        /// <summary>
        /// CollapseAllOutlineNodes
        /// </summary>
        public static void CollapseAllOutlineNodes()
        {
            try
            {
               // PageRegistry.Outline?.CollapseAll();
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        #endregion

        #region MainView Actions 

        /// <summary>
        /// RefreshCurrentDocument
        /// </summary>
        public static void RefreshCurrentDocument()
        {
            try
            {
              //  PageRegistry.MainView?.RefreshDocument();
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        /// <summary>
        /// ApplyDocumentChanges
        /// </summary>
        public static void ApplyDocumentChanges()
        {
            try
            {
               // PageRegistry.MainView?.ApplyChanges();
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        /// <summary>
        /// UndoDocumentChange
        /// </summary>
        public static void UndoDocumentChange()
        {
            try
            {
              //  PageRegistry.MainView?.Undo();
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        /// <summary>
        /// RedoDocumentChange
        /// </summary>
        public static void RedoDocumentChange()
        {
            try
            {
               // PageRegistry.MainView?.Redo();
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        #endregion

        #region Utility Actions  

        /// <summary>
        /// IsPageAvailable
        /// </summary>
        public static bool IsPageAvailable(string pageName)
        {
            return PageRegistry.IsPageAvailable(pageName);
        }

        /// <summary>
        /// SafeExecute
        /// </summary>
        public static void SafeExecute(Action action, string actionName = "Unknown")
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                Shell.ReportException(new Exception($"{actionName}: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// SafeExecuteAsync
        /// </summary>
        public static async Task SafeExecuteAsync(Func<Task> action, string actionName = "Unknown")
        {
            try
            {
                if (action != null)
                {
                    await action();
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(new Exception($" {actionName}: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// HandleOpenChoice
        /// </summary>
        public static async Task HandleOpenChoice(string choice)
        {
            try
            {
                switch (choice)
                {
                    case "project":
                        await OpenProject();
                        break;
                    case "folder":
                        await OpenFolderDialog();
                        break;
                    case "file":
                        await OpenFile();
                        break;
                    default:
                        Console.WriteLine($"خيار غير معروف: {choice}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        #endregion
    }
}