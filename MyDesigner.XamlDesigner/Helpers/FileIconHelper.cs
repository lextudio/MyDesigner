using Avalonia.Media;

using MyDesigner.XamlDesigner.Models;
using System;
using System.IO;
using static MyDesigner.XamlDesigner.ViewModels.ProjectExplorerViewViewModel;

namespace MyDesigner.XamlDesigner.Helpers;

 
public static class FileIconHelper
{
    
    private const string AssetBase = "avares://MyDesigner.XamlDesigner/Assets";
    
    public static string GetIconForFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return $"{AssetBase}/Reference.png";

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
           
            ".xaml" => $"{AssetBase}/xaml_icon.png",
            ".axaml" => $"{AssetBase}/xaml_icon.png",

          
            ".cs" => $"{AssetBase}/CS_16x.png",

          
            ".csproj" => $"{AssetBase}/Visual_Studio_Icon_2022.png",
            ".sln" => $"{AssetBase}/VS.png",

           
            ".json" => $"{AssetBase}/Reference.png",

           
            ".xml" => $"{AssetBase}/Reference.png",
            ".config" => $"{AssetBase}/Reference.png",

          
            ".txt" => $"{AssetBase}/Reference.png",
            ".md" => $"{AssetBase}/Reference.png",
            ".readme" => $"{AssetBase}/Reference.png",

           
            ".png" => $"{AssetBase}/Tag.png",
            ".jpg" => $"{AssetBase}/Tag.png",
            ".jpeg" => $"{AssetBase}/Tag.png",
            ".gif" => $"{AssetBase}/Tag.png",
            ".ico" => $"{AssetBase}/Tag.png",
            ".svg" => $"{AssetBase}/Tag.png",

            
            _ => $"{AssetBase}/Reference.png"
        };
    }

   
    public static IImage? GetVectorIconForFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return IconResourceHelper.ReferenceIcon;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".xaml" or ".axaml" => IconResourceHelper.XamlFileIcon,
            ".cs" => IconResourceHelper.CSharpFileIcon,
            ".csproj" => IconResourceHelper.ProjectIcon,
            ".sln" => IconResourceHelper.SolutionIcon,
            ".json" => IconResourceHelper.JsonFileIcon,
            ".xml" or ".config" => IconResourceHelper.XmlFileIcon,
            ".txt" or ".md" or ".readme" => IconResourceHelper.TextFileIcon,
            ".png" or ".jpg" or ".jpeg" or ".gif" or ".ico" or ".svg" => IconResourceHelper.ImageFileIcon,
            _ => IconResourceHelper.ReferenceIcon
        };
    }

    /// <summary>
    /// الحصول على أيقونة المجلد
    /// </summary>
    public static string GetFolderIcon(bool isExpanded = false)
    {
        return isExpanded ? $"{AssetBase}/Open.png" : $"{AssetBase}/Open.png";
    }

    /// <summary>
    /// الحصول على أيقونة المجلد المتجهية
    /// </summary>
    public static IImage? GetVectorFolderIcon(bool isExpanded = false)
    {
        return isExpanded ? IconResourceHelper.FolderOpenIcon : IconResourceHelper.FolderIcon;
    }

    /// <summary>
    /// الحصول على أيقونة المشروع
    /// </summary>
    public static string GetProjectIcon()
    {
        return $"{AssetBase}/Visual_Studio_Icon_2022.png";
    }

    /// <summary>
    /// الحصول على أيقونة المشروع المتجهية
    /// </summary>
    public static IImage? GetVectorProjectIcon()
    {
        return IconResourceHelper.ProjectIcon;
    }

    
    public static string GetDependenciesIcon()
    {
        return $"{AssetBase}/Reference.png";
    }

   
    public static IImage? GetVectorDependenciesIcon()
    {
        return IconResourceHelper.DependenciesIcon;
    }

   
    public static string GetFrameworkIcon()
    {
        return $"{AssetBase}/Reference.png";
    }

    
    public static IImage? GetVectorFrameworkIcon()
    {
        return IconResourceHelper.FrameworkIcon;
    }

    
    public static string GetPackageIcon()
    {
        return $"{AssetBase}/Reference.png";
    }

    /// <summary>
    /// الحصول على أيقونة Packages المتجهية
    /// </summary>
    public static IImage? GetVectorPackageIcon()
    {
        return IconResourceHelper.PackageIcon;
    }

    /// <summary>
    /// الحصول على أيقونة Assemblies
    /// </summary>
    public static string GetAssemblyIcon()
    {
        return $"{AssetBase}/Reference.png";
    }

    /// <summary>
    /// الحصول على أيقونة Assemblies المتجهية
    /// </summary>
    public static IImage? GetVectorAssemblyIcon()
    {
        return IconResourceHelper.AssemblyIcon;
    }

    /// <summary>
    /// الحصول على أيقونة Projects (مشاريع مرجعية)
    /// </summary>
    public static string GetProjectReferenceIcon()
    {
        return $"{AssetBase}/Visual_Studio_Icon_2022.png";
    }

    /// <summary>
    /// الحصول على أيقونة Projects المتجهية
    /// </summary>
    public static IImage? GetVectorProjectReferenceIcon()
    {
        return IconResourceHelper.ProjectIcon;
    }

    /// <summary>
    /// الحصول على أيقونة Analyzers المتجهية
    /// </summary>
    public static IImage? GetVectorAnalyzerIcon()
    {
        return IconResourceHelper.AnalyzerIcon;
    }

    /// <summary>
    /// الحصول على أيقونة Properties المتجهية
    /// </summary>
    public static IImage? GetVectorPropertiesIcon()
    {
        return IconResourceHelper.PropertiesIcon;
    }
}