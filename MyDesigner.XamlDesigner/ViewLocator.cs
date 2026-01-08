using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.Core;
using MyDesigner.Designer.OutlineView;
using MyDesigner.Designer.PropertyGrid;
using MyDesigner.Designer.ThumbnailView;

using MyDesigner.XamlDesigner.ViewModels;
using MyDesigner.XamlDesigner.ViewModels.Tools;
using MyDesigner.XamlDesigner.Views;

using StaticViewLocator;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MyDesigner.XamlDesigner
{
    [StaticViewLocator]
    public partial class ViewLocator : IDataTemplate
    {
        public Control? Build(object? data)
        {
            if (data is null)
            {
                return null;
            }
            return data switch
            {
                ProjectExplorerDock vm => new ProjectExplorerView (),
                PropertyGridDock vm => new Views.Tools.PropertyGridToolView(),
                ToolboxDock vm => new FromToolboxView { DataContext = Toolbox.Instance },
                ErrorsToolDock vm => new Views.Tools.ErrorListToolView(),
                OutlineDock vm => new Views.Tools.OutlineToolView(),
                ThumbnailDock vm => new Views.Tools.ThumbnailToolView(),
                DocumentDock vm => new DocumentView(),
            
                _ => new TextBlock { Text = $"View not found for {data.GetType().Name}" }
            };
          
        }

        public bool Match(object? data)
        {
            if (data is null)
            {
                return false;
            }

            var type = data.GetType();
            return data is IDockable || s_views.ContainsKey(type);
        }
    }
}