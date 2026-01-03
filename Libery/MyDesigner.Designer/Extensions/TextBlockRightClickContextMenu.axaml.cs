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
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using Avalonia.Controls;
using Avalonia.Interactivity;
using MyDesigner.Design.UIExtensions;
using MyDesigner.Designer.PropertyGrid.Editors.FormatedTextEditor;

namespace MyDesigner.Designer.Extensions;

public partial class TextBlockRightClickContextMenu : ContextMenu
{
    private readonly DesignItem designItem;

    public TextBlockRightClickContextMenu(DesignItem designItem)
    {
        this.designItem = designItem;
        InitializeComponent();
    }

    private void Click_EditFormatedText(object sender, RoutedEventArgs e)
    {
        var dlg = new Window
        {
            Content = new FormatedTextEditor(designItem),
            Width = 440,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var designPanel = (DesignPanel)designItem.Context.Services.DesignPanel;
        var parentWindow = designPanel.FindAncestorOfType<Window>();
        if (parentWindow != null)
        {
            dlg.ShowDialog(parentWindow);
        }
        else
        {
            // In Avalonia, we need a parent window for ShowDialog
            var topLevel = TopLevel.GetTopLevel(designPanel);
            if (topLevel is Window window)
            {
                dlg.ShowDialog(window);
            }
        }
    }
}