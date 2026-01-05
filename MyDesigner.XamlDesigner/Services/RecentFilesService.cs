using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace MyDesigner.XamlDesigner.Services
{
    /// <summary>
    /// Service for managing recent files list
    /// </summary>
    public class RecentFilesService
    {
        private const int MaxRecentFiles = 10;
        private readonly ObservableCollection<string> _recentFiles;

        public RecentFilesService()
        {
            _recentFiles = new ObservableCollection<string>();
        }

        public ObservableCollection<string> RecentFiles => _recentFiles;

        public void AddFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                    return;

                var fullPath = Path.GetFullPath(filePath);

                // Remove if already exists
                if (_recentFiles.Contains(fullPath))
                {
                    _recentFiles.Remove(fullPath);
                }

                // Add to beginning
                _recentFiles.Insert(0, fullPath);

                // Limit to max files
                while (_recentFiles.Count > MaxRecentFiles)
                {
                    _recentFiles.RemoveAt(_recentFiles.Count - 1);
                }

                // Clean up non-existent files
                CleanupNonExistentFiles();
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        public void RemoveFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return;

                var fullPath = Path.GetFullPath(filePath);
                _recentFiles.Remove(fullPath);
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        public void Clear()
        {
            _recentFiles.Clear();
        }

        public void CleanupNonExistentFiles()
        {
            try
            {
                var filesToRemove = _recentFiles.Where(f => !File.Exists(f)).ToList();
                foreach (var file in filesToRemove)
                {
                    _recentFiles.Remove(file);
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        public void LoadFromList(IEnumerable<string> files)
        {
            try
            {
                _recentFiles.Clear();
                
                foreach (var file in files.Take(MaxRecentFiles))
                {
                    if (File.Exists(file))
                    {
                        _recentFiles.Add(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        public List<string> ToList()
        {
            return _recentFiles.ToList();
        }
    }
}