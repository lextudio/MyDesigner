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
using Avalonia.Media;

namespace MyDesigner.Designer.PropertyGrid.Editors.FormatedTextEditor;

/// <summary>
/// Simplified FormatedTextEditor for Avalonia
/// Note: This is a simplified version since Avalonia doesn't have RichTextBox
/// </summary>
public partial class FormatedTextEditor : UserControl
{
    private readonly DesignItem designItem;
    
   

    public FormatedTextEditor(DesignItem designItem)
    {
        InitializeComponent();
        this.designItem = designItem;

        // Initialize with current TextBlock values
        var textBlock = (TextBlock)designItem.Component;
        InitializeFromTextBlock(textBlock);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        
        FontFamilyCombo = this.FindControl<ComboBox>("FontFamilyCombo");
        FontSizeCombo = this.FindControl<ComboBox>("FontSizeCombo");
        BoldButton = this.FindControl<ToggleButton>("BoldButton");
        ItalicButton = this.FindControl<ToggleButton>("ItalicButton");
        UnderlineButton = this.FindControl<ToggleButton>("UnderlineButton");
        TextEditor = this.FindControl<TextBox>("TextEditor");
        OkButton = this.FindControl<Button>("OkButton");
        CancelButton = this.FindControl<Button>("CancelButton");
        
        InitializeFontFamilies();
        InitializeFontSizes();
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
        
        FontFamilyCombo.ItemsSource = fontFamilies;
    }

    private void InitializeFontSizes()
    {
        var fontSizes = new[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
        FontSizeCombo.ItemsSource = fontSizes;
    }

    private void InitializeFromTextBlock(TextBlock textBlock)
    {
        // Set text content
        TextEditor.Text = textBlock.Text ?? "";
        
        // Set font family
        FontFamilyCombo.SelectedItem = textBlock.FontFamily?.Name ?? "Arial";
        
        // Set font size
        FontSizeCombo.SelectedItem = (int)textBlock.FontSize;
        
        // Set font weight
        BoldButton.IsChecked = textBlock.FontWeight == FontWeight.Bold;
        
        // Set font style
        ItalicButton.IsChecked = textBlock.FontStyle == FontStyle.Italic;
        
        // Note: Avalonia TextBlock doesn't have direct underline support like WPF
        // This would need to be handled through TextDecorations if available
        UnderlineButton.IsChecked = false;
        
        // Set colors
        TextEditor.Foreground = textBlock.Foreground;
        TextEditor.Background = textBlock.Background;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        var changeGroup = designItem.OpenGroup("Formatted Text");

        try
        {
            // Update TextBlock properties
            var textBlock = (TextBlock)designItem.Component;
            
            // Set text
            designItem.Properties.GetProperty(TextBlock.TextProperty).SetValue(TextEditor.Text);
            
            // Set font family
            if (FontFamilyCombo.SelectedItem != null)
            {
                var fontFamily = new FontFamily(FontFamilyCombo.SelectedItem.ToString());
                designItem.Properties.GetProperty(TextBlock.FontFamilyProperty).SetValue(fontFamily);
            }
            
            // Set font size
            if (FontSizeCombo.SelectedItem != null)
            {
                designItem.Properties.GetProperty(TextBlock.FontSizeProperty).SetValue((double)(int)FontSizeCombo.SelectedItem);
            }
            
            // Set font weight
            var fontWeight = BoldButton.IsChecked == true ? FontWeight.Bold : FontWeight.Normal;
            designItem.Properties.GetProperty(TextBlock.FontWeightProperty).SetValue(fontWeight);
            
            // Set font style
            var fontStyle = ItalicButton.IsChecked == true ? FontStyle.Italic : FontStyle.Normal;
            designItem.Properties.GetProperty(TextBlock.FontStyleProperty).SetValue(fontStyle);
            
            // Note: TextDecorations for underline would need special handling in Avalonia
            
            changeGroup.Commit();
        }
        catch
        {
            changeGroup.Abort();
            throw;
        }

        CloseWindow();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        CloseWindow();
    }

    private void CloseWindow()
    {
        // Find parent window and close it
        var window = this.FindLogicalAncestorOfType<Window>();
        window?.Close();
    }
}