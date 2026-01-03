using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace MyDesigner.Designer.PropertyGrid.Editors;

public class CollectionTemplateSelector : IDataTemplate
{
    public Control Build(object data)
    {
        if (data is DesignItem di)
        {
            if (di.Component is Point)
                return CreatePointTemplate(di);
            
            if (di.Component is string)
                return CreateStringTemplate(di);
                
            return CreateDefaultTemplate(di);
        }

        return new TextBlock { Text = data?.ToString() ?? "" };
    }

    public bool Match(object data)
    {
        return data is DesignItem;
    }

    private Control CreatePointTemplate(DesignItem di)
    {
        var panel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
        panel.Children.Add(new TextBlock { Text = "Point (" });
        
        var xBlock = new TextBlock();
        xBlock.Bind(TextBlock.TextProperty, new Avalonia.Data.Binding("Component.X") { Source = di });
        panel.Children.Add(xBlock);
        
        panel.Children.Add(new TextBlock { Text = " / " });
        
        var yBlock = new TextBlock();
        yBlock.Bind(TextBlock.TextProperty, new Avalonia.Data.Binding("Component.Y") { Source = di });
        panel.Children.Add(yBlock);
        
        panel.Children.Add(new TextBlock { Text = ")" });
        
        return panel;
    }

    private Control CreateStringTemplate(DesignItem di)
    {
        var textBlock = new TextBlock();
        textBlock.Bind(TextBlock.TextProperty, new Avalonia.Data.Binding("Component") { Source = di });
        return textBlock;
    }

    private Control CreateDefaultTemplate(DesignItem di)
    {
        var textBlock = new TextBlock();
        textBlock.Bind(TextBlock.TextProperty, new Avalonia.Data.Binding("Component") { Source = di });
        return textBlock;
    }
}