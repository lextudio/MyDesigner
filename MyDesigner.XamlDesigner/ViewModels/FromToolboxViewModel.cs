using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyDesigner.Designer.Services;

namespace MyDesigner.XamlDesigner.ViewModels
{
    public partial class FromToolboxViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<AssemblyNodeViewModel> assemblyNodes;

        [ObservableProperty]
        private AssemblyNodeViewModel selectedAssembly;

        [ObservableProperty]
        private ControlNodeViewModel selectedControl;

        public FromToolboxViewModel()
        {
           
            
            AssemblyNodes = new ObservableCollection<AssemblyNodeViewModel>();
          
            // Initialize with default Avalonia assemblies
            LoadDefaultAssemblies();
            
        }

        private void LoadDefaultAssemblies()
        {
            try
            {
                AddAssembly(typeof(Avalonia.Controls.Button).Assembly);
                AddAssembly(typeof(Avalonia.Controls.Primitives.TemplatedControl).Assembly);
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        public void AddAssembly(string path)
        {
            try
            {
                var assembly = MyDesigner.XamlDesigner.Services.AssemblyService.LoadAssembly(path);
                if (assembly != null)
                {
                    AddAssembly(assembly);
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        public void AddAssembly(Assembly assembly)
        {
            try
            {
                // Check if assembly is already added
                if (AssemblyNodes.Any(n => n.Assembly.FullName == assembly.FullName))
                    return;

                var assemblyViewModel = new AssemblyNodeViewModel(assembly);
                
                if (assemblyViewModel.Controls.Any())
                {
                    AssemblyNodes.Add(assemblyViewModel);
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        public void RemoveAssembly(AssemblyNodeViewModel assembly)
        {
            try
            {
                AssemblyNodes.Remove(assembly);
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        [RelayCommand]
        private void BrowseForAssemblies()
        {
            // This command will trigger the event handler in the View
            // The actual file picker logic is in the View's code-behind
            // because it needs access to TopLevel
        }

        [RelayCommand]
        private void RemoveSelectedAssembly()
        {
            try
            {
                if (SelectedAssembly != null)
                {
                    RemoveAssembly(SelectedAssembly);
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        [RelayCommand]
        private void ActivateTool()
        {
            try
            {
                if (SelectedControl != null)
                {
                    PrepareTool(SelectedControl, true);
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        partial void OnSelectedControlChanged(ControlNodeViewModel value)
        {
            if (value != null)
            {
                PrepareTool(value, false);
            }
        }

        private void PrepareTool(ControlNodeViewModel controlNode, bool activate)
        {
            try
            {
                if (controlNode?.Type != null && Shell.Instance.CurrentDocument?.DesignContext != null)
                {
                    
                    
                    if (activate)
                    {
                         
                    }
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }
    }

    public partial class AssemblyNodeViewModel : ObservableObject
    {
        [ObservableProperty]
        private Assembly assembly;

        [ObservableProperty]
        private ObservableCollection<ControlNodeViewModel> controls;

        [ObservableProperty]
        private string path;

        public AssemblyNodeViewModel(Assembly assembly)
        {
            Assembly = assembly;
            Path = assembly.Location;
            Controls = new ObservableCollection<ControlNodeViewModel>();
            
            LoadControls();
        }

        public string Name => Assembly?.GetName().Name ?? "Unknown";

        private void LoadControls()
        {
            try
            {
                var types = MyDesigner.XamlDesigner.Services.AssemblyService.GetTypesFromAssembly(Assembly);
                
                foreach (var type in types)
                {
                    if (IsValidControlType(type))
                    {
                        Controls.Add(new ControlNodeViewModel(type));
                    }
                }

                // Sort controls by name
                var sortedControls = Controls.OrderBy(c => c.Name).ToList();
                Controls.Clear();
                foreach (var control in sortedControls)
                {
                    Controls.Add(control);
                }
            }
            catch (Exception ex)
            {
                Shell.ReportException(ex);
            }
        }

        private bool IsValidControlType(Type type)
        {
            try
            {
                return !type.IsAbstract && 
                       !type.IsGenericTypeDefinition && 
                       (typeof(Avalonia.Controls.Control).IsAssignableFrom(type) || 
                        typeof(Avalonia.Controls.Primitives.TemplatedControl).IsAssignableFrom(type)) &&
                       type.GetConstructor(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, Type.EmptyTypes, null) != null;
            }
            catch
            {
                return false;
            }
        }
    }

    public partial class ControlNodeViewModel : ObservableObject
    {
        [ObservableProperty]
        private Type type;

        public ControlNodeViewModel(Type type)
        {
            Type = type;
        }

        public string Name => Type?.Name ?? "Unknown";
        public string FullName => Type?.FullName ?? "Unknown";
    }
}