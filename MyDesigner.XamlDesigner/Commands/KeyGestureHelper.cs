using System;
using System.Collections.Generic;
using Avalonia.Input;

namespace MyDesigner.XamlDesigner.Commands
{
    /// <summary>
    /// Helper class for managing keyboard shortcuts in Avalonia
    /// </summary>
    public static class KeyGestureHelper
    {
        private static readonly Dictionary<string, KeyGesture> _keyGestures = new Dictionary<string, KeyGesture>();

        static KeyGestureHelper()
        {
            // Initialize standard keyboard shortcuts
            RegisterKeyGesture("New", new KeyGesture(Key.N, KeyModifiers.Control));
            RegisterKeyGesture("Open", new KeyGesture(Key.O, KeyModifiers.Control));
            RegisterKeyGesture("Save", new KeyGesture(Key.S, KeyModifiers.Control));
            RegisterKeyGesture("SaveAs", new KeyGesture(Key.S, KeyModifiers.Control | KeyModifiers.Shift));
            RegisterKeyGesture("SaveAll", new KeyGesture(Key.S, KeyModifiers.Control | KeyModifiers.Shift));
            RegisterKeyGesture("Close", new KeyGesture(Key.W, KeyModifiers.Control));
            RegisterKeyGesture("Exit", new KeyGesture(Key.F4, KeyModifiers.Alt));
            RegisterKeyGesture("Refresh", new KeyGesture(Key.F5));
            RegisterKeyGesture("Run", new KeyGesture(Key.F5, KeyModifiers.Shift));
            RegisterKeyGesture("Undo", new KeyGesture(Key.Z, KeyModifiers.Control));
            RegisterKeyGesture("Redo", new KeyGesture(Key.Y, KeyModifiers.Control));
            RegisterKeyGesture("Cut", new KeyGesture(Key.X, KeyModifiers.Control));
            RegisterKeyGesture("Copy", new KeyGesture(Key.C, KeyModifiers.Control));
            RegisterKeyGesture("Paste", new KeyGesture(Key.V, KeyModifiers.Control));
            RegisterKeyGesture("Delete", new KeyGesture(Key.Delete));
            RegisterKeyGesture("SelectAll", new KeyGesture(Key.A, KeyModifiers.Control));
            RegisterKeyGesture("Find", new KeyGesture(Key.F, KeyModifiers.Control));
        }

        public static void RegisterKeyGesture(string commandName, KeyGesture keyGesture)
        {
            _keyGestures[commandName] = keyGesture;
        }

        public static KeyGesture GetKeyGesture(string commandName)
        {
            return _keyGestures.TryGetValue(commandName, out var gesture) ? gesture : null;
        }

        public static string GetKeyGestureText(string commandName)
        {
            var gesture = GetKeyGesture(commandName);
            return gesture?.ToString() ?? string.Empty;
        }

        public static bool TryExecuteCommand(KeyEventArgs e, Shell shell)
        {
            var gesture = new KeyGesture(e.Key, e.KeyModifiers);
            
            foreach (var kvp in _keyGestures)
            {
                if (kvp.Value.Matches(e))
                {
                    return ExecuteCommand(kvp.Key, shell);
                }
            }
            
            return false;
        }

        private static bool ExecuteCommand(string commandName, Shell shell)
        {
            try
            {
                switch (commandName)
                {
                    case "New":
                        shell.NewCommand.Execute(null);
                        return true;
                    case "Open":
                        shell.OpenCommand.Execute(null);
                        return true;
                    case "Save":
                        shell.SaveCommand.Execute(null);
                        return true;
                    case "SaveAs":
                        shell.SaveAsCommand.Execute(null);
                        return true;
                    case "SaveAll":
                        shell.SaveAllCommand.Execute(null);
                        return true;
                    case "Close":
                        shell.CloseCommand.Execute(null);
                        return true;
                    case "Exit":
                        shell.ExitCommand.Execute(null);
                        return true;
                    case "Refresh":
                        shell.RefreshCommand.Execute(null);
                        return true;
                    case "Run":
                        shell.RunCommand.Execute(null);
                        return true;
                    case "Undo":
                        shell.UndoCommand.Execute(null);
                        return true;
                    case "Redo":
                        shell.RedoCommand.Execute(null);
                        return true;
                    case "Cut":
                        shell.CutCommand.Execute(null);
                        return true;
                    case "Copy":
                        shell.CopyCommand.Execute(null);
                        return true;
                    case "Paste":
                        shell.PasteCommand.Execute(null);
                        return true;
                    case "Delete":
                        shell.DeleteCommand.Execute(null);
                        return true;
                    case "SelectAll":
                        shell.SelectAllCommand.Execute(null);
                        return true;
                    case "Find":
                        shell.FindCommand.Execute(null);
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
                return false;
            }
        }
    }
}