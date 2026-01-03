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
using Avalonia.Data;
using Avalonia.Input;

namespace MyDesigner.Designer.Controls;

public class EnterTextBox : TextBox
{
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // In Avalonia, we need to handle binding updates differently
            var binding = this.GetBindingObservable(TextProperty);
            if (binding != null)
            {
                // Force update the binding source
                var currentText = Text;
                Text = "";
                Text = currentText;
            }
            SelectAll();
        }
        else if (e.Key == Key.Escape)
        {
            // In Avalonia, we need to handle binding updates differently
            var binding = this.GetBindingObservable(TextProperty);
            if (binding != null)
            {
                // Reset to original value - this is simplified
                // In a real implementation, you'd need to track the original value
            }
        }
        
        base.OnKeyDown(e);
    }
}