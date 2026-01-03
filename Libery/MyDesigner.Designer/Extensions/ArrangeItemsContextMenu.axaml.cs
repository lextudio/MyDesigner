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
using Avalonia.Markup.Xaml;

namespace MyDesigner.Designer.Extensions;

public partial class ArrangeItemsContextMenu : ContextMenu
{
    private readonly DesignItem designItem;

    public ArrangeItemsContextMenu()
    {
        InitializeComponent();
    }

    public ArrangeItemsContextMenu(DesignItem designItem) : this()
    {
        this.designItem = designItem;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Click_ArrangeLeft(object sender, RoutedEventArgs e)
    {
        ModelTools.ArrangeItems(designItem.Services.Selection.SelectedItems, ArrangeDirection.Left);
    }

    private void Click_ArrangeHorizontalCentered(object sender, RoutedEventArgs e)
    {
        ModelTools.ArrangeItems(designItem.Services.Selection.SelectedItems, ArrangeDirection.HorizontalMiddle);
    }

    private void Click_ArrangeRight(object sender, RoutedEventArgs e)
    {
        ModelTools.ArrangeItems(designItem.Services.Selection.SelectedItems, ArrangeDirection.Right);
    }

    private void Click_ArrangeTop(object sender, RoutedEventArgs e)
    {
        ModelTools.ArrangeItems(designItem.Services.Selection.SelectedItems, ArrangeDirection.Top);
    }

    private void Click_ArrangeVerticalCentered(object sender, RoutedEventArgs e)
    {
        ModelTools.ArrangeItems(designItem.Services.Selection.SelectedItems, ArrangeDirection.VerticalMiddle);
    }

    private void Click_ArrangeBottom(object sender, RoutedEventArgs e)
    {
        ModelTools.ArrangeItems(designItem.Services.Selection.SelectedItems, ArrangeDirection.Bottom);
    }
}