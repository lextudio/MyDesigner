using Avalonia;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using MyDesigner.XamlDesigner.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyDesigner.XamlDesigner.View;

/// <summary>
/// Class for managing WPF/Avalonia/MAUI project execution
/// </summary>
public class ProjectRunner
{
    private Process _runningProcess;

    /// <summary>
    /// Run the current project
    /// </summary>
    public async void RunProject()
    {
        try
        {
            var projectPath = Settings.Default.ProjectPath;
            var projectType = Settings.Default.ProjectType;
            var projectName = Settings.Default.ProjectName;

            if (string.IsNullOrEmpty(projectPath) || string.IsNullOrEmpty(projectType))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("الرجاء فتح مشروع أولاً", "Warning", ButtonEnum.Ok, Icon.Warning);
                var result = await box.ShowAsync(); // Use await
                return;
            }

           
            StopProject();

            switch (projectType)
            {
                case "WPF":
                    RunWpfProject(projectPath, projectName);
                    break;
                case "Avalonia":
                    RunAvaloniaProject(projectPath, projectName);
                    break;
                case "Maui":
                    RunMauiProject(projectPath, projectName);
                    break;
                default:
                    var box = MessageBoxManager.GetMessageBoxStandard($"نوع المشروع '{projectType}' غير مدعوم", "Error", ButtonEnum.Ok, Icon.Error);
                    var result = await box.ShowAsync(); // Use await
                    break;
            }
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard($"خطأ في تشغيل المشروع:\n{ex.Message}", "Error", ButtonEnum.Ok, Icon.Error);
            var result = await box.ShowAsync(); // Use await
        }
    }

    /// <summary>
    /// Run WPF project
    /// </summary>
    private async void RunWpfProject(string projectPath, string projectName)
    {
        var csprojFile = FindCsprojFile(projectPath, projectName);
        if (csprojFile == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                $"لم يتم العثور على ملف .csproj\nCould not find .csproj file\n\nاسم المشروع: {projectName}\nالمسار: {projectPath}\n\nتأكد من تعيين المشروع الصحيح كمشروع رئيسي (Set as Startup Project)",
                "خطأ - Error", 
                ButtonEnum.Ok,
                Icon.Error);
            var result = await box.ShowAsync(); // Use await
            return;
        }
        
       
        var exePath = FindExecutable(projectPath, projectName, "exe");

        if (exePath != null && File.Exists(exePath))
        {
         
            RunExecutable(exePath);
        }
        else
        {
          
            BuildAndRunProject(csprojFile);
        }
    }

    /// <summary>
    /// Run Avalonia project
    /// </summary>
    private async void RunAvaloniaProject(string projectPath, string projectName)
    {
        var csprojFile = FindCsprojFile(projectPath, projectName);
        if (csprojFile == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                $"لم يتم العثور على ملف .csproj\nCould not find .csproj file\n\nاسم المشروع: {projectName}\nالمسار: {projectPath}",
                "خطأ - Error", 
                ButtonEnum.Ok,
                Icon.Error);
            var result = await box.ShowAsync(); // Use await
            return;
        }

      
        BuildAndRunProject(csprojFile);
    }

    /// <summary>
    /// Run MAUI project
    /// </summary>
    private async void RunMauiProject(string projectPath, string projectName)
    {
        var csprojFile = FindCsprojFile(projectPath, projectName);
        if (csprojFile == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                $"لم يتم العثور على ملف .csproj\nCould not find .csproj file\n\nاسم المشروع: {projectName}\nالمسار: {projectPath}",
                "خطأ - Error", 
                ButtonEnum.Ok,
                Icon.Error);
            var result = await box.ShowAsync(); // Use await
            return;
        }

      

        // MAUI يحتاج إلى تحديد framework
        BuildAndRunProject(csprojFile, "-f net8.0-windows10.0.19041.0");
    }

    /// <summary>
    /// Search for .csproj file
    /// </summary>
    private string FindCsprojFile(string projectPath, string projectName)
    {
      
        var csprojFiles = Directory.GetFiles(projectPath, "*.csproj", SearchOption.TopDirectoryOnly);

        if (csprojFiles.Length > 0)
        {
          
            var matchingFile = csprojFiles.FirstOrDefault(f =>
                Path.GetFileNameWithoutExtension(f).Equals(projectName, StringComparison.OrdinalIgnoreCase));

            if (matchingFile != null)
                return matchingFile;

            return csprojFiles[0];
        }

       
        var subfolderCsprojFiles = Directory.GetFiles(projectPath, "*.csproj", SearchOption.AllDirectories);

        if (subfolderCsprojFiles.Length > 0)
        {
            
            var matchingFile = subfolderCsprojFiles.FirstOrDefault(f =>
                Path.GetFileNameWithoutExtension(f).Equals(projectName, StringComparison.OrdinalIgnoreCase));

            if (matchingFile != null)
            {
                
                return matchingFile;
            }

          
            return subfolderCsprojFiles[0];
        }

        return null;
    }

    /// <summary>
    /// Search for executable file
    /// </summary>
    private string FindExecutable(string projectPath, string projectName, string extension)
    {
        var binPath = Path.Combine(projectPath, "bin");
        if (!Directory.Exists(binPath))
            return null;

        var exeFiles = Directory.GetFiles(binPath, $"{projectName}.{extension}", SearchOption.AllDirectories);

       
        return exeFiles.OrderByDescending(f => File.GetLastWriteTime(f)).FirstOrDefault();
    }

    /// <summary>
    /// Run executable file
    /// </summary>
    private async void RunExecutable(string exePath)
    {
        _runningProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(exePath)
            }
        };

        _runningProcess.Start();
        var box = MessageBoxManager.GetMessageBoxStandard($"تم تشغيل التطبيق:\n{Path.GetFileName(exePath)}", "نجح", ButtonEnum.Ok,Icon.Info);
        var result = await box.ShowAsync(); // Use await
    }

    /// <summary>
    /// Build and run project using dotnet
    /// </summary>
    private async void BuildAndRunProject(string csprojPath, string additionalArgs = "")
    {
        var projectDir = Path.GetDirectoryName(csprojPath);

        var box = MessageBoxManager.GetMessageBoxStandard("جاري بناء المشروع...", "Info", ButtonEnum.Ok, Icon.Info);
        var result = await box.ShowAsync(); // Use await
       
        await Task.Run(() =>
        {
           
            var buildProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"build \"{csprojPath}\" --configuration Release",
                    UseShellExecute = false,
                    WorkingDirectory = projectDir,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

          
            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            buildProcess.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    outputBuilder.AppendLine(e.Data);
            };

            buildProcess.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    errorBuilder.AppendLine(e.Data);
            };

            buildProcess.Start();

          
            buildProcess.BeginOutputReadLine();
            buildProcess.BeginErrorReadLine();

            buildProcess.WaitForExit();

            if (buildProcess.ExitCode != 0)
            {
                var error = errorBuilder.ToString();
               
                return;
            }

         
            var projectName = Path.GetFileNameWithoutExtension(csprojPath);
            var exePath = FindExecutable(projectDir, projectName, "exe");

            //Application.Current.Dispatcher.Invoke(() =>
            //{
                if (exePath != null && File.Exists(exePath))
                {
                   
                    RunExecutable(exePath);
                }
                else
                {
                 
                    _runningProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "dotnet",
                            Arguments = $"run --project \"{csprojPath}\" {additionalArgs} --no-build",
                            UseShellExecute = false,
                            WorkingDirectory = projectDir,
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        }
                    };

                    _runningProcess.Start();
                    var box = MessageBoxManager.GetMessageBoxStandard("تم تشغيل المشروع بنجاح", "Info", ButtonEnum.Ok,Icon.Info);
                }
            //});
        });
    }

    /// <summary>
    /// Stop running project
    /// </summary>
    public async void StopProject()
    {
        if (_runningProcess != null && !_runningProcess.HasExited)
        {
            try
            {
                _runningProcess.Kill();
                _runningProcess.Dispose();
                _runningProcess = null;
                var box = MessageBoxManager.GetMessageBoxStandard("StopProject", "Info", ButtonEnum.Ok,Icon.Info);
                var result = await box.ShowAsync(); // Use await
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard($"StopProject:\n{ex.Message}", "Error", ButtonEnum.Ok, Icon.Error);
                var result = await box.ShowAsync(); // Use await
            }
        }
    }

    /// <summary>
    /// Check if dotnet CLI exists
    /// </summary>
    public static bool IsDotnetInstalled()
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            process?.WaitForExit();
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
