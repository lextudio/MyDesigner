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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using MyDesigner.Designer.PropertyGrid.Editors.FormatedTextEditor;

namespace MyDesigner.Designer.Controls;

/// <summary>
///     Supports editing Text in the Designer
/// </summary>
public class InPlaceEditor : TemplatedControl
{
    protected override Type StyleKeyOverride => typeof(InPlaceEditor);
    /// <summary>
    ///     This property is binded to the Text Property of the editor.
    /// </summary>
    public static readonly StyledProperty<string> BindProperty =
        AvaloniaProperty.Register<InPlaceEditor, string>(nameof(Bind));

    private readonly DesignItem designItem;

    private bool _isChangeGroupOpen;
    private ChangeGroup changeGroup;
    private TextBox editor; // Using TextBox instead of RichTextBox for simplicity in Avalonia

    public InPlaceEditor(DesignItem designItem)
    {
        this.designItem = designItem;

     
    }

    public string Bind
    {
        get => GetValue(BindProperty);
        set => SetValue(BindProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        editor = e.NameScope.Find<TextBox>("PART_Editor"); // Gets the TextBox-editor from the Template
        if (editor != null)
        {
            editor.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                    e.Handled = true;
            };
            
            ToolTip.SetTip(this, "Edit the Text. Press" + Environment.NewLine + 
                          "Enter to make changes." + Environment.NewLine +
                          "Shift+Enter to insert a newline." + Environment.NewLine + 
                          "Esc to cancel editing.");

            // Set initial text from TextBlock
            if (designItem.Component is TextBlock textBlock)
            {
                editor.Text = textBlock.Text ?? string.Empty;
            }
            
            editor.TextChanged += editor_TextChanged;
        }
    }

    private void editor_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (designItem.Component is TextBlock textBlock && editor != null)
        {
            // Simple text assignment - for rich text, would need more complex handling
            designItem.Properties[TextBlock.TextProperty].SetValue(editor.Text);
        }
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        StartEditing();
    }

    /// <summary>
    ///     Change is committed if the user releases the Escape Key.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            switch (e.Key)
            {
                case Key.Enter:
                    // Commit the changes to DOM.
                    if (editor != null)
                    {
                        if (designItem.Properties[TemplatedControl.FontFamilyProperty].GetConvertedValueOnInstance<FontFamily>() !=
                            editor.FontFamily)
                            designItem.Properties[TemplatedControl.FontFamilyProperty].SetValue(editor.FontFamily);
                        if (designItem.Properties[TemplatedControl.FontSizeProperty].GetConvertedValueOnInstance<double>() !=
                            editor.FontSize)
                            designItem.Properties[TemplatedControl.FontSizeProperty].SetValue(editor.FontSize);
                        if (designItem.Properties[TemplatedControl.FontStyleProperty].GetConvertedValueOnInstance<FontStyle>() !=
                            editor.FontStyle)
                            designItem.Properties[TemplatedControl.FontStyleProperty].SetValue(editor.FontStyle);
                        if (designItem.Properties[TemplatedControl.FontWeightProperty].GetConvertedValueOnInstance<FontWeight>() !=
                            editor.FontWeight)
                            designItem.Properties[TemplatedControl.FontWeightProperty].SetValue(editor.FontWeight);
                    }

                    if (changeGroup != null && _isChangeGroupOpen)
                    {
                        if (designItem.Component is TextBlock && editor != null)
                        {
                            designItem.Properties[TextBlock.TextProperty].SetValue(editor.Text);
                        }
                        changeGroup.Commit();
                        _isChangeGroupOpen = false;
                    }

                    changeGroup = null;
                    IsVisible = false;
                    designItem.ReapplyAllExtensions();
                    if (designItem.Component is TextBlock textBlock)
                        textBlock.IsVisible = true;
                    break;
                case Key.Escape:
                    AbortEditing();
                    break;
            }
        else if (e.Key == Key.Enter && editor != null) 
        {
            // Insert newline for Shift+Enter
            var caretIndex = editor.CaretIndex;
            var text = editor.Text ?? string.Empty;
            editor.Text = text.Insert(caretIndex, Environment.NewLine);
            editor.CaretIndex = caretIndex + Environment.NewLine.Length;
        }
    }

    public void AbortEditing()
    {
        if (changeGroup != null && _isChangeGroupOpen)
        {
            changeGroup.Abort();
            _isChangeGroupOpen = false;
        }

        IsVisible = false;
    }

    public void StartEditing()
    {
        if (changeGroup == null)
        {
            changeGroup = designItem.OpenGroup("Change Text");
            _isChangeGroupOpen = true;
        }

        IsVisible = true;
    }
}