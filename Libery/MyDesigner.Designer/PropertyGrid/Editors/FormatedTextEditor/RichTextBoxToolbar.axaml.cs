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
using Avalonia.Media;
using MyDesigner.Designer.Controls;

namespace MyDesigner.Designer.PropertyGrid.Editors.FormatedTextEditor;

/// <summary>
/// Simplified RichTextBoxToolbar for Avalonia
/// Note: This is a simplified version since Avalonia doesn't have RichTextBox
/// </summary>
public partial class RichTextBoxToolbar : UserControl
{
    public TextBox RichTextBox
    {
        get { return (TextBox)GetValue(RichTextBoxProperty); }
        set { SetValue(RichTextBoxProperty, value); }
    }

  

    public static readonly StyledProperty<TextBox> RichTextBoxProperty =
       AvaloniaProperty.Register<RichTextBoxToolbar, TextBox>(nameof(RichTextBox), null);

    public RichTextBoxToolbar()
    {
        InitializeComponent();
        InitializeFontFamilies();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        
        cmbFontFamily = this.FindControl<ComboBox>("cmbFontFamily");
        cmbFontSize = this.FindControl<ComboBox>("cmbFontSize");
        btnBold = this.FindControl<ToggleButton>("btnBold");
        btnItalic = this.FindControl<ToggleButton>("btnItalic");
        btnUnderline = this.FindControl<ToggleButton>("btnUnderline");
    }

    private void InitializeFontFamilies()
    {
        // Add common font families
        var fontFamilies = new[]
        {
            "Arial", "Helvetica", "Times New Roman", "Courier New", 
            "Verdana", "Georgia", "Comic Sans MS", "Impact",
            "Trebuchet MS", "Arial Black", "Palatino", "Garamond"
        };
        
        if (cmbFontFamily != null)
        {
            cmbFontFamily.ItemsSource = fontFamilies;
        }
    }

    public void SetValuesFromTextBlock(TextBlock textBlock)
    {
        if (textBlock == null) return;

        // Set font family
        if (cmbFontFamily != null)
        {
            cmbFontFamily.SelectedItem = textBlock.FontFamily?.Name ?? "Arial";
        }

        // Set font size
        if (cmbFontSize != null)
        {
            cmbFontSize.SelectedItem = ((int)textBlock.FontSize).ToString();
        }

        // Set font weight
        if (btnBold != null)
        {
            btnBold.IsChecked = textBlock.FontWeight == FontWeight.Bold;
        }

        // Set font style
        if (btnItalic != null)
        {
            btnItalic.IsChecked = textBlock.FontStyle == FontStyle.Italic;
        }

        // Note: Underline would need special handling in Avalonia
        if (btnUnderline != null)
        {
            btnUnderline.IsChecked = false;
        }
    }
}