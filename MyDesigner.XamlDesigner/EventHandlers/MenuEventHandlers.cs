using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MyDesigner.XamlDesigner.EventHandlers
{
    /// <summary>
    /// Event handlers for menu items and other UI events
    /// </summary>
    public static class MenuEventHandlers
    {
        public static void RecentFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (e.Source is MenuItem menuItem && menuItem.Header is string path)
                {
                    Shell.Instance.Open(path);
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        public static void NewCommand_Executed()
        {
            Shell.Instance.New();
        }

        public static void OpenCommand_Executed()
        {
            Shell.Instance.Open();
        }

        public static void CloseCommand_Executed()
        {
            Shell.Instance.CloseCurrentDocument();
        }

        public static void CloseAllCommand_Executed()
        {
            Shell.Instance.CloseAll();
        }

        public static void SaveCommand_Executed()
        {
            Shell.Instance.SaveCurrentDocument();
        }

        public static void SaveAsCommand_Executed()
        {
            Shell.Instance.SaveCurrentDocumentAs();
        }

        public static void SaveAllCommand_Executed()
        {
            Shell.Instance.SaveAll();
        }

        public static void RunCommand_Executed()
        {
            Shell.Instance.Run();
        }

        public static void RenderToBitmapCommand_Executed()
        {
            Shell.Instance.RenderToBitmap();
        }

        public static void ExitCommand_Executed()
        {
            Shell.Instance.Exit();
        }

        public static bool CurrentDocument_CanExecute()
        {
            return Shell.Instance.CurrentDocument != null;
        }

        public static void RefreshCommand_Executed()
        {
            Shell.Instance.Refresh();
        }

        public static bool RefreshCommand_CanExecute()
        {
            return Shell.Instance.CanRefresh();
        }

        // Edit commands
        public static void UndoCommand_Executed()
        {
            Shell.Instance.Undo();
        }

        public static void RedoCommand_Executed()
        {
            Shell.Instance.Redo();
        }

        public static void CutCommand_Executed()
        {
            Shell.Instance.Cut();
        }

        public static void CopyCommand_Executed()
        {
            Shell.Instance.Copy();
        }

        public static void PasteCommand_Executed()
        {
            Shell.Instance.Paste();
        }

        public static void DeleteCommand_Executed()
        {
            Shell.Instance.Delete();
        }

        public static void SelectAllCommand_Executed()
        {
            Shell.Instance.SelectAll();
        }

        public static void FindCommand_Executed()
        {
            Shell.Instance.Find();
        }
    }
}