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
using Avalonia.VisualTree;

namespace MyDesigner.Designer.Services;

internal sealed class AvaloniaTopLevelWindowService : ITopLevelWindowService
{
    public ITopLevelWindow GetTopLevelWindow(Control element)
    {
        var window = element.FindAncestorOfType<Window>();
        if (window != null)
            return new AvaloniaTopLevelWindow(window);
        return null;
    }

    public ITopLevelWindow GetTopLevelWindow(Visual element)
    {
        var window = element.FindAncestorOfType<Window>();
        if (window != null)
            return new AvaloniaTopLevelWindow(window);
        return null;
    }

    private sealed class AvaloniaTopLevelWindow : ITopLevelWindow
    {
        private readonly Window window;

        public AvaloniaTopLevelWindow(Window window)
        {
            this.window = window;
        }

        public void SetOwner(Window child)
        {
            // In Avalonia, we can set the parent window differently
            if (child is Window avaloniaChild)
            {
                // Set the parent window for proper modal behavior
                avaloniaChild.ShowDialog(window);
            }
        }

        public bool Activate()
        {
            window.Activate();
            return true;
        }
    }
}