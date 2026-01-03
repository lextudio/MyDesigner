// Copyright (c) 2019 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.

#nullable enable
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.IO;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System.Xml;
using MyDesigner.Designer.themes;
using MyDesigner.Designer.Xaml;
using MyDesigner.XamlDom;

namespace MyDesigner.Designer.Extensions;

public partial class EditStyleContextMenu : ContextMenu
{
    private readonly DesignItem designItem;

    public EditStyleContextMenu(DesignItem designItem)
    {
        this.designItem = designItem;

        SpecialInitializeComponent();
    }

    /// <summary>
    ///     Fixes InitializeComponent with multiple Versions of same Assembly loaded
    /// </summary>
    public void SpecialInitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Click_EditStyle(object? sender, RoutedEventArgs e)
    {
        var cg = designItem.OpenGroup("Edit Style");

        var element = designItem.View;
        var defaultStyleKey = element.StyleKey;
        var style = Application.Current?.TryFindResource(defaultStyleKey, out var resource) == true ? resource as Style : null;

        var service = (XamlComponentService)designItem.Services.Component;

        var ms = new MemoryStream();
        var writer = new XmlTextWriter(ms, Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        
        // Note: Avalonia doesn't have XamlWriter.Save equivalent
        // This would need to be implemented differently or use a different approach
        // XamlWriter.Save(style, writer);

        var rootItem = designItem.Context.RootItem as XamlDesignItem;

        ms.Position = 0;
        var sr = new StreamReader(ms);
        var xaml = sr.ReadToEnd();

        var xamlObject = XamlParser.ParseSnippet(rootItem.XamlObject, xaml,
            ((XamlDesignContext)designItem.Context).ParserSettings);

        var styleDesignItem = service.RegisterXamlComponentRecursive(xamlObject);
        try
        {
            designItem.Properties.GetProperty("Resources").CollectionElements.Add(styleDesignItem);
            cg.Commit();
        }
        catch (Exception)
        {
            cg.Abort();
        }
    }
}