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
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using DialogHostAvalonia;

namespace MyDesigner.Designer.PropertyGrid.Editors.BrushEditor;

public partial class BrushEditorView : UserControl
{
    public BrushEditor BrushEditor { get; }

    public BrushEditorView()
    {
        BrushEditor = new BrushEditor();
        DataContext = BrushEditor;

        InitializeComponent();
        
        // ربط أحداث الأزرار
        var okButton = this.FindControl<Button>("OkButton");
        var cancelButton = this.FindControl<Button>("CancelButton");
        
        if (okButton != null)
            okButton.Click += OkButton_Click;
            
        if (cancelButton != null)
            cancelButton.Click += CancelButton_Click;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        // تطبيق التغييرات
        BrushEditor.Commit();
        
        // إغلاق الحوار مع نتيجة إيجابية
        DialogHost.Close("BrushEditorDialog", true);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        // إغلاق الحوار بدون تطبيق التغييرات
        DialogHost.Close("BrushEditorDialog", false);
    }
}