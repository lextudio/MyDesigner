using System;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using System.IO;
using MyDesigner.XamlDesigner.Views;

namespace MyDesigner.XamlDesigner.Services
{
    public class ProjectService
    {
        private static ProjectService? _instance;
        public static ProjectService Instance => _instance ??= new ProjectService();

     
        public event Action<string>? ProjectLoaded;
        public event Action<string>? ProjectOpened;
        public event Action? ProjectClosed;
        
     
        private ProjectExplorerView? _projectExplorerView;

        public void RegisterProjectExplorer(ProjectExplorerView projectExplorer)
        {
            _projectExplorerView = projectExplorer;
        }

        public async Task<bool> OpenProjectAsync(IStorageProvider storageProvider)
        {
            try
            {
                if (_projectExplorerView == null)
                {
                    throw new InvalidOperationException("ProjectExplorerView is not registered");
                }

            
                _projectExplorerView.OpenFolder();
                
         
                var currentPath = Configuration.Settings.Default.ProjectPath;
                if (!string.IsNullOrEmpty(currentPath))
                {
                    ProjectOpened?.Invoke(currentPath);
                }
                
                return true;
            }
            catch (Exception ex)
            {
           
                return false;
            }
        }

        public void LoadProject(string projectPath)
        {
            try
            {
                if (_projectExplorerView == null)
                {
                    throw new InvalidOperationException("ProjectExplorerView is not registered");
                }

                if (!Directory.Exists(projectPath))
                {
                   
                }

             
                ProjectLoaded?.Invoke(projectPath);
            }
            catch (Exception ex)
            {
              
                throw; 
            }
        }

        public void RefreshProject()
        {
            try
            {
                if (_projectExplorerView == null) return;

            
                var currentPath = Configuration.Settings.Default.ProjectPath;
                if (!string.IsNullOrEmpty(currentPath))
                {
                    LoadProject(currentPath);
                }
            }
            catch (Exception ex)
            {
               
                throw;
            }
        }

        public void CloseProject()
        {
            try
            {
                if (_projectExplorerView == null) return;

             
                _projectExplorerView.CloseAllDocuments();
                
              
                Configuration.Settings.Default.ProjectPath = string.Empty;
                Configuration.Settings.Default.ProjectName = string.Empty;
                Configuration.Settings.Default.Save();
                
              
                ProjectClosed?.Invoke();
            }
            catch (Exception ex)
            {
               
            }
        }

        public bool IsProjectLoaded => !string.IsNullOrEmpty(Configuration.Settings.Default.ProjectPath);
        
        public string? CurrentProjectPath => Configuration.Settings.Default.ProjectPath;
        
        public string? CurrentProjectName => Configuration.Settings.Default.ProjectName;

        public bool CanExecuteProjectCommands => IsProjectLoaded && _projectExplorerView != null;
    }
}