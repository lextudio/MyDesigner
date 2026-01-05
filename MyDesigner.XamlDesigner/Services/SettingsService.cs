using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MyDesigner.XamlDesigner.Configuration;

namespace MyDesigner.XamlDesigner.Services
{
    /// <summary>
    /// Advanced settings management service
    /// </summary>
    public static class SettingsService
    {
        private static readonly Dictionary<string, object> RuntimeSettings = new Dictionary<string, object>();

        public static T GetSetting<T>(string key, T defaultValue = default(T))
        {
            try
            {
                if (RuntimeSettings.ContainsKey(key))
                {
                    var value = RuntimeSettings[key];
                    if (value is T typedValue)
                        return typedValue;
                    
                    // Try to convert
                    return (T)Convert.ChangeType(value, typeof(T));
                }

                return defaultValue;
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
                return defaultValue;
            }
        }

        public static void SetSetting<T>(string key, T value)
        {
            try
            {
                RuntimeSettings[key] = value;
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        public static void LoadFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var settings = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    
                    if (settings != null)
                    {
                        foreach (var kvp in settings)
                        {
                            RuntimeSettings[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        public static void SaveToFile(string filePath)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(RuntimeSettings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        public static void ResetToDefaults()
        {
            RuntimeSettings.Clear();
            
            // Set default values
            SetSetting("AutoSave", true);
            SetSetting("AutoSaveInterval", 300); // 5 minutes
            SetSetting("ShowGrid", true);
            SetSetting("SnapToGrid", true);
            SetSetting("GridSize", 10);
            SetSetting("ShowRulers", false);
            SetSetting("EnableUndo", true);
            SetSetting("UndoLevels", 50);
        }

        public static IEnumerable<KeyValuePair<string, object>> GetAllSettings()
        {
            return RuntimeSettings;
        }
    }
}