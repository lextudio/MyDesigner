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

namespace MyDesigner.Designer.Extensions;

public partial class RightClickContextMenu : ContextMenu
{
    private readonly DesignItem designItem;

    public RightClickContextMenu(DesignItem designItem)
    {
        this.designItem = designItem;
        InitializeComponent();
    }

    private void Click_BringToFront(object sender, RoutedEventArgs e)
    {
        if (designItem.ParentProperty == null || !designItem.ParentProperty.IsCollection)
            return;

        var collection = designItem.ParentProperty.CollectionElements;
        collection.Remove(designItem);
        collection.Add(designItem);
    }

    private void Click_SendToBack(object sender, RoutedEventArgs e)
    {
        if (designItem.ParentProperty == null || !designItem.ParentProperty.IsCollection)
            return;

        var collection = designItem.ParentProperty.CollectionElements;
        collection.Remove(designItem);
        collection.Insert(0, designItem);
    }

    private void Click_Backward(object sender, RoutedEventArgs e)
    {
        if (designItem.ParentProperty == null || !designItem.ParentProperty.IsCollection)
            return;

        var collection = designItem.ParentProperty.CollectionElements;
        var idx = collection.IndexOf(designItem);
        collection.RemoveAt(idx);
        collection.Insert(--idx < 0 ? 0 : idx, designItem);
    }

    private void Click_Forward(object sender, RoutedEventArgs e)
    {
        if (designItem.ParentProperty == null || !designItem.ParentProperty.IsCollection)
            return;

        var collection = designItem.ParentProperty.CollectionElements;
        var idx = collection.IndexOf(designItem);
        collection.RemoveAt(idx);
        var cnt = collection.Count;
        collection.Insert(++idx > cnt ? cnt : idx, designItem);
    }
}