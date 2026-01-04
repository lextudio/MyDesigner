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
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace MyDesigner.Designer.Controls;

public class ClearableTextBox : EnterTextBox
{
    protected override Type StyleKeyOverride => typeof(ClearableTextBox);
    
    private Button textRemoverButton;

    public ClearableTextBox()
    {
        GotFocus += TextBoxGotFocus;
        LostFocus += TextBoxLostFocus;
        PropertyChanged += (s, e) =>
        {
            if (e.Property == TextProperty)
                TextBoxTextChanged();
        };
        KeyUp += ClearableTextBox_KeyUp;
    }

    private void ClearableTextBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            TextRemoverClick(sender, null);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        textRemoverButton = e.NameScope.Find<Button>("TextRemover");
        if (null != textRemoverButton) 
            textRemoverButton.Click += TextRemoverClick;

        UpdateState();
    }

    protected void UpdateState()
    {
        if (string.IsNullOrEmpty(Text))
        {
            PseudoClasses.Remove(":text-remover-visible");
            PseudoClasses.Add(":text-remover-hidden");
        }
        else
        {
            PseudoClasses.Remove(":text-remover-hidden");
            PseudoClasses.Add(":text-remover-visible");
        }
    }

    private void TextBoxTextChanged()
    {
        UpdateState();
    }

    private void TextRemoverClick(object sender, RoutedEventArgs e)
    {
        Text = null;
        Focus();
    }

    private void TextBoxGotFocus(object sender, GotFocusEventArgs e)
    {
        UpdateState();
    }

    private void TextBoxLostFocus(object sender, RoutedEventArgs e)
    {
        UpdateState();
    }
}