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
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MyDesigner.Design.PropertyGrid;
using DialogHostAvalonia;
using System.Threading.Tasks;

namespace MyDesigner.Designer.PropertyGrid.Editors.BrushEditor;

[TypeEditor(typeof(IBrush))]
public partial class BrushTypeEditor : UserControl
{
    public BrushTypeEditor()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override async void OnPointerReleased(PointerReleasedEventArgs e)
    {
        var propertyNode = DataContext as PropertyNode;
        if (propertyNode == null)
        {
            base.OnPointerReleased(e);
            return;
        }

        try
        {
            // إنشاء BrushEditorView جديد للحوار
            var brushEditorView = new BrushEditorView();
            brushEditorView.BrushEditor.Property = propertyNode;

            // فتح الحوار باستخدام DialogHost
            var result = await DialogHost.Show(brushEditorView, "BrushEditorDialog");
            
            // يمكن معالجة النتيجة هنا إذا لزم الأمر
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening brush editor dialog: {ex.Message}");
        }
        
        base.OnPointerReleased(e);
    }
}